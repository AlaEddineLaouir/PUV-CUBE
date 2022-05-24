using System;
using System.Collections;
using System.Collections.Generic;
using Google.OrTools.LinearSolver;

namespace PUV_Cube
{
    class QueryWorkloadManager
    {
        public ArrayList qwl = null;
        public string cube_name;

        //For stats

        public int count_qwl_values;


        public double queries_error = 0.0;
        public double inference_error = 0.0;
        public double user_inference_error = 0.0;

        // For our new metric of privacy
        public double original_inference_error = 0.0;
        public double new_inference_error = 0.0;

        public double global_m_r, global_l_r, global_alpha_r;
        public double global_sum; public int global_count;

        internal void load_query_workload_from_file(string path, string file)
        {
            this.qwl = (ArrayList)Helper.load_object_from_file(path, file);
        }

        public QueryWorkloadManager(string cube_name,ArrayList qwl)
        {
            this.qwl = qwl;
            this.cube_name = cube_name;
        }
        public void create_accuracy_grids(int[] deltas)
        {
            foreach (SQuery query in qwl)
            {
                query.init_stats();
                query.create_accuracy_grid(deltas);
            }
        }
        public void compute_individual_storage_space(int global_allocated_space)
        {
            // i used this loop to compute also the count of non empty values
            // as it's needed in comûting qwl stats
            this.count_qwl_values = 0;

            double sum_a = 0, sum_b = 0;
            foreach (SQuery query in qwl)
            {
                if (query.count_non_empty_values > 0)
                {
                    // Reason --- first comment
                    this.count_qwl_values += query.count_non_empty_values;

                    sum_a += query.m_r;
                    sum_b += query.l_r * query.alpha_r;
                }
            }
            foreach (SQuery query in qwl)
            {
                if (query.count_non_empty_values > 0)
                {
                    double query_B_propotion = ((query.m_r + (query.l_r * query.alpha_r)) / (sum_a + sum_b));
                    int b_query = (int)(global_allocated_space * query_B_propotion);
                    query.set_query_allocated_space(b_query);
                }

            }
        }

        public bool qwl_satisfy_thresholds(double accracy_error_threshold, double inference_error_threshold)
        {
            compute_qwl_stats();
            return (accracy_error_threshold >= this.queries_error && this.inference_error >= inference_error_threshold);
        }

        public void compute_individual_storage_space_lp(int allocation,double ratio)
        {

            this.count_qwl_values = 0;

            Variable[] queries_spaces = new Variable[qwl.Count];
            double[] queries_coef_lp = new double[qwl.Count];

            // Create the linear solver with the SCIP backend.
            Solver solver = Solver.CreateSolver("SCIP");

            for (int i = 0; i< qwl.Count;i++)
            {
                SQuery query = (SQuery)qwl[i];

                // Reason --- To compute the queries Error correctly with weigths
                this.count_qwl_values += query.count_non_empty_values;

                double coef = (double)-1.0 / (double)(query.count_non_empty_values * qwl.Count);
                queries_coef_lp[i] = coef;
                
                // Variable for query allocation
                Variable var = solver.MakeIntVar(0.0, double.PositiveInfinity, "space_"+i);
                solver.Add(var <= query.count_non_empty_values);

                int min_allo = (int)(query.count_non_empty_values * ratio / 2);
                if (min_allo == 0)
                    min_allo = 1;

                solver.Add(var >= min_allo);

                queries_spaces[i] = var;
            }
            Constraint cons = solver.MakeConstraint(allocation, allocation);
            for (int i = 0; i < queries_spaces.Length; i++)
            {
                cons.SetCoefficient(queries_spaces[i], 1);
            }

            Objective minimize_error = solver.Objective();
            for(int i = 0; i < queries_spaces.Length;i++)
            {
                minimize_error.SetCoefficient(queries_spaces[i], queries_coef_lp[i]);
            }
            minimize_error.SetOffset(1);
            minimize_error.SetMinimization();

            //Console.WriteLine(solver.ExportModelAsLpFormat(false));

            solver.Solve();
            int total_space_devided = 0;
            for (int i = 0; i < qwl.Count; i++)
            {
                int space_allocated = (int)queries_spaces[i].SolutionValue();
                total_space_devided += space_allocated;
                SQuery q = ((SQuery)qwl[i]);
                q.set_query_lp_allocated_space(space_allocated,ratio);
            }
            Console.WriteLine();

        }

        private void compute_qwl_stats()
        {
            this.inference_error = 0.0;
            this.user_inference_error = 0.0;
            this.queries_error = 0.0;

            foreach (SQuery query in qwl)
            {
                // this.count_qwl_values > was computed in > compute_individual_storage_space(int)
                //double query_weigth = (double)query.count_non_empty_values / (double)this.count_qwl_values;//1.0/(double)qwl.Count;//
                // Just to test avrage error and see the diffrence
                double query_weigth = (double)1.0 / (double)this.qwl.Count;
                this.inference_error += query_weigth * query.inference_error;
                this.queries_error += query_weigth * query.query_error;
                this.user_inference_error += query_weigth * query.user_inference_error;
                //For our new privaccy metric
                //this.original_inference_error += query_weigth * query.original_inference_error;
                this.new_inference_error += query_weigth * query.new_inference_error;
            }
        }

        public void compute_global_stats()
        {
            double sum_qwl = 0.0;
            int count_non_empty_vals = 0;
            ArrayList non_empty_values_mdx_intervals = new ArrayList();


            foreach(SQuery query in qwl)
            {
                sum_qwl += query.sum_result;
                count_non_empty_vals += query.non_empty_values_mdx_intervals.Count;
                non_empty_values_mdx_intervals.InsertRange(non_empty_values_mdx_intervals.Count, query.non_empty_values_mdx_intervals);
            }

            this.global_sum = sum_qwl;
            this.global_count = count_non_empty_vals;

            if (count_non_empty_vals == 1)
            {
                this.global_m_r = 0;
                this.global_l_r = 0;
                this.global_alpha_r = 0;
            }
            else
            {
                double mean = sum_qwl / count_non_empty_vals;
                double ro_y = Math.Sqrt((double)6 / (double)count_non_empty_vals);

                double diff_pow_2 = 0;
                double diff_pow_3 = 0;

                foreach (ArrayList intervals in non_empty_values_mdx_intervals)
                {
                    double val = MDXQueryExecuter.get_value_from_cube(cube_name, intervals);

                    double diff = val - mean;
                    diff_pow_2 += Math.Pow(diff, 2);
                    diff_pow_3 += Math.Pow(diff, 3);
                }
                double teta_1 = Math.Pow(diff_pow_3, 2) / Math.Pow(diff_pow_2, 3);
                if (diff_pow_2 == 0)
                    teta_1 = 0;


                this.global_alpha_r = (teta_1 / ro_y) - 2.6;

                if ((teta_1 / ro_y) > 2.6)
                    this.global_l_r = 1;
                else
                    this.global_l_r = 0;

                this.global_m_r = diff_pow_2 + (Math.Abs(teta_1) / count_non_empty_vals);
            }
        }

        public void sort_queries(double accuracy_error_threshold, double inference_error_threshold)
        {
            int n = qwl.Count - 1;
            for (int i = n; i >= 0; i--)
            {
                for (int j = 1; j <= i; j++)
                {
                    SQuery q1 = ((SQuery)qwl[j - 1]);
                    SQuery q2 = ((SQuery)qwl[j]);
                    double q1_dist = query_distance(q1, accuracy_error_threshold, inference_error_threshold);
                    double q2_dist = query_distance(q2, accuracy_error_threshold, inference_error_threshold);
                    if (q1_dist > q2_dist)
                    {
                        SQuery temp = (SQuery)qwl[j - 1];
                        qwl[j - 1] = qwl[j];
                        qwl[j] = temp;
                    }
                }
            }

        }
        private double query_distance(SQuery query, double accuracy_error_threshold, double inference_error_threshold)
        {
            double distance = query.inference_error - inference_error_threshold;
            distance += accuracy_error_threshold - query.query_error;

            return distance;
        }

        public int get_needed_swap_size(double accuracy_error_threshold, double inference_error_threshold)
        {
            SQuery best_query = (SQuery)qwl[0];
            SQuery worst_query = (SQuery)qwl[qwl.Count - 1];

            //compute prefered transfert size
            double coef = (1.0 - accuracy_error_threshold) / (1.0 - inference_error_threshold);


            int best_swap_size = ((int)(coef * worst_query.count_non_empty_values)) - worst_query.space_b;
            int maximum_swap_size = best_query.space_b - ((int)(coef * best_query.count_non_empty_values));

            if (best_swap_size >= maximum_swap_size)
                if (maximum_swap_size < 0)
                    return 0;
                else
                    return maximum_swap_size;
            else
                return best_swap_size;

        }

        public Array refine_qwl(Array cube, int samples_swap_size)
        {
            SQuery best_query = (SQuery)qwl[0];
            SQuery worst_query = (SQuery)qwl[qwl.Count - 1];

            best_query.set_query_allocated_space(best_query.space_b - samples_swap_size);
            worst_query.set_query_allocated_space(worst_query.space_b + samples_swap_size);

            //clean query region before re-sampling
            cube = best_query.clean_query_region(cube);
            cube = worst_query.clean_query_region(cube);

            cube = best_query.get_samples(cube);
            cube = worst_query.get_samples(cube);

            return cube;
        }

        public double[] print_view_qwl_stats()
        {
            compute_qwl_stats();


            double[] stats = new double[3];
            stats[0] = Math.Round(queries_error, 2);
            stats[1] = Math.Round(inference_error, 2);
            stats[2] = Math.Round(user_inference_error, 2);

            Console.WriteLine("Cube name >> "+cube_name+" Error : " + stats[0] + " Privacy : " + stats[1] );

            return stats;
        }

    }
}

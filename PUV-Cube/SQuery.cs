using System;
using System.Collections;
using System.Collections.Generic;
using Google.OrTools.LinearSolver;
using System.Runtime.Serialization;
using System.Text;

namespace PUV_Cube
{
    [Serializable()]
    class SQuery : ISerializable
    {
        public string cube_name;

        public int[] starting_indexs;
        public int[] dim_travels;
        public int[] deltas_grid;

        public ArrayList subqueries_list;
        public ArrayList mdx_intervals;
        public ArrayList non_empty_values_mdx_intervals;

        //Stat properties
        public int count_non_empty_values = 0;
        public double sum_result = 0.0;

        public int new_count = 0;
        public double new_sum = 0.0;

        public double query_error = 0.0;
        public double inference_error = 0.0;
        public double user_inference_error = 0.0;

        // For our new privacy metric
        public double original_inference_error = 0.0;
        public double new_inference_error = 0.0;

        /// Space computation
        public double m_r, l_r, alpha_r;
        public int space_b = 0;

        public SQuery(string cube_name,int[] starting_indexs, int[] dim_travels)
        {
            this.cube_name = cube_name;
            this.starting_indexs = starting_indexs;
            this.dim_travels = dim_travels;

            ArrayList intervals = Helper.create_intervals(starting_indexs, dim_travels);

            this.mdx_intervals = MDXQueryExecuter.mdx_query_ranges(cube_name,intervals);
            //done here instead of in init stats
            //because query generator needs the count to accept or refuse the query 
            this.count_non_empty_values = MDXQueryExecuter.get_count_values_cube(cube_name,this.mdx_intervals);
        }

        public int reset_query_indexs()
        {
            
            ArrayList intervals = Helper.create_intervals(starting_indexs, dim_travels);

            this.mdx_intervals = MDXQueryExecuter.mdx_query_ranges(cube_name, intervals);
            //done here instead of in init stats
            //because query generator needs the count to accept or refuse the query
            this.count_non_empty_values = MDXQueryExecuter.get_count_values_cube(cube_name,this.mdx_intervals);
            return this.count_non_empty_values;


        }
        public void init_stats()
        {
            compute_query_stats();
        }

        public void create_accuracy_grid(int[] deltas)
        {
            this.deltas_grid = deltas;
            subqueries_list = new ArrayList();

            this.sum_result = 0.0;
            this.count_non_empty_values = 0;
            this.non_empty_values_mdx_intervals = new ArrayList();

            bool done = false;
            int[] temp_grid_start_indexs = Helper.copy_array_ints(starting_indexs);
            while (!done)
            {
                SSubQuery ag_cell = new SSubQuery(cube_name,Helper.copy_array_ints(temp_grid_start_indexs), this.deltas_grid);
                if (ag_cell.count_non_empty_values > 1)
                {
                    this.subqueries_list.Add(ag_cell);

                    this.sum_result += ag_cell.sum_result;
                    this.count_non_empty_values += ag_cell.count_non_empty_values;

                    this.non_empty_values_mdx_intervals.AddRange(ag_cell.non_empty_values_mdx_intervals);
                }

                bool pass = true;
                for (int i = temp_grid_start_indexs.Length - 1; (i >= 0 && pass); i--)
                {
                    if (temp_grid_start_indexs[i] + (2*this.deltas_grid[i]) + 1 > starting_indexs[i] + this.dim_travels[i])
                    {
                        temp_grid_start_indexs[i] = starting_indexs[i];
                    }
                    else
                    {
                        temp_grid_start_indexs[i] += this.deltas_grid[i] + 1;
                        pass = false;
                    }
                }
                done = pass;
            }
        }

        public void set_query_allocated_space(int new_space)
        {
            // check if it got more then needed
            if (new_space >= this.count_non_empty_values)
                this.space_b = this.count_non_empty_values;
            else
                this.space_b = new_space;

            double sum_a = 0, sum_b = 0;
            foreach (SSubQuery cell in this.subqueries_list)
            {
                if (cell.count_non_empty_values > 0)
                {
                    sum_a += cell.m_r;
                    sum_b += cell.l_r * cell.alpha_r;
                }
            }

            foreach (SSubQuery cell in this.subqueries_list)
            {
                if (cell.count_non_empty_values > 0)
                {
                    double cell_B_propotion = ((cell.m_r + (cell.l_r * cell.alpha_r)) / (sum_a + sum_b));
                    int b_cell = (int)(this.space_b * cell_B_propotion);
                    if (b_cell < 0)
                        b_cell = 0;
                    if (cell.count_non_empty_values < b_cell)
                        cell.subquery_space = cell.count_non_empty_values;
                    else
                        cell.subquery_space = b_cell;
                }
            }
        }

        internal void set_query_lp_allocated_space(int v,double ratio)
        {
            if(v > this.subqueries_list.Count)
            {
                Variable[] sub_queries_spaces = new Variable[subqueries_list.Count];
                double[] sub_queries_coef_lp = new double[subqueries_list.Count];

                // Create the linear solver with the SCIP backend.
                Solver solver = Solver.CreateSolver("SCIP");

                int effective_gird_total_space = 0;

                for (int i = 0; i < subqueries_list.Count; i++)
                {
                    SSubQuery query = (SSubQuery)subqueries_list[i];
                    //Some points would be lost by creating the accuracy grid
                    //So the query would have (N) and AG have (N - epsilon)
                    effective_gird_total_space += query.count_non_empty_values;

                    sub_queries_coef_lp[i] = (double)-1.0 / (double)(query.count_non_empty_values * subqueries_list.Count);

                    // Variable for query allocation
                    Variable var = solver.MakeIntVar(0.0, double.PositiveInfinity, "space_" + i);
                    solver.Add(var <= query.count_non_empty_values);
                    int min_allo = (int)(query.count_non_empty_values * ratio / 2);
                    solver.Add(var >= min_allo);

                    sub_queries_spaces[i] = var;
                }
                Constraint cons;
                if( v <= effective_gird_total_space )
                    cons = solver.MakeConstraint(v, v);
                else
                    cons = solver.MakeConstraint(effective_gird_total_space, effective_gird_total_space);
                for (int i = 0; i < sub_queries_spaces.Length; i++)
                {
                    cons.SetCoefficient(sub_queries_spaces[i], 1);
                }

                Objective minimize_error = solver.Objective();
                for (int i = 0; i < sub_queries_spaces.Length; i++)
                {
                    minimize_error.SetCoefficient(sub_queries_spaces[i], sub_queries_coef_lp[i]);
                }
                minimize_error.SetOffset(1);
                minimize_error.SetMinimization();

                //Console.WriteLine(solver.ExportModelAsLpFormat(false));


                solver.Solve();

                for (int i = 0; i < subqueries_list.Count; i++)
                {
                    int space_allocated = (int)sub_queries_spaces[i].SolutionValue();
                    ((SSubQuery)subqueries_list[i]).subquery_space = space_allocated;

                }
            }
            else
            {
                for (int i = 0; (i < subqueries_list.Count && v > 0); i++)
                {
                    ((SSubQuery)subqueries_list[i]).subquery_space = 1;
                    v--;
                }
            }
        }

        public Array get_samples(Array cube, view_creation_algo type)
        {
            this.new_sum = 0.0;
            this.new_count = 0;
            this.inference_error = 0.0;
            this.query_error = 0.0;

            int count = 0;
            foreach (SSubQuery subquery in this.subqueries_list)
            {
               if(subquery.count_non_empty_values > 1 )
                {
                    count++;

                    switch(type)
                    {
                        case view_creation_algo.Mx_privacy:
                            cube = subquery.sample_subquery_region_privacy(cube);
                            break;
                        case view_creation_algo.Mx_utility:
                            cube = subquery.sample_subquery_region_accuracy(cube);
                            break;
                        case view_creation_algo.Perturbation:
                            cube = subquery.sample_preturb_subquery_region_accuracy(cube);
                            break;
                    }

                    this.new_sum += subquery.new_sum;
                    this.new_count += subquery.new_value_count;

                    this.inference_error += subquery.avg_inference_error;
                    //this.query_error += cell.sum_accuracy_error;

                }
            }

            compute_accuracy_and_inference_errors();
            return cube;
        }

        public void compute_accuracy_and_inference_errors()
        {
            double infered_avg, user_infered_avg, original_avg;
            if (this.count_non_empty_values > 0)
            {
                original_avg = this.sum_result / (double)this.count_non_empty_values;
                user_infered_avg = this.new_sum / (double)this.count_non_empty_values;
                if (this.new_count > 0)
                {
                    infered_avg = this.new_sum / (double)this.new_count;
                }
                else
                    infered_avg = 0.0;
            }
            else
            {
                this.inference_error = 0.0;
                this.user_inference_error = 0.0;
                this.query_error = 1.0;

               
                return;
            }

            this.query_error = Math.Round((this.sum_result - this.new_sum) / this.sum_result, 2);
            if(this.new_count > 0)
            {
                this.user_inference_error = Math.Abs(Math.Round((original_avg - user_infered_avg) / original_avg, 2));
            }
            else
            {
                this.inference_error = 0;
                this.user_inference_error = 0;

            }
        }

       
        private void compute_query_stats()
        {
            if (count_non_empty_values == 1)
            {
                this.m_r = 0;
                this.l_r = 0;
                this.alpha_r = 0;
            }
            else
            {
                double mean = this.sum_result / this.count_non_empty_values;
                double ro_y = Math.Sqrt((double)6 / (double)this.count_non_empty_values);

                double diff_pow_2 = 0;
                double diff_pow_3 = 0;

                foreach (ArrayList intervals in this.non_empty_values_mdx_intervals)
                {
                    double val = MDXQueryExecuter.get_value_from_cube(cube_name,intervals);

                    double diff = val - mean;
                    diff_pow_2 += Math.Pow(diff, 2);
                    diff_pow_3 += Math.Pow(diff, 3);
                }
                double teta_1 = Math.Pow(diff_pow_3, 2) / Math.Pow(diff_pow_2, 3);

                if (diff_pow_2 <= 0)
                    teta_1 = 0;

                this.alpha_r = (teta_1 / ro_y) - 2.6;

                if ((teta_1 / ro_y) > 2.6)
                    this.l_r = 1;
                else
                    this.l_r = 0;

                this.m_r = diff_pow_2 + (Math.Abs(teta_1) / this.count_non_empty_values);
            }
        }

        public Array clean_query_region(Array cube)
        {
            foreach (ArrayList intervals in this.non_empty_values_mdx_intervals)
            {
                int[] indexs_in_cube = MDXQueryExecuter.query_cell_indexs_from_mdx(cube_name,intervals);
                cube.SetValue(0, indexs_in_cube);
            }

            return cube;
        }

    }
}

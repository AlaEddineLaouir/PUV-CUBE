using System;
using System.Collections;

namespace PUV_Cube
{
    class SSubQuery
    {
        public string cube_name;
        public int[] starting_indexs;
        public int[] dim_travels;

        public ArrayList query_accuracy_grid;
        public ArrayList mdx_intervals;
        public ArrayList non_empty_values_mdx_intervals;

        //Stat properties
        public int count_non_empty_values = 0;
        public double sum_result = 0.0;
        public int new_value_count = 0;
        public double new_sum = 0.0;
        // I added this to compute the inference error on each subquery
        public double avg_inference_error= 0.0;
        public double sum_accuracy_error = 0.0;

        public double[] subquery_vals;
        int best_starting_index;


        // Our privacy metric
        public double original_inference_error = 0.0;
        public double new_inferece_error = 0.0;

        /// Space computation
        public double m_r, l_r, alpha_r;
        public int subquery_space = 0;

        public SSubQuery(string cube_name,int[] starting_indexs, int[] dim_travels)
        {
            this.cube_name = cube_name;
            this.starting_indexs = starting_indexs;
            this.dim_travels = dim_travels;

            ArrayList intervals = Helper.create_intervals(starting_indexs, dim_travels);

            this.mdx_intervals = MDXQueryExecuter.mdx_query_ranges(cube_name,intervals);
            //init stats
            this.sum_result = MDXQueryExecuter.get_value_from_cube(cube_name,this.mdx_intervals);
            this.count_non_empty_values = MDXQueryExecuter.get_count_values_cube(cube_name,this.mdx_intervals);
            this.non_empty_values_mdx_intervals = MDXQueryExecuter.get_non_empty_intervals_query(cube_name,this.mdx_intervals);

            compute_query_stats();
        }


        public Array sample_subquery_region_privacy(Array cube)
        {


            if (this.count_non_empty_values <= 1)
                return cube;
            this.new_value_count = 0;
            this.new_sum = 0.0;
            this.avg_inference_error = 0;
            this.sum_accuracy_error = 1;

            if(subquery_space > 0)
            {
                this.subquery_vals = new double[this.non_empty_values_mdx_intervals.Count];

                // Getting the values and sorting them
                //
                int index = 0;
                foreach (ArrayList intervals in this.non_empty_values_mdx_intervals)
                {
                    double val = MDXQueryExecuter.get_value_from_cube(cube_name, intervals);

                    subquery_vals[index] = val;
                    index++;
                }
                Array.Sort(subquery_vals);

                //Selecting the a subgroup that will garanties the best privacy
                best_starting_index = 0;
                int temp_strating_index = 0;

                double best_inference_error = 0.0;
                double best_sum = 0.0;


                double original_avg = this.sum_result / (double)this.count_non_empty_values;

                while (temp_strating_index + this.subquery_space <= this.non_empty_values_mdx_intervals.Count)
                {
                    double temp_sum = 0.0;
                    for (int i = temp_strating_index; i < temp_strating_index + this.subquery_space; i++)
                    {
                        temp_sum += subquery_vals[i];
                    }
                    double temp_avg = temp_sum / (double)this.subquery_space;
                    double temp_infrence_error = Math.Abs(temp_avg - original_avg) / original_avg;

                    if (temp_infrence_error >= best_inference_error)
                    {
                        best_sum = temp_sum;
                        best_inference_error = temp_infrence_error;
                        best_starting_index = temp_strating_index;
                    }
                    temp_strating_index++;
                }

                this.new_sum = best_sum;
                this.new_value_count = this.subquery_space;

                this.sum_accuracy_error = Math.Abs(this.new_sum - this.sum_result) / this.sum_result;
                this.avg_inference_error = best_inference_error;


                // Our privacy metric
                if (this.new_value_count > 0)
                {
                    double new_avg = this.new_sum / (double)this.new_value_count;
                    for (int i = best_starting_index; i < best_starting_index + this.subquery_space; i++)
                    {
                        double val = subquery_vals[i];

                        this.new_inferece_error += Math.Abs(val - new_avg) / Math.Max(val, 1);
                    }
                    this.new_inferece_error = this.new_inferece_error / (double)this.count_non_empty_values;
                    //this.new_inferece_error = this.new_inferece_error / new_avg;

                }

                //We need a another for loop to populate the cube with the selected values
                //Also in the previous loop, when we sorted the values we should also sort the intefavls
                // so we van recreate the the cube with multi dimensional array

            }
            return cube;
        }

        public Array sample_preturb_subquery_region_accuracy(Array cube)
        {
            this.new_value_count = 0;
            this.new_sum = 0.0;
            if (this.count_non_empty_values <= 1)
                return cube;

            this.avg_inference_error = 0;
            this.sum_accuracy_error = 1;

            if (subquery_space > 0)
            {
                this.subquery_vals = new double[this.non_empty_values_mdx_intervals.Count];

                // Getting the values and sorting them
                //
                int index = 0;
                foreach (ArrayList intervals in this.non_empty_values_mdx_intervals)
                {
                    double val = MDXQueryExecuter.get_value_from_cube(cube_name, intervals);

                    subquery_vals[index] = val;
                    index++;
                }
                Array.Sort(subquery_vals);

                //Selecting the a subgroup that will garanties the best privacy
                best_starting_index = subquery_vals.Length - this.subquery_space;

                double temp_sum = 0.0;
                for (int i = best_starting_index; i < best_starting_index + this.subquery_space; i++)
                {
                    temp_sum += subquery_vals[i];
                }

                double noise_by_suppresion = 0.0;

                for(int j = 0; j < best_starting_index;j++)
                {
                    noise_by_suppresion += subquery_vals[j];
                }

                this.new_sum = temp_sum;
                this.new_value_count = this.subquery_space;

                double best_inference_error = Math.Abs(((this.sum_result / (double)this.count_non_empty_values) - (this.new_sum / (double)this.subquery_space)) / (this.sum_result / (double)this.count_non_empty_values));

                this.sum_accuracy_error = Math.Abs(this.new_sum - this.sum_result) / this.sum_result;
                this.avg_inference_error = best_inference_error;


                double noise_added = 0.0;
                double max_noise = noise_by_suppresion * 1.2;

                int index_val_to_pertrub = subquery_vals.Length - 1;
                while(index_val_to_pertrub >= 0 && noise_added <= max_noise && this.avg_inference_error < 0.5)
                {
                    double noise_to_add = subquery_vals[index_val_to_pertrub] * 0.27;
                    if(noise_to_add+noise_added > max_noise)
                    {
                        noise_to_add = max_noise - noise_added;
                    }

                    this.new_sum += noise_to_add;
                    this.avg_inference_error = Math.Abs(((this.sum_result / (double)this.count_non_empty_values) - (this.new_sum / (double)this.subquery_space)) / (this.sum_result / (double)this.count_non_empty_values));

                    noise_added += noise_to_add;
                    index_val_to_pertrub--;
                }
            }

            return cube;
        }

        public Array sample_subquery_region_accuracy(Array cube)
        {

            this.new_value_count = 0;
            this.new_sum = 0.0;
            if (this.count_non_empty_values <= 1)
                return cube;
            
            this.avg_inference_error = 0;
            this.sum_accuracy_error = 1;

            if (subquery_space > 0)
            {
                this.subquery_vals = new double[this.non_empty_values_mdx_intervals.Count];

                // Getting the values and sorting them
                //
                int index = 0;
                foreach (ArrayList intervals in this.non_empty_values_mdx_intervals)
                {
                    double val = MDXQueryExecuter.get_value_from_cube(cube_name, intervals);

                    subquery_vals[index] = val;
                    index++;
                }
                Array.Sort(subquery_vals);

                //Selecting the a subgroup that will garanties the best privacy
                best_starting_index = subquery_vals.Length - this.subquery_space;

                double temp_sum = 0.0;
                for (int i = best_starting_index; i < best_starting_index + this.subquery_space; i++)
                {
                    temp_sum += subquery_vals[i];
                }


                this.new_sum = temp_sum;
                this.new_value_count = this.subquery_space;

                double best_inference_error = Math.Abs(((this.sum_result / (double)this.count_non_empty_values) - (this.new_sum / (double)this.subquery_space))/ (this.sum_result / (double)this.count_non_empty_values));

                this.sum_accuracy_error = Math.Abs(this.new_sum - this.sum_result) / this.sum_result;
                this.avg_inference_error = best_inference_error;

                // Our privacy metric
                if (this.new_value_count > 0)
                {
                    double new_avg = this.new_sum / (double)this.new_value_count;
                    for (int i = best_starting_index; i < best_starting_index + this.subquery_space; i++)
                    {
                        double val = subquery_vals[i];

                        this.new_inferece_error += Math.Abs(val - new_avg) / Math.Max(val, 1);
                    }
                    this.new_inferece_error = this.new_inferece_error / (double)this.count_non_empty_values;
                    //this.new_inferece_error = this.new_inferece_error / new_avg;

                }

                //We need a another for loop to populate the cube with the selected values
                //Also in the previous loop, when we sorted the values we should also sort the intefavls
                // so we van recreate the the cube with multi dimensional array

            }
            return cube;
        }

        private void compute_query_stats()
        {
            this.original_inference_error = 0.0;
            if (count_non_empty_values <= 1)
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
                    double val = MDXQueryExecuter.get_value_from_cube(cube_name, intervals);

                    double diff = val - mean;
                    diff_pow_2 += Math.Pow(diff, 2);
                    diff_pow_3 += Math.Pow(diff, 3);
                    //For our new privacy metric
                    this.original_inference_error += Math.Abs(diff)/Math.Max(val,1);
                }

                // For our privacy mesure
                this.original_inference_error = this.original_inference_error/(double)this.count_non_empty_values;
                //this.original_inference_error = this.original_inference_error / mean;

                double teta_1 = Math.Pow(diff_pow_3, 2) / Math.Pow(diff_pow_2, 3);
                if (diff_pow_2 == 0)
                    teta_1 = 0;

                this.alpha_r = (teta_1 / ro_y) - 2.6;

                if ((teta_1 / ro_y) > 2.6)
                    this.l_r = 1;
                else
                    this.l_r = 0;

                this.m_r = diff_pow_2 + (Math.Abs(teta_1) / this.count_non_empty_values);
                if (this.m_r == double.NaN)
                    Console.WriteLine("");
            }
        }

    }
}

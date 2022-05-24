using System;
using System.Collections;
using System.Text;

namespace PUV_Cube
{
    class SCube
    {
        public string cube_name = "";
        public Array cube;

        public QueryWorkLoadGenerator qwg;
        public QueryWorkloadManager qwm;

        public double cuboid_weigth = 0.0;

       

        public ArrayList children;

        public double M_c
        {
            get => qwm.global_m_r;
        }
        public double Alpha_c
        {
            get => qwm.global_alpha_r;
        }
        public double L_c
        {
            get => qwm.global_l_r;
        }
        public int Count_vals
        {
            get => qwm.global_count;
        }
        public double Sum_vals
        {
            get => qwm.global_sum;
        }

        public double Accuracy_error
        {
            get => qwm.queries_error;
        }
        public double Inference_error
        {
            get => qwm.inference_error;
        }
        public double Inference_2_error
        {
            get => qwm.user_inference_error;
        }
        //For our new privacy metric
        public double Original_inference_error
        {
            get => qwm.original_inference_error;
        }
        public double New_inferece_error
        {
            get => qwm.new_inference_error;
        }

        public SCube(string name)
        {
            cube_name = name;

            qwg = new QueryWorkLoadGenerator(cube_name);
            //cube = Array.CreateInstance(typeof(double), qwg.dims_length);

            children = new ArrayList();
        }

        public void create_query_workload(int[] starting_indexs, int[] max_dim_travles, int[] step_changing_starting, int[] grid_deltas)
        {
            ArrayList qwl = qwg.create_qwl_in_region(starting_indexs, max_dim_travles, step_changing_starting, grid_deltas);
            qwm = new QueryWorkloadManager(this.cube_name, qwl);

            foreach(SQuery query in qwl)
            {
                qwm.global_count += query.count_non_empty_values;
            }
            //qwm.compute_global_stats();
        }
       
        public void devide_space_lp_on_qwl(int allocation,double ratio)
        {
            qwm.compute_individual_storage_space_lp(allocation,ratio);
        }
        public void devide_space_allocation_on_qwl(int allocation)
        {
            qwm.compute_individual_storage_space(allocation);
        }
        public void create_view_cube(view_creation_algo type)
        {
            foreach (SQuery query in qwm.qwl)
            {
                this.cube = query.get_samples(this.cube,type);
            }
            qwm.print_view_qwl_stats();
        }
        public void refine_view_cube(int allowed_swap_size, double query_error_threshold, double inference_error_threshold)
        {
            while (!qwm.qwl_satisfy_thresholds(query_error_threshold, inference_error_threshold) && allowed_swap_size > 0)
            {
                qwm.sort_queries(query_error_threshold, inference_error_threshold);
                int needed_swap = qwm.get_needed_swap_size(query_error_threshold, inference_error_threshold);

                if (needed_swap <= 0)
                {
                    Console.WriteLine("Best query can't give up on samples ");
                    return;
                }
                else
                {
                    if (needed_swap > allowed_swap_size)
                        needed_swap = allowed_swap_size;

                    cube = qwm.refine_qwl(cube, needed_swap);

                    allowed_swap_size -= needed_swap;
                }

            }
            qwm.print_view_qwl_stats();
        }
        

        public void save_cube()
        {
            Helper.save_array_in_file(cube);
        }
    }
}

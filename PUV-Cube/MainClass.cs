using System;

using System.Threading.Tasks;
using System.Threading;
using System.Collections;

namespace PUV_Cube
{

    public enum view_creation_algo
    {
        Mx_privacy = 0,
        Mx_utility = 1,
        Perturbation =2,

    }

    class MainClass
    {

        /// <summary>
        /// Part one of the expirements where we compare with existing work
        /// we used our LP we both view creation algos (and the same for their allocation algorithm)
        /// </summary>
        
        public static void testing_cube_tpcds_PUV_max_utility()
        {
            int expirement_run_count = 1;

            string[] trace_rows = new string[2000];
            trace_rows[0] = "id;selectivity;space;Err_1;Inf_1;New_inf";


            for (int selectivity_factor = ; selectivity_factor < 10; selectivity_factor++)
            {

                for (double space_ratio = 0.1; space_ratio < 1.0; space_ratio += 0.1)
                {
                    Console.WriteLine("Space allocation : " + space_ratio + " ------------------ Selectivity : " + selectivity_factor);

                    int[] starting_indexs = { 0, 0, 0, 0, 0 };
                    int[] max_dim_travles = { 2, 150, 1800, 700 * selectivity_factor, 189 };

                    int[] step_changing_starting = { 2, 30, 300, 700 * selectivity_factor, 189 };
                    int[] grid_deltas = { 2, 30, 100, 700 * selectivity_factor, 63 };

                    SCube data_cube = new SCube("TPC-DS-lev1");
                    data_cube.create_query_workload(starting_indexs, max_dim_travles, step_changing_starting, grid_deltas);

                    int space = (int)(space_ratio * data_cube.Count_vals);
                    data_cube.devide_space_lp_on_qwl(space, space_ratio);
                    data_cube.create_view_cube(view_creation_algo.Mx_utility);

                    

                    string result_trace = expirement_run_count + ";" + selectivity_factor + ";" + space_ratio + ";";
                    result_trace += data_cube.Accuracy_error + ";" + data_cube.Inference_error + ";";
                    result_trace += data_cube.New_inferece_error + ";";
                    trace_rows[expirement_run_count] = result_trace;
                    Helper.print_trace(trace_rows);
                    expirement_run_count++;
                }

            }


        }
        public static void testing_cube_tpcds_related_max_utility()
        {
            int expirement_run_count = 1;

            string[] trace_rows = new string[2000];
            trace_rows[0] = "id;selectivity;space;Err_1;Inf_1;New_inf";


            for (int selectivity_factor = 1; selectivity_factor < 10; selectivity_factor++)
            {

                for (double space_ratio = 0.1; space_ratio < 1.0; space_ratio += 0.1)
                {
                    Console.WriteLine("Space allocation : " + space_ratio + " ------------------ Selectivity : " + selectivity_factor);

                    int[] starting_indexs = { 0, 0, 0, 0, 0 };
                    int[] max_dim_travles = { 2, 150, 1800, 700 * selectivity_factor, 189 };

                    int[] step_changing_starting = { 2, 30, 300, 700 * selectivity_factor, 189 };
                    int[] grid_deltas = { 2, 30, 100, 700 * selectivity_factor, 63 };

                    SCube data_cube = new SCube("TPC-DS-lev1");
                    data_cube.create_query_workload(starting_indexs, max_dim_travles, step_changing_starting, grid_deltas);

                    int space = (int)(space_ratio * data_cube.Count_vals);
                    data_cube.devide_space_allocation_on_qwl(space);
                    data_cube.create_view_cube(view_creation_algo.Mx_utility);

                    int alloweed_swap = (int)(space * 0.1);
                    data_cube.refine_view_cube(alloweed_swap, 0.2, 0.5);

                    string result_trace = expirement_run_count + ";" + selectivity_factor + ";" + space_ratio + ";";
                    result_trace += data_cube.Accuracy_error + ";" + data_cube.Inference_error + ";";
                    result_trace += data_cube.New_inferece_error + ";";
                    trace_rows[expirement_run_count] = result_trace;
                    Helper.print_trace(trace_rows);
                    expirement_run_count++;
                }

            }

        }

        public static void testing_cube_tpcds_PUV_max_privacy()
        {
            int expirement_run_count = 1;

            string[] trace_rows = new string[2000];
            trace_rows[0] = "id;selectivity;space;Err_1;Inf_1;New_inf";


            for (int selectivity_factor = 1; selectivity_factor < 10; selectivity_factor++)
            {

                for (double space_ratio = 0.1; space_ratio < 1.0; space_ratio += 0.1)
                {
                    Console.WriteLine("Space allocation : " + space_ratio + " ------------------ Selectivity : " + selectivity_factor);

                    int[] starting_indexs = { 0, 0, 0, 0, 0 };
                    int[] max_dim_travles = { 2, 150, 1800, 700 * selectivity_factor, 189 };

                    int[] step_changing_starting = { 2, 30, 300, 700 * selectivity_factor, 189 };
                    int[] grid_deltas = { 2, 30, 100, 700 * selectivity_factor, 63 };

                    SCube data_cube = new SCube("TPC-DS-lev1");
                    data_cube.create_query_workload(starting_indexs, max_dim_travles, step_changing_starting, grid_deltas);

                    int space = (int)(space_ratio * data_cube.Count_vals);
                    data_cube.devide_space_lp_on_qwl(space, space_ratio);
                    data_cube.create_view_cube(view_creation_algo.Mx_privacy);

                    int alloweed_swap = (int)(space * 0.1);
                    //data_cube.refine_synopsis_cube(alloweed_swap, 0.2, 0.5);

                    string result_trace = expirement_run_count + ";" + selectivity_factor + ";" + space_ratio + ";";
                    result_trace += data_cube.Accuracy_error + ";" + data_cube.Inference_error + ";";
                    result_trace += data_cube.New_inferece_error + ";";
                    trace_rows[expirement_run_count] = result_trace;
                    Helper.print_trace(trace_rows);
                    expirement_run_count++;
                }

            }

        }
        public static void testing_cube_tpcds_related_max_privacy()
        {
            int expirement_run_count = 1;

            string[] trace_rows = new string[2000];
            trace_rows[0] = "id;selectivity;space;Err_1;Inf_1;New_inf";


            for (int selectivity_factor = 1; selectivity_factor < 10; selectivity_factor++)
            {

                for (double space_ratio = 0.1; space_ratio < 1.0; space_ratio += 0.1)
                {
                    Console.WriteLine("Space allocation : " + space_ratio + " ------------------ Selectivity : " + selectivity_factor);

                    int[] starting_indexs = { 0, 0, 0, 0, 0 };
                    int[] max_dim_travles = { 2, 150, 1800, 700 * selectivity_factor, 189 };

                    int[] step_changing_starting = { 2, 30, 300, 700 * selectivity_factor, 189 };
                    int[] grid_deltas = { 2, 30, 100, 700 * selectivity_factor, 63 };

                    SCube data_cube = new SCube("TPC-DS-lev1");
                    data_cube.create_query_workload(starting_indexs, max_dim_travles, step_changing_starting, grid_deltas);

                    int space = (int)(space_ratio * data_cube.Count_vals);
                    data_cube.devide_space_allocation_on_qwl(space);
                    data_cube.create_view_cube(view_creation_algo.Mx_privacy);

                    int alloweed_swap = (int)(space * 0.1);
                    data_cube.refine_view_cube(alloweed_swap, 0.2, 0.5);

                    string result_trace = expirement_run_count + ";" + selectivity_factor + ";" + space_ratio + ";";
                    result_trace += data_cube.Accuracy_error + ";" + data_cube.Inference_error + ";";
                    result_trace += data_cube.New_inferece_error + ";";
                    trace_rows[expirement_run_count] = result_trace;
                    Helper.print_trace(trace_rows);
                    expirement_run_count++;
                }

            }

        }

        /// <summary>
        /// Part two of the expiremnts where we compared both our creation view algorithms
        /// using both databases
        /// </summary>
        public static void testing_cube_tpcds_PUV_perturbation()
        {
            int expirement_run_count = 1;

            string[] trace_rows = new string[2000];
            trace_rows[0] = "id;selectivity;space;Err_1;Inf_1;New_inf";


            for (int selectivity_factor = 1; selectivity_factor < 10; selectivity_factor++)
            {

                for (double space_ratio = 0.1; space_ratio < 1.0; space_ratio += 0.1)
                {
                    Console.WriteLine("Space allocation : " + space_ratio + " ------------------ Selectivity : " + selectivity_factor);

                    int[] starting_indexs = { 0, 0, 0, 0, 0 };
                    int[] max_dim_travles = { 2, 150, 1800, 700 * selectivity_factor, 189 };

                    int[] step_changing_starting = { 2, 30, 300, 700 * selectivity_factor, 189 };
                    int[] grid_deltas = { 2, 30, 100, 700 * selectivity_factor, 63 };

                    SCube data_cube = new SCube("TPC-DS-lev1");
                    data_cube.create_query_workload(starting_indexs, max_dim_travles, step_changing_starting, grid_deltas);

                    int space = (int)(space_ratio * data_cube.Count_vals);
                    data_cube.devide_space_lp_on_qwl(space, space_ratio);
                    data_cube.create_view_cube(view_creation_algo.Perturbation);

                    int alloweed_swap = (int)(space * 0.1);
                    //data_cube.refine_synopsis_cube(alloweed_swap, 0.2, 0.5);

                    string result_trace = expirement_run_count + ";" + selectivity_factor + ";" + space_ratio + ";";
                    result_trace += data_cube.Accuracy_error + ";" + data_cube.Inference_error + ";";
                    result_trace += data_cube.New_inferece_error + ";";
                    trace_rows[expirement_run_count] = result_trace;
                    Helper.print_trace(trace_rows);
                    expirement_run_count++;
                }

            }
        }
        public static void testing_cube_aw_lp_max_privacy()
        {
            int expirement_run_count = 1;

            string[] trace_rows = new string[2000];
            trace_rows[0] = "id;selectivity;space;Err_1;Inf_1";


            for (int selectivity_factor = 4; selectivity_factor < 5; selectivity_factor++)
            {

                for (double space_ratio = 0.1; space_ratio < 1.0; space_ratio += 0.1)
                {
                    Console.WriteLine("Space allocation : " + space_ratio + " ------------------ Selectivity : " + selectivity_factor);

                    int[] starting_indexs = { 0, 0, 0 };
                    int[] max_dim_travles = { 2500, 500, 150 };

                    int[] step_changing_starting = { 1250 * selectivity_factor, 250, 150 };
                    int[] grid_deltas = { 625 * selectivity_factor, 250, 150 };

                    SCube lev1_cuboid = new SCube("AW12-3");
                    lev1_cuboid.create_query_workload(starting_indexs, max_dim_travles, step_changing_starting, grid_deltas);


                    int space = (int)(space_ratio * lev1_cuboid.Count_vals);
                    lev1_cuboid.devide_space_lp_on_qwl(space,space_ratio);

                    lev1_cuboid.create_view_cube(view_creation_algo.Mx_privacy);

                    string result_trace = expirement_run_count + ";" + selectivity_factor + ";" + space_ratio + ";";
                    result_trace += lev1_cuboid.Accuracy_error + ";" + lev1_cuboid.Inference_error + ";";

                    trace_rows[expirement_run_count] = result_trace;
                    Helper.print_trace(trace_rows);
                    expirement_run_count++;
                }

            }

        }
        public static void testing_cube_aw_lp_perturbation()
        {
            int expirement_run_count = 1;

            string[] trace_rows = new string[2000];
            trace_rows[0] = "id;selectivity;space;Err_1;Inf_1";


            for (int selectivity_factor = 4; selectivity_factor < 5; selectivity_factor++)
            {

                for (double space_ratio = 0.1; space_ratio < 1.0; space_ratio += 0.1)
                {
                    Console.WriteLine("Space allocation : " + space_ratio + " ------------------ Selectivity : " + selectivity_factor);

                    int[] starting_indexs = { 0, 0, 0 };
                    int[] max_dim_travles = { 2500, 500, 150 };

                    int[] step_changing_starting = { 1250 * selectivity_factor, 250, 150 };
                    int[] grid_deltas = { 625 * selectivity_factor, 250, 150 };

                    SCube lev1_cuboid = new SCube("AW12-3");
                    lev1_cuboid.create_query_workload(starting_indexs, max_dim_travles, step_changing_starting, grid_deltas);


                    int space = (int)(space_ratio * lev1_cuboid.Count_vals);
                    lev1_cuboid.devide_space_lp_on_qwl(space, space_ratio);

                    lev1_cuboid.create_view_cube(view_creation_algo.Perturbation);

                    string result_trace = expirement_run_count + ";" + selectivity_factor + ";" + space_ratio + ";";
                    result_trace += lev1_cuboid.Accuracy_error + ";" + lev1_cuboid.Inference_error + ";";

                    trace_rows[expirement_run_count] = result_trace;
                    Helper.print_trace(trace_rows);
                    expirement_run_count++;
                }

            }



        }

        static void Main(string[] args)
        {

            // Call any expirement here 

        }
    }
}

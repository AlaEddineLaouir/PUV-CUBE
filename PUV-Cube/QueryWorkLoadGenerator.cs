using System;
using System.Linq;
using System.Collections;

namespace PUV_Cube
{
    class QueryWorkLoadGenerator
    {
        public int[] dims_length;
        //private ArrayList qwl;
        private string cube_name;

        public ArrayList Qwl => null;

        public QueryWorkLoadGenerator(string cube_name)
        {
            this.cube_name = cube_name;
            this.dims_length =  MDXQueryExecuter.get_cube_dims_members(cube_name);
        }

        public ArrayList create_qwl_in_region(int[] starting_indexs, int[] max_dim_travles, int[] step_changing_starting, int[] grid_deltas)
        {
            ArrayList qwl = new ArrayList();

            bool done = false;
            int[] temp_grid_start_indexs = Helper.copy_array_ints(starting_indexs);
            while (!done)
            {
                SQuery query = new SQuery(cube_name, Helper.copy_array_ints(temp_grid_start_indexs), Helper.copy_array_ints(step_changing_starting));
                
                    
                    query.create_accuracy_grid(grid_deltas);
                    query.init_stats();
                if (query.count_non_empty_values > 1)
                {
                    qwl.Add(query);
                }

                bool pass = true;
                for (int i = temp_grid_start_indexs.Length - 1; (i >= 0 && pass); i--)
                {
                    if (temp_grid_start_indexs[i] + (2* step_changing_starting[i]) +1 > max_dim_travles[i])
                    {
                        temp_grid_start_indexs[i] = starting_indexs[i];
                    }
                    else
                    {
                        temp_grid_start_indexs[i] += step_changing_starting[i] + 1;
                        pass = false;
                    }
                }
                done = pass;
            }

            return qwl;
        }

    }
}

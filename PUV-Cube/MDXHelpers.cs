using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;

namespace PUV_Cube
{
    interface MDXHelpers
    {
        //the name must be unique
        public string cube_name { get; }
        public bool loaded { get; set; }
        public Dictionary<int, int>[] map_index_value_dims { get; set; }

        public  ArrayList[] dims { get; set; }
        public int[] dims_length { get; set; }
        public int dimensions { get; }

        public string connection_string();
        public string[] dimensions_vals();
        public string get_value_from_cube(ArrayList intervals);
        public string get_count_values_from_cube(ArrayList intervals);
        public string get_non_empty_intervalls_query(ArrayList intervals);

        public int[]  create_dims_indexs(ArrayList[] input_dims)
        {
            dims = input_dims;
            map_index_value_dims = new Dictionary<int, int>[dims.Length];
            dims_length = new int[dims.Length];
            for (int inde = 0; inde < dims.Length; inde++)
            {
                map_index_value_dims[inde] = get_index_value_mapping(dims[inde]);
                dims_length[inde] = map_index_value_dims[inde].Count;
            }

            save_cube_metadata();

            return dims_length;
        }
        private Dictionary<int, int> get_index_value_mapping(ArrayList dim_original)
        {
            // This part is add to sort elements, and query can be easly defiend with values sorted
            ArrayList dim = new ArrayList();
            dim = (ArrayList)dim_original.Clone();
            //dim.Sort();

            int maxLoop = dim.Count;
            Dictionary<int, int> map_value_index_dim = new Dictionary<int, int>();

            int index_gird_array_dim = 0;

            for (int i = 0; i < maxLoop -1;)
            {
                bool add = false;
                int current_val = Convert.ToInt32(dim[i]);

                if (!map_value_index_dim.ContainsValue(current_val))
                {
                    //Compute next index
                    for (int j = i + 1; j < maxLoop;)
                    {
                        if (Convert.ToInt32(dim[j]) == current_val)
                        {
                            j++;
                            add = (j == maxLoop);
                        }
                        else add = true;

                        if (add)
                        {
                            map_value_index_dim.Add(index_gird_array_dim, current_val);
                            index_gird_array_dim++;
                            i = j;
                            break;
                        }
                    }
                }
                else i++;

            }
            return map_value_index_dim;
        }

        public ArrayList mdx_query_ranges(ArrayList intervals)
        {
            ArrayList mdx_ranges = new ArrayList();

            for (int i = 0; i < intervals.Count; i++)
                mdx_ranges.Add(index_to_mdx_range((int[])intervals[i], i));

            return mdx_ranges;
        }
        private int[] index_to_mdx_range(int[] indexs, int dimension_index)
        {
            int[] mdx_range = new int[2];

            map_index_value_dims[dimension_index].TryGetValue(indexs[0], out mdx_range[0]);
            map_index_value_dims[dimension_index].TryGetValue(indexs[1], out mdx_range[1]);

            return mdx_range;
        }

        public int[] query_cell_indexs_from_mdx(ArrayList intervals)
        {
            int[] indexs = new int[intervals.Count];

            for (int i = 0; i < indexs.Length - 1; i++)
                indexs[i] = map_index_value_dims[i].FirstOrDefault(x => x.Value == ((int[])intervals[i])[0]).Key;

            return indexs;
        }
    
        private  void save_cube_metadata()
        {
            string path = "./Cubes/Cube" + cube_name + "/metadata/";

            string filename = "dims_vals";
            Helper.save_object_in_file(this.dims, path , filename);

            filename = "map_index_value_dims";
            Helper.save_object_in_file(this.map_index_value_dims, path , filename);

            filename = "dims_lengths";
            Helper.save_object_in_file(this.dims_length, path, filename);
        }
        public void try_loading_cube_metadata()
        {
            string path = "./Cubes/Cube" + cube_name + "/metadata/";
           
            try
            {
                string filename = "dims_vals";
                this.dims = (ArrayList[])Helper.load_object_from_file(path, filename);
                filename = "map_index_value_dims";
                this.map_index_value_dims = (Dictionary<int, int>[])Helper.load_object_from_file(path, filename);
                filename = "dims_lengths";
                this.dims_length = (int[])Helper.load_object_from_file(path, filename);


                loaded = true;
            }catch(Exception e)
            {
                Console.WriteLine("Failed tp load previose metadata !!!");
                loaded = false;
            }

        }
            
    }
}

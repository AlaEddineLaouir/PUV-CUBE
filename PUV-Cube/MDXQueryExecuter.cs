using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PUV_Cube
{
    class MDXQueryExecuter
    {
        private static AdomdConnection cnx;
        private static string current_connected_cube = "";
        private static Dictionary<string, MDXHelpers> query_helpers;

        static MDXQueryExecuter()
        {
            var helpers = from t in Assembly.GetExecutingAssembly().GetTypes()
                            where t.GetInterfaces().Contains(typeof(MDXHelpers))
                                     && t.GetConstructor(Type.EmptyTypes) != null
                            select Activator.CreateInstance(t) as MDXHelpers;


            query_helpers = new Dictionary<string, MDXHelpers>();

            foreach (var helper in helpers)
            {
                query_helpers.Add(helper.cube_name, helper);
            }

        }

        public static int[] get_cube_dims_members(string cube_name)
        {
            MDXHelpers helper = query_helpers.GetValueOrDefault(cube_name);

            if(cube_name != current_connected_cube)
            {
                connect_to_cube(helper);
            }

            if(!helper.loaded)
            {
                //Init
                string[] queries = helper.dimensions_vals();

                ArrayList[] dims = new ArrayList[helper.dimensions];
                for (int i = 0; i < helper.dimensions; i++)
                {
                    dims[i] = new ArrayList();

                    AdomdCommand retrieveCube = new AdomdCommand(queries[i], cnx);

                    AdomdDataReader dataRecords = retrieveCube.ExecuteReader();

                    while (dataRecords.Read())
                    {

                        // Idea : Add each dim type in helper
                        int val = 0;
                        try
                        {
                            val = Convert.ToInt32(dataRecords[0]);
                        }
                        catch (Exception e) { }
                        dims[i].Add(val);

                    }
                    dataRecords.Close();
                }
                helper.create_dims_indexs(dims);
            }

            return helper.dims_length;
        }

        internal static ArrayList mdx_query_ranges(string cube_name, ArrayList intervals)
        {
            MDXHelpers helper = query_helpers.GetValueOrDefault(cube_name);
            return helper.mdx_query_ranges(intervals);
        }

        public static double get_value_from_cube(string cube_name,ArrayList intervals)
        {
            MDXHelpers helper = query_helpers.GetValueOrDefault(cube_name);

            if (cube_name != current_connected_cube)
            {
                connect_to_cube(helper);
            }

            string query = helper.get_value_from_cube(intervals);

            AdomdCommand retrieveCube = new AdomdCommand(query, cnx);

            AdomdDataReader dataRecords = retrieveCube.ExecuteReader();

            double result = 0.0;
            if (dataRecords.Read())
            {
                result = dataRecords.GetDouble(0);
            }
            dataRecords.Close();

            return result;
        }

        public static int get_count_values_cube(string cube_name, ArrayList intervals)
        {
            MDXHelpers helper = query_helpers.GetValueOrDefault(cube_name);

            if (cube_name != current_connected_cube)
            {
                connect_to_cube(helper);
            }

            string query = helper.get_count_values_from_cube(intervals);

            AdomdCommand retrieveCube = new AdomdCommand(query, cnx);

            AdomdDataReader dataRecords = retrieveCube.ExecuteReader();

            int result = 0;
            if (dataRecords.Read())
            {
                try
                {
                    result = dataRecords.GetInt32(0);
                }catch(Exception e) { result = 0; }
            }
            dataRecords.Close();
            
            return result;
        }

        public static ArrayList get_non_empty_intervals_query(string cube_name,ArrayList intervals) 
        {
            MDXHelpers helper = query_helpers.GetValueOrDefault(cube_name);

            if (cube_name != current_connected_cube)
            {
                connect_to_cube(helper);
            }
            ArrayList values_intervales = new ArrayList();

            string query = helper.get_non_empty_intervalls_query(intervals);

            AdomdCommand retrieveCube = new AdomdCommand(query, cnx);

            AdomdDataReader dataRecords = retrieveCube.ExecuteReader();

            while (dataRecords.Read())
            {
                ArrayList value_interval = new ArrayList();
                int dim_index = 0;

                try
                {
                    for (int i = 0; i < helper.dimensions; i++, dim_index += 2)
                    {
                        int[] temp_dim = { Convert.ToInt32(dataRecords[dim_index]), Convert.ToInt32(dataRecords[dim_index]) };
                        value_interval.Add(temp_dim);
                    }

                    values_intervales.Add(value_interval);
                }catch(Exception e) { }
            }
            dataRecords.Close();

            return values_intervales;

        }

        private static void connect_to_cube(MDXHelpers helper)
        {
            if (cnx != null)
            {
                try
                {
                    cnx.Close();

                }catch(Exception e)
                {
                    cnx = null;
                }
            }
            cnx = new AdomdConnection(helper.connection_string());
            try
            {
                cnx.Open();
            }
            catch(Exception e )
            {
                 Console.WriteLine("");
            }
            current_connected_cube = helper.cube_name;
        }

        internal static int[] query_cell_indexs_from_mdx(string cube_name, ArrayList intervals)
        {
            MDXHelpers helper = query_helpers.GetValueOrDefault(cube_name);
            return helper.query_cell_indexs_from_mdx(intervals);
        }
    }
}

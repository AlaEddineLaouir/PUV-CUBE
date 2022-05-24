using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

using System.Runtime.Serialization.Formatters.Binary;

namespace PUV_Cube
{
    class Helper
    {
        public static int[] copy_array_ints(int[] array_source)
        {
            int[] copy = new int[array_source.Length];
            for (int i = 0; i < copy.Length; i++)
            {
                copy[i] = array_source[i];
            }
            return copy;
        }
        public static bool is_prime(int n)
        {
            if (n == 2) return true;
            if (n % 2 == 0) return false;

            for (int x = 3; x * x <= n; x += 2)
                if (n % x == 0)
                    return false;

            return true;
        }

        public static int[] get_divisors(int n)
        {
            List<int> divisors = new List<int>();

            if (n < 2)
            {
                return null;
            }
            else if (is_prime(n))
            {
                int[] res = { 1, n };
                return res;
            }
            else
            {
                for (int i = 1; i <= n; i++)
                    if (n % i == 0)
                        divisors.Add(i);
            }

            return divisors.ToArray();
        }
        public static int get_gcd(int[] dividors_1, int[] dividors_2)
        {

            for (int i = dividors_1.Length - 1; i > 0; i--)
            {
                if (list_contains_int(dividors_2, dividors_1[i]))
                    return dividors_1[i];
            }
            return 1;
        }
        public static bool list_contains_int(int[] list, int search_val)
        {
            foreach (int val in list)
                if (val == search_val)
                    return true;
            return false;
        }

        public static async Task print_trace(string[] rows)
        {
            try
            {
                await File.WriteAllLinesAsync("Trace_execution" + System.DateTime.Now.ToFileTimeUtc() + "X.csv", rows);
            }
            catch (Exception e)
            {
                Console.WriteLine("Trace not working >>>" + e.Message);

            }
            finally
            {
                //Console.WriteLine("done trace");
            }
        }
        public static void save_array_in_file(Array array)
        {
            BinaryFormatter bf = new BinaryFormatter();

            // writing
            using (FileStream fs = new FileStream("Array_" + System.DateTime.Now.ToFileTimeUtc() + "X.dat", FileMode.Create))
                bf.Serialize(fs, array);

            return;
            // reading
            using (FileStream fs = new FileStream("mySave.dat", FileMode.Open))
                array = (int[,])bf.Deserialize(fs);
        }
        public static void save_object_in_file(Object o, string file_directory,string filename)
        {
            try
            {
                if(!Directory.Exists(file_directory))
                {
                    Directory.CreateDirectory(file_directory);
                }
                BinaryFormatter bf = new BinaryFormatter();

                // writing
                using (FileStream fs = new FileStream(file_directory + filename + ".dat", FileMode.Create))
                    bf.Serialize(fs, o);
            }catch(Exception e)
            {
                Console.WriteLine("Couldn't save the data !!!!! ");
            }
        }
        public static Object load_object_from_file(string file_directory,string filename)
        {
            BinaryFormatter bf = new BinaryFormatter();
            // reading
            object o = null;
            using (FileStream fs = new FileStream(file_directory+filename+".dat", FileMode.Open))
                o = (object)bf.Deserialize(fs);
            return o;
        }

        public static string array_to_string(int[] array)
        {
            string res = "";
            foreach (int i in array)
                res += i + ",";
            return res;
        }
        public static bool arrays_equal(int[] array_1, int[] array_2)
        {
            //same length

            for (int i = 0; i < array_1.Length; i++)
            {
                if (array_1[i] != array_2[i])
                    return false;
            }
            return true;
        }
        public static ArrayList create_intervals(int[] starting_indexs, int[] dim_travels)
        {
            ArrayList intervals = new ArrayList();
            for (int i = 0; i < starting_indexs.Length; i++)
            {
                int[] temp = { starting_indexs[i], starting_indexs[i] + dim_travels[i] };
                intervals.Add(temp);
            }
            return intervals;
        }
        public static int[] create_starting_indexs(ArrayList intervals)
        {
            int[] starting_indexs = new int[intervals.Count];
            for (int i = 0; i < starting_indexs.Length; i++)
            {
                starting_indexs[i] = ((int[])intervals[i])[0];
            }
            return starting_indexs;
        }
        public static ArrayList concat_arraylists(ArrayList list1, ArrayList list2)
        {
            ArrayList the_new_array_list = new ArrayList();
            // I could do bettre, i know
            foreach (Object o in list1)
                the_new_array_list.Add(o);
            foreach (Object o in list2)
                the_new_array_list.Add(o);

            return the_new_array_list;
        }
    }
}

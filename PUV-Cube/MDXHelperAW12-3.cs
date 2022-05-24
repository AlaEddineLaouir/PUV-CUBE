using System.Collections;
using System.Collections.Generic;

namespace PUV_Cube
{
    class MDXHelperAW12_3 : MDXHelpers
    {
        public string cube_name => "AW12-3";
        public bool _loaded = false;
        public int dimensions => 3;

        public Dictionary<int, int>[] _map_index_value_dims;
        public int[] _dims_length;
        public ArrayList[] _dims;
        public Dictionary<int, int>[] map_index_value_dims
        {
            get => _map_index_value_dims;
            set => _map_index_value_dims = value;
        }
        public int[] dims_length
        {
            get => _dims_length;
            set => _dims_length = value;
        }
        public ArrayList[] dims
        {
            get => _dims;
            set => _dims = value;
        }
        public bool loaded { get => _loaded; set => _loaded = value; }
        public MDXHelperAW12_3()
        {
            // Can't have a constructor in the interface
            ((MDXHelpers)this).try_loading_cube_metadata();
        }
        public string connection_string()
        {
            string connection_string = "Data Source=localhost;Initial catalog=AdvetureWorkData;Cube=" + cube_name + ";persist security info=True; Integrated Security = SSPI;";
            return connection_string;
        }

        public string[] dimensions_vals()
        {
            string query_dim_3 = "  SELECT NON EMPTY { [Measures].[Sales Amount] } ON COLUMNS, NON EMPTY { (";
            query_dim_3 += "[Dim Customer].[Customer Key].[Customer Key].ALLMEMBERS";
            query_dim_3 += " ) } DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS FROM ["+cube_name+"] CELL PROPERTIES VALUE, BACK_COLOR, FORE_COLOR, FORMATTED_VALUE, FORMAT_STRING, FONT_NAME, FONT_SIZE, FONT_FLAGS";

            string query_dim_4 = "  SELECT NON EMPTY { [Measures].[Sales Amount] } ON COLUMNS, NON EMPTY { (";
            query_dim_4 += "[Due Date].[Date Key].[Date Key].ALLMEMBERS";
            query_dim_4 += " ) } DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS FROM [" + cube_name + "] CELL PROPERTIES VALUE, BACK_COLOR, FORE_COLOR, FORMATTED_VALUE, FORMAT_STRING, FONT_NAME, FONT_SIZE, FONT_FLAGS";

            string query_dim_5 = "  SELECT NON EMPTY { [Measures].[Sales Amount] } ON COLUMNS, NON EMPTY { (";
            query_dim_5 += "[Dim Product].[Product Key].[Product Key].ALLMEMBERS";
            query_dim_5 += " ) } DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS FROM [" + cube_name + "] CELL PROPERTIES VALUE, BACK_COLOR, FORE_COLOR, FORMATTED_VALUE, FORMAT_STRING, FONT_NAME, FONT_SIZE, FONT_FLAGS";


            string[] dims_queries = { query_dim_3, query_dim_4,query_dim_5 };

            return dims_queries;

        }

        public string get_value_from_cube(ArrayList intervals)
        {
            string query = " SELECT NON EMPTY { [Measures].[Sales Amount] } ON COLUMNS from [" + cube_name + "] where { (";
            query += "[Dim Customer].[Customer Key].[" + (((int[])intervals[0])[0]) + "]:[Dim Customer].[Customer Key].[" + (((int[])intervals[0])[1]) + "],";
            query += "[Due Date].[Date Key].[" + (((int[])intervals[1])[0]) + "]: [Due Date].[Date Key].[" + (((int[])intervals[1])[1]) + "],";
            query += "[Dim Product].[Product Key].[" + (((int[])intervals[2])[0]) + "]: [Dim Product].[Product Key].[" + (((int[])intervals[2])[1]) + "]";

            query += " )}";

            return query;
        }

        public string get_count_values_from_cube(ArrayList intervals)
        {
            string query = "with member [Measures].[rowCount] as count(nonempty((";

            query += "[Dim Customer].[Customer Key].[" + (((int[])intervals[0])[0]) + "]:[Dim Customer].[Customer Key].[" + (((int[])intervals[0])[1]) + "],";
            query += "[Due Date].[Date Key].[" + (((int[])intervals[1])[0]) + "]: [Due Date].[Date Key].[" + (((int[])intervals[1])[1]) + "],";
            query += "[Dim Product].[Product Key].[" + (((int[])intervals[2])[0]) + "]: [Dim Product].[Product Key].[" + (((int[])intervals[2])[1]) + "]";

            query += "),{[Measures].[Sales Amount] } )) SELECT {[Measures].[rowCount]} ON COLUMNS FROM [" + cube_name + "]";

            return query;
        }

        public string get_non_empty_intervalls_query(ArrayList intervals)
        {
            string query = "SELECT NON EMPTY { [Measures].[Sales Amount]  } ON COLUMNS, NON EMPTY {( ";

            query += "[Dim Customer].[Customer Key].[" + (((int[])intervals[0])[0]) + "]:[Dim Customer].[Customer Key].[" + (((int[])intervals[0])[1]) + "],";
            query += "[Due Date].[Date Key].[" + (((int[])intervals[1])[0]) + "]: [Due Date].[Date Key].[" + (((int[])intervals[1])[1]) + "],";
            query += "[Dim Product].[Product Key].[" + (((int[])intervals[2])[0]) + "]: [Dim Product].[Product Key].[" + (((int[])intervals[2])[1]) + "]";

            query += " ) } DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS FROM[" + cube_name + "]";

            return query;
        }
    }
}

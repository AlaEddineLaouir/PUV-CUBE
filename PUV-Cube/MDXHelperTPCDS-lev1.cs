using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PUV_Cube
{
    class MDXHelperTPCDS_lev1 : MDXHelpers
    {
        public string cube_name => "TPC-DS-lev1";
        public bool _loaded = false;
        public int dimensions => 5;

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
        public MDXHelperTPCDS_lev1()
        {
            // Can't have a constructor in the interface
            ((MDXHelpers)this).try_loading_cube_metadata();
        }
        public string connection_string()
        {
            string connection_string = "Data Source=localhost;Initial catalog=TPCDS-DataCubes;Cube=" + cube_name + ";persist security info=True; Integrated Security = SSPI;";
            return connection_string;
        }

        public string[] dimensions_vals()
        {
            string query_dim_2 = "SELECT NON EMPTY { [Measures].[Ss Net Paid] } ON COLUMNS, NON EMPTY { (";
            query_dim_2 += "[Store].[s Store Sk].[s Store Sk].ALLMEMBERS";
            query_dim_2 += ") } DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS FROM [" + cube_name + "] CELL PROPERTIES VALUE, BACK_COLOR, FORE_COLOR, FORMATTED_VALUE, FORMAT_STRING, FONT_NAME, FONT_SIZE, FONT_FLAGS";

            string query_dim_3 = "SELECT NON EMPTY { [Measures].[Ss Net Paid] } ON COLUMNS, NON EMPTY { (";
            query_dim_3 += "[Item].[i Item Sk].[i Item Sk].ALLMEMBERS";
            query_dim_3 += ") } DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS FROM [" + cube_name + "] CELL PROPERTIES VALUE, BACK_COLOR, FORE_COLOR, FORMATTED_VALUE, FORMAT_STRING, FONT_NAME, FONT_SIZE, FONT_FLAGS";

            string query_dim_4 = "SELECT NON EMPTY { [Measures].[Ss Net Paid] } ON COLUMNS, NON EMPTY { (";
            query_dim_4 += "[Date Dim].[d Date Sk].[d Date Sk].ALLMEMBERS";
            query_dim_4 += ") } DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS FROM [" + cube_name + "] CELL PROPERTIES VALUE, BACK_COLOR, FORE_COLOR, FORMATTED_VALUE, FORMAT_STRING, FONT_NAME, FONT_SIZE, FONT_FLAGS";


            string query_dim_5 = "SELECT NON EMPTY { [Measures].[Ss Net Paid] } ON COLUMNS, NON EMPTY { (";
            query_dim_5 += "[Household Demographics].[Hd Demo Sk].[Hd Demo Sk].ALLMEMBERS";
            query_dim_5 += ") } DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS FROM [" + cube_name + "] CELL PROPERTIES VALUE, BACK_COLOR, FORE_COLOR, FORMATTED_VALUE, FORMAT_STRING, FONT_NAME, FONT_SIZE, FONT_FLAGS";

            string query_dim_6 = "SELECT NON EMPTY { [Measures].[Ss Net Paid] } ON COLUMNS, NON EMPTY { (";
            query_dim_6 += "[Promotion].[p Promo Sk].[p Promo Sk].ALLMEMBERS";
            query_dim_6 += ") } DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS FROM [" + cube_name + "] CELL PROPERTIES VALUE, BACK_COLOR, FORE_COLOR, FORMATTED_VALUE, FORMAT_STRING, FONT_NAME, FONT_SIZE, FONT_FLAGS";


            string[] dims_queries = { query_dim_2, query_dim_3, query_dim_4, query_dim_5, query_dim_6 };

            return dims_queries;

        }

        public string get_value_from_cube(ArrayList intervals)
        {
            string query = " SELECT NON EMPTY { [Measures].[Ss Net Paid] } ON COLUMNS from [" + cube_name + "] where { (";
            query += "[Store].[s Store Sk].[" + (((int[])intervals[0])[0]) + "]:[Store].[s Store Sk].[" + (((int[])intervals[0])[1]) + "],";
            query += "[Item].[i Item Sk].[" + (((int[])intervals[1])[0]) + "]:[Item].[i Item Sk].[" + (((int[])intervals[1])[1]) + "],";
            query += "[Date Dim].[d Date Sk].[" + (((int[])intervals[2])[0]) + "]: [Date Dim].[d Date Sk].[" + (((int[])intervals[2])[1]) + "],";
            query += "[Household Demographics].[Hd Demo Sk].[" + (((int[])intervals[3])[0]) + "]:[Household Demographics].[Hd Demo Sk].[" + (((int[])intervals[3])[1]) + "],";
            query += "[Promotion].[p Promo Sk].[" + (((int[])intervals[4])[0]) + "]: [Promotion].[p Promo Sk].[" + (((int[])intervals[4])[1]) + "]";
            query += " )}";

            return query;
        }

        public string get_count_values_from_cube(ArrayList intervals)
        {
            string query = "with member [Measures].[rowCount] as count(nonempty((";

            query += "[Store].[s Store Sk].[" + (((int[])intervals[0])[0]) + "]:[Store].[s Store Sk].[" + (((int[])intervals[0])[1]) + "],";
            query += "[Item].[i Item Sk].[" + (((int[])intervals[1])[0]) + "]:[Item].[i Item Sk].[" + (((int[])intervals[1])[1]) + "],";
            query += "[Date Dim].[d Date Sk].[" + (((int[])intervals[2])[0]) + "]: [Date Dim].[d Date Sk].[" + (((int[])intervals[2])[1]) + "],";
            query += "[Household Demographics].[Hd Demo Sk].[" + (((int[])intervals[3])[0]) + "]:[Household Demographics].[Hd Demo Sk].[" + (((int[])intervals[3])[1]) + "],";
            query += "[Promotion].[p Promo Sk].[" + (((int[])intervals[4])[0]) + "]: [Promotion].[p Promo Sk].[" + (((int[])intervals[4])[1]) + "]";
            query += "),{[Measures].[Ss Net Paid]} )) SELECT {[Measures].[rowCount]} ON COLUMNS FROM [" + cube_name + "]";

            return query;
        }

        public string get_non_empty_intervalls_query(ArrayList intervals)
        {
            string query = "SELECT NON EMPTY { [Measures].[Ss Net Paid] } ON COLUMNS, NON EMPTY {( ";

            query += "[Store].[s Store Sk].[" + (((int[])intervals[0])[0]) + "]:[Store].[s Store Sk].[" + (((int[])intervals[0])[1]) + "],";
            query += "[Item].[i Item Sk].[" + (((int[])intervals[1])[0]) + "]:[Item].[i Item Sk].[" + (((int[])intervals[1])[1]) + "],";
            query += "[Date Dim].[d Date Sk].[" + (((int[])intervals[2])[0]) + "]: [Date Dim].[d Date Sk].[" + (((int[])intervals[2])[1]) + "],";
            query += "[Household Demographics].[Hd Demo Sk].[" + (((int[])intervals[3])[0]) + "]:[Household Demographics].[Hd Demo Sk].[" + (((int[])intervals[3])[1]) + "],";
            query += "[Promotion].[p Promo Sk].[" + (((int[])intervals[4])[0]) + "]: [Promotion].[p Promo Sk].[" + (((int[])intervals[4])[1]) + "]";

            query += " ) } DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS FROM[" + cube_name + "]";

            return query;
        }

    }
}

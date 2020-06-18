using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data;
using Indexer.Model;

namespace Indexer
{
    public class SQL_DB
    {
        //connection code
        static string connectionString = "Data Source=LAPTOP-ID63QH56\\SQLEXPRESS;Initial Catalog=Information Retrival;Integrated Security=True";

        //Get All Url Data (the ID,URL and the content(text)) from DB 
        public static List<URL_Data> GetAllUrlData()
        {
            List<URL_Data> ContentLst = new List<URL_Data>();
            //select all the info and put it in a list data structure
            var dt = GetQueryResult("Select * from Documentss");
            foreach (DataRow r in dt.Rows)
            {

                ContentLst.Add(new URL_Data
                {
                    ID = int.Parse(r["ID"].ToString()),
                    URLS = r["URL"].ToString(),
                    content = r["Content"].ToString()
                });

            }
            return ContentLst;
        }




        //get all the info and put it in a data table data structure
        public static DataTable GetQueryResult(string query)
        {
            DataTable dt = new DataTable();
            SqlDataAdapter adapte = new SqlDataAdapter(query, connectionString);
            adapte.Fill(dt);
            return dt;
        }


        private static List<String> GetAllterms( string cmdStr, SqlConnection connection = null)
        {
            SqlConnection con;
            if (connection == null)
            {
                con = new SqlConnection(connectionString);
                con.Open();
            }
            else
                con = connection;

            SqlCommand cmd = new SqlCommand(string.Format(cmdStr, "@qurey"), con);

            object id = cmd.ExecuteScalar();
            List<String> ints = new List<String>();
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    ints.Add(reader.GetString(0)); // provided that first (0-index) column is int which you are looking for
                }
            }
            con.Close();
            return ints;
        }

        public static List<String> get_terms(SqlConnection connection = null)
        {

            return GetAllterms( @"select Term from Terms");

        }

        //insert_into_DB_invertedindexTable
        //that have all the stemming words and its pos and its freq etc.
        public static void insert_into_DB_invertedindexTable(string term, inverted_index item)
        {

            //SQLQuery
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            string insertStr = @" insert into invertedIndex(Term_id, Term, DocId, Frequency, Positions)values(@Term_id, @Term, @DocId, @Frequency, @Positions)";
            SqlCommand cmd = new SqlCommand(insertStr, con);

            SqlParameter term_id = new SqlParameter("@Term_id", item.Term_id);
            cmd.Parameters.Add(term_id);

            SqlParameter term_name = new SqlParameter("@Term", term);
            cmd.Parameters.Add(term_name);

            SqlParameter doc_id = new SqlParameter("@DocId", item.DocId);
            cmd.Parameters.Add(doc_id);

            SqlParameter freq = new SqlParameter("@Frequency", item.Frequency);
            cmd.Parameters.Add(freq);

            SqlParameter pos = new SqlParameter("@Positions", item.Position);
            cmd.Parameters.Add(pos);

            cmd.ExecuteNonQuery();

            con.Close();
        }

        //insert_into_DB_TermDocTable
        //that have all the words and its positions in the doc 
        public static void insert_into_DB_TermDocTable(string term, inverted_index item)
        {
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            string insertStr = @" insert into Terms_DOCID(Term, DOCID, Frequency, Positions)values(@Term_text, @Doc_id, @Frequency, @Positions)";
            SqlCommand cmd = new SqlCommand(insertStr, con);

            SqlParameter term_name = new SqlParameter("@Term_text", term);
            cmd.Parameters.Add(term_name);

            SqlParameter doc_id = new SqlParameter("@Doc_id", item.DocId);
            cmd.Parameters.Add(doc_id);

            SqlParameter freq = new SqlParameter("@Frequency", item.Frequency);
            cmd.Parameters.Add(freq);

            SqlParameter pos = new SqlParameter("@Positions", item.Position);
            cmd.Parameters.Add(pos);
            cmd.ExecuteNonQuery();

            con.Close();
        }

        //insert_into_DB_BIGRAMTable
        //have the bigrams without any repetitions
        public static List<string> term = new List<string>();
        public static int insert_into_DB_BIGRAMTable(string BI)
        {
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            int id = get_Bigram_id(BI, con);
            
            if (!term.Contains(BI))
            {
               
                string insertStr = @"insert into Bigram_indexx(Bigram)values(@Kgram)";
                term.Add(BI);
                SqlCommand cmd = new SqlCommand(insertStr, con);

                SqlParameter BI_gram = new SqlParameter("@Kgram", BI);
                cmd.Parameters.Add(BI_gram);

                cmd.ExecuteNonQuery();

             
                id = get_Bigram_id(BI,con);
            }
            con.Close();
            return id;

        }



        public static void insert_into_DB_BI_TERM_Table(int BI_ID, int TERM_ID)
        {
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();

           //if ((BI_ID == default(int)&&(TERM_ID == default(int))))
            //{

                string insertStr = @" insert into KgramID_TermID(k_gram_ID,Term_ID)values(@k_gram_ID,@Term_ID)";
                SqlCommand cmd = new SqlCommand(insertStr, con);

                SqlParameter BI_gram_id = new SqlParameter("@k_gram_ID", BI_ID);
                cmd.Parameters.Add(BI_gram_id);

                SqlParameter Term_ID = new SqlParameter("@Term_ID", TERM_ID);
                cmd.Parameters.Add(Term_ID);

                cmd.ExecuteNonQuery();


               
            //}
            con.Close();
            
        }


        //insert_into_DB_termsTable
        //holds all the terms without any repetitions and its IDs
        public static int insert_into_DB_termsTable(string term)
        {
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            int id = get_term_id(term, con);
            if (id == default(int))
            {
                //SQLQuery

                string insertStr = @"insert into Terms(Term) values(@Term)";
                SqlCommand cmd = new SqlCommand(insertStr, con);

                SqlParameter term_name = new SqlParameter("@Term", term);
                cmd.Parameters.Add(term_name);


                cmd.ExecuteNonQuery();
                id = get_term_id(term, con);
            }
            con.Close();
            return id;

        }

        //get_term_id
        //from terms table in the DB
        public static int get_term_id(string term, SqlConnection connection = null)
        {
            return get_id(term, @"Select TERM_ID from Terms where term = {0}");

        }

        //get_Bigram_id
        //from Bigram_index table in the DB
        public static int get_Bigram_id(string bigram, SqlConnection connection = null)
        {

            return get_id(bigram, @"Select id from  Bigram_indexx where Bigram = {0}");
          
        }

        //the main function that we call in get_term_id & get_Bigram_id 
        //to reduce the number of opening the connection and closing it.
        private static int get_id(string qurey, string cmdStr, SqlConnection connection = null)
        {
            SqlConnection con;
            if (connection == null)
            {
                con = new SqlConnection(connectionString);
                con.Open();
            }
            else
                con = connection;

            SqlCommand cmd = new SqlCommand(string.Format(cmdStr, "@qurey"), con);

            SqlParameter paramName = new SqlParameter("@qurey", qurey);
            cmd.Parameters.Add(paramName);
            object id = cmd.ExecuteScalar();
            int resultID;
            if ((id == DBNull.Value) || (id == null))
                resultID = default(int);
            else
                resultID = (int)id;
            if (connection == null)
                con.Close();
            return resultID;
        }

        //insert_into_DB_SoundexTable
        public static void insert_into_DB_SoundexTable(string term_code)
        {
            //SQLQuery
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            string insertStr = @" insert into soundex_index(Soundex)values(@code)";
            SqlCommand cmd = new SqlCommand(insertStr, con);

            SqlParameter TermCode = new SqlParameter("@code", term_code);
            cmd.Parameters.Add(TermCode);

            cmd.ExecuteNonQuery();

            con.Close();
        }

        private static List<int> get_docs(string qurey, string cmdStr, SqlConnection connection = null)
        {
            SqlConnection con;
            if (connection == null)
            {
                con = new SqlConnection(connectionString);
                con.Open();
            }
            else
                con = connection;

            SqlCommand cmd = new SqlCommand(string.Format(cmdStr, "@qurey"), con);

            SqlParameter paramName = new SqlParameter("@qurey", qurey);
            cmd.Parameters.Add(paramName);
            object id = cmd.ExecuteScalar();
            List<int> ints = new List<int>();
            using (SqlDataReader reader = cmd.ExecuteReader())
            {

                while (reader.Read())
                {
                    ints.Add(reader.GetInt32(0)); // provided that first (0-index) column is int which you are looking for
                }
            }
            con.Close();
            return ints;
        }

        public static List<int> get_doc_id(string id, SqlConnection connection = null)
        {

            return get_docs(id, @"Select DocId from invertedIndex where term_id= {0}");

        }

        public static string  positions;

        private static string get_pos(string tid,string did, string cmdStr, SqlConnection connection = null)
        {
            SqlConnection con;
            if (connection == null)
            {
                con = new SqlConnection(connectionString);
                con.Open();
            }
            else
                con = connection;

            SqlCommand cmd = new SqlCommand(string.Format(cmdStr, "@qurey"), con);

            SqlParameter param = new SqlParameter();
            param.ParameterName = "@query";
            param.ParameterName = "@query1";
            param.Value = tid;
            param.Value = did;
            cmd.Parameters.Add(param);
            SqlDataReader reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                positions = (string)reader[0];
             
            }

            con.Close();
            return positions;
        }

        public static string get_positions(string term_id,string doc_id, SqlConnection connection = null)
        {
            return get_pos(term_id,doc_id, @"select positions from invertedIndex where term_id="+term_id+"and DocId="+doc_id);
        }

        private static string get_url(int id,string cmdStr, SqlConnection connection = null)
        {
            SqlConnection con;
            if (connection == null)
            {
                con = new SqlConnection(connectionString);
                con.Open();
            }
            else
                con = connection;

            SqlCommand cmd = new SqlCommand(string.Format(cmdStr, "@qurey"), con);

            SqlParameter paramName = new SqlParameter("@qurey", id);
            cmd.Parameters.Add(paramName);
            SqlDataReader reader = cmd.ExecuteReader();
            string url="";
            while (reader.Read())
            {
                 url= (string)reader[0];

            }

            con.Close();
            return url;
        }

        public static string getURL(int id, SqlConnection connection = null)
        {

            return get_url(id, @"select URL from Documentss where id={0}");

        }

        private static List<int> get_terms(int kgram_id, string cmdStr, SqlConnection connection = null)
        {
            SqlConnection con;
            if (connection == null)
            {
                con = new SqlConnection(connectionString);
                con.Open();
            }
            else
                con = connection;

            SqlCommand cmd = new SqlCommand(string.Format(cmdStr, "@qurey"), con);

            SqlParameter paramName = new SqlParameter("@qurey", kgram_id);
            cmd.Parameters.Add(paramName);
            object id = cmd.ExecuteScalar();
            List<int> ints = new List<int>();
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    ints.Add(reader.GetInt32(0)); // provided that first (0-index) column is int which you are looking for
                }
            }
            con.Close();
            return ints;
        }

        public static List<int> get_terms_id(int id, SqlConnection connection = null)
        {

            return get_terms(id, @"Select Term_ID from KgramID_TermID where k_gram_ID= {0}");

        }

        private static string get_Term_Name(int id, string cmdStr, SqlConnection connection = null)
        {
            SqlConnection con;
            if (connection == null)
            {
                con = new SqlConnection(connectionString);
                con.Open();
            }
            else
                con = connection;

            SqlCommand cmd = new SqlCommand(string.Format(cmdStr, "@qurey"), con);

            SqlParameter paramName = new SqlParameter("@qurey", id);
            cmd.Parameters.Add(paramName);
            SqlDataReader reader = cmd.ExecuteReader();
            string name = "";
            while (reader.Read())
            {
                name = (string)reader[0];

            }

            con.Close();
            return name;
        }

        public static string getTermName(int id, SqlConnection connection = null)
        {

            return get_Term_Name(id, @"select Term from Terms where TERM_ID={0}");

        }
    }
}

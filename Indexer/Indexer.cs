using Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Text.RegularExpressions;

namespace Indexer
{
    public class Indexer
    {
        //parse the text(didn't use because we have a managed data)
        private string ParseHTMLContent(string HTMLContent)
        {
            XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml("$<body>{HTMLContent}</body>");
            StringBuilder content = new StringBuilder();

            foreach (XmlNode node in doc.SelectNodes("body"))
            {
                content.Append(node.InnerText);
            }

            byte[] byteArray = Encoding.UTF8.GetBytes(Convert.ToString(content));
            return System.Text.Encoding.UTF8.GetString(byteArray);
        }

        //take the text and return the tokens in alist of tokens
        public static List<string> tokenize(string textOfContent)

        {
            //take the text and return the tokens in alist of tokens
            List<string> SplittedContentList = new List<string>();
            //convert the text of contents to tokens
            SplittedContentList = textOfContent.Split(new char[] { ' ', ',', ':', ';', '?', '\n' }).ToList();
            //remove all the empty cells
            SplittedContentList.RemoveAll(str => String.IsNullOrEmpty(str)); 

            return SplittedContentList;
        }

        //do some linguistics algorithms on the token
        public static List<string> linguistics_Algo(List<string> SplittedContentList)
        {
            //Remove any punctuation character from the word (i.e. dot, !, ?...etc)
            for (int j = 0; j < SplittedContentList.Count; j++)
            {
                var character = from ch in SplittedContentList[j]
                                where !Char.IsPunctuation(ch)
                                select ch;

                var bytes = UnicodeEncoding.ASCII.GetBytes(character.ToArray());
                SplittedContentList[j] = UnicodeEncoding.ASCII.GetString(bytes);

                //CaseFolding
                SplittedContentList[j] = SplittedContentList[j].ToLower();
            }

            return SplittedContentList;
        }

        Dictionary<string, List<inverted_index>> terms = new Dictionary<string, List<inverted_index>>();
        Dictionary<string, List<inverted_index>> inverted_index_term = new Dictionary<string, List<inverted_index>>();
        Dictionary<string, List<inverted_index>> inverted_index_table = new Dictionary<string, List<inverted_index>>();

        //the whole indexing process
        public void indexing()
        {
            //select all the contents of the URLs
            List<URL_Data> contents = SQL_DB.GetAllUrlData();
            //loop on these contents
            for (int i = 0; i < contents.Count; i++)
            {
                //1. Parse each Content (to extract the text)
                //didn't use to reduse the run time beacause we have managed data
                // string textOfContent = ParseHTMLContent(contents[i].content);


                //2.tokenize each content
                string textOfContent =contents[i].content;
                List<string> splittedContentList = tokenize(textOfContent);

                //3.linguistics Algo
                List<string> doc_Tokens = linguistics_Algo(splittedContentList);

                //4.dic to hold terms without stopwords , and position of term in the doc before remove the stopwords (as comma seprated string)
                Dictionary<string, string> doc_Terms = new Dictionary<string, string>();
                for (int j = 0; j < doc_Tokens.Count; j++)
                {
                    if (!Constants.StopWords.Contains(doc_Tokens[j]))
                    {
                        string term = doc_Tokens[j];
                        if (doc_Terms.Keys.Contains(term))
                            doc_Terms[term] = doc_Terms[term] + "," + j.ToString();
                        else
                            doc_Terms.Add(term, j.ToString());

                    }

                }
                //big dic to hold all the unique terms of all documents(dic=(term,list[docID,freq,pos]))
                ////this list holds many val.
                //so this dic holds all words from all documents without repetition and without stopwords and with all its info in a list 
                foreach (var dTerm in doc_Terms)
                {
                    inverted_index doc_term_data = new inverted_index
                    {
                        DocId = contents[i].ID,
                        Frequency = dTerm.Value.Count(c => c == ',')+1,
                        Position = dTerm.Value
                    };

                    if (terms.Keys.Contains(dTerm.Key))
                        //new value only
                        (terms[dTerm.Key]).Add(doc_term_data);
                    else
                    {
                        //new row (key, value)
                        terms.Add(dTerm.Key, new List<inverted_index> { doc_term_data });
                    }

                }

            }

            //save the term and its doc_id in a table before stemming to build next module
            foreach (var term in terms)
            {

                for (int i = 0; i < term.Value.Count; i++)
                {
                    //if ((Regex.IsMatch(term.Key, "^[a-zA-Z0-9]*$")) && (term.Key != " "))
                    if ((Regex.IsMatch(term.Key, @"^[a-zA-Z]+$")) && (term.Key != " "))
                    {
                        //Console.WriteLine("filtered");
                        SQL_DB.insert_into_DB_TermDocTable(term.Key, term.Value[i]);
                    }
                    //else
                    //{
                    //    Console.WriteLine("bye bye term");
                    //}
                }
            }

            ////get the bigram of the term and put it in the DB table
            foreach (var term in terms)
            {
                string text = term.Key;

                AddBiGram(text);
            }
            //Console.WriteLine("5alas add bigram");
            //get the soundex of the term and put it in the DB table
            foreach (var term in terms)
            {
                string single_term = term.Key;

                AddSoundex(single_term);
            }

            inverted_index_table = stemming(); //stemming Algo

            //building the inverted_index after stemming
            foreach (var rec in inverted_index_table)
            {
                if ((Regex.IsMatch(rec.Key, @"^[a-zA-Z]+$")) && (rec.Key != " "))
                {
                    int term_id = SQL_DB.insert_into_DB_termsTable(rec.Key);
                    for (int i = 0; i < rec.Value.Count; i++)
                    {
                        rec.Value[i].Term_id = term_id;
                        SQL_DB.insert_into_DB_invertedindexTable(rec.Key, rec.Value[i]);
                    }
                }
            }


        }

        //the soundex code table insertion
        public static void AddSoundex(string single_term)
        {
            if(single_term == "")
            {
                return;
            }

            string codeterm = single_term[0].ToString();
            
            //Loop On Term Length
            for (int i = 1; i <= single_term.Length - 1; i++)
            {
                //Get The Code Of EACH Character
                foreach (var c in Constants.DictionarysoundexList)
                {
                    if (c.Key.Contains(single_term[i]))
                    {
                        codeterm += c.Value;
                        break;
                    }
                }
            }

            //Loop On Code Term And Remove Repeated Character
            for (int j = codeterm.Length - 1; j >= 1; j--)
            {
                if (codeterm[j] == codeterm[j - 1])
                    codeterm = codeterm.Remove(j, 1);
            }

            //Remove All Zeros
            for (int n = 0; n < codeterm.Length; n++)
            {
                if (codeterm[n].Equals('0'))
                {
                    codeterm = codeterm.Remove(n, 1);
                }
            }

            //If CodeLenght less than 4 Concate zeros
            if (codeterm.Length < 4)
            {
                int diff = 4 - codeterm.Length;
                for (int k = 0; k < diff; k++)
                {
                    codeterm += '0';

                }
            }

            SQL_DB.insert_into_DB_SoundexTable(codeterm);

        }

        //the bigram of a word table insertion
        public static void AddBiGram(string text)
        {

            //take the term and put the doolar sign with the first letter and the last letter and we must not repeat the bi grams so we will check the occurance first

            for (int i = -1; i < text.Length; i++)
            {
                string dollar_sign = "$";
                string BI1 ;
                string BI2;
                if (i == -1 )
                    BI1 = dollar_sign;
                else
                    BI1 = text[i].ToString();

                if (i + 1 == text.Length)
                    BI2 = dollar_sign;
                else
                    BI2 = text[i + 1].ToString();

                string BI;
              
                BI = BI1 + BI2;
                SQL_DB.insert_into_DB_BIGRAMTable(BI);
            }
        }

        //stemming process 
        private Dictionary<string, List<inverted_index>> stemming()
        {
            foreach (var vTerm in terms)
            {
                //call the stemming func in the stemming class
                Stemming_Algo stemmer = new Stemming_Algo();
                string inverted_index_term_str = stemmer.stem(vTerm.Key);

                //if the dic of the stemming words have the input stemmer word
                if (inverted_index_term.Keys.Contains(inverted_index_term_str))
                {            
                    //we will append the new pos of these term       
                    inverted_index_term[inverted_index_term_str] = MergeIndexValues(vTerm.Value, inverted_index_term[inverted_index_term_str]);
                }
                else
                {
                    //we will add it
                    inverted_index_term.Add(inverted_index_term_str, vTerm.Value);
                }
            }

            return inverted_index_term;
        }
       
        // Merge 2 inverted_index list by comparing the doc id and appending if it matchs
        private List<inverted_index> MergeIndexValues(List<inverted_index> list1, List<inverted_index> list2)
        {

            List<inverted_index> inverted_index_val= new List<inverted_index> { } ;
           for (int j = 0; j < list1.Count; j++)
            {
                for (int i=0; i< list2.Count;i++)
                {
                    if (list1[j].DocId == list2[i].DocId)
                    {
                        inverted_index inverted_index_obj = new inverted_index
                        {
                            DocId = list1[j].DocId,
                            Frequency = list1[j].Frequency + list2[i].Frequency+1,
                            Position = list1[j].Position + "," + list2[i].Position
                        };
                        inverted_index_val.Add(inverted_index_obj);
                        list1.Remove(list1[j]);
                        list2.Remove(list2[i]);
                        i--;j--;
                        break;
                    }
                     
                }
            }

            inverted_index_val.AddRange(list1);
            inverted_index_val.AddRange(list2);
            return inverted_index_val;
        }
    }
}

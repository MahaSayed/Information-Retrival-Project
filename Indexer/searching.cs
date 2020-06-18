using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indexer
{
    public class searching
    {
        List<string> trm_stm=new List<string>();
        public static List<List<int>> docarr=new List<List<int>>();
        public static List<int> commondoc;
        public static List<string> corrected_word = new List<string>();

        public List<string> search(string query)
        {
            if (!query.Contains('"')) 
            {
                 /*Non Exact search*/

                //Apply indexing steps 
                trm_stm = indexing_steps(query);

                List<int> id = new List<int>();
                List<int> docs = new List<int>();
                commondoc = new List<int>();
                //Get th ID of each term
                for (int i = 0; i < trm_stm.Count(); i++)
                {
                    id.Add(SQL_DB.get_term_id(trm_stm[i]));
                }

                //Get the Documents that contain each term and put them in list of lists
                for (int i = 0; i < id.Count(); i++)
                {
                    docarr.Add(SQL_DB.get_doc_id(id[i].ToString()));

                }


                List<int> firstlist = new List<int>();
                bool flag = false;
                firstlist = docarr[0];

                //get the common documents 
                foreach (int item in firstlist)
                {
                    for (int j = 1; j < docarr.Count(); j++)
                    {
                        if (docarr[j].Contains(item))
                        {
                            flag = true;
                        }
                        else
                        {
                            flag = false;
                            break;
                        }

                    }
                    if (flag == true)
                    {
                        commondoc.Add(item);
                    }
                }

                
                List<List<int>> positionint = new List<List<int>>();
                Dictionary<int, List<List<int>>> terms = new Dictionary<int, List<List<int>>>();
                List<int> pstion = new List<int>();
                
                //get the positions of each term in each document
                for (int j = 0; j < commondoc.Count; j++)
                {
                    for (int i = 0; i < id.Count; i++)
                    {

                        string pos = SQL_DB.get_positions(id[i].ToString(), commondoc[j].ToString());
                        string[] p = pos.Split(',');

                        for (int k = 0; k < p.Count(); k++)
                        {

                            pstion.Add(Convert.ToInt32(p[k]));

                        }


                        positionint.Add(pstion);
                        pstion = new List<int>();

                    }
                    // by this I have the common documents containing the required terms and their positions 
                    terms.Add(commondoc[j], positionint);
                    positionint = new List<List<int>>();

                }

                List<int> minValuesBetTerms = minimumvalues(terms);  //get minimum values between terms in each document

                List<int> keyList = new List<int>(terms.Keys);
                Dictionary<int, int> dic = new Dictionary<int, int>();

                
                for (int i = 0; i < keyList.Count; i++)
                {
                    dic.Add(keyList[i], minValuesBetTerms[i]);
                }
                 // get min value from x list and get its document 
                List<string> retrieved_Docs = new List<string>();
                int minId;
               
                for (int i = 0; i < minValuesBetTerms.Count; i++)
                {
                    int docID = 0;
                    minId = minValuesBetTerms.Min();
                    for (int j = 0; j < dic.Count; j++)
                    {
                        int key = keyList[j];
                        if (minId == dic[key])
                        {
                            docID = key;
                            break;
                        }
                    }

                    retrieved_Docs.Add(SQL_DB.getURL(docID)); //put the url in a list
                    minValuesBetTerms.Remove(minId);


                }
                return retrieved_Docs; //return the list that contain all document sorted by distance between the query terms 
            }
            else
            {
                /*Exact Search*/

                List<string> retrieved_Docs = new List<string>();
                List<int> indexs = new List<int>();
                List<string> tokenterms;
                List<int> differences = new List<int>();

                //divide the query into Tokens and insert them in tokenterms list
                tokenterms = Indexer.tokenize(query);

                //apply linguistics_algo on tokens 
                List<string> lingterms;
                lingterms = Indexer.linguistics_Algo(tokenterms);

                //Remove stop words
                List<string> tokentermsnostop = new List<string>();
                for (int j = 0; j < lingterms.Count; j++)
                {
                    if (!Constants.StopWords.Contains(lingterms[j]))
                    {
                        tokentermsnostop.Add(lingterms[j]);

                        //save position of non stop words in indexs
                        indexs.Add(j);
                    }

                }

                //Apply stemming Algo on tokens
                for (int i = 0; i < tokentermsnostop.Count(); i++)
                {
                    string term = tokentermsnostop[i];
                    string trmstm;
                    Stemming_Algo stming = new Stemming_Algo();
                    trmstm = stming.stem(term);
                    trm_stm.Add(trmstm);

                }

                //Get The difference between terms in the query
                for (int i = 0; i < indexs.Count; i++)
                {
                    
                    int min = 0;
                    if ((i + 1) != indexs.Count)
                    {
                        min = indexs[i + 1] - indexs[i];
                        differences.Add(min);
                    }
                }

                List<int> id = new List<int>();
                List<int> docs = new List<int>();
                commondoc = new List<int>();

                //Get th ID of each term
                for (int i = 0; i < trm_stm.Count(); i++)
                {
                    id.Add(SQL_DB.get_term_id(trm_stm[i]));
                }

                //Get the Documents that contain each term and put them in list of lists
                for (int i = 0; i < id.Count(); i++)
                {
                    docarr.Add(SQL_DB.get_doc_id(id[i].ToString()));

                }
                List<int> firstlist = new List<int>();
                bool flag = false;
                firstlist = docarr[0];

                //get the common documents 
                foreach (int item in firstlist)
                {
                    for (int j = 1; j < docarr.Count(); j++)
                    {
                        if (docarr[j].Contains(item))
                        {
                            flag = true;
                        }
                        else
                        {
                            flag = false;
                            break;
                        }

                    }
                    if (flag == true)
                    {
                        commondoc.Add(item);
                    }
                }

                List<List<int>> positionint = new List<List<int>>();
                Dictionary<int, List<List<int>>> terms = new Dictionary<int, List<List<int>>>();
                List<int> pstion = new List<int>();

                //get the positions of each term in each document
                for (int j = 0; j < commondoc.Count; j++)
                {
                    for (int i = 0; i < id.Count; i++)
                    {

                        string pos = SQL_DB.get_positions(id[i].ToString(), commondoc[j].ToString());
                        string[] p = pos.Split(',');

                        for (int k = 0; k < p.Count(); k++)
                        {

                            pstion.Add(Convert.ToInt32(p[k]));

                        }


                        positionint.Add(pstion);
                        pstion = new List<int>();

                    }
                    terms.Add(commondoc[j], positionint);
                    positionint = new List<List<int>>();

                }


                List<int> numOccurofQuery = maxvalues_exact(terms, differences);  //Get the Number of Occurence of the query in each document

                List<int> keyList = new List<int>(terms.Keys);
                Dictionary<int, int> dic = new Dictionary<int, int>();

                for (int i = 0; i < keyList.Count; i++)
                {
                    dic.Add(keyList[i], numOccurofQuery[i]);
                }
                retrieved_Docs = new List<string>();
                int maxId;

                //get the max occurence
                for (int i = 0; i < numOccurofQuery.Count; i++)
                {
                    int docID = 0;
                    maxId = numOccurofQuery.Max();
                    for (int j = 0; j < dic.Count; j++)
                    {
                        int key = keyList[j];
                        if (maxId == dic[key])
                        {
                            docID = key;
                            break;
                        }
                    }
                    retrieved_Docs.Add(SQL_DB.getURL(docID));  //put the url in a list
                    numOccurofQuery.Remove(maxId);

                }
                return retrieved_Docs;    //return the list that contain all document sorted by occurence of the query 
            }

        }

        public static List<int> minimumvalues(Dictionary<int, List<List<int>>> terms)
        {

            List<int> minresult = new List<int>(); ;
            List<int> finalMinList = new List<int>();
            for (int i = 0; i < terms.Count; i++)
            {
                // doc id
                List<int> keyList = new List<int>(terms.Keys);

                List<List<int>> listvalue = new List<List<int>>();
                int k = keyList[i];
                listvalue = terms[k];


                List<int> firstlistValue = new List<int>();
                List<int> secondlistValue = new List<int>();

                List<int> minsubresult = new List<int>();
                minresult = new List<int>();
                for (int h = 0; h < listvalue.Count - 1; h++)
                {
                    minsubresult = new List<int>();
                    firstlistValue = listvalue[h];
                    secondlistValue = listvalue[h + 1];
                    for (int j = 0; j < firstlistValue.Count; j++)
                    {
                        List<int> subresult = new List<int>();
                        for (int l = 0; l < secondlistValue.Count; l++)
                        {

                            int n1 = firstlistValue[j];
                            int n2 = secondlistValue[l];
                            subresult.Add(Math.Abs(n1 - n2));

                        }
                        minsubresult.Add(subresult.Min());
                        
                    }
                    minresult.Add(minsubresult.Min());

                }
                //add all values in min result
                int finalmin = 0;
                foreach (int item in minresult)
                {
                    finalmin += item;
                }
                //add the result to final list 

                finalMinList.Add(finalmin);

            }

            return finalMinList;
        }

        public static List<int> maxvalues_exact(Dictionary<int, List<List<int>>> terms, List<int> differences)
        {

            List<int> freq = new List<int>();
            List<int> finalMinList = new List<int>();
            for (int i = 0; i < terms.Count; i++)
            {
                int count = 0;
                List<int> keyList = new List<int>(terms.Keys);

                List<List<int>> listvalue = new List<List<int>>();
                int k = keyList[i];
                listvalue = terms[k];


                List<int> firstlistValue = new List<int>();
                List<int> secondlistValue = new List<int>();

                List<int> counters = new List<int>();
                for (int h = 0; h < listvalue.Count - 1; h++)
                {
                    count = 0;
                    firstlistValue = listvalue[h];
                    secondlistValue = listvalue[h + 1];
                    for (int j = 0; j < firstlistValue.Count; j++)
                    {

                        for (int l = 0; l < secondlistValue.Count; l++)
                        {

                            int n1 = firstlistValue[j];
                            int n2 = secondlistValue[l];
                            if ((n1 - n2) == differences[h])
                            {
                                count++;

                            }

                        }
                    }
                    counters.Add(count);

                }

                freq.Add(counters.Min());

            }

            return freq;
        }

        public List<string> Spell_Checking(string query)
        {
            //Apply indexing steps 
            trm_stm = indexing_steps(query);


            List<string> wrong_words = new List<string>();

            //if entered query contain wrong words add them into list
            for (int i = 0; i < trm_stm.Count; i++)
            {
                int id = SQL_DB.get_term_id(trm_stm[i]);
                if (id == 0) // lw maloosh id tb2a wrong word
                {

                    wrong_words.Add(trm_stm[i]);

                }

            }
            
            //get correct words for each wrong word
            for (int j = 0; j < wrong_words.Count; j++)
            {
                List<string> terms_from_juccard = new List<string>();
                List<string> query_bigrams = new List<string>();
                string word = wrong_words[j];

                //get bigrams of the wrong word
                for (int i = -1; i < word.Length; i++)
                {
                    string dollar_sign = "$";
                    string BI1;
                    string BI2;
                    if (i == -1)
                        BI1 = dollar_sign;
                    else
                        BI1 = word[i].ToString();

                    if (i + 1 == word.Length)
                        BI2 = dollar_sign;
                    else
                        BI2 = word[i + 1].ToString();

                    string BI;

                    BI = BI1 + BI2;
                    query_bigrams.Add(BI);
                    
                }
            

                //get bigrams' ids
                List<int> bigram_ids=new List<int>();
                for (int k = 0; k < query_bigrams.Count; k++)
                {
                    bigram_ids.Add(SQL_DB.get_Bigram_id(query_bigrams[k]));
                }

                List<int> terms_id = new List<int>();

                
                for (int h = 0; h < bigram_ids.Count; h++)
                {
                    //get ids of terms that contain each bigram 
                    terms_id = SQL_DB.get_terms_id(bigram_ids[h]);

                    //get the names of these terms 
                    List<string> terms_name = new List<string>();
                    for (int f = 0; f < terms_id.Count; f++)
                    {
                        terms_name.Add(SQL_DB.getTermName(terms_id[f]));

                    }

                    List<string> dictionary_bigrams;

                    for (int d = 0; d < terms_name.Count; d++)
                    {
                        dictionary_bigrams = new List<string>();
                        string word1 = terms_name[d];

                        //get bigrams of each term
                        for (int i = -1; i < word1.Length; i++)
                        {
                            string dollar_sign = "$";
                            string BI1;
                            string BI2;
                            if (i == -1)
                                BI1 = dollar_sign;
                            else
                                BI1 = word1[i].ToString();

                            if (i + 1 == word1.Length)
                                BI2 = dollar_sign;
                            else
                                BI2 = word1[i + 1].ToString();

                            string BI;

                            BI = BI1 + BI2;
                            dictionary_bigrams.Add(BI);
                        }

                        //get common bigrams between each term and the wrong word
                        int common_bigram_count = 0;
                        for (int g = 0; g < query_bigrams.Count; g++)
                        {
                            
                            for (int q = 0; q < dictionary_bigrams.Count; q++)
                            {
                                if (query_bigrams[g] == dictionary_bigrams[q])
                                {
                                    common_bigram_count++;  //Number of common bigrams
                                }
                            }
                        }
                        //calculate juccard similarity
                       
                        float multip=(2 * common_bigram_count);
                        float sum=(query_bigrams.Count + dictionary_bigrams.Count);
                        float juccard =  multip/sum ;

                        //if the calculated juccard > 0.45 add this term in a list, else ignore this term 
                        if (juccard > 0.45)    //Preset threshold is 0.45 for jaccard
                        {
                            if (!terms_from_juccard.Contains(terms_name[d]))
                            {
                                terms_from_juccard.Add(terms_name[d]);
                            }
                            
                        }
                    }

                }
               
                //calculate edit distance between the wrong word and each term in (terms_from_juccard) list 
                List<int> distance = new List<int>();
                for (int c = 0; c < terms_from_juccard.Count; c++)
                {
                    int dist=CalcLevenshteinDistance(wrong_words[j], terms_from_juccard[c]);
                    distance.Add(dist);   //put the distance in a list
                }

                 Dictionary<string, int> dic = new Dictionary<string, int>();
                for(int i=0;i<terms_from_juccard.Count;i++)
                {
                    dic.Add(terms_from_juccard[i],distance[i]);
                }

                
                List<int> valueList = new List<int>(dic.Values);
                List<string> keyList = new List<string>(dic.Keys);
                
                //get the min edit distance
                int min = valueList.Min();
                for (int a = 0; a < dic.Count; a++)
                {

                   string key = keyList[a];
                    if (min == dic[key])
                    {
                        corrected_word.Add(key);  //add terms which has min edit distance in a list & return this list
                        
                    }
                }
               

            }
            return corrected_word; 
           
        }
        
        //Edit Distance Algorithm
        private static int CalcLevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
          
        public void bi()
        {
            List<String> Terms_list = SQL_DB.get_terms();
            for (int i = 0; i < Terms_list.Count; i++)
            {
                AddBiGram(Terms_list[i]);
            }
        }

        public static void AddBiGram(string text)
        {

            //take the term and put the doolar sign with the first letter and the last letter and we must not repeat the bi grams so we will check the occurance first

            for (int i = -1; i < text.Length; i++)
            {
                string dollar_sign = "$";
                string BI1;
                string BI2;
                if (i == -1)
                    BI1 = dollar_sign;
                else
                    BI1 = text[i].ToString();

                if (i + 1 == text.Length)
                    BI2 = dollar_sign;
                else
                    BI2 = text[i + 1].ToString();

                string BI;

                BI = BI1 + BI2;
                int term_id=SQL_DB.get_term_id(text);
               
                SQL_DB.insert_into_DB_BIGRAMTable(BI);

                int BI_ID =SQL_DB.get_Bigram_id(BI);

                SQL_DB.insert_into_DB_BI_TERM_Table(BI_ID, term_id);
            }
        }

        public List<string> indexing_steps(string query)
        {
            //divide the query into Tokens and insert them in tokenterms list
            List<string> tokenterms;
            tokenterms = Indexer.tokenize(query);

            //apply linguistics_algo on tokens 
            List<string> lingterms;
            lingterms = Indexer.linguistics_Algo(tokenterms);

            //Remove stop words
            List<string> tokentermsnostop = new List<string>();
            for (int j = 0; j < lingterms.Count; j++)
            {
                if (!Constants.StopWords.Contains(lingterms[j]))
                {
                    tokentermsnostop.Add(lingterms[j]);

                }

            }

            //Apply stemming Algo on tokens
            for (int i = 0; i < tokentermsnostop.Count(); i++)
            {
                string term = tokentermsnostop[i];
                string trmstm;
                Stemming_Algo stming = new Stemming_Algo();
                trmstm = stming.stem(term);
                trm_stm.Add(trmstm);

            }

            return trm_stm;


        }
    }
}

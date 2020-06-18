using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Indexer.Model;

using Indexer;

namespace final_IR_project
{
    public partial class WebForm1 : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnIndexing_Click(object sender, EventArgs e)
        {
            //Indexer.Indexer i = new Indexer.Indexer();
            //i.indexing();
            //searching s = new searching();
            //s.bi();

        }
        List<string> correct_words = new List<string>();
        protected void Button1_Click(object sender, EventArgs e)
        {

            DropDownList1.Items.Clear();
            
            //Spell Checking
            if (RadioButton1.Checked)
            {
                searching se = new searching();
                correct_words=se.Spell_Checking(TextBox1.Text);

                //Add the correct words into dropdownlist
                for (int i = 0; i < correct_words.Count; i++)
                {
                    DropDownList1.Items.Add(correct_words[i]);
                }
               

            }
            List<string> retDocs = new List<string>();
            searching s = new searching();
            retDocs= s.search(TextBox1.Text);
            List<string> uniDocs= new List<string>();
            for (int i = 0; i < retDocs.Count; i++)
            {
                if (!uniDocs.Contains(retDocs[i]))
                {
                    uniDocs.Add(retDocs[i]);
                }
            }

            //add the returned urls into the listbox
            for (int i = 0; i < uniDocs.Count;i++ )
            {
                    ListBox1.Items.Add(uniDocs[i]);    
            }
            
        }
        public void ListBox1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            // Get the currently selected item in the ListBox.
            string selected_URL = ListBox1.SelectedItem.ToString();

            //redirect to the site of the selected url
            //Response.Redirect(selected_URL);


        }

    }
}
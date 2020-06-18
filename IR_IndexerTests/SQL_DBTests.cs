using Microsoft.VisualStudio.TestTools.UnitTesting;
using Indexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Indexer.Model;

namespace Indexer.Tests
{
    [TestClass()]
    public class SQL_DBTests
    {
        [TestMethod()]
        public void GetAllUrlDataTest()
        {
         // List<URL_Data> s=  SQL_DB.GetAllUrlData();
            Indexer i = new Indexer();
            i.indexing();
        }
    }
}
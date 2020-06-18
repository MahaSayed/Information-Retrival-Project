using Microsoft.VisualStudio.TestTools.UnitTesting;
using Indexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indexer.Tests
{
    [TestClass()]
    public class IndexerTests
    {
        [TestMethod()]
        public void AddSoundexTest()
        {
            Indexer.AddSoundex("herman");


        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indexer.Model
{
    public class inverted_index
    {
        public int Id { get; set; }
        public int Term_id { get; set; }
        public int DocId { get; set; }
        public string Position { get; set; }
        public int Frequency { get; set; }
    }
}

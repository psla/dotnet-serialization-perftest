using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerializationPerfTest.BiggerObject
{
    public class BiggerObjectWithBytes
    {
        public Uri Uri { get; set; }

        public string Header { get; set; }

        public byte[] Content { get; set; }
    }
}

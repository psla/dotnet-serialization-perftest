using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerializationPerfTest.BiggerObject
{
    [Bond.Schema]
    public class BiggerObjectWithBytesBond
    {
        // note that Bond uses string as uri
        [Bond.Id(10)]
        public string Uri { get; set; }

        [Bond.Id(20)]
        public string Header { get; set; }

        [Bond.Id(30)]
        public byte[] Content { get; set; }
    }
}

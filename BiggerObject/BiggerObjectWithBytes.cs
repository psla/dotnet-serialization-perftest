using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerializationPerfTest.BiggerObject
{
    public class BiggerObjectWithBytes
    {
        Uri Uri { get; set; }

        string Header { get; set; }

        byte[] Content { get; set; }
    }
}

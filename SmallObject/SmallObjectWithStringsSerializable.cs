using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerializationPerfTest
{
    [Serializable]
    public class SmallObjectWithStringsSerializable
    {
        public string String1 { get; set; }

        public string String2 { get; set; }

        public string String3 { get; set; }

        public string String4 { get; set; }
    }
}

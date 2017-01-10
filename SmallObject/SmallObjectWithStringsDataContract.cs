using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SerializationPerfTest
{
    [DataContract]
    class SmallObjectWithStringsDataContract
    {
        [DataMember(Order = 10)]
        public string String1 { get; set; }

        [DataMember(Order = 20)]
        public string String2 { get; set; }

        [DataMember(Order = 30)]
        public string String3 { get; set; }

        [DataMember(Order = 40)]
        public string String4 { get; set; }
    }
}

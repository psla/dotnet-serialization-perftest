using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SerializationPerfTest.BiggerObject
{
    [DataContract]
    public class BiggerObjectWithBytesDataContract
    {
        [DataMember(Order = 10)]
        public Uri Uri { get; set; }

        [DataMember(Order = 20)]
        public string Header { get; set; }

        [DataMember(Order = 30)]
        public byte[] Content { get; set; }
    }
}

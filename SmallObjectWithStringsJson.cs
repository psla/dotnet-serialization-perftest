using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerializationPerfTest
{
    public class SmallObjectWithStringsJson
    {
        [JsonProperty]
        public string String1 { get; set; }

        [JsonProperty]
        public string String2 { get; set; }

        [JsonProperty]
        public string String3 { get; set; }

        [JsonProperty]
        public string String4 { get; set; }
    }
}

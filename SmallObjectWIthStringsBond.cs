using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerializationPerfTest
{
    [Bond.Schema]
    class SmallObjectWithStringsBond
    {
        public class Foo
        {
            [Bond.Id(10)]

            public string String1 { get; set; }

            [Bond.Id(20)]
            public string String2 { get; set; }

            [Bond.Id(30)]
            public string String3 { get; set; }

            [Bond.Id(40)]
            public string String4 { get; set; }
        }
    }
}

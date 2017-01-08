using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerializationPerfTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var serialziationSummary = BenchmarkRunner.Run<SerializationTestSuite>();
            // var deserializationSummary = BenchmarkRunner.Run<DeserializationTests>();
        }
    }
}

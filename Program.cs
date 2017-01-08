﻿using BenchmarkDotNet.Attributes;
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
            var suite = new SerializationTestSuite();
            suite.GenerateObject();
            PrintLengths(suite);

            // var serialziationSummary = BenchmarkRunner.Run<SerializationTestSuite>();
            // var deserializationSummary = BenchmarkRunner.Run<DeserializationTests>();
        }

        private static void PrintLengths(SerializationTestSuite testSuite)
        {
            var type = typeof(SerializationTestSuite);
            foreach (var method in type.GetMethods())
            {
                if (method.GetCustomAttributes(true).Any(x => x is BenchmarkAttribute))
                {
                    var result = method.Invoke(testSuite, null);
                    if (result == null)
                    {
                        throw new InvalidOperationException("Serialization method must return result on method " + method.Name);
                    }
                    if (result is byte[])
                    {
                        Console.WriteLine("{0}: {1} bytes", method.Name, ((byte[])result).Length);
                    }
                    else if (result is string)
                    {
                        Console.WriteLine("{0}: {1} bytes", method.Name, Encoding.UTF8.GetBytes((string)result).Length);
                    }
                    else if (result is int)
                    {
                        Console.WriteLine("{0}: {1} bytes", method.Name, (int)result);
                    }
                    else
                    {
                        throw new InvalidOperationException("Serialization method must return result");
                    }
                }
            }
        }
    }
}

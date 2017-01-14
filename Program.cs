using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Google.Protobuf;
using SerializationPerfTest.BiggerObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Thrift;

namespace SerializationPerfTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var suite = new SerializationTestSuite<SmallObjectWithStrings, SmallObjectWithStringsDataContract, SmallObjectWithStringsBond, SmallObjectWithStringsProtobuf, SmallObjectWithStringsSerializable, SmallObjectWithStringsThrift>();
            suite.GenerateObject();
            PrintLengths(suite);
            Console.WriteLine("------------------");
            var suite2 = new SerializationTestSuite<BiggerObjectWithBytes, BiggerObjectWithBytesDataContract, BiggerObjectWithBytesBond, BiggerObjectWithBytesProtobuf, BiggerObjectWithBytesSerializable, BiggerObjectWithBytesThrift>();
            suite2.GenerateObject();
            PrintLengths(suite2);

            // var serialziationSummary = BenchmarkRunner.Run<SerializationTestSuite>();
            // var deserializationSummary = BenchmarkRunner.Run<DeserializationTests>();
            // BenchmarkRunner.Run<SerializationTestSuite<BiggerObjectWithBytes, BiggerObjectWithBytesDataContract, BiggerObjectWithBytesBond, BiggerObjectWithBytesProtobuf, BiggerObjectWithBytesSerializable, BiggerObjectWithBytesThrift>>();
        }

        private static void PrintLengths<TBase, TContract, TBond, TProtobuf, TPoco, TThrift>(SerializationTestSuite<TBase, TContract, TBond, TProtobuf, TPoco, TThrift> testSuite) where TProtobuf : IMessage where TThrift : Thrift.Protocol.TBase
        {
            var type = typeof(SerializationTestSuite<TBase, TContract, TBond, TProtobuf, TPoco, TThrift>);
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

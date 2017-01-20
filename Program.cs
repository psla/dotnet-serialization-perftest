using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Google.Protobuf;
using SerializationPerfTest.BiggerObject;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
            Console.SetOut(new StreamWriter(File.OpenWrite(@"C:\tmp\perfout-small.txt")));
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
            BenchmarkRunner.Run<SerializationTestSuite<SmallObjectWithStrings, SmallObjectWithStringsDataContract, SmallObjectWithStringsBond, SmallObjectWithStringsProtobuf, SmallObjectWithStringsSerializable, SmallObjectWithStringsThrift>>();
        }

        private static void PrintLengths<TBase, TContract, TBond, TProtobuf, TPoco, TThrift>(SerializationTestSuite<TBase, TContract, TBond, TProtobuf, TPoco, TThrift> testSuite) where TProtobuf : IMessage where TThrift : Thrift.Protocol.TBase
        {
            var type = typeof(SerializationTestSuite<TBase, TContract, TBond, TProtobuf, TPoco, TThrift>);
            Console.WriteLine("Name\tbytes raw\tcompressed (optimal)\tcompressed (fastest)");
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
                        var resultBytes = (byte[])result;
                        Console.WriteLine("{0}\t{1}\t{2}\t{3}", method.Name, resultBytes.Length, Compress(resultBytes, CompressionLevel.Optimal).Length, Compress(resultBytes, CompressionLevel.Fastest).Length);
                    }
                    else if (result is string)
                    {
                        var resultBytes = Encoding.UTF8.GetBytes((string)result);
                        Console.WriteLine("{0}\t{1}\t{2}\t{3}", method.Name, resultBytes.Length, Compress(resultBytes, CompressionLevel.Optimal).Length, Compress(resultBytes, CompressionLevel.Fastest).Length);
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

        private static byte[] Compress(byte[] bytes, CompressionLevel compressionLevel)
        {
            using (var outputStream = new MemoryStream(bytes.Length / 2))
            {
                using (var compression = new DeflateStream(outputStream, compressionLevel))
                {
                    compression.Write(bytes, 0, bytes.Length);
                }

                return outputStream.ToArray();
            }
        }
    }
}

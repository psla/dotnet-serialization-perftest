using AutoMapper;
using AutoMapper.Mappers;
using BenchmarkDotNet.Attributes;
using Bond;
using Bond.IO.Unsafe;
using Bond.Protocols;
using Google.Protobuf;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Hadoop.Avro;

namespace SerializationPerfTest
{
    public class SerializationTestSuite
    {
        private SmallObjectWithStrings sampleStringObject;
        private SmallObjectWithStringsDataContract dataContractObject;
        private SmallObjectWithStringsBond bondObject;
        private JsonSerializer jsonSerializer;
        private SmallObjectWithStringsJson jsonObject;
        private DataContractSerializer dataContractSerializer;
        private SmallObjectWithStringsProtobuf protoObject;
        private SmallObjectWithStringsSerializable sampleStringObjectSerializable;
        private IAvroSerializer<SmallObjectWithStringsDataContract> avroSerializer;

        [Setup]
        public void GenerateObject()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.AddConditionalObjectMapper().Where((s, d) => d.Name.Contains(s.Name));
            });

            this.sampleStringObject = new SmallObjectWithStrings
            {
                String1 = "123",
                String2 = "使用下列语言搜索设置 · 网络历史记录. 谷歌. 高级搜索语言工具. 谷歌中国换新家g.cn跳转至サービスをご利用になる際には、お客様の情報を安心して Google にお任せください。このプライバシー ポリシーは、Google が収集するデータ、データを収集する理由、Google でのデータの取り扱いについて理解を深めていただくためのものです。重要な内容ですので、必ずお読みいただくようお願いいたします。お客様の情報の管理やプライバシーとセキュリティの保護に関しては、[アカウント情報] で設定できます。",
                String3 = "\r\n\t\r\n\t\r\n\t\r\n\t\r\n\t\r\n\t",
                String4 = null
            };

            var mapper = config.CreateMapper();
            this.sampleStringObjectSerializable = mapper.Map<SmallObjectWithStringsSerializable>(this.sampleStringObject);
            this.dataContractObject = mapper.Map<SmallObjectWithStringsDataContract>(this.sampleStringObject);
            this.bondObject = mapper.Map<SmallObjectWithStringsBond>(this.sampleStringObject);
            this.jsonObject = mapper.Map<SmallObjectWithStringsJson>(this.sampleStringObject);

            this.jsonSerializer = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore };

            this.dataContractSerializer = new DataContractSerializer(typeof(SmallObjectWithStringsDataContract));
            this.avroSerializer = Microsoft.Hadoop.Avro.AvroSerializer.Create<SmallObjectWithStringsDataContract>();
            // can't use automapper here because I have null
            // this.protoObject = mapper.Map<SmallObjectWithStringsProtobuf>(this.sampleStringObject);
            this.protoObject = new SmallObjectWithStringsProtobuf
            {
                String1 = this.sampleStringObject.String1,
                String2 = this.sampleStringObject.String2,
                String3 = this.sampleStringObject.String3
            };
        }

        [Benchmark]
        public byte[] NewtonsoftJsonReusedSerializer()
        {
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms))
            using (var jw = new JsonTextWriter(sw))
            {
                this.jsonSerializer.Serialize(jw, this.jsonObject);
                jw.Flush();
                return ms.ToArray();
            }
        }

        [Benchmark]
        public string NewtonsoftJsonGenericSerializer()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this.sampleStringObject);
        }

        [Benchmark]
        public string NewtonsoftJsonDataContract()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this.dataContractObject);
        }

        [Benchmark]
        public byte[] Xml()
        {
            using (var ms = new MemoryStream())
            {
                this.dataContractSerializer.WriteObject(ms, this.dataContractObject);
                return ms.ToArray();
            }
        }

        [Benchmark]
        public int BondUnsafe()
        {
            var output = new OutputBuffer();
            var writer = new CompactBinaryWriter<OutputBuffer>(output);

            Serialize.To(writer, this.bondObject);

            return output.Data.Count;
        }

        [Benchmark]
        public byte[] Proto3()
        {
            using (var ms = new MemoryStream())
            {
                this.protoObject.WriteTo(ms);
                return ms.ToArray();
            }
        }

        [Benchmark]
        public byte[] BinaryFormatter()
        {
            using (var ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, this.sampleStringObjectSerializable);
                return ms.ToArray();
            }
        }

        [Benchmark]
        public byte[] Avro()
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var binaryEncoder = new BinaryEncoder(ms))
                    {
                        this.avroSerializer.Serialize(binaryEncoder, this.dataContractObject);
                        return ms.ToArray();
                    }
                }

            }
            catch
            {
                // avro does not handle nulls
                return new byte[0];
            }
         }
    }
}
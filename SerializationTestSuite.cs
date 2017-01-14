using AutoMapper;
using AutoMapper.Mappers;
using BenchmarkDotNet.Attributes;
using Bond;
using Bond.IO.Unsafe;
using Bond.Protocols;
using Google.Protobuf;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Hadoop.Avro;
using System;
using System.Collections.Generic;
using SerializationPerfTest.BiggerObject;
using MsgPack.Serialization;

namespace SerializationPerfTest
{
    public class SerializationTestSuite<TBase, TContract, TBond, TProtobuf, TPoco> where TProtobuf : IMessage
    {
        private TBase sampleStringObject;
        private TContract dataContractObject;
        private TBond bondObject;
        private JsonSerializer jsonSerializer;
        private TBase jsonObject;
        private DataContractSerializer dataContractSerializer;
        private IMessage protoObject;
        private TPoco sampleStringObjectSerializable;
        private IAvroSerializer<TContract> avroSerializer;

        private static Dictionary<Type, object> factories = new Dictionary<Type, object>() {
            {
                typeof(SmallObjectWithStrings), new SmallObjectWithStrings
                {
                    String1 = "123",
                    String2 = "使用下列语言搜索设置 · 网络历史记录. 谷歌. 高级搜索语言工具. 谷歌中国换新家g.cn跳转至サービスをご利用になる際には、お客様の情報を安心して Google にお任せください。このプライバシー ポリシーは、Google が収集するデータ、データを収集する理由、Google でのデータの取り扱いについて理解を深めていただくためのものです。重要な内容ですので、必ずお読みいただくようお願いいたします。お客様の情報の管理やプライバシーとセキュリティの保護に関しては、[アカウント情報] で設定できます。",
                    String3 = "\r\n\t\r\n\t\r\n\t\r\n\t\r\n\t\r\n\t",
                    String4 = null
                }
            },
            {
                typeof(BiggerObjectWithBytes),
                new BiggerObjectWithBytes {
                    Uri = new Uri("https://www.walmart.com/store/4340/"),
                    Header = @"
accept-ranges:bytes
cache-control:max-age=0, no-cache, no-store
content-encoding:gzip
content-length:53638
content-type:text/html;charset=UTF-8
date:Tue, 10 Jan 2017 01:35:54 GMT
expires:Tue, 10 Jan 2017 01:35:54 GMT
last-modified:Mon, 09 Jan 2017 17:35:54 GMT
logmon_top_tx_id:8f63b5cc-af488-15986046d90000
origin-cc:no-cache,no-store,max-age=0
origin-ex:Thu, 01 Jan 1970 00:00:00 GMT
pragma:no-cache
set-cookie:akavpau_p9=1484012754~id=d7a57f9b265a90c011fc1ff99652abc3; path=/
set-cookie:bstc=XHQg2HdmFP7QvQl75MvtYw;Path=/;Domain=.walmart.com;Expires=Tue, 10 Jan 2017 02:05:54 GMT;Max-Age=1800
set-cookie:akavpau_p9=1484012754~id=d7a57f9b265a90c011fc1ff99652abc3; path=/
set-cookie:com.wm.reflector=""reflectorid:19943950091684260293@lastupd:1484012154257@firstcreate:1483930865349""; Version=1; Domain=.walmart.com; Max-Age=315360000; Expires=Fri, 08-Jan-2027 01:35:54 GMT; Path=/
set-cookie:AID=wmlspartner%3Dlw9MynSeamY%3Areflectorid%3D19943950091684260293%3Alastupd%3D1484012154257; Version=1; Domain=walmart.com; Path=/; HttpOnly
set-cookie:vtc=emhcptv92Ru2oVHY_Pa11k;Path=/;Domain=.walmart.com;Expires=Sun, 10 Jan 2027 13:35:54 GMT;Max-Age=315576000
status:200
vary:Accept-Encoding
wm_qos.correlation_id:8f63b5cc-af488-15986046d90000,8f63b5cc-af488-15986046d90000
x-ak-protocol:h2
x-frame-options:DENY
x-tb:1
x-tb-debug-routing-tenant:defaultStorePath=/ store / 4340 /
x - tb - electrodesupportedbrowser:",
                    Content = File.ReadAllBytes(@"BiggerObject\index.html")
                }
            }
        };

        private static Dictionary<Tuple<Type, Type>, Func<object, object>> manualMapFromTypeToType = new Dictionary<Tuple<Type, Type>, Func<object, object>>()
        {
            {
                Tuple.Create(typeof(SmallObjectWithStrings), typeof(SmallObjectWithStringsProtobuf)),
                s =>
                {
                        var obj = (SmallObjectWithStrings)s;
                        return new SmallObjectWithStringsProtobuf
                        {
                            String1 = obj.String1,
                            String2 = obj.String2,
                            String3 = obj.String3
                        };
                }
            },
            {
                Tuple.Create(typeof(BiggerObjectWithBytes), typeof(BiggerObjectWithBytesProtobuf)),
                s =>
                {
                        var obj = (BiggerObjectWithBytes)s;
                        return new BiggerObjectWithBytesProtobuf
                        {
                            Uri = obj.Uri.ToString(),
                            Header = obj.Header,
                            Content = ByteString.CopyFrom(obj.Content)
                        };
                }
            },
        };
        private MessagePackSerializer<TBase> messagePackSerializer;
        private Serializer<CompactBinaryWriter<OutputBuffer>> compactBondSerializer;
        private Serializer<SimpleBinaryWriter<OutputBuffer>> simpleBondSerializer;
        private OutputBuffer outputBuffer = new OutputBuffer();

        [Setup]
        public void GenerateObject()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddConditionalObjectMapper().Where((s, d) => d.Name.Contains(s.Name));
            });

            this.sampleStringObject = (TBase)factories[typeof(TBase)];

            var mapper = config.CreateMapper();
            this.sampleStringObjectSerializable = mapper.Map<TPoco>(this.sampleStringObject);
            this.dataContractObject = mapper.Map<TContract>(this.sampleStringObject);
            this.bondObject = mapper.Map<TBond>(this.sampleStringObject);
            this.jsonObject = mapper.Map<TBase>(this.sampleStringObject);

            this.jsonSerializer = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore };

            this.dataContractSerializer = new DataContractSerializer(typeof(TContract));
            this.avroSerializer = Microsoft.Hadoop.Avro.AvroSerializer.Create<TContract>();
            this.messagePackSerializer = MessagePackSerializer.Get<TBase>();
            this.compactBondSerializer = new Serializer<CompactBinaryWriter<OutputBuffer>>(typeof(TBond));
            this.simpleBondSerializer = new Serializer<SimpleBinaryWriter<OutputBuffer>>(typeof(TBond));

            // can't use automapper here because I have null
            // this.protoObject = mapper.Map<SmallObjectWithStringsProtobuf>(this.sampleStringObject);
            Func<object, object> map;
            if (manualMapFromTypeToType.TryGetValue(Tuple.Create<Type, Type>(typeof(TBase), typeof(TProtobuf)), out map))
                this.protoObject = (IMessage)map(this.sampleStringObject);
            else
            {
                mapper.Map<TProtobuf>(this.sampleStringObject);
            }
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
        public byte[] BondUnsafeCompact()
        {
            var output = new OutputBuffer();
            var writer = new CompactBinaryWriter<OutputBuffer>(output);

            Serialize.To(writer, this.bondObject);

            return output.Data.Array;
        }

        [Benchmark]
        public byte[] BondUnsafeSimple()
        {
            var output = new OutputBuffer();
            var writer = new SimpleBinaryWriter<OutputBuffer>(output);

            Serialize.To(writer, this.bondObject);

            return output.Data.Array;
        }

        [Benchmark]
        public byte[] BondUnsafeCompactReused()
        {
            var output = new OutputBuffer();
            var writer = new CompactBinaryWriter<OutputBuffer>(output);

            this.compactBondSerializer.Serialize(this.bondObject, writer);

            return output.Data.Array;
        }

        [Benchmark]
        public byte[] BondUnsafeSimpleReused()
        {
            var output = new OutputBuffer();
            var writer = new SimpleBinaryWriter<OutputBuffer>(output);

            this.simpleBondSerializer.Serialize(this.bondObject, writer);

            return output.Data.Array;
        }

        [Benchmark]
        public byte[] BondUnsafeSimpleReusedBuffer()
        {
            this.outputBuffer.Position = 0;
            var writer = new SimpleBinaryWriter<OutputBuffer>(this.outputBuffer);

            this.simpleBondSerializer.Serialize(this.bondObject, writer);

            return this.outputBuffer.Data.Array;
        }
        // [Benchmark]
        // public byte[] BondUnsafeJson()
        // {
        //     using (var ms = new MemoryStream())
        //     {
        //         var writer = new SimpleJsonWriter();
        // 
        //         Serialize.To(writer, this.bondObject);
        //         return ms.ToArray();
        //     }
        // }



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
        public byte[] MessagePack()
        {
            using (var byteStream = new MemoryStream())
            {
                this.messagePackSerializer.Pack(byteStream, this.sampleStringObject);
                return byteStream.ToArray();
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
using AutoMapper;
using AutoMapper.Mappers;
using BenchmarkDotNet.Attributes;

namespace SerializationPerfTest
{
    public class SerializationTestSuite
    {
        private SmallObjectWithStrings sampleStringObject;
        private SmallObjectWithStringsDataContract dataContractObject;
        private SmallObjectWithStringsBond bondObject;

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
            this.dataContractObject = mapper.Map<SmallObjectWithStringsDataContract>(this.sampleStringObject);
            this.bondObject = mapper.Map<SmallObjectWithStringsBond>(this.sampleStringObject);
        }

        [Benchmark]
        public void NewtonsoftJsonReusedSerializer()
        {

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
        public void Xml()
        {

        }
    }
}
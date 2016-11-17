using GeoCoder;
using GeoCoder.Yandex;
using NUnit.Framework;

namespace Tests.Yandex
{
    [TestFixture]
    public class GeoCoderResponseTests
    {
        [Test]
        public void Should_deserialize()
        {
            var json = @"{
  ""response"": {
    ""GeoObjectCollection"": {
      ""metaDataProperty"": {
        ""GeocoderResponseMetaData"": {
          ""request"": ""Воронеж, Московский пр., 129/1"",
          ""found"": ""1"",
          ""results"": ""10""
        }
      },
      ""featureMember"": [
        {
          ""GeoObject"": {
            ""metaDataProperty"": {
              ""GeocoderMetaData"": {
                ""kind"": ""house"",
                ""text"": ""Россия, Воронеж, Московский проспект, 129/1"",
                ""precision"": ""exact"",
                ""AddressDetails"": {
                  ""Country"": {
                    ""AddressLine"": ""Воронеж, Московский проспект, 129/1"",
                    ""CountryNameCode"": ""RU"",
                    ""CountryName"": ""Россия"",
                    ""AdministrativeArea"": {
                      ""AdministrativeAreaName"": ""Воронежская область"",
                      ""SubAdministrativeArea"": {
                        ""SubAdministrativeAreaName"": ""городской округ Воронеж"",
                        ""Locality"": {
                          ""LocalityName"": ""Воронеж"",
                          ""Thoroughfare"": {
                            ""ThoroughfareName"": ""Московский проспект"",
                            ""Premise"": {
                              ""PremiseNumber"": ""129/1""
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            },
            ""description"": ""Воронеж, Россия"",
            ""name"": ""Московский проспект, 129/1"",
            ""boundedBy"": {
              ""Envelope"": {
                ""lowerCorner"": ""39.173427 51.715197"",
                ""upperCorner"": ""39.181638 51.720297""
              }
            },
            ""Point"": {
              ""pos"": ""39.177533 51.717747""
            }
          }
        }
      ]
    }
  }
}";

            var response = json.JsonDeserialize<GeoCoderApiResponse>();

            Assert.IsNotNull(response.response);
            Assert.IsNotNull(response.response.GeoObjectCollection);
            Assert.IsNotNull(response.response.GeoObjectCollection.featureMember);
            Assert.AreEqual(1, response.response.GeoObjectCollection.featureMember.Length);
            Assert.IsNotNull(response.response.GeoObjectCollection.featureMember[0].GeoObject);
            Assert.IsNotNull(response.response.GeoObjectCollection.featureMember[0].GeoObject.Point);
            Assert.IsNotNull(response.response.GeoObjectCollection.featureMember[0].GeoObject.Point.pos);
            Assert.AreEqual("39.177533 51.717747", response.response.GeoObjectCollection.featureMember[0].GeoObject.Point.pos);
            Assert.AreEqual("Воронеж", response.response.GeoObjectCollection.featureMember[0].LocalityName());
            Assert.AreEqual("Воронежская область", response.response.GeoObjectCollection.featureMember[0].AdministrativeAreaName());
            Assert.AreEqual("городской округ Воронеж", response.response.GeoObjectCollection.featureMember[0].SubAdministrativeAreaName());
            Assert.AreEqual("exact", response.response.GeoObjectCollection.featureMember[0].Precision());
            Assert.AreEqual("Воронеж, Московский проспект, 129/1", response.response.GeoObjectCollection.featureMember[0].Text());
        }

        [Test]
        public void Should_deserialize1()
        {
            var json = @"{""response"":{""GeoObjectCollection"":{""metaDataProperty"":{""GeocoderResponseMetaData"":{""request"":""Мытищинский район, ТПЗ АШАН, вл. 3"",""found"":""1"",""results"":""10"",""boundedBy"":{""Envelope"":{""lowerCorner"":""-0.002497 -0.002496"",""upperCorner"":""0.002497 0.002496""}}}},""featureMember"":[{""GeoObject"":{""metaDataProperty"":{""GeocoderMetaData"":{""kind"":""house"",""text"":""Россия, Московская область, городской округ Мытищи, с1вл3"",""precision"":""near"",""AddressDetails"":{""Country"":{""AddressLine"":""Московская область, городской округ Мытищи, с1вл3"",""CountryNameCode"":""RU"",""CountryName"":""Россия"",""AdministrativeArea"":{""AdministrativeAreaName"":""Московская область"",""SubAdministrativeArea"":{""SubAdministrativeAreaName"":""городской округ Мытищи"",""Locality"":""""}}}}}},""description"":""Московская область, Россия"",""name"":""городской округ Мытищи, с1вл3"",""boundedBy"":{""Envelope"":{""lowerCorner"":""37.53623 56.091517"",""upperCorner"":""37.544441 56.096107""}},""Point"":{""pos"":""37.540335 56.093812""}}}]}}}";

            var response = json.JsonDeserialize<GeoCoderApiResponse>();

            Assert.IsNotNull(response.response);
            Assert.IsNotNull(response.response.GeoObjectCollection);
            Assert.IsNotNull(response.response.GeoObjectCollection.featureMember);
            Assert.AreEqual(1, response.response.GeoObjectCollection.featureMember.Length);
            Assert.IsNotNull(response.response.GeoObjectCollection.featureMember[0].GeoObject);
            Assert.IsNotNull(response.response.GeoObjectCollection.featureMember[0].GeoObject.Point);
            Assert.IsNotNull(response.response.GeoObjectCollection.featureMember[0].GeoObject.Point.pos);
            Assert.AreEqual("37.540335 56.093812", response.response.GeoObjectCollection.featureMember[0].GeoObject.Point.pos);
            Assert.AreEqual(string.Empty, response.response.GeoObjectCollection.featureMember[0].LocalityName());
            Assert.AreEqual("Московская область", response.response.GeoObjectCollection.featureMember[0].AdministrativeAreaName());
            Assert.AreEqual("городской округ Мытищи", response.response.GeoObjectCollection.featureMember[0].SubAdministrativeAreaName());
            Assert.AreEqual("near", response.response.GeoObjectCollection.featureMember[0].Precision());
            Assert.AreEqual("Московская область, городской округ Мытищи, с1вл3", response.response.GeoObjectCollection.featureMember[0].Text());
        }

        [Test]
        public void Should_deserialize2()
        {
            var json = @"{
  ""response"": {
    ""GeoObjectCollection"": {
      ""metaDataProperty"": {
        ""GeocoderResponseMetaData"": {
          ""request"": ""Москва, ул.Маршала Бирюзова, д.32"",
          ""found"": ""2"",
          ""results"": ""10"",
          ""boundedBy"": {
            ""Envelope"": {
              ""lowerCorner"": ""-0.002497 -0.002496"",
              ""upperCorner"": ""0.002497 0.002496""
            }
          }
        }
      },
      ""featureMember"": [
        {
          ""GeoObject"": {
            ""metaDataProperty"": { ""SearchMetaData"": { ""logId"": ""dHlwZT1nZW87YWRkcmVzcz3QoNC+0YHRgdC40Y8sINCc0L7RgdC60LLQsCwg0YPQu9C40YbQsCDQnNCw0YDRiNCw0LvQsCDQkdC40YDRjtC30L7QstCwLCAzMi8zNy40ODMwMjMsNTUuNzk5NTg4"" } },
            ""description"": ""Москва, Россия"",
            ""name"": ""улица Маршала Бирюзова, 32"",
            ""boundedBy"": {
              ""Envelope"": {
                ""lowerCorner"": ""37.478917 55.797275"",
                ""upperCorner"": ""37.487128 55.8019""
              }
            },
            ""Point"": { ""pos"": ""37.483023 55.799588"" }
          }
        },
        {
          ""GeoObject"": {
            ""metaDataProperty"": { ""SearchMetaData"": { ""logId"": ""dHlwZT1nZW87YWRkcmVzcz3QoNC+0YHRgdC40Y8sINCc0L7RgdC60L7QstGB0LrQsNGPINC+0LHQu9Cw0YHRgtGMLCDQntC00LjQvdGG0L7QstC+LCDRg9C70LjRhtCwINCc0LDRgNGI0LDQu9CwINCR0LjRgNGO0LfQvtCy0LAsIDMw0JAvMzcuMjY2NzYyLDU1LjY4MTk5MQ=="" } },
            ""description"": ""Одинцово, Московская область, Россия"",
            ""name"": ""улица Маршала Бирюзова, 30А"",
            ""boundedBy"": {
              ""Envelope"": {
                ""lowerCorner"": ""37.262657 55.679671"",
                ""upperCorner"": ""37.270868 55.68431""
              }
            },
            ""Point"": { ""pos"": ""37.266762 55.681991"" }
          }
        }
      ]
    }
  }
}";

            var response = json.JsonDeserialize<GeoCoderApiResponse>();

            Assert.IsNotNull(response.response);
            Assert.IsNotNull(response.response.GeoObjectCollection);
            Assert.IsNotNull(response.response.GeoObjectCollection.featureMember);
            Assert.AreEqual(2, response.response.GeoObjectCollection.featureMember.Length);
            Assert.IsNotNull(response.response.GeoObjectCollection.featureMember[0].GeoObject);
            Assert.IsNotNull(response.response.GeoObjectCollection.featureMember[0].GeoObject.Point);
            Assert.IsNotNull(response.response.GeoObjectCollection.featureMember[0].GeoObject.Point.pos);
            Assert.AreEqual("37.483023 55.799588", response.response.GeoObjectCollection.featureMember[0].GeoObject.Point.pos);
            Assert.AreEqual(string.Empty, response.response.GeoObjectCollection.featureMember[0].LocalityName());
            Assert.AreEqual(string.Empty, response.response.GeoObjectCollection.featureMember[0].AdministrativeAreaName());
            Assert.AreEqual(string.Empty, response.response.GeoObjectCollection.featureMember[0].SubAdministrativeAreaName());
            Assert.AreEqual(string.Empty, response.response.GeoObjectCollection.featureMember[0].Precision());
            Assert.AreEqual("улица Маршала Бирюзова, 32", response.response.GeoObjectCollection.featureMember[0].Text());
        }
    }
}

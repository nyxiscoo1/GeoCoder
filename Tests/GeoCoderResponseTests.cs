﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoCoder;
using NUnit.Framework;

namespace Tests
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
        }
    }
}

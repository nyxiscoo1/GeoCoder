using GeoCoder;
using GeoCoder.Google;
using NUnit.Framework;

namespace Tests.Google
{
    [TestFixture]
    public class GeoCoderResponseTests
    {
        [Test]
        public void Should_deserialize()
        {
            var json = @"{
  ""results"": [
    {
      ""address_components"": [
        {
          ""long_name"": ""27"",
          ""short_name"": ""27"",
          ""types"": [
            ""street_number""
          ]
        },
        {
          ""long_name"": ""Leningradskaya ulitsa"",
          ""short_name"": ""Leningradskaya ul."",
          ""types"": [
            ""route""
          ]
        },
        {
          ""long_name"": ""Kazan"",
          ""short_name"": ""Kazan"",
          ""types"": [
            ""locality"",
            ""political""
          ]
        },
        {
          ""long_name"": ""gorod Kazan'"",
          ""short_name"": ""g. Kazan'"",
          ""types"": [
            ""administrative_area_level_2"",
            ""political""
          ]
        },
        {
          ""long_name"": ""Respublika Tatarstan"",
          ""short_name"": ""Respublika Tatarstan"",
          ""types"": [
            ""administrative_area_level_1"",
            ""political""
          ]
        },
        {
          ""long_name"": ""Russia"",
          ""short_name"": ""RU"",
          ""types"": [
            ""country"",
            ""political""
          ]
        },
        {
          ""long_name"": ""420127"",
          ""short_name"": ""420127"",
          ""types"": [
            ""postal_code""
          ]
        }
      ],
      ""formatted_address"": ""Leningradskaya ul., 27, Kazan, Respublika Tatarstan, Russia, 420127"",
      ""geometry"": {
        ""bounds"": {
          ""northeast"": {
            ""lat"": 55.8626352,
            ""lng"": 49.0856635
          },
          ""southwest"": {
            ""lat"": 55.8626322,
            ""lng"": 49.0856455
          }
        },
        ""location"": {
          ""lat"": 55.8626352,
          ""lng"": 49.0856455
        },
        ""location_type"": ""RANGE_INTERPOLATED"",
        ""viewport"": {
          ""northeast"": {
            ""lat"": 55.8639826802915,
            ""lng"": 49.0870034802915
          },
          ""southwest"": {
            ""lat"": 55.8612847197085,
            ""lng"": 49.0843055197085
          }
        }
      },
      ""partial_match"": true,
      ""place_id"": ""EmbRg9C7LiDQm9C10L3QuNC90LPRgNCw0LTRgdC60LDRjywgMjcsINCa0LDQt9Cw0L3RjCwg0KDQtdGB0L8uINCi0LDRgtCw0YDRgdGC0LDQvSwg0KDQvtGB0YHQuNGPLCA0MjAxMjc"",
      ""types"": [
        ""street_address""
      ]
    }
  ],
  ""status"": ""OK""
}";

            var response = json.JsonDeserialize<GeoCodeApiResponse>();

            Assert.AreEqual("OK", response.status);
            Assert.IsNull(response.error_message);
            Assert.IsNotNull(response.results);
            Assert.AreEqual(1, response.results.Length);
            Assert.IsNotNull(response.results[0].geometry);
            Assert.IsNotNull(response.results[0].geometry.location);
            Assert.AreEqual("55.8626352", response.results[0].geometry.location.lat);
            Assert.AreEqual("49.0856455", response.results[0].geometry.location.lng);

            Assert.IsNotNull(response.results[0].types);
            Assert.AreEqual(1, response.results[0].types.Length);
            Assert.AreEqual("street_address", response.results[0].types[0]);
        }
    }
}

﻿using GeoCoder.DaData;
using NUnit.Framework;

namespace Tests.DaData
{
    [TestFixture]
    public class DaDataAddressResponseTests
    {
        [Test]
        public void Should_parse()
        {
            string json = @"[
  {
    ""source"": ""Красногорский район, 7-ой км Пятницкого шоссе, владение 2"",
    ""result"": ""Ленинградская обл, Всеволожский р-н, массив 39 км Мурманского шоссе, снт Двигатель, линия 7-я, влд 2"",
    ""postal_code"": ""188640"",
    ""country"": ""Россия"",
    ""region_fias_id"": ""6d1ebb35-70c6-4129-bd55-da3969658f5d"",
    ""region_kladr_id"": ""4700000000000"",
    ""region_with_type"": ""Ленинградская обл"",
    ""region_type"": ""обл"",
    ""region_type_full"": ""область"",
    ""region"": ""Ленинградская"",
    ""area_fias_id"": ""db8758d6-b8ea-453d-a75f-8f2d3ff49fb0"",
    ""area_kladr_id"": ""4700500000000"",
    ""area_with_type"": ""Всеволожский р-н"",
    ""area_type"": ""р-н"",
    ""area_type_full"": ""район"",
    ""area"": ""Всеволожский"",
    ""city_fias_id"": ""4351e8e4-2aef-4f8d-aad9-e5460f38caef"",
    ""city_kladr_id"": ""4700502800000"",
    ""city_with_type"": ""массив 39 км Мурманского шоссе"",
    ""city_type"": ""массив"",
    ""city_type_full"": ""массив"",
    ""city"": ""39 км Мурманского шоссе"",
    ""city_area"": null,
    ""city_district_fias_id"": null,
    ""city_district_kladr_id"": null,
    ""city_district_with_type"": null,
    ""city_district_type"": null,
    ""city_district_type_full"": null,
    ""city_district"": null,
    ""settlement_fias_id"": ""5b68c2df-c3e8-4246-8963-f60a56000e93"",
    ""settlement_kladr_id"": ""4700502800200"",
    ""settlement_with_type"": ""снт Двигатель"",
    ""settlement_type"": ""снт"",
    ""settlement_type_full"": ""садовое неком-е товарищество"",
    ""settlement"": ""Двигатель"",
    ""street_fias_id"": ""0c726042-ff7a-49bc-b4bc-07e3d4b98622"",
    ""street_kladr_id"": ""47005028002000700"",
    ""street_with_type"": ""линия 7-я"",
    ""street_type"": ""линия"",
    ""street_type_full"": ""линия"",
    ""street"": ""7-я"",
    ""house_fias_id"": null,
    ""house_kladr_id"": null,
    ""house_type"": ""влд"",
    ""house_type_full"": ""владение"",
    ""house"": ""2"",
    ""block_type"": null,
    ""block_type_full"": null,
    ""block"": null,
    ""flat_type"": null,
    ""flat_type_full"": null,
    ""flat"": null,
    ""flat_area"": null,
    ""square_meter_price"": null,
    ""flat_price"": null,
    ""postal_box"": null,
    ""fias_id"": ""0c726042-ff7a-49bc-b4bc-07e3d4b98622"",
    ""fias_level"": ""7"",
    ""kladr_id"": ""47005028002000700"",
    ""capital_marker"": ""0"",
    ""okato"": ""41212563000"",
    ""oktmo"": ""41612163051"",
    ""tax_office"": ""4703"",
    ""tax_office_legal"": null,
    ""timezone"": ""UTC+3"",
    ""geo_lat"": null,
    ""geo_lon"": null,
    ""beltway_hit"": null,
    ""beltway_distance"": null,
    ""qc_geo"": 5,
    ""qc_complete"": 9,
    ""qc_house"": 10,
    ""qc"": 1,
    ""unparsed_parts"": ""КРАСНОГОРСКИЙ, РАЙОН, ПЯТНИЦКОГО""
  }
]";
            var response = DaDataAddressResponse.Parse(json);

            Assert.AreEqual(1, response.Length);
            Assert.AreEqual("Ленинградская обл, Всеволожский р-н, массив 39 км Мурманского шоссе, снт Двигатель, линия 7-я, влд 2", response[0].result);
            Assert.AreEqual(1, response[0].qc);
        }

        [Test]
        public void Should_parse_multiple()
        {
            string json = @"[
  {
    ""source"": ""мск сухонска 11/-89"",
    ""result"": ""г Москва, ул Сухонская, д 11, кв 89"",
    ""postal_code"": ""127642"",
    ""country"": ""Россия"",
    ""region_fias_id"": ""0c5b2444-70a0-4932-980c-b4dc0d3f02b5"",
    ""region_kladr_id"": ""7700000000000"",
    ""region_with_type"": ""г Москва"",
    ""region_type"": ""г"",
    ""region_type_full"": ""город"",
    ""region"": ""Москва"",
    ""area_fias_id"": null,
    ""area_kladr_id"": null,
    ""area_with_type"": null,
    ""area_type"": null,
    ""area_type_full"": null,
    ""area"": null,
    ""city_fias_id"": null,
    ""city_kladr_id"": null,
    ""city_with_type"": null,
    ""city_type"": null,
    ""city_type_full"": null,
    ""city"": null,
    ""city_area"": ""Северо-восточный"",
    ""city_district_fias_id"": null,
    ""city_district_kladr_id"": null,
    ""city_district_with_type"": ""р-н Северное Медведково"",
    ""city_district_type"": ""р-н"",
    ""city_district_type_full"": ""район"",
    ""city_district"": ""Северное Медведково"",
    ""settlement_fias_id"": null,
    ""settlement_kladr_id"": null,
    ""settlement_with_type"": null,
    ""settlement_type"": null,
    ""settlement_type_full"": null,
    ""settlement"": null,
    ""street_fias_id"": ""95dbf7fb-0dd4-4a04-8100-4f6c847564b5"",
    ""street_kladr_id"": ""77000000000283600"",
    ""street_with_type"": ""ул Сухонская"",
    ""street_type"": ""ул"",
    ""street_type_full"": ""улица"",
    ""street"": ""Сухонская"",
    ""house_fias_id"": ""5ee84ac0-eb9a-4b42-b814-2f5f7c27c255"",
    ""house_kladr_id"": ""7700000000028360004"",
    ""house_type"": ""д"",
    ""house_type_full"": ""дом"",
    ""house"": ""11"",
    ""block_type"": null,
    ""block_type_full"": null,
    ""block"": null,
    ""flat_type"": ""кв"",
    ""flat_type_full"": ""квартира"",
    ""flat"": ""89"",
    ""flat_area"": ""34.6"",
    ""square_meter_price"": ""198113"",
    ""flat_price"": ""6854710"",
    ""postal_box"": null,
    ""fias_id"": ""5ee84ac0-eb9a-4b42-b814-2f5f7c27c255"",
    ""fias_level"": ""8"",
    ""kladr_id"": ""7700000000028360004"",
    ""capital_marker"": ""0"",
    ""okato"": ""45280583000"",
    ""oktmo"": ""45362000"",
    ""tax_office"": ""7715"",
    ""tax_office_legal"": null,
    ""timezone"": ""UTC+3"",
    ""geo_lat"": ""55.8783151"",
    ""geo_lon"": ""37.65363"",
    ""beltway_hit"": ""IN_MKAD"",
    ""beltway_distance"": null,
    ""qc_geo"": 0,
    ""qc_complete"": 0,
    ""qc_house"": 2,
    ""qc"": 0,
    ""unparsed_parts"": null
  },
  {
    ""source"": ""Саратов, 3-я Дачная, ул."",
    ""result"": ""г Саратов, ул Дачная 3-я"",
    ""postal_code"": ""410009"",
    ""country"": ""Россия"",
    ""region_fias_id"": ""df594e0e-a935-4664-9d26-0bae13f904fe"",
    ""region_kladr_id"": ""6400000000000"",
    ""region_with_type"": ""Саратовская обл"",
    ""region_type"": ""обл"",
    ""region_type_full"": ""область"",
    ""region"": ""Саратовская"",
    ""area_fias_id"": null,
    ""area_kladr_id"": null,
    ""area_with_type"": null,
    ""area_type"": null,
    ""area_type_full"": null,
    ""area"": null,
    ""city_fias_id"": ""bf465fda-7834-47d5-986b-ccdb584a85a6"",
    ""city_kladr_id"": ""6400000100000"",
    ""city_with_type"": ""г Саратов"",
    ""city_type"": ""г"",
    ""city_type_full"": ""город"",
    ""city"": ""Саратов"",
    ""city_area"": null,
    ""city_district_fias_id"": null,
    ""city_district_kladr_id"": null,
    ""city_district_with_type"": ""р-н Ленинский"",
    ""city_district_type"": ""р-н"",
    ""city_district_type_full"": ""район"",
    ""city_district"": ""Ленинский"",
    ""settlement_fias_id"": null,
    ""settlement_kladr_id"": null,
    ""settlement_with_type"": null,
    ""settlement_type"": null,
    ""settlement_type_full"": null,
    ""settlement"": null,
    ""street_fias_id"": ""421a7fc2-8e38-4331-ad2a-10e37d0f35a2"",
    ""street_kladr_id"": ""64000001000508200"",
    ""street_with_type"": ""ул Дачная 3-я"",
    ""street_type"": ""ул"",
    ""street_type_full"": ""улица"",
    ""street"": ""Дачная 3-я"",
    ""house_fias_id"": null,
    ""house_kladr_id"": null,
    ""house_type"": null,
    ""house_type_full"": null,
    ""house"": null,
    ""block_type"": null,
    ""block_type_full"": null,
    ""block"": null,
    ""flat_type"": null,
    ""flat_type_full"": null,
    ""flat"": null,
    ""flat_area"": null,
    ""square_meter_price"": null,
    ""flat_price"": null,
    ""postal_box"": null,
    ""fias_id"": ""421a7fc2-8e38-4331-ad2a-10e37d0f35a2"",
    ""fias_level"": ""7"",
    ""kladr_id"": ""64000001000508200"",
    ""capital_marker"": ""2"",
    ""okato"": ""63401376000"",
    ""oktmo"": ""63701000"",
    ""tax_office"": ""6453"",
    ""tax_office_legal"": null,
    ""timezone"": ""UTC+3"",
    ""geo_lat"": ""51.5772851"",
    ""geo_lon"": ""45.9736297"",
    ""beltway_hit"": null,
    ""beltway_distance"": null,
    ""qc_geo"": 2,
    ""qc_complete"": 4,
    ""qc_house"": 10,
    ""qc"": 0,
    ""unparsed_parts"": null
  },
  {
    ""source"": ""Горно-Алтайск, Коммунистический пр., 11"",
    ""result"": ""г Горно-Алтайск, пр-кт Коммунистический, д 11"",
    ""postal_code"": ""649000"",
    ""country"": ""Россия"",
    ""region_fias_id"": ""5c48611f-5de6-4771-9695-7e36a4e7529d"",
    ""region_kladr_id"": ""0400000000000"",
    ""region_with_type"": ""Респ Алтай"",
    ""region_type"": ""Респ"",
    ""region_type_full"": ""республика"",
    ""region"": ""Алтай"",
    ""area_fias_id"": null,
    ""area_kladr_id"": null,
    ""area_with_type"": null,
    ""area_type"": null,
    ""area_type_full"": null,
    ""area"": null,
    ""city_fias_id"": ""0839d751-b940-4d3d-afb6-5df03fdd7791"",
    ""city_kladr_id"": ""0400000100000"",
    ""city_with_type"": ""г Горно-Алтайск"",
    ""city_type"": ""г"",
    ""city_type_full"": ""город"",
    ""city"": ""Горно-Алтайск"",
    ""city_area"": null,
    ""city_district_fias_id"": null,
    ""city_district_kladr_id"": null,
    ""city_district_with_type"": null,
    ""city_district_type"": null,
    ""city_district_type_full"": null,
    ""city_district"": null,
    ""settlement_fias_id"": null,
    ""settlement_kladr_id"": null,
    ""settlement_with_type"": null,
    ""settlement_type"": null,
    ""settlement_type_full"": null,
    ""settlement"": null,
    ""street_fias_id"": ""28284954-1913-405e-9207-f0639bc2e603"",
    ""street_kladr_id"": ""04000001000004400"",
    ""street_with_type"": ""пр-кт Коммунистический"",
    ""street_type"": ""пр-кт"",
    ""street_type_full"": ""проспект"",
    ""street"": ""Коммунистический"",
    ""house_fias_id"": ""7350ed55-081d-43d0-b0e6-80086b191f65"",
    ""house_kladr_id"": ""0400000100000440573"",
    ""house_type"": ""д"",
    ""house_type_full"": ""дом"",
    ""house"": ""11"",
    ""block_type"": null,
    ""block_type_full"": null,
    ""block"": null,
    ""flat_type"": null,
    ""flat_type_full"": null,
    ""flat"": null,
    ""flat_area"": ""18.3"",
    ""square_meter_price"": null,
    ""flat_price"": null,
    ""postal_box"": null,
    ""fias_id"": ""7350ed55-081d-43d0-b0e6-80086b191f65"",
    ""fias_level"": ""8"",
    ""kladr_id"": ""0400000100000440573"",
    ""capital_marker"": ""2"",
    ""okato"": ""84401000000"",
    ""oktmo"": ""84701000"",
    ""tax_office"": ""0400"",
    ""tax_office_legal"": null,
    ""timezone"": ""UTC+7"",
    ""geo_lat"": ""51.9581658"",
    ""geo_lon"": ""85.9593767"",
    ""beltway_hit"": null,
    ""beltway_distance"": null,
    ""qc_geo"": 0,
    ""qc_complete"": 5,
    ""qc_house"": 2,
    ""qc"": 0,
    ""unparsed_parts"": null
  }
]";
            var response = DaDataAddressResponse.Parse(json);

            Assert.AreEqual(3, response.Length);
            Assert.AreEqual("Ленинградская обл, Всеволожский р-н, массив 39 км Мурманского шоссе, снт Двигатель, линия 7-я, влд 2", response[0].result);
            Assert.AreEqual(1, response[0].qc);
        }
    }
}

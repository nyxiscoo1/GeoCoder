using System.Collections.Generic;

namespace GeoCoder.Yandex
{
    public class featureMember
    {
        public GeoObject GeoObject { get; set; }

        public string LocalityName()
        {
            return
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Locality"]?.AsString("LocalityName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["Locality"]?.AsString("LocalityName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["AdministrativeArea"]?["Locality"]?.AsString("LocalityName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["AdministrativeArea"]?["SubAdministrativeArea"]?["Locality"]?.AsString("LocalityName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["AdministrativeArea"]?["Locality"]?.AsString("LocalityName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["AdministrativeArea"]?["SubAdministrativeArea"]?["Locality"]?.AsString("LocalityName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["AdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["Locality"]?.AsString("LocalityName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["AdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["Locality"]?.AsString("LocalityName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["AdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["Locality"]?.AsString("LocalityName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["AdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["Locality"]?.AsString("LocalityName") ??
                string.Empty;

        }

        public string AdministrativeAreaName()
        {
            return
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["AdministrativeArea"]?.AsString("AdministrativeAreaName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["AdministrativeArea"]?.AsString("AdministrativeAreaName") ??
                string.Empty;
        }

        public string SubAdministrativeAreaName()
        {
            var subArea = GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["SubAdministrativeArea"] ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["SubAdministrativeArea"] ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["AdministrativeArea"]?["SubAdministrativeArea"] ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["AdministrativeArea"]?["SubAdministrativeArea"];

            var areas = new List<string>();

            while (subArea != null)
            {
                areas.Add(subArea.AsString("SubAdministrativeAreaName"));
                subArea = subArea["SubAdministrativeArea"];
            }

            if (areas.Count == 0)
                return string.Empty;

            return string.Join(", ", areas);
        }

        public string Precision()
        {
            return GeoObject.metaDataProperty["GeocoderMetaData"]?.AsString("precision") ?? string.Empty;
        }

        public string Text()
        {
            return (GeoObject.metaDataProperty["GeocoderMetaData"]?.AsString("text") ?? GeoObject.name ?? string.Empty).Replace("Россия, ", string.Empty);
        }
    }
}
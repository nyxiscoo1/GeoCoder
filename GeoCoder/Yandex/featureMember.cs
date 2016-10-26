using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace GeoCoder.Yandex
{
    public class featureMember
    {
        public GeoObject GeoObject { get; set; }

        public string LocalityName()
        {
            return
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Locality"]?.Value<string>("LocalityName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["Locality"]?.Value<string>("LocalityName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["AdministrativeArea"]?["Locality"]?.Value<string>("LocalityName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["AdministrativeArea"]?["SubAdministrativeArea"]?["Locality"]?.Value<string>("LocalityName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["AdministrativeArea"]?["Locality"]?.Value<string>("LocalityName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["AdministrativeArea"]?["SubAdministrativeArea"]?["Locality"]?.Value<string>("LocalityName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["AdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["Locality"]?.Value<string>("LocalityName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["AdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["Locality"]?.Value<string>("LocalityName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["AdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["Locality"]?.Value<string>("LocalityName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["AdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?["Locality"]?.Value<string>("LocalityName") ??
                string.Empty;

        }

        public string AdministrativeAreaName()
        {
            return
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["AdministrativeArea"]?.Value<string>("AdministrativeAreaName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["AdministrativeArea"]?.Value<string>("AdministrativeAreaName") ??
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
                areas.Add(subArea.Value<string>("SubAdministrativeAreaName"));
                subArea = subArea["SubAdministrativeArea"];
            }

            if (areas.Count == 0)
                return string.Empty;

            return string.Join(", ", areas);

            return
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["SubAdministrativeArea"]?.Value<string>("SubAdministrativeAreaName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["SubAdministrativeArea"]?.Value<string>("SubAdministrativeAreaName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["AdministrativeArea"]?["SubAdministrativeArea"]?.Value<string>("SubAdministrativeAreaName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["AdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?.Value<string>("SubAdministrativeAreaName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["AdministrativeArea"]?["SubAdministrativeArea"]?.Value<string>("SubAdministrativeAreaName") ??
                GeoObject.metaDataProperty["GeocoderMetaData"]?["AddressDetails"]?["Country"]?["AdministrativeArea"]?["SubAdministrativeArea"]?["SubAdministrativeArea"]?.Value<string>("SubAdministrativeAreaName") ??
                string.Empty;
        }

        public string Precision()
        {
            return GeoObject.metaDataProperty["GeocoderMetaData"]?.Value<string>("precision") ?? string.Empty;
        }

        public string Text()
        {
            return GeoObject.metaDataProperty["GeocoderMetaData"]?.Value<string>("text") ?? string.Empty;
        }
    }
}
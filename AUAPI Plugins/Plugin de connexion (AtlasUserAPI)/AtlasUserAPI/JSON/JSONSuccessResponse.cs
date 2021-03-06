﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using SCPSLApiNodeJS.JSON;
//
//    var succesResponseSteamId = SuccesResponseSteamId.FromJson(jsonString);

namespace AtlasUserAPI.JSON.Success
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class SuccessResponseJSON
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("isBanned")]
        public bool IsBanned { get; set; }

        [JsonProperty("isRegister")]
        public bool? IsRegister { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("role", NullValueHandling = NullValueHandling.Ignore)]
        public string Role { get; set; }
    }

    public partial class SuccessResponseJSON
    {
        public static SuccessResponseJSON FromJson(string json) => JsonConvert.DeserializeObject<SuccessResponseJSON>(json, AtlasUserAPI.JSON.Success.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this SuccessResponseJSON self) => JsonConvert.SerializeObject(self, AtlasUserAPI.JSON.Success.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}

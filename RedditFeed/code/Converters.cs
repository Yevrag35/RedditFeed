using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RedditFeed
{
    public class CamelEnumConverter : StringEnumConverter
    {
        public CamelEnumConverter()
            : base(new CamelCaseNamingStrategy())
        {
        }
    }

    //public class PostConverter : JsonConverter<Post>
    //{
    //    public override Post ReadJson(JsonReader reader, Type objectType, Post existingValue, bool hasExistingValue, JsonSerializer serializer)
    //    {
    //        JsonConvert
    //    }
    //    public override void WriteJson(JsonWriter writer, Post value, JsonSerializer serializer)
    //    {

    //    }
    //}
}

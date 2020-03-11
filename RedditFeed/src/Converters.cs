using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace RedditFeed
{
    public class CamelEnumConverter : StringEnumConverter
    {
        public CamelEnumConverter()
            : base(new CamelCaseNamingStrategy())
        {
        }
    }
}

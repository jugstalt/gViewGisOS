using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Cmd.FillElasticSearch
{
    [ElasticsearchType(Name="item")]
    public class Item
    {
        [String(Name = "id", Index = FieldIndexOption.No)]
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [String(Name="suggested_text", Boost = 2.0, Index = FieldIndexOption.Analyzed)]
        [JsonProperty(PropertyName = "suggested_text")]
        public string SuggestedText { get; set; }

        [String(Name = "subtext", Boost = 1.0, Index = FieldIndexOption.Analyzed)]
        [JsonProperty(PropertyName = "subtext")]
        public string SubText { get; set; }

        [String(Name = "category", Boost = 0.8, Index = FieldIndexOption.NotAnalyzed)]
        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        [String(Name = "thumbnail_url", Index = FieldIndexOption.No, Store = false)]
        [JsonProperty(PropertyName = "thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [GeoPoint()]
        public Nest.GeoLocation Geo { get; set; }

        [String(Name = "bbox", Index = FieldIndexOption.No, Store = false)]
        public string BBox { get; set; }
    }

    [ElasticsearchType(Name = "meta")]
    public class Meta
    {
        [String(Name = "id", Index = FieldIndexOption.No)]
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [String(Name = "category",Index = FieldIndexOption.NotAnalyzed)]
        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        [String(Name = "sample", Index = FieldIndexOption.No, Store = false)]
        [JsonProperty(PropertyName = "sample")]
        public string Sample { get; set; }

        [String(Name = "description", Index = FieldIndexOption.No, Store = false)]
        [JsonProperty(PropertyName = "description")]
        public string Descrption { get; set; }

        [String(Name = "service", Index = FieldIndexOption.No, Store = false)]
        [JsonProperty(PropertyName = "service")]
        public string Service { get; set; }

        [String(Name = "query", Index = FieldIndexOption.No, Store = false)]
        [JsonProperty(PropertyName = "query")]
        public string Query { get; set; }
    }
}

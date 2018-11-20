using Newtonsoft.Json;
using System;

namespace Hypar.Functions
{
    /// <summary>
    /// A data parameter which accepts various encodings.
    /// </summary>
    public class DataParameter: InputOutputBase
    {
        /// <summary>
        /// The content-type of the data.
        /// Supported types are `text/csv`, `text/plain`, and `application/json`.
        /// </summary>
        [JsonProperty("content_type", Required=Required.Always)]
        public string ContentType{get;}

        /// <summary>
        /// A URL that can get used to fetch data.
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url{get;}

        /// <summary>
        /// Construct a DataParameter.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="contentType"></param>
        /// <param name="url"></param>
        public DataParameter(string description, string contentType, string url=null) : base(description, HyparParameterType.Data)
        {
            this.ContentType = contentType;
            this.Url = url;
        }
    }
}
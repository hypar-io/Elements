using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements.Annotations
{
    /// <summary>An element that stores warning messages.</summary>
    [JsonConverter(typeof(Elements.Serialization.JSON.JsonInheritanceConverter), "discriminator")]
    // TODO: We probably don't want to inherit from GeometricElement for ever.  It would be better for Messages to
    // behave more like actual Annotation elements while still being capable of having an appearance in 3D in some circumstances.
    public partial class Message : GeometricElement
    {
        /// <summary>
        /// Default json constructor for a message that may have geometry.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="shortMessage"></param>
        /// <param name="stackTrace"></param>
        /// <param name="severity"></param>
        /// <param name="transform"></param>
        /// <param name="material"></param>
        /// <param name="representation"></param>
        /// <param name="isElementDefinition"></param>
        /// <param name="id"></param>
        /// <param name="name"></param>
        [JsonConstructor]
        public Message(string @message, string @shortMessage, string @stackTrace, MessageSeverity @severity, Transform @transform = null, Material @material = null, Representation @representation = null, bool @isElementDefinition = false, System.Guid @id = default, string @name = null)
            : base(transform, material, representation, isElementDefinition, id, name)
        {
            this.ShortMessage = @shortMessage;
            this.MessageText = @message;
            this.StackTrace = @stackTrace;
            this.Severity = @severity;
        }

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public Message()
            : base()
        {
        }

        /// <summary>A short message for the user. For a more detailed message, use MessageText.</summary>
        [JsonProperty("ShortMessage", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string ShortMessage { get; set; }

        /// <summary>A warning message for the user.</summary>
        [JsonProperty("Message", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string MessageText { get; set; }

        /// <summary>Developer specific message about the failure in the code.</summary>
        [JsonProperty("Stack Trace", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string StackTrace { get; set; }

        /// <summary>Developer specific message about the failure in the code.</summary>
        [JsonProperty("Severity", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public MessageSeverity Severity { get; set; }
    }
}
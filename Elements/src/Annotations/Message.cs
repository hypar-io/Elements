using System.Text.Json.Serialization;
using Elements.Geometry;

namespace Elements.Annotations
{
    /// <summary>An element that stores warning messages.</summary>
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
        [JsonPropertyName("ShortMessage")]
        public string ShortMessage { get; set; }

        /// <summary>A warning message for the user.</summary>
        [JsonPropertyName("Message")]
        public string MessageText { get; set; }

        /// <summary>Developer specific message about the failure in the code.</summary>
        [JsonPropertyName("Stack Trace")]
        public string StackTrace { get; set; }

        /// <summary>Developer specific message about the failure in the code.</summary>
        [JsonPropertyName("Severity")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MessageSeverity Severity { get; set; }
    }
}
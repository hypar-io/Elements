using Elements.Geometry;
using Elements.Geometry.Solids;
using System;
using System.Collections.Generic;

namespace Elements.Annotations
{
    public partial class Message
    {
        /// <summary>
        /// The material used for messages with "Info" severity.
        /// </summary>
        public static Material InfoMaterial { get; set; } = new Material("InfoMessage",
                                                                         new Color(0, 0, 1, 0.7));


        /// <summary>
        /// The material used for messages with "Warning" severity.
        /// </summary>
        public static Material WarningMaterial { get; set; } = new Material("WarningMessage",
                                                                            new Color(1, 1, 0, 0.7));

        /// <summary>
        /// The material used for messages with "Error" severity.
        /// </summary>
        public static Material ErrorMaterial { get; set; } = new Material("ErrorMessage",
                                                                          new Color(1, 0, 0, 0.7));


        private const string DefaultName = "Message";
        private const double DefaultSideLength = 0.3;

        /// <summary>
        /// Create a simple message from the given text.
        /// </summary>
        /// <param name="message">The message to the user.</param>
        /// <param name="name">The name given to the message.</param>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="stackTrace">Any stack trace associated with the message.</param>
        /// <param name="shortMessage">A short message.</param>
        public static Message FromText(string message,
                                       string name = null,
                                       MessageSeverity severity = MessageSeverity.Warning,
                                       string stackTrace = null,
                                       string shortMessage = null)
        {
            return new Message(message, shortMessage, stackTrace, severity, name: name ?? DefaultName);
        }

        /// <summary>
        /// Create a simple message at a point.
        /// </summary>
        /// <param name="messageText">The message to the user.</param>
        /// <param name="shortMessage">A short message.</param>
        /// <param name="point"></param>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="sideLength"></param>
        /// <param name="name">The name given to the message.</param>
        /// <param name="stackTrace">Any stack trace associated with the message.</param>
        public static Message FromPoint(string messageText,
                                        Vector3? point,
                                        MessageSeverity severity = MessageSeverity.Warning,
                                        double sideLength = DefaultSideLength,
                                        string name = null,
                                        string stackTrace = null,
                                        string shortMessage = null)
        {
            var transform = point.HasValue ? new Transform(point.Value) : new Transform();
            var message = new Message(messageText,
                                      shortMessage,
                                      stackTrace,
                                      severity,
                                      transform,
                                      MaterialForSeverity(severity),
                                      null,
                                      false,
                                      Guid.NewGuid(),
                                      name ?? DefaultName);
            if (point.HasValue)
            {
                var rectangle = Polygon.Rectangle(sideLength, sideLength).TransformedPolygon(
                    new Transform(0, 0, -sideLength / 2));
                var extrude = new Extrude(rectangle, sideLength, Vector3.ZAxis, false);

                message.Representation = new Representation(new[] { extrude });
            }

            return message;
        }

        /// <summary>
        /// Create a simple message along a curve.
        /// </summary>
        /// <param name="messageText">The message to the user.</param>
        /// <param name="shortMessage">A short message.</param>
        /// <param name="curve"></param>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="sideLength"></param>
        /// <param name="name">The name given to the message.</param>
        /// <param name="stackTrace">Any stack trace associated with the message.</param>
        public static Message FromCurve(string messageText,
                                        BoundedCurve curve,
                                        MessageSeverity severity = MessageSeverity.Warning,
                                        double sideLength = DefaultSideLength,
                                        string name = null,
                                        string stackTrace = null,
                                        string shortMessage = null)
        {
            var profile = Polygon.Rectangle(sideLength, sideLength);
            var sweep = new Sweep(profile, curve, 0, 0, 0, false);
            var message = new Message(messageText,
                                      shortMessage,
                                      stackTrace,
                                      severity,
                                      null,
                                      MaterialForSeverity(severity),
                                      new Representation(new[] { sweep }),
                                      id: Guid.NewGuid(),
                                      name: name ?? DefaultName);
            return message;
        }

        /// <summary>
        /// Create a simple message from a polygon.
        /// </summary>
        /// <param name="messageText">The message to the user.</param>
        /// <param name="shortMessage">A short message.</param>
        /// <param name="polygon"></param>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="height"></param>
        /// <param name="name">The name given to the message.</param>
        /// <param name="stackTrace">Any stack trace associated with the message.</param>
        public static Message FromPolygon(string messageText,
                                          Polygon polygon,
                                          MessageSeverity severity = MessageSeverity.Warning,
                                          double height = 0,
                                          string name = null,
                                          string stackTrace = null,
                                          string shortMessage = null)
        {
            const double messageHeightSubtleLift = 0.02;
            SolidOperation solid = height > 0 ? new Extrude(polygon, height, Vector3.ZAxis, false) : new Lamina(polygon, false) as SolidOperation;
            var message = new Message(messageText,
                                      shortMessage,
                                      stackTrace,
                                      severity,
                                      new Transform().Moved(z: messageHeightSubtleLift),
                                      MaterialForSeverity(severity),
                                      new Representation(new[] { solid }),
                                      false,
                                      Guid.NewGuid(),
                                      name ?? DefaultName);
            return message;
        }

        /// <summary>
        /// Create a simple message from polygons.
        /// </summary>
        /// <param name="messageText">The message to the user.</param>
        /// <param name="shortMessage">A short message.</param>
        /// <param name="polygons"></param>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="height"></param>
        /// <param name="name">The name given to the message.</param>
        /// <param name="stackTrace">Any stack trace associated with the message.</param>
        public static Message[] FromPolygons(string messageText,
                                             IEnumerable<Polygon> polygons,
                                             MessageSeverity severity = MessageSeverity.Warning,
                                             double height = 0,
                                             string name = null,
                                             string stackTrace = null,
                                             string shortMessage = null)
        {
            var messages = new List<Message>();
            foreach (var polygon in polygons)
            {
                var message = FromPolygon(messageText, polygon, severity, height, name, shortMessage);
                messages.Add(message);
            }
            return messages.ToArray();
        }

        /// <summary>
        /// Create an informational message.
        /// </summary>
        public static Message Info(Vector3? point, string message = null)
        {
            return Message.FromPoint(message, point, MessageSeverity.Info, shortMessage: "💡");
        }

        /// <summary>
        /// Create a warning message.
        /// </summary>
        public static Message Warning(Vector3? point, string message = null)
        {
            return Message.FromPoint(message, point, MessageSeverity.Warning, shortMessage: "⚠️");
        }

        /// <summary>
        /// Create an error message.
        /// </summary>
        public static Message Error(Vector3? point, string message = null)
        {
            return Message.FromPoint(message, point, MessageSeverity.Error, shortMessage: "🛑");
        }

        /// <summary>
        /// Extract the string details including any stack trace from the exception.
        /// </summary>
        /// <param name="e">The Exception.</param>
        /// <returns></returns>
        public static string DetailsFromException(Exception e)
        {
            string message = e.Message;
            if (!string.IsNullOrWhiteSpace(e.StackTrace))
            {
                message += "\n" + e.StackTrace;
            }
            return message;
        }

        private static Material MaterialForSeverity(MessageSeverity severity)
        {
            switch (severity)
            {
                case MessageSeverity.Info:
                    return InfoMaterial;
                case MessageSeverity.Warning:
                    return WarningMaterial;
                case MessageSeverity.Error:
                    return ErrorMaterial;
                default:
                    throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
            }
        }
    }
}

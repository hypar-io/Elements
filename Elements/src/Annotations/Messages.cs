using Elements;
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
        /// <returns></returns>
        public static Message FromText(string message,
                                       string name = null,
                                       MessageSeverity severity = MessageSeverity.Warning,
                                       string stackTrace = null)
        {
            return new Message(message, stackTrace, severity, name: name ?? DefaultName);
        }

        /// <summary>
        /// Create a simple message at a point.
        /// </summary>
        /// <param name="messageText">The message to the user.</param>
        /// <param name="point"></param>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="sideLength"></param>
        /// <param name="name">The name given to the message.</param>
        /// <param name="stackTrace">Any stack trace associated with the message.</param>
        /// <returns></returns>
        public static Message FromPoint(string messageText,
                                             Vector3? point,
                                             MessageSeverity severity = MessageSeverity.Warning,
                                             double sideLength = DefaultSideLength,
                                             string name = null,
                                             string stackTrace = null)
        {
            var transform = point.HasValue
                ? new Transform(point.Value).Moved(z: -sideLength / 2)
                : new Transform();
            var message = new Message(messageText,
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
                var extrude = new Extrude(Polygon.Rectangle(
                    sideLength, sideLength), sideLength, Vector3.ZAxis, false);

                message.Representation = new Representation(new[] { extrude });
            }

            return message;
        }

        /// <summary>
        /// Create a simple message along a line.
        /// </summary>
        /// <param name="messageText">The message to the user.</param>
        /// <param name="line"></param>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="sideLength"></param>
        /// <param name="name">The name given to the message.</param>
        /// <param name="stackTrace">Any stack trace associated with the message.</param>
        /// <returns></returns>
        [Obsolete("Use FromCurve instead.")]
        public static Message FromLine(string messageText,
                                       Line line,
                                       MessageSeverity severity = MessageSeverity.Warning,
                                       double sideLength = DefaultSideLength,
                                       string name = null,
                                       string stackTrace = null)
        {
            var xAxis = line.Direction();
            Vector3 yAxis;
            if (xAxis.IsParallelTo(Vector3.ZAxis))
            {
                yAxis = xAxis.Cross(Vector3.YAxis);
            }
            else
            {
                yAxis = xAxis.Cross(Vector3.ZAxis);
            }
            var zAxis = yAxis.Cross(xAxis);
            var toPipeLineTransform = new Transform(line.Start, xAxis, yAxis, zAxis);

            var localLine = new Line(new Vector3(0, 0, 0), new Vector3(line.Length(), 0, 0));

            var extrude = new Extrude(localLine.Thicken(sideLength),
                                      sideLength,
                                      Vector3.ZAxis,
                                      false);
            var message = new Message(messageText,
                                      stackTrace,
                                      severity,
                                      toPipeLineTransform.Moved(new Vector3(0, 0, -sideLength / 2)),
                                      MaterialForSeverity(severity),
                                      new Representation(new[] { extrude }),
                                      id: Guid.NewGuid(),
                                      name: name ?? DefaultName);
            return message;
        }

        /// <summary>
        /// Create a simple message along a curve.
        /// </summary>
        /// <param name="messageText">The message to the user.</param>
        /// <param name="curve"></param>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="sideLength"></param>
        /// <param name="name">The name given to the message.</param>
        /// <param name="stackTrace">Any stack trace associated with the message.</param>
        /// <returns></returns>
        public static Message FromCurve(string messageText,
                                           Curve curve,
                                           MessageSeverity severity = MessageSeverity.Warning,
                                           double sideLength = DefaultSideLength,
                                           string name = null,
                                           string stackTrace = null)
        {
            var profile = Polygon.Rectangle(sideLength, sideLength);
            var sweep = new Sweep(profile, curve, 0, 0, 0, false);
            var message = new Message(messageText,
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
        /// <param name="polygon"></param>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="height"></param>
        /// <param name="name">The name given to the message.</param>
        /// <param name="stackTrace">Any stack trace associated with the message.</param>
        /// <returns></returns>
        public static Message FromPolygon(string messageText,
                                          Polygon polygon,
                                          MessageSeverity severity = MessageSeverity.Warning,
                                          double height = 0,
                                          string name = null,
                                          string stackTrace = null)
        {
            const double messageHeightSubtleLift = 0.02;
            SolidOperation solid = height > 0 ? new Extrude(polygon, height, Vector3.ZAxis, false) : new Lamina(polygon, false) as SolidOperation;
            var message = new Message(messageText,
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
        /// <param name="polygons"></param>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="height"></param>
        /// <param name="name">The name given to the message.</param>
        /// <param name="stackTrace">Any stack trace associated with the message.</param>
        /// <returns></returns>
        public static Message[] FromPolygons(string messageText,
                                             IEnumerable<Polygon> polygons,
                                             MessageSeverity severity = MessageSeverity.Warning,
                                             double height = 0,
                                             string name = null,
                                             string stackTrace = null)
        {
            var messages = new List<Message>();
            foreach (var polygon in polygons)
            {
                var message = FromPolygon(messageText, polygon, severity, height, name);
                messages.Add(message);
            }
            return messages.ToArray();
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

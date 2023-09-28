using System.Collections.Generic;
using System.Linq;
using Elements.Annotations;

namespace Elements.Flow
{
    public partial class Tree
    {
        /// <summary>
        /// Check for each node that incoming connections don't occupy the same direction.
        /// </summary>
        /// <returns>List of errors.</returns>
        public List<Message> CheckOverlappingIncomingConnections()
        {
            var errors = new List<Message>();
            RecursiveCheckConnections(this.Outlet, new HashSet<Connection>(), errors);
            return errors;
        }

        private void RecursiveCheckConnections(Node lastNode, HashSet<Connection> checkedConnections, List<Message> errors)
        {
            var incoming = GetIncomingConnections(lastNode);
            if (incoming.Count == 1 && GetOutgoingConnections(lastNode).Count <= 1)
            {
                if (checkedConnections.Contains(incoming[0]))
                {
                    return;
                }
                checkedConnections.Add(incoming[0]);
                RecursiveCheckConnections(incoming[0].Start, checkedConnections, errors);
            }
            else if (incoming.Count > 1)
            {
                var outgoing = GetOutgoingConnection(lastNode);
                IEnumerable<Connection> sorted = new List<Connection>();
                try
                {
                    // `ConnectionComparer.Compare` method will throw an exception if two connections occupy the same direction.
                    sorted = incoming.OrderBy(c => c, new ConnectionComparer(outgoing, true)).ToList();
                }
                catch (System.Exception e)
                {
                    var message = e.InnerException != null ? $"{e.Message}. {e.InnerException.Message}" : $"{e.Message}";
                    errors.Add(Message.FromPoint(message, outgoing.Start.Position, MessageSeverity.Error));
                    return;
                }
                foreach (var income in sorted)
                {
                    if (checkedConnections.Contains(income))
                    {
                        continue;
                    }
                    checkedConnections.Add(income);
                    RecursiveCheckConnections(income.Start, checkedConnections, errors);
                }
            }
        }
    }
}
namespace Compiler.Nodes
{
    /// <summary>
    /// A node corresponding to a while command
    /// </summary>
    public class WhileForeverCommandNode : ICommandNode
    {
        /// <summary>
        /// The command inside the loop
        /// </summary>
        public ICommandNode Command { get; }

        /// <summary>
        /// The position in the code where the content associated with the node begins
        /// </summary>
        public Position Position { get; }

        /// <summary>
        /// Creates a new while node
        /// </summary>
        /// <param name="expression">The condition associated with the loop</param>
        /// <param name="command">The command inside the loop</param>
        /// <param name="position">The position in the code where the content associated with the node begins</param>
        public WhileForeverCommandNode(ICommandNode command, Position position)
        {
            Command = command;
            Position = position;
        }
    }
}
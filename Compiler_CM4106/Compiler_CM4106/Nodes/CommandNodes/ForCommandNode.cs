namespace Compiler.Nodes
{
    /// <summary>
    /// A node corresponding to a for command
    /// </summary>
    public class ForCommandNode : ICommandNode
    {
        /// <summary>
        /// The first command
        /// </summary>
        public ICommandNode FirstCommand { get; }

        /// <summary>
        /// The condition expression
        /// </summary>
        public IExpressionNode Expression { get; }

        /// <summary>
        /// The second command
        /// </summary>
        public ICommandNode SecondCommand { get; }

        /// <summary>
        /// The do command
        /// </summary>
        public ICommandNode DoCommand { get; }

        /// <summary>
        /// The position in the code where the content associated with the node begins
        /// </summary>
        public Position Position { get; }

        /// <summary>
        /// Creates a new if command node
        /// </summary>
        /// <param name="firstCommand">The first command</param>
        /// <param name="expression">The condition expression</param>
        /// <param name="secondCommand">The second command</param>
        /// <param name="doCommand">The do command</param>
        /// <param name="position">The position in the code where the content associated with the node begins</param>
        public ForCommandNode(ICommandNode firstCommand, IExpressionNode expression, ICommandNode secondCommand, ICommandNode doCommand, Position position)
        {
            FirstCommand = firstCommand;
            Expression = expression;
            SecondCommand = secondCommand;
            DoCommand = doCommand;
            Position = position;
        }
    }
}
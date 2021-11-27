using Compiler.IO;
using Compiler.Nodes;
using System.Reflection;
using static System.Reflection.BindingFlags;

namespace Compiler.SemanticAnalysis
{
    /// <summary>
    /// A type checker
    /// </summary>
    public class TypeChecker
    {
        /// <summary>
        /// The error reporter
        /// </summary>
        public ErrorReporter Reporter { get; }

        /// <summary>
        /// Creates a new type checker
        /// </summary>
        /// <param name="reporter">The error reporter to use</param>
        public TypeChecker(ErrorReporter reporter)
        {
            Reporter = reporter;
        }

        /// <summary>
        /// Carries out type checking on a program
        /// </summary>
        /// <param name="tree">The program to check</param>
        public void PerformTypeChecking(ProgramNode tree)
        {
            PerformTypeCheckingOnProgram(tree);
        }

        /// <summary>
        /// Carries out type checking on a node
        /// </summary>
        /// <param name="node">The node to perform type checking on</param>
        private void PerformTypeChecking(IAbstractSyntaxTreeNode node)
        {
            if (node is null)
                // Shouldn't have null nodes - there is a problem with your parsing
                Debugger.Write("Tried to perform type checking on a null tree node");
            else if (node is ErrorNode)
                // Shouldn't have error nodes - there is a problem with your parsing
                Debugger.Write("Tried to perform type checking on an error tree node");
            else
            {
                string functionName = "PerformTypeCheckingOn" + node.GetType().Name.Remove(node.GetType().Name.Length - 4);
                MethodInfo function = this.GetType().GetMethod(functionName, NonPublic | Public | Instance | Static);
                if (function == null)
                    // There is not a correctly named function below
                    Debugger.Write($"Couldn't find the function {functionName} when type checking");
                else
                    function.Invoke(this, new[] { node });
            }
        }

        /// <summary>
        /// Carries out type checking on a program node
        /// </summary>
        /// <param name="programNode">The node to perform type checking on</param>
        private void PerformTypeCheckingOnProgram(ProgramNode programNode)
        {
            PerformTypeChecking(programNode.Command);
        }

        /// <summary>
        /// Carries out type checking on a blank command node
        /// </summary>
        /// <param name="blankCommand">The node to perform type checking on</param>
        private void PerformTypeCheckingOnBlankCommand(BlankCommandNode blankCommand)
        {
        }

        /// <summary>
        /// Carries out type checking on an assign command node
        /// </summary>
        /// <param name="assignCommand">The node to perform type checking on</param>
        private void PerformTypeCheckingOnAssignCommand(AssignCommandNode assignCommand)
        {
            PerformTypeChecking(assignCommand.Identifier);
            PerformTypeChecking(assignCommand.Expression);

            // Check identifier is a variable and is the correct type
            if (!(assignCommand.Identifier.Declaration is IVariableDeclarationNode variable))
            {
                Reporter.RecordError("Assigning identifier is not a variable", assignCommand.Position);
            }
            else if (variable.EntityType != assignCommand.Expression.Type)
            {
                Reporter.RecordError("Assign identifier is not the same type as expression", assignCommand.Position);
            }
        }

        /// <summary>
        /// Carries out type checking on a call command node
        /// </summary>
        /// <param name="callCommand">The node to perform type checking on</param>
        private void PerformTypeCheckingOnCallCommand(CallCommandNode callCommand)
        {
            PerformTypeChecking(callCommand.Identifier);
            PerformTypeChecking(callCommand.Parameter);

            // Check identifier is a function and checking arguments of the function
            if (!(callCommand.Identifier.Declaration is FunctionDeclarationNode functionDeclaration))
            {
                Reporter.RecordError("CallCommand identifier is not a function", callCommand.Position);
            }
            else if (GetNumberOfArguments(functionDeclaration.Type) == 0)
            {
                if (!(callCommand.Parameter is BlankParameterNode))
                {
                    Reporter.RecordError("Function must have zero parameters", callCommand.Position);
                }

            } 
            else if (GetNumberOfArguments(functionDeclaration.Type) == 1)
            {
                if (callCommand.Parameter is BlankParameterNode)
                {
                    Reporter.RecordError("Function must have one parameter argument", callCommand.Position);
                }
                else
                {
                    if (GetArgumentType(functionDeclaration.Type, 0) != callCommand.Parameter.Type)
                    {
                        Reporter.RecordError("Function parameter type was not expected", callCommand.Position);
                    }
                    if (ArgumentPassedByReference(functionDeclaration.Type, 0))
                    {
                        if (!(callCommand.Parameter is VarParameterNode))
                        {
                            Reporter.RecordError("Passed by Reference parameter was not a Var Parameter", callCommand.Position);
                        }
                    } 
                    else
                    {
                        if (!(callCommand.Parameter is ExpressionParameterNode))
                        {
                            Reporter.RecordError("Passed by value parameter was not an Expression Parameter", callCommand.Position);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Carries out type checking on an if command node
        /// </summary>
        /// <param name="ifCommand">The node to perform type checking on</param>
        private void PerformTypeCheckingOnIfCommand(IfCommandNode ifCommand)
        {
            PerformTypeChecking(ifCommand.Expression);
            PerformTypeChecking(ifCommand.ThenCommand);
            PerformTypeChecking(ifCommand.ElseCommand);

            // Check the expression type is boolean
            if (ifCommand.Expression.Type != StandardEnvironment.BooleanType)
                Reporter.RecordError("if Expression must be a boolean value", ifCommand.Position);
        }

        /// <summary>
        /// Carries out type checking on a while command node
        /// </summary>
        /// <param name="whileCommand">The node to perform type checking on</param>
        private void PerformTypeCheckingOnWhileCommand(WhileCommandNode whileCommand)
        {
            PerformTypeChecking(whileCommand.Expression);
            PerformTypeChecking(whileCommand.Command);

            // Check the expression type is boolean
            if (whileCommand.Expression.Type != StandardEnvironment.BooleanType)
                Reporter.RecordError("While Expression is not a boolean value", whileCommand.Position);
        }

        /// <summary>
        /// Carries out type checking on a while forever command node
        /// </summary>
        /// <param name="whileForeverCommand">The node to perform type checking on</param>
        private void PerformTypeCheckingOnWhileForeverCommand(WhileCommandNode whileForeverCommand)
        {
            PerformTypeChecking(whileForeverCommand.Command);
        }

        /// <summary>
        /// Carries out type checking on an if command node
        /// </summary>
        /// <param name="forCommand">The node to perform type checking on</param>
        private void PerformTypeCheckingOnForCommand(ForCommandNode forCommand)
        {
            PerformTypeChecking(forCommand.FirstCommand);
            PerformTypeChecking(forCommand.Expression);
            PerformTypeChecking(forCommand.SecondCommand);
            PerformTypeChecking(forCommand.DoCommand);

            // Check the expression type is boolean
            if (forCommand.Expression.Type != StandardEnvironment.BooleanType)
                Reporter.RecordError("for Expression must be a boolean value", forCommand.Position);
        }

        /// <summary>
        /// Carries out type checking on a let command node
        /// </summary>
        /// <param name="letCommand">The node to perform type checking on</param>
        private void PerformTypeCheckingOnLetCommand(LetCommandNode letCommand)
        {
            PerformTypeChecking(letCommand.Declaration);
            PerformTypeChecking(letCommand.Command);
        }

        /// <summary>
        /// Carries out type checking on a sequential command node
        /// </summary>
        /// <param name="sequentialCommand">The node to perform type checking on</param>
        private void PerformTypeCheckingOnSequentialCommand(SequentialCommandNode sequentialCommand)
        {
            foreach (ICommandNode command in sequentialCommand.Commands)
                PerformTypeChecking(command);
        }

        /// <summary>
        /// Carries out type checking on a const declaration node
        /// </summary>
        /// <param name="constDeclaration"The node to perform type checking on></param>
        private void PerformTypeCheckingOnConstDeclaration(ConstDeclarationNode constDeclaration)
        {
            PerformTypeChecking(constDeclaration.Identifier);
            PerformTypeChecking(constDeclaration.Expression);
        }

        /// <summary>
        /// Carries out type checking on a sequential declaration node
        /// </summary>
        /// <param name="sequentialDeclaration">The node to perform type checking on</param>
        private void PerformTypeCheckingOnSequentialDeclaration(SequentialDeclarationNode sequentialDeclaration)
        {
            foreach (IDeclarationNode declaration in sequentialDeclaration.Declarations)
                PerformTypeChecking(declaration);
        }

        /// <summary>
        /// Carries out type checking on a var declaration node
        /// </summary>
        /// <param name="varDeclaration">The node to perform type checking on</param>
        private void PerformTypeCheckingOnVarDeclaration(VarDeclarationNode varDeclaration)
        {
            PerformTypeChecking(varDeclaration.TypeDenoter);
            PerformTypeChecking(varDeclaration.Identifier);
        }

        /// <summary>
        /// Carries out type checking on a blank parameter
        /// </summary>
        /// <param name="blankParameter">The node to perform type checking on</param>
        private void PerformTypeCheckingOnBlankParameter(BlankParameterNode blankParameter)
        {
        }

        /// <summary>
        /// Carries out type checking on an expression parameter node
        /// </summary>
        /// <param name="expressionParameter">The node to perform type checking on</param>
        private void PerformTypeCheckingOnExpressionParameter(ExpressionParameterNode expressionParameter)
        {
            PerformTypeChecking(expressionParameter.Expression);
            // Set the node's type to be the type of the expression
            expressionParameter.Type = expressionParameter.Expression.Type;
        }

        /// <summary>
        /// Carries out type checking on a var parameter node
        /// </summary>
        /// <param name="varParameter">The node to perform type checking on</param>
        private void PerformTypeCheckingOnVarParameter(VarParameterNode varParameter)
        {
            PerformTypeChecking(varParameter.Identifier);
            // Check the identifier is a variable
            if (!(varParameter.Identifier.Declaration is IVariableDeclarationNode variable))
            {
                Reporter.RecordError("Type in VarParameter is not a variable", varParameter.Position);
            }
            else // Set the node's type to be the type of the identifier 
                varParameter.Type = variable.EntityType;
        }

        /// <summary>
        /// Carries out type checking on a type denoter node
        /// </summary>
        /// <param name="typeDenoter">The node to perform type checking on</param>
        private void PerformTypeCheckingOnTypeDenoter(TypeDenoterNode typeDenoter)
        {
            PerformTypeChecking(typeDenoter.Identifier);

            // Check the identifier is a type
            if (!(typeDenoter.Identifier.Declaration is SimpleTypeDeclarationNode declaration))
            {
                Reporter.RecordError("Type of TypeDenoter identifier was not an expected type", typeDenoter.Position);
            }
            else // Set the node's type to be the declaration of the identifier 
                typeDenoter.Type = declaration;
        }

        /// <summary>
        /// Carries out type checking on a binary expression node
        /// </summary>
        /// <param name="binaryExpression">The node to perform type checking on</param>
        private void PerformTypeCheckingOnBinaryExpression(BinaryExpressionNode binaryExpression)
        {
            PerformTypeChecking(binaryExpression.Op);
            PerformTypeChecking(binaryExpression.LeftExpression);
            PerformTypeChecking(binaryExpression.RightExpression);

            // Check the operator is a binary operation and if the operator, left and right expression types match
            if (!(binaryExpression.Op.Declaration is BinaryOperationDeclarationNode opDeclaration))
            {
                Reporter.RecordError("BinaryExpression operator is not a binary operation", binaryExpression.Position);
            }
            else
            {
                if (GetArgumentType(opDeclaration.Type, 0) == StandardEnvironment.AnyType)
                {
                    if (binaryExpression.LeftExpression.Type != binaryExpression.RightExpression.Type)
                    {
                        Reporter.RecordError("Left and Right Expression Type of BinaryExpression are not the same", binaryExpression.Position);
                    }
                }
                else
                {
                    if (GetArgumentType(opDeclaration.Type, 0) != binaryExpression.LeftExpression.Type)
                    {
                        Reporter.RecordError("Left hand expression is wrong type", binaryExpression.Position);
                    }
                    if (GetArgumentType(opDeclaration.Type, 1) != binaryExpression.RightExpression.Type)
                    {
                        Reporter.RecordError("Right hand expression is wrong type", binaryExpression.Position);
                    }
                }
                // Set the node's type to be the return type of the operation
                binaryExpression.Type = GetReturnType(opDeclaration.Type);
            }
        }

        /// <summary>
        /// Carries out type checking on a  node
        /// </summary>
        /// <param name="integerExpression">The node to perform type checking on</param>
        private void PerformTypeCheckingOnIntegerExpression(IntegerExpressionNode integerExpression)
        {
            PerformTypeChecking(integerExpression.IntLit);
            // Set the node's type to be integer
            integerExpression.Type = StandardEnvironment.IntegerType;
        }

        /// <summary>
        /// Carries out type checking on a character expression node
        /// </summary>
        /// <param name="characterExpression">The node to perform type checking on</param>
        private void PerformTypeCheckingOnCharacterExpression(CharacterExpressionNode characterExpression)
        {
            PerformTypeChecking(characterExpression.CharLit);
            // Set the node's type to be character
            characterExpression.Type = StandardEnvironment.CharType;
        }

        /// <summary>
        /// Carries out type checking on an ID expression node
        /// </summary>
        /// <param name="idExpression">The node to perform type checking on</param>
        private void PerformTypeCheckingOnIdExpression(IdExpressionNode idExpression)
        {
            PerformTypeChecking(idExpression.Identifier);

            // Check the identifier is either a variable or constant
            if (!(idExpression.Identifier.Declaration is IEntityDeclarationNode declaration))
            {
                Reporter.RecordError("Identifier is not a variable or constant", idExpression.Position);
            }
            else // Set the node's type to be the same as the identifier 
                idExpression.Type = declaration.EntityType;
            
        }

        /// <summary>
        /// Carries out type checking on a call expression node
        /// </summary>
        /// <param name="callExpression">The node to perform type checking on</param>
        private void PerformTypeCheckingOnCallExpression(CallExpressionNode callExpression)
        {
            PerformTypeChecking(callExpression.Identifier);
            PerformTypeChecking(callExpression.Parameter);

            // Check identifier is a function and checking arguments of the function
            if (!(callExpression.Identifier.Declaration is FunctionDeclarationNode functionDeclaration))
            {
                Reporter.RecordError("CallExpression identifier is not a function or procedure", callExpression.Position);
            }
            // The call must be to a function, not a procedure, so must not return Void
            else if (GetReturnType(functionDeclaration.Type) == StandardEnvironment.VoidType)
            {
                Reporter.RecordError("CallExpression function must return a value", callExpression.Position);
            }
            else if (GetNumberOfArguments(functionDeclaration.Type) == 0)
            {
                if (!(callExpression.Parameter is BlankParameterNode))
                {
                    Reporter.RecordError("Function must have zero parameters", callExpression.Position);
                }
                // Set the node's type to be the return type of the function
                callExpression.Type = GetReturnType(functionDeclaration.Type);
            }
            else if (GetNumberOfArguments(functionDeclaration.Type) == 1)
            {
                if (callExpression.Parameter is BlankParameterNode)
                {
                    Reporter.RecordError("Function must have one parameter argument", callExpression.Position);
                }
                else
                {
                    // Checking function parameter type matches with given parameter
                    if (GetArgumentType(functionDeclaration.Type, 0) != callExpression.Parameter.Type)
                    {
                        Reporter.RecordError("Function parameter type was not expected", callExpression.Position);
                    }
                    if (ArgumentPassedByReference(functionDeclaration.Type, 0))
                    {
                        if (!(callExpression.Parameter is VarParameterNode))
                        {
                            Reporter.RecordError("Passed by Reference parameter was not a Var Parameter", callExpression.Position);
                        }
                    }
                    else
                    {
                        if (!(callExpression.Parameter is ExpressionParameterNode))
                        {
                            Reporter.RecordError("Passed by value parameter was not an Expression Parameter", callExpression.Position);
                        }
                    }
                }
                // Set the node's type to be the return type of the function
                callExpression.Type = GetReturnType(functionDeclaration.Type);
            }
        }

        /// <summary>
        /// Carries out type checking on a unary expression node
        /// </summary>
        /// <param name="unaryExpression">The node to perform type checking on</param>
        private void PerformTypeCheckingOnUnaryExpression(UnaryExpressionNode unaryExpression)
        {
            PerformTypeChecking(unaryExpression.Op);
            PerformTypeChecking(unaryExpression.Expression);

            // Check the operation is a unary expression and correct type
            if (!(unaryExpression.Op.Declaration is UnaryOperationDeclarationNode opDeclaration))
            {
                Reporter.RecordError("UnaryOpDeclaration operator is not a unary operator", unaryExpression.Position);
            }
            else
            {
                if (GetArgumentType(opDeclaration.Type, 0) != unaryExpression.Expression.Type)
                {
                    Reporter.RecordError("Expected argument type does not match expression type", unaryExpression.Position);
                }
                // Set the node's type to be the return type of the operation
                unaryExpression.Type = GetReturnType(opDeclaration.Type);
            }
        }

        /// <summary>
        /// Carries out type checking on an operation node
        /// </summary>
        /// <param name="operation">The node to perform type checking on</param>
        private void PerformTypeCheckingOnOperator(OperatorNode operation)
        {
        }

        /// <summary>
        /// Carries out type checking on an integer literal node
        /// </summary>
        /// <param name="integerLiteral">The node to perform type checking on</param>
        private void PerformTypeCheckingOnIntegerLiteral(IntegerLiteralNode integerLiteral)
        {
            // Check the value is between short.MinValue and short.MaxValue
            if (integerLiteral.Value < short.MinValue || integerLiteral.Value > short.MaxValue)
            {
                Reporter.RecordError("Integer Literal value is out of scope", integerLiteral.Position);
            }
        }

        /// <summary>
        /// Carries out type checking on a character literal node
        /// </summary>
        /// <param name="characterLiteral">The node to perform type checking on</param>
        private void PerformTypeCheckingOnCharacterLiteral(CharacterLiteralNode characterLiteral)
        {
            // Check the value is between short.MinValue and short.MaxValue
            if (characterLiteral.Value < short.MinValue || characterLiteral.Value > short.MaxValue)
            {
                Reporter.RecordError("Character Literal value is out of scope", characterLiteral.Position);
            }
        }

        /// <summary>
        /// Carries out type checking on an identifier node
        /// </summary>
        /// <param name="identifier">The node to perform type checking on</param>
        private void PerformTypeCheckingOnIdentifier(IdentifierNode identifier)
        {
        }

        /// <summary>
        /// Gets the number of arguments that a function takes
        /// </summary>
        /// <param name="node">The function</param>
        /// <returns>The number of arguments taken by the function</returns>
        private static int GetNumberOfArguments(FunctionTypeDeclarationNode node)
        {
            return node.Parameters.Length;
        }

        /// <summary>
        /// Gets the type of a function's argument
        /// </summary>
        /// <param name="node">The function</param>
        /// <param name="argument">The index of the argument</param>
        /// <returns>The type of the given argument to the function</returns>
        private static SimpleTypeDeclarationNode GetArgumentType(FunctionTypeDeclarationNode node, int argument)
        {
            return node.Parameters[argument].type;
        }

        /// <summary>
        /// Gets the whether an argument to a function is passed by reference
        /// </summary>
        /// <param name="node">The function</param>
        /// <param name="argument">The index of the argument</param>
        /// <returns>True if and only if the argument is passed by reference</returns>
        private static bool ArgumentPassedByReference(FunctionTypeDeclarationNode node, int argument)
        {
            return node.Parameters[argument].byRef;
        }

        /// <summary>
        /// Gets the return type of a function
        /// </summary>
        /// <param name="node">The function</param>
        /// <returns>The return type of the function</returns>
        private static SimpleTypeDeclarationNode GetReturnType(FunctionTypeDeclarationNode node)
        {
            return node.ReturnType;
        }
    }
}

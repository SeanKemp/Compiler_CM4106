using Compiler.IO;
using Compiler.Tokenization;
using Compiler.Nodes;
using System;
using System.Collections.Generic;
using static Compiler.Tokenization.TokenType;

namespace Compiler.SyntacticAnalysis
{
    /// <summary>
    /// A recursive descent parser
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// The error reporter
        /// </summary>
        public ErrorReporter Reporter { get; }

        /// <summary>
        /// The tokens to be parsed
        /// </summary>
        private List<Token> tokens;

        /// <summary>
        /// The index of the current token in tokens
        /// </summary>
        private int currentIndex;

        /// <summary>
        /// The current token
        /// </summary>
        private Token CurrentToken { get { return tokens[currentIndex]; } }

        /// <summary>
        /// Advances the current token to the next one to be parsed
        /// </summary>
        private void MoveNext()
        {
            if (currentIndex < tokens.Count - 1)
                currentIndex += 1;
        }

        /// <summary>
        /// Creates a new parser
        /// </summary>
        /// <param name="reporter">The error reporter to use</param>
        public Parser(ErrorReporter reporter)
        {
            Reporter = reporter;
        }

        /// <summary>
        /// Checks the current token is the expected kind and moves to the next token
        /// </summary>
        /// <param name="expectedType">The expected token type</param>
        private void Accept(TokenType expectedType)
        {
            if (CurrentToken.Type == expectedType)
            {
                Debugger.Write($"Accepted {CurrentToken}");
                MoveNext();
            }
            else
            {
                Reporter.RecordError($"Token is not the expected token: '{CurrentToken.Spelling}', found {CurrentToken.Type} when looking for {expectedType}", CurrentToken.Position);
            }
        }

        /// <summary>
        /// Parses a program
        /// </summary>
        /// <param name="tokens">The tokens to parse</param>
        public ProgramNode Parse(List<Token> tokens)
        {
            this.tokens = tokens;
            ProgramNode program = ParseProgram();
            return program;
        }

        /// <summary>
        /// Parses a program
        /// </summary>
        private ProgramNode ParseProgram()
        {
            Debugger.Write("Parsing program");
            ICommandNode command = ParseCommand();
            return new ProgramNode(command);
        }

        /// <summary>
        /// Parses a command
        /// </summary>
        private ICommandNode ParseCommand()
        {
            Debugger.Write("Parsing command");
            List<ICommandNode> commands = new List<ICommandNode>();
            commands.Add(ParseSingleCommand());
            while (CurrentToken.Type == Comma) // Changed from semicolon to comma
            {
                Accept(Comma);
                commands.Add(ParseSingleCommand());
            }
            if (commands.Count == 1)
                return commands[0];
            else
                return new SequentialCommandNode(commands);
        }

        /// <summary>
        /// Parses a single command
        /// </summary>
        private ICommandNode ParseSingleCommand() // Uses nothing terminal instead of allowing blank commands
        {
            Debugger.Write("Parsing Single Command");
            switch (CurrentToken.Type)
            {
                case Nothing:
                    return ParseNothingCommand();
                case Identifier:
                    return ParseAssignmentOrCallCommand();
                case If:
                    return ParseIfCommand();
                case While:
                    return ParseWhileOrForeverCommand();
                case For:
                    return ParseForCommand();
                case Let:
                    return ParseLetCommand();
                case Begin:
                    return ParseBeginCommand();            
                default:
                    Reporter.RecordError("Token in Single Command is not recognised or blank", CurrentToken.Position);
                    return new ErrorNode(CurrentToken.Position);
            }
        }

        private NothingCommandNode ParseNothingCommand() // New
        {
            Debugger.Write("Parsing Nothing Command");
            Accept(Nothing);
            return new NothingCommandNode(CurrentToken.Position);
        }

        /// <summary>
        /// Parses an assignment or call command
        /// </summary>
        private ICommandNode ParseAssignmentOrCallCommand()
        {
            Debugger.Write("Parsing Assignment Command or Call Command");
            IdentifierNode identifier = ParseIdentifier();
            switch (CurrentToken.Type)
            {
                case Becomes:
                    Debugger.Write("Parsing Assignment Command");
                    Accept(Becomes);
                    IExpressionNode expression = ParseExpression();
                    return new AssignCommandNode(identifier, expression);
                case LeftBracket:
                    Debugger.Write("Parsing Call Command");
                    Accept(LeftBracket);
                    IParameterNode parameter = ParseParameter();
                    Accept(RightBracket);
                    return new CallCommandNode(identifier, parameter);
                default:
                    Reporter.RecordError("Token is not the expected token when Parsing Assignment Command or " +
                        $"Call Command: '{CurrentToken.Spelling}'", CurrentToken.Position);
                    return new ErrorNode(CurrentToken.Position);
            }
            
        }

        private IfCommandNode ParseIfCommand()
        {
            Debugger.Write("Parsing If Command");
            Position pos = CurrentToken.Position;
            Accept(If);
            IExpressionNode expression = ParseExpression();
            Accept(Then);
            ICommandNode thenCommand = ParseSingleCommand();
            Accept(Else);
            ICommandNode elseCommand = ParseSingleCommand();
            return new IfCommandNode(expression, thenCommand, elseCommand, pos);
        }

        private ICommandNode ParseWhileOrForeverCommand()
        {
            Debugger.Write("Parsing While or While Forever Command");
            Position pos = CurrentToken.Position;
            Accept(While);
            if (CurrentToken.Type != Forever) // New
            {
                IExpressionNode expression = ParseExpression();
                Accept(Do);
                return new WhileCommandNode(expression, ParseSingleCommand(), pos);
            }
            else 
            {
                Accept(Forever);
                Accept(Do);
                return new WhileForeverCommandNode(ParseSingleCommand(), pos);
            }
        }

        private ForCommandNode ParseForCommand() // New
        {
            Debugger.Write("Parsing For Command");
            Position pos = CurrentToken.Position;
            Accept(For);
            Accept(LeftBracket);
            ICommandNode firstCommand = ParseSingleCommand();
            Accept(Comma);
            IExpressionNode expression = ParseExpression();
            Accept(Comma);
            ICommandNode secondCommand = ParseSingleCommand();
            Accept(RightBracket);
            Accept(Do);
            ICommandNode doCommand = ParseSingleCommand();
            return new ForCommandNode(firstCommand, expression, secondCommand, doCommand, pos);
        }

            private LetCommandNode ParseLetCommand()
        {
            Debugger.Write("Parsing Let Command");
            Position pos = CurrentToken.Position;
            Accept(Let);
            IDeclarationNode declaration = ParseDeclaration();
            Accept(In);
            ICommandNode command = ParseSingleCommand();
            return new LetCommandNode(declaration, command, pos);
        }

        /// <summary>
        /// Parses a begin command
        /// </summary>
        private ICommandNode ParseBeginCommand()
        {
            Debugger.Write("Parsing Begin Command");
            Accept(Begin);
            ICommandNode command = ParseCommand();
            Accept(End);
            return command;
        }

        private IDeclarationNode ParseDeclaration()
        {
            Debugger.Write("Parsing Declaration");
            List<IDeclarationNode> declarations = new List<IDeclarationNode>();
            declarations.Add(ParseSingleDeclaration());
            while (CurrentToken.Type == Comma) // New
            {
                Accept(Comma);
                declarations.Add(ParseSingleDeclaration());
            }
            if (declarations.Count == 1) return declarations[0];
            return new SequentialDeclarationNode(declarations);
        }

        private IDeclarationNode ParseSingleDeclaration()
        {
            Debugger.Write("Parsing Single Declaration");
            Position pos = CurrentToken.Position;
            IdentifierNode identifier = ParseIdentifier();
            switch (CurrentToken.Type)
            {
                case Tilde:
                    Debugger.Write("Parsing Const Declaration");
                    Accept(Tilde);
                    IExpressionNode expression = ParseExpression();
                    return new ConstDeclarationNode(identifier, expression, pos);
                case Colon:
                    Debugger.Write("Parsing Var Declaration");
                    Accept(Colon);
                    TypeDenoterNode typeDenoter = ParseTypeDenoter();
                    return new VarDeclarationNode(identifier, typeDenoter, pos);
                default:
                    Reporter.RecordError($"Token is not the expected token when Parsing Single Declaration: " +
                        $"'{CurrentToken.Spelling}'", pos);
                    return new ErrorNode(pos);
            }
        }

        private IParameterNode ParseParameter()
        {
            Debugger.Write("Parsing Parameter");
            Position pos = CurrentToken.Position;
            if (CurrentToken.Type == RightBracket) return new BlankParameterNode(pos);
            else if (CurrentToken.Type == Var)
            {
                Debugger.Write("Parsing Var Parameter");
                Accept(Var);
                VarParameterNode varParameter = new VarParameterNode(ParseIdentifier(), pos);
                return varParameter;
            }
            else
            {
                Debugger.Write("Parsing Value Parameter");
                return new ExpressionParameterNode(ParseExpression());
            }
        }

        private TypeDenoterNode ParseTypeDenoter()
        {
            Debugger.Write("Parsing Type Denoter");
            TypeDenoterNode typeDenoter = new TypeDenoterNode(ParseIdentifier());
            return typeDenoter;
        }

        /// <summary>
        /// Parses an expression
        /// </summary>
        private IExpressionNode ParseExpression()
        {
            Debugger.Write("Parsing Expression");
            IExpressionNode expression = ParsePrimaryExpression();
            while (CurrentToken.Type == Operator)
            {
                OperatorNode oper = ParseOperator();
                IExpressionNode rightExpression = ParsePrimaryExpression();
                expression = new BinaryExpressionNode(expression, oper, rightExpression);
            }
            return expression;
        }

        private IExpressionNode ParsePrimaryExpression()
        {
            Debugger.Write("Parsing Primary Expression");
            switch (CurrentToken.Type)
            {
                case IntLiteral:
                    return new IntegerExpressionNode(ParseIntLiteral());
                case CharLiteral:
                    return new CharacterExpressionNode(ParseCharLiteral());
                case Identifier:
                    IdentifierNode identifier = ParseIdentifier();
                    if (CurrentToken.Type == LeftBracket) // New
                    {
                        Accept(LeftBracket);
                        IParameterNode parameter = ParseParameter();
                        Accept(RightBracket);
                        return new CallExpressionNode(identifier, parameter);
                    } 
                    else
                        return new IdExpressionNode(identifier);
                case Operator:
                    return new UnaryExpressionNode(ParseOperator(), ParsePrimaryExpression());
                case LeftBracket:
                    Accept(LeftBracket);
                    IExpressionNode expression = ParseExpression();
                    Accept(RightBracket);
                    return expression;
                default:
                    Reporter.RecordError($"Token is not the expected token when Parsing Primary Expression: " +
                        $"'{CurrentToken.Spelling}'", CurrentToken.Position);
                    return new ErrorNode(CurrentToken.Position);
            }
        }

        private OperatorNode ParseOperator()
        {
            Debugger.Write("Parsing operator");
            Token OperatorToken = CurrentToken;
            Accept(Operator);
            return new OperatorNode(OperatorToken);
        }

        private IntegerLiteralNode ParseIntLiteral()
        {
            Debugger.Write("Parsing integer literal");
            Token IntLitToken = CurrentToken;
            Accept(IntLiteral);
            return new IntegerLiteralNode(IntLitToken);
        }

        private CharacterLiteralNode ParseCharLiteral()
        {
            Debugger.Write("Parsing character literal");
            Token CharLitToken = CurrentToken;
            Accept(CharLiteral);
            return new CharacterLiteralNode(CharLitToken);
        }

        /// <summary>
        /// Parses an identifier
        /// </summary>
        private IdentifierNode ParseIdentifier()
        {
            Debugger.Write("Parsing identifier");
            Token IdentifierToken = CurrentToken;
            Accept(Identifier);
            return new IdentifierNode(IdentifierToken);
        }
    }
}

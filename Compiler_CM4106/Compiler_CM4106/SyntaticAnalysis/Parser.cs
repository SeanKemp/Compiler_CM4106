using Compiler.IO;
using Compiler.Tokenization;
//using Compiler.Nodes;
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
        public void Parse(List<Token> tokens)
        {
            this.tokens = tokens;
            //ProgramNode program = 
            ParseProgram();
            //return program;
        }

        /// <summary>
        /// Parses a program
        /// </summary>
        private void ParseProgram()
        {
            Debugger.Write("Parsing program");
            //ICommandNode command = 
            ParseCommand();
            //return new ProgramNode(command);
        }

        /// <summary>
        /// Parses a command
        /// </summary>
        private void ParseCommand()
        {
            Debugger.Write("Parsing command");
            //List<ICommandNode> commands = new List<ICommandNode>();
            //commands.Add(ParseSingleCommand());
            ParseSingleCommand();
            while (CurrentToken.Type == Comma) // Changed from semicolon to comma
            {
                Accept(Comma);
                ParseSingleCommand();
                //commands.Add(ParseSingleCommand());
            }
            //if (commands.Count == 1)
            //    return commands[0];
            //else
            //    return new SequentialCommandNode(commands);
        }

        /// <summary>
        /// Parses a single command
        /// </summary>
        private void ParseSingleCommand() // Uses nothing terminal instead of allowing blank commands
        {
            Debugger.Write("Parsing Single Command");
            switch (CurrentToken.Type)
            {
                case Nothing:
                    ParseBlankCommand();
                    break;
                case Identifier:
                    ParseAssignmentOrCallCommand();
                    break;
                case If:
                    ParseIfCommand();
                    break;
                case While:
                    ParseWhileOrForeverCommand();
                    break;
                case For:
                    ParseForCommand();
                    break;
                case Let:
                    ParseLetCommand();
                    break;
                case Begin:
                    ParseBeginCommand();
                    break;                
                default:
                    Reporter.RecordError("Token in Single Command is not recognised or blank", CurrentToken.Position);
                    break;
            }
        }

        private void ParseBlankCommand()
        {
            Debugger.Write("Parsing Blank Command");
            //Position pos = CurrentToken.Position;
            //return new BlankCommandNode(pos);
        }

        /// <summary>
        /// Parses an assignment or call command
        /// </summary>
        private void ParseAssignmentOrCallCommand()
        {
            Debugger.Write("Parsing Assignment Command or Call Command");
            //IdentifierNode identifier = 
            ParseIdentifier();
            switch (CurrentToken.Type)
            {
                case Becomes:
                    Debugger.Write("Parsing Assignment Command");
                    Accept(Becomes);
                    //IExpressionNode expression = 
                    ParseExpression();
                    break;
                    //return new AssignCommandNode(identifier, expression);
                case LeftBracket:
                    Debugger.Write("Parsing Call Command");
                    Accept(LeftBracket);
                    //IParameterNode parameter = 
                    ParseParameter();
                    Accept(RightBracket);
                    break;
                    //return new CallCommandNode(identifier, parameter);
                default:
                    Reporter.RecordError($"Token is not the expected token when Parsing Assignment Command or Call Command: '{CurrentToken.Spelling}'", CurrentToken.Position);
                    break;
                    //return new ErrorNode(CurrentToken.Position);
            }
            
        }

        private void ParseIfCommand()
        {
            Debugger.Write("Parsing If Command");
            //Position pos = CurrentToken.Position;
            Accept(If);
            //IExpressionNode expression = 
            ParseExpression();
            Accept(Then);
            //ICommandNode thenCommand = 
            ParseSingleCommand();
            Accept(Else);
            //ICommandNode elseCommand = 
            ParseSingleCommand();
            //return new IfCommandNode(expression, thenCommand, elseCommand, pos);
        }

        private void ParseWhileOrForeverCommand()
        {
            Debugger.Write("Parsing While Command");
            //Position pos = CurrentToken.Position;
            Accept(While);
            if (CurrentToken.Type != Forever) // New
            {
                //IExpressionNode expression = 
                ParseExpression();
            }
            else Accept(Forever);

            Accept(Do);
            //ICommandNode command = 
            ParseSingleCommand();
            //return new WhileCommandNode(expression, command, pos);
        }

        private void ParseForCommand() // New
        {
            Debugger.Write("Parsing For Command");
            Accept(For);
            Accept(LeftBracket);
            ParseSingleCommand();
            Accept(Comma);
            ParseExpression();
            Accept(Comma);
            ParseSingleCommand();
            Accept(RightBracket);
            Accept(Do);
            ParseSingleCommand();

        }

            private void ParseLetCommand()
        {
            Debugger.Write("Parsing Let Command");
            //Position pos = CurrentToken.Position;
            Accept(Let);
            //IDeclarationNode declaration = 
            ParseDeclaration();
            Accept(In);
            //ICommandNode command = 
            ParseSingleCommand();
            //return new LetCommandNode(declaration, command, pos);
        }

        /// <summary>
        /// Parses a begin command
        /// </summary>
        private void ParseBeginCommand()
        {
            Debugger.Write("Parsing Begin Command");
            Accept(Begin);
            //ICommandNode command = 
            ParseCommand();
            Accept(End);
            //return command;
        }

        private void ParseDeclaration()
        {
            Debugger.Write("Parsing Declaration");
            //List<IDeclarationNode> declarations = new List<IDeclarationNode>();
            //declarations.Add(ParseSingleDeclaration());
            ParseSingleDeclaration();
            while (CurrentToken.Type == Comma) // New
            {
                Accept(Comma);
                ParseSingleDeclaration();
                //declarations.Add(ParseSingleDeclaration());
            }
            //if (declarations.Count == 1) return declarations[0];
            //return new SequentialDeclarationNode(declarations);
        }

        private void ParseSingleDeclaration()
        {
            Debugger.Write("Parsing Single Declaration");
            ParseIdentifier();
            switch (CurrentToken.Type)
            {
                case Tilde:
                    ParseConstDeclaration();
                    break;
                case Colon:
                    ParseVarDeclaration();
                    break;
                default:
                    Reporter.RecordError($"Token is not the expected token when Parsing Single Declaration: '{CurrentToken.Spelling}'", CurrentToken.Position);
                    break;
                    //return new ErrorNode(CurrentToken.Position);
            }
        }

        private void ParseConstDeclaration() // Maybe need to put identifier in parameter
        {
            Debugger.Write("Parsing Const Declaration");
            //Position pos = CurrentToken.Position;
            //IdentifierNode identifier = ParseIdentifier();
            Accept(Tilde);

            //IExpressionNode expression = 
            ParseExpression();
            //return new ConstDeclarationNode(identifier, expression, pos);
        }

        private void ParseVarDeclaration()
        {
            Debugger.Write("Parsing Var Declaration");
            //Position pos = CurrentToken.Position;
            //IdentifierNode identifier = ParseIdentifier();
            Accept(Colon);
            //TypeDenoterNode typeDenoter = 
            ParseTypeDenoter();
            //return new VarDeclarationNode(identifier, typeDenoter, pos);
        }

        private IParameterNode ParseParameter()
        {
            Debugger.Write("Parsing Parameter");
            Position pos = CurrentToken.Position;
            if (CurrentToken.Type == RightBracket) return new BlankParameterNode(pos);
            else if (CurrentToken.Type == Var)
            {
                Accept(Var);
                VarParameterNode varParameter =  new VarParameterNode(ParseIdentifier(), pos);
                return varParameter;
            }
            else
            {
                return new ExpressionParameterNode(ParseExpression());
            }
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
            Position pos = CurrentToken.Position;
            switch (CurrentToken.Type)
            {
                case IntLiteral:
                    return new IntegerExpressionNode(ParseIntLiteral());
                case CharLiteral:
                    return new CharacterExpressionNode(ParseCharLiteral());
                case Identifier:
                    return new IdExpressionNode(ParseIdentifier());
                case Operator:
                    return new UnaryExpressionNode(ParseOperator(), ParsePrimaryExpression());
                case LeftBracket:
                    Accept(LeftBracket);
                    IExpressionNode expression = ParseExpression();
                    Accept(RightBracket);
                    return expression;
                default:
                    Reporter.RecordError($"Token is not the expected token when Parsing Primary Expression: '{CurrentToken.Spelling}'", CurrentToken.Position);
                    return new ErrorNode(pos);
            }
        }

        private IExpressionNode ParseBracketExpression()
        {
            Debugger.Write("Parsing Bracket Expression");
            Accept(LeftBracket);
            IExpressionNode expression = ParseExpression();
            Accept(RightBracket);
            return expression;
        }

        

        private TypeDenoterNode ParseTypeDenoter()
        {
            Debugger.Write("Parsing Type Denoter");
            TypeDenoterNode typeDenoter = new TypeDenoterNode(ParseIdentifier());
            return typeDenoter;
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

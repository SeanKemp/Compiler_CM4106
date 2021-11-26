namespace Compiler
{
    /// <summary>
    /// A position in a file
    /// </summary>
    public class Position
    {

        public int LineNum { get; set; }
        public int PosInLine { get; set; }

        public Position(int lineNum, int posInLine)
        {
            LineNum = lineNum;
            PosInLine = posInLine;
        }

        public override string ToString()
        {
            if (this == BuiltIn)
                return "System defined";
            else
                return $"Line Number: {LineNum}, Position in line: {PosInLine}";
        }

        public static Position BuiltIn { get; } = new Position(-1, -1);
    }
}
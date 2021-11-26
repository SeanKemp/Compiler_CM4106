using System;
using System.Collections.Generic;

namespace Compiler.IO
{
    /// <summary>
    /// An object for reporting errors in the compilation process
    /// </summary>
    public class ErrorReporter
    {
        /// <summary>
        /// Whether or not any errors have been encountered
        /// </summary>
        public bool HasErrors { get { return Errors.Count > 0; } }

        public IList<Tuple<string,Position>> Errors { get; set; }

        public ErrorReporter()
        {
            Errors = new List<Tuple<string, Position>>();
        }

        public void RecordError(string error, Position pos)
        {
            Errors.Add(new Tuple<string, Position>(error, pos));
        }

    }
}
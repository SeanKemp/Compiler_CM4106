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

        /// <summary>
        /// The list of the description and position of the errors
        /// </summary>
        public IList<Tuple<string,Position>> Errors { get; set; }

        public ErrorReporter()
        {
            Errors = new List<Tuple<string, Position>>();
        }

        /// <summary>
        /// Adds an error and position into the list of errors
        /// </summary>
        public void RecordError(string error, Position pos)
        {
            Errors.Add(new Tuple<string, Position>(error, pos));
        }

    }
}
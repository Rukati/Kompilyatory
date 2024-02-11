using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kompilyatory
{
    internal class MyLexer : IAntlrErrorListener<int>

    {
        public void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] int offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            string errorMessage = $"Ошибка на строке {line}:{charPositionInLine} - {msg}";
            Console.WriteLine(errorMessage);    
        }
    }
}

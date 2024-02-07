using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;

namespace Kompilyatory
{
    class Program
    {
        static void KompFilesG4()
        {
            ProcessStartInfo psi;
            psi = new ProcessStartInfo("cmd", @"/c java -jar antlr-4.9.3-complete.jar -Dlanguage=CSharp -visitor Expr.g4");
            Process.Start(psi);
        }
        static void Main(string[] args)
        {

            AntlrFileStream antlrInputStream = new AntlrFileStream("lang.txt",Encoding.UTF8) ;

            ExprLexer lexer = new ExprLexer(antlrInputStream);

            CommonTokenStream commonToken = new CommonTokenStream(lexer);

            ExprParser parser = new ExprParser(commonToken);

            IParseTree tree = parser.prog();
            
            MyVisitor visitor = new MyVisitor();
            visitor.Visit(tree);
           
            Console.WriteLine(tree.ToStringTree());
        }
    }
}

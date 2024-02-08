using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;
using Newtonsoft.Json;

namespace Kompilyatory
{
    class Program
    {
        public class expr
        {

        }
        public class initialization
        {
            public string TYPE{  get; set; }
            public string ID{ get; set; }
            
            public string END { get; set; }
        }
        public class stat
        {
            [JsonProperty("initialization")]
            public initialization Initialization { get; set; }
        }
        public class AST
        {
            [JsonProperty("stat")]
            public List<stat> Stats { get;set; }   
        }
        static public AST ast = new AST();
         static private void Main(string[] args)
        {

            AntlrFileStream antlrInputStream = new AntlrFileStream("lang.txt",Encoding.UTF8) ;

            ExprLexer lexer = new ExprLexer(antlrInputStream);

            CommonTokenStream commonToken = new CommonTokenStream(lexer);

            ExprParser parser = new ExprParser(commonToken);

            IParseTree tree = parser.prog();
            
            MyVisitor visitor = new MyVisitor();
            visitor.Visit(tree);

            //Console.WriteLine(tree.ToStringTree());

            string json = JsonConvert.SerializeObject(MyVisitor.InitNode, Formatting.Indented);

            // Сохраняем JSON в файл
            File.WriteAllText("ast.json", json);
        }
    }
}

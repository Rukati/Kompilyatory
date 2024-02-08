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
using System.ComponentModel;

namespace Kompilyatory
{
    class Program
    {
        private class initialization
        {
            [JsonProperty("type")]
            public string TYPE{  get; set; }
            [JsonProperty("ID")]
            public string ID{ get; set; }
            [JsonProperty("expr")]
            public List<string> EXPR { get; set; }
            [JsonProperty("END")]
            public string END { get; set; }
        }
        private class AST
        {
            [JsonProperty("initialization")]
            public initialization Initialization { get; set; }
        }

        static private List<AST> ast = new List<AST>();

        static public void WriteCorect(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        static public void WriteWrong(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ResetColor();
            Environment.Exit(0);
        }
         static private void Main(string[] args)
        {

            AntlrFileStream antlrInputStream = new AntlrFileStream("lang.txt",Encoding.UTF8) ;

            ExprLexer lexer = new ExprLexer(antlrInputStream);

            CommonTokenStream commonToken = new CommonTokenStream(lexer);

            ExprParser parser = new ExprParser(commonToken);

            IParseTree tree = parser.prog();
            
            
            MyVisitor visitor = new MyVisitor();
            if (parser.NumberOfSyntaxErrors == 0)
            {
                WriteCorect("Lexer and Parser completed successfully");

                visitor.Visit(tree);

                //Console.WriteLine(tree.ToStringTree());

                string json = JsonConvert.SerializeObject(MyVisitor.InitNode, Formatting.Indented);

                File.WriteAllText("ast.json", json);

                ast = JsonConvert.DeserializeObject<List<AST>>(json);

                foreach (var item in ast)
                {
                    item.Initialization.EXPR = CalculatingTheValue(item);
                }
                
                PrintID();

                WriteCorect("The program ran successfully");
            }
            
        }

        static private List<string> CalculatingTheValue(AST itemAST)
        {
            if (itemAST.Initialization.EXPR.Count == 1) return itemAST.Initialization.EXPR;
            if (itemAST.Initialization.EXPR.Count == 0) WriteWrong("An uninitialized variable was used");

            Type targetType = null;
            if(itemAST.Initialization.TYPE == "int") targetType = typeof(int);
            else if (itemAST.Initialization.TYPE == "float") targetType = typeof(float);
            else if (itemAST.Initialization.TYPE == "bool") targetType = typeof(bool);
            else if (itemAST.Initialization.TYPE == "string") targetType = typeof(string);

            string right = null;
            string left = null;

            if (!char.IsDigit(Char.Parse(itemAST.Initialization.EXPR[0])))
            {
                var RNAME = ast.Any(item => item.Initialization.ID == itemAST.Initialization.EXPR[0]) ? ast.First(item => item.Initialization.ID == itemAST.Initialization.EXPR[0]) : null;
                right = RNAME != null ? CalculatingTheValue(RNAME)[0] : itemAST.Initialization.EXPR[0];
            }
            else right = itemAST.Initialization.EXPR[0]; ;

            if (!char.IsDigit(Char.Parse(itemAST.Initialization.EXPR[1])))
            {
                var LNAME = ast.Any(item => item.Initialization.ID == itemAST.Initialization.EXPR[1]) ? ast.First(item => item.Initialization.ID == itemAST.Initialization.EXPR[1]) : null;
                left = LNAME != null ? CalculatingTheValue(LNAME)[0] : itemAST.Initialization.EXPR[1];
            }
            else left = itemAST.Initialization.EXPR[1];

            AST tempAST = new AST();
            tempAST.Initialization = new initialization()
            {
                EXPR = new List<string>(),
                TYPE = itemAST.Initialization.TYPE,
            }; 
            if (targetType == typeof(int))
            {
                switch (char.Parse(itemAST.Initialization.EXPR[2]))
                {
                    case '+':
                        tempAST.Initialization.EXPR.Add( (int.Parse(left) + int.Parse(right)).ToString());
                        tempAST.Initialization.EXPR.AddRange(itemAST.Initialization.EXPR.Skip(3));
                        return CalculatingTheValue(tempAST);
                    case '-':
                        tempAST.Initialization.EXPR.Add((int.Parse(left) - int.Parse(right)).ToString());
                        tempAST.Initialization.EXPR.AddRange(itemAST.Initialization.EXPR.Skip(3));
                        return CalculatingTheValue(tempAST);
                    case '/':
                        try { tempAST.Initialization.EXPR.Add((int.Parse(left) / int.Parse(right)).ToString()); }
                        catch (DivideByZeroException ex) { WriteWrong(ex.Message); }
                        tempAST.Initialization.EXPR.AddRange(itemAST.Initialization.EXPR.Skip(3));
                        return CalculatingTheValue(tempAST);
                    case '*':
                        tempAST.Initialization.EXPR.Add((int.Parse(left) * int.Parse(right)).ToString());
                        tempAST.Initialization.EXPR.AddRange(itemAST.Initialization.EXPR.Skip(3));
                        return CalculatingTheValue(tempAST);
                }
            }

            return null;
        }

        static private void PrintID()
        {
            foreach (var state in ast)
            {
                Console.WriteLine($"Переменная: {state.Initialization.TYPE} {state.Initialization.ID} | Значение: {(state.Initialization.EXPR == null ? "" : state.Initialization.EXPR[0]) }");
                
            }
        }

    }
}

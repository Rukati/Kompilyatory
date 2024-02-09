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
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;


namespace Kompilyatory
{
    class Program
    {
        static private List<string> CalculatingTheValue(AST itemAST)
        {
            if (itemAST.Initialization.EXPR.Count == 1) return itemAST.Initialization.EXPR;
            if (itemAST.Initialization.EXPR.Count == 0) WriteWrong("An uninitialized variable was used");


            
            int j;
            while (int.TryParse(itemAST.Initialization.EXPR[2], out j))
            {
                int k = 0;
                for (; ; k++)
                {
                    if (itemAST.Initialization.EXPR[k].Length == 1)
                        if (!char.IsDigit(itemAST.Initialization.EXPR[k][0])) break;
                }

                AST intervalExpr = new AST();
                intervalExpr.Initialization = new initialization();
                intervalExpr.Initialization.EXPR = new List<string>();
                intervalExpr.Initialization.EXPR.AddRange(itemAST.Initialization.EXPR.GetRange(k - 2, 3));
                intervalExpr.Initialization.TYPE = itemAST.Initialization.TYPE;

                itemAST.Initialization.EXPR.RemoveRange(k - 2, 3);
                itemAST.Initialization.EXPR.Insert(k - 2, CalculatingTheValue(intervalExpr)[0]);
            }

            Type targetType = null;
            if (itemAST.Initialization.TYPE == "int") targetType = typeof(int);
            else if (itemAST.Initialization.TYPE == "float") targetType = typeof(float);
            else if (itemAST.Initialization.TYPE == "bool") targetType = typeof(bool);
            else if (itemAST.Initialization.TYPE == "string") targetType = typeof(string);

            string right = null;
            string left = null;

            AST tempAST = new AST();
            tempAST.Initialization = new initialization()
            {
                EXPR = new List<string>(),
                TYPE = itemAST.Initialization.TYPE,
            };
            if (targetType == typeof(int))
            {
                int i;
                if (!int.TryParse(itemAST.Initialization.EXPR[0], out i))
                {
                    var RNAME = ast.Any(item => item.Initialization.ID == itemAST.Initialization.EXPR[0]) ? ast.First(item => item.Initialization.ID == itemAST.Initialization.EXPR[0]) : null;
                    if (i == 0 && RNAME == null) // i = 0 -> ExceptionParse | RNAME = null -> ExceptionFind
                        WriteWrong($"The name {itemAST.Initialization.EXPR[0]} does not exist in the current context.");
                    right = RNAME != null ? CalculatingTheValue(RNAME)[0] : itemAST.Initialization.EXPR[0];
                }
                else right = itemAST.Initialization.EXPR[0]; 

                if (!int.TryParse(itemAST.Initialization.EXPR[1], out i))
                {
                    var LNAME = ast.Any(item => item.Initialization.ID == itemAST.Initialization.EXPR[1]) ? ast.First(item => item.Initialization.ID == itemAST.Initialization.EXPR[1]) : null;
                    if (i == 0 && LNAME == null) // i = 0 -> ExceptionParse | LNAME = null -> ExceptionFind
                        WriteWrong($"The name {itemAST.Initialization.EXPR[1]} does not exist in the current context.");
                    left = LNAME != null ? CalculatingTheValue(LNAME)[0] : itemAST.Initialization.EXPR[1];
                }
                else left = itemAST.Initialization.EXPR[1];


                switch (char.Parse(itemAST.Initialization.EXPR[2]))
                {
                    case '+':
                        tempAST.Initialization.EXPR.Add((int.Parse(left) + int.Parse(right)).ToString());
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
            else if (targetType == typeof(float))
            {
                float i;
                if (!float.TryParse(itemAST.Initialization.EXPR[0], out i))
                {
                    var RNAME = ast.Any(item => item.Initialization.ID == itemAST.Initialization.EXPR[0]) ? ast.First(item => item.Initialization.ID == itemAST.Initialization.EXPR[0]) : null;
                    if (i == 0 && RNAME == null) // i = 0 -> ExceptionParse | RNAME = null -> ExceptionFind
                        WriteWrong($"The name {itemAST.Initialization.EXPR[0]} does not exist in the current context.");
                    right = RNAME != null ? CalculatingTheValue(RNAME)[0] : itemAST.Initialization.EXPR[0];
                }
                else right = itemAST.Initialization.EXPR[0]; ;

                if (!float.TryParse(itemAST.Initialization.EXPR[1], out i))
                {
                    var LNAME = ast.Any(item => item.Initialization.ID == itemAST.Initialization.EXPR[1]) ? ast.First(item => item.Initialization.ID == itemAST.Initialization.EXPR[1]) : null;
                    if (i == 0 && LNAME == null) // i = 0 -> ExceptionParse | LNAME = null -> ExceptionFind
                        WriteWrong($"The name {itemAST.Initialization.EXPR[1]} does not exist in the current context.");
                    left = LNAME != null ? CalculatingTheValue(LNAME)[0] : itemAST.Initialization.EXPR[1];
                }
                else left = itemAST.Initialization.EXPR[1];

                switch (char.Parse(itemAST.Initialization.EXPR[2]))
                {
                    case '+':
                        tempAST.Initialization.EXPR.Add((float.Parse(left) + float.Parse(right)).ToString());
                        tempAST.Initialization.EXPR.AddRange(itemAST.Initialization.EXPR.Skip(3));
                        return CalculatingTheValue(tempAST);
                    case '-':
                        tempAST.Initialization.EXPR.Add((float.Parse(left) - float.Parse(right)).ToString());
                        tempAST.Initialization.EXPR.AddRange(itemAST.Initialization.EXPR.Skip(3));
                        return CalculatingTheValue(tempAST);
                    case '/':
                        try { tempAST.Initialization.EXPR.Add((float.Parse(left) / float.Parse(right)).ToString()); }
                        catch (DivideByZeroException ex) { WriteWrong(ex.Message); }
                        tempAST.Initialization.EXPR.AddRange(itemAST.Initialization.EXPR.Skip(3));
                        return CalculatingTheValue(tempAST);
                    case '*':
                        tempAST.Initialization.EXPR.Add((float.Parse(left) * float.Parse(right)).ToString());
                        tempAST.Initialization.EXPR.AddRange(itemAST.Initialization.EXPR.Skip(3));
                        return CalculatingTheValue(tempAST);
                }
            }

            return null;
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

                ILGenerator.GenerateILFromAST(ast);
                

                foreach (var item in ast)
                {
                    if (item.Initialization.EXPR.Count >= 3)
                        item.Initialization.EXPR = CalculatingTheValue(item);
                }
                
                PrintID();

                WriteCorect("The program ran successfully");
            }
            
        }
        public  class initialization
        {
            [JsonProperty("type")]
            public string TYPE { get; set; }
            [JsonProperty("ID")]
            public string ID { get; set; }
            [JsonProperty("expr")]
            public List<string> EXPR { get; set; }
            [JsonProperty("END")]
            public string END { get; set; }
        }
        public class AST
        {
            [JsonProperty("initialization")]
            public initialization Initialization { get; set; }
        }

        static public List<AST> ast = new List<AST>();

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
        static private void PrintID()
        {
            foreach (var state in ast)
            {
                Console.WriteLine($"Переменная: {state.Initialization.TYPE} {state.Initialization.ID} | Значение: {(state.Initialization.EXPR.Count == 0 ? "" : state.Initialization.EXPR[0]) }");
                
            }
        }

    }
}

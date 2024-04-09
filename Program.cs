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
using LLVMSharp;


namespace Kompilyatory
{
    using System;
    using Antlr4.Runtime;

    public class CustomErrorListener<Symbol> : ConsoleErrorListener<Symbol>
    {
        public override void SyntaxError(
            IRecognizer recognizer,
            Symbol offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e)
        {
            // Переопределение метода SyntaxError
            Console.WriteLine("line " + line + ":" + charPositionInLine + " " + msg);
        }
    }
    public class Program
    {
        static public List<Dictionary<string, Dictionary<string, Dictionary<string, object>>>> InitNode = new List<Dictionary<string, Dictionary<string, Dictionary<string, object>>>>();

        [Obsolete]
        static private void Main(string[] args)
        {
            AntlrFileStream antlrInputStream = new AntlrFileStream(args[0],Encoding.UTF8) ;

            ExprLexer lexer = new ExprLexer(antlrInputStream);

            CommonTokenStream commonToken = new CommonTokenStream(lexer);
            
            ExprParser parser = new ExprParser(commonToken);
            CustomErrorListener<IToken> customErrorListener = new CustomErrorListener<IToken>();

            // Добавляем пользовательский ErrorListener в парсер
            parser.RemoveErrorListeners(); // Удаляем все предыдущие ErrorListener'ы
            parser.AddErrorListener(customErrorListener); // Добавляем пользовательский ErrorListener

            IParseTree tree = parser.prog();
            
            MyVisitor visitor = new MyVisitor();
            
            if (parser.NumberOfSyntaxErrors == 0)
            {
                //WriteCorect("Lexer and Parser completed successfully");

                visitor.Visit(tree);

                string json = JsonConvert.SerializeObject(InitNode, Formatting.Indented);

                File.WriteAllText("ast.json", json);

                Ast = JsonConvert.DeserializeObject<List<AST>>(json);

                LL.Gen();
            }
        }
        public class AST
        {
            [JsonProperty("state")] public State State { get; set; } 
            [JsonProperty("return")] public List<List<string>> Return { get; set; } 
            public void HandlingStatus(ref LL.AreaOfVisibility local)
            {
                if (Return != null) Instructions.BuildReturnBody(Return,ref local);
                else if  (State.Initialization != null)
                {
                    var variable = State.Initialization;
                    Instructions.Initialization(ref variable, ref local);
                }
                else if (State.Writeln != null)
                {
                    List<LLVMValueRef> listArgs = new List<LLVMValueRef>();
                    StringBuilder format_string = new StringBuilder();

                    if (State.Writeln.Arguments.Count != 0)
                    {
                        foreach (var value in State.Writeln.Arguments)
                        {
                            if (!string.IsNullOrEmpty(value.Variable))
                            {
                                Initialization variable;
                                variable = local.FindVariable(value.Variable);
                                if (variable == null) // Переменная не найдена
                                    WriteWrong($"Unknown variable \"{value.Variable}\" in display instruction");

                                if (variable.type == "float")
                                    format_string.Append("%0.1f");
                                else format_string.Append("%d");
                                
                                Instructions.Display(value.Variable, "variable", ref local, ref listArgs, variable:variable);
                                continue;
                            }

                            if (!string.IsNullOrEmpty(value.Line))
                            {
                                format_string.Append("%s");
                                Instructions.Display(value.Line, "line", ref local, ref listArgs);
                                continue;
                            }

                            if (value.Expression != null)
                            {
                                var expr = LL.CalculatingTheExpression(value.Expression.Value, ref local,
                                    value.Expression.Type);
                                if (value.Expression.Type == "float")
                                    format_string.Append("%0.1f");
                                else format_string.Append("%d");

                                Instructions.Display(LL.GetValue(expr.GetValueName()), "expr", ref local, ref listArgs,value.Expression.Type);
                            }

                            if (!string.IsNullOrEmpty(value.Numeric))
                            {
                                format_string.Append("%s");
                                Instructions.Display(value.Numeric, "numeric", ref local, ref listArgs);
                            }
                        }
                    }
                    else
                    {
                        format_string.Append("%s");
                        Instructions.Display("", "line", ref local, ref listArgs);
                    }
                    
                    var getPuts = LLVM.GetNamedFunction(LL.module, "printf");
                    var str = LLVM.BuildGlobalStringPtr(LL.builder, State.Writeln.ln ? format_string.ToString() + '\n' : format_string.ToString(), $"Format_string");
                    listArgs.Insert(0,str);
                    LLVM.BuildCall(LL.builder, getPuts, listArgs.ToArray(), $"");
                }
                else if (State.iF != null) Instructions._if(State.iF,ref local);
                else if (State.changeValue != null) Instructions.ChangeValue(State.changeValue,ref local);
                else if (State.whilE != null)
                {
                    if (State.whilE.body.Count > 0) Instructions._while(State.whilE, ref local);
                }
                else if (State.doWhile != null)
                {
                    if (State.doWhile.body.Count > 0) Instructions._doWhile(State.doWhile, ref local);
                }
                else if (State.FOR != null) Instructions._for(State.FOR,ref local);
                else if (State.Function != null) Instructions.BuildFunction(State.Function);
                else if (State.CallFunction != null) Instructions.CallFunction(State.CallFunction, null,ref local);
            }
        }
        public class Initialization
        {
            public string type { get; set; }
            [JsonProperty("ID")] public string Id { get; set; }
            public List<string> expr { get; set; }
            public LLVMValueRef VariableRef { get; set; }
            public LLVMValueRef ValueRef { get; set; }
            public CallFunction func { get; set; }
        }
        public class Function
        {
            public string ID { get; set; }
            public List<Initialization> args { get; set; }
            public string type { get; set; }
            public List<AST> body { get; set; }
            [JsonProperty("return")] public List<List<string>> Return { get; set; }
        }
        public class CallFunction
        {
            public string ID { get; set; }
            public List<List<string>> argc { get; set; }
        }
        public class State
        {
            [JsonProperty("callFunc")]
            public CallFunction CallFunction { get; set; }
            [JsonProperty("function")] public Function Function { get; set; }
            [JsonProperty("initialization")] public Initialization Initialization { get; set; }
            [JsonProperty("writeln")] public Write Writeln {  get; set; }
            [JsonProperty("if")] public If iF { get; set; }
            public While whilE {get; set; }
            public ChangeValue changeValue {  get; set; }
            [JsonProperty("DoWhile")] public DoWhile doWhile { get; set; }
            [JsonProperty("for")] public For FOR { get; set; }
        }
        public class For
        {
            [JsonProperty("equation")] public equation Equation { get; set; }
            [JsonProperty("init")] public Initialization Init {  get; set; }
            [JsonProperty("changeValue")] public ChangeValue changeValue { get; set; }
            public List<AST> body { get; set; }
            public class equation
            {
                [JsonProperty("left")] public List<string> left {  get; set; }
                [JsonProperty("right")] public List<string> right { get; set; }
                [JsonProperty("operator")] public string Operator { get; set; }
            }
        }
        public class ChangeValue
        {
            public string ID { get; set; }
            public List<string> expr { get; set; }
        }
        public class While : If{ }
        public class DoWhile : If { }
        public class If
        {
            public List<string> left { get; set; }
            public List<string> right { get; set; }
            [JsonProperty("operator")] public string Operator { get; set; }
            public List<AST> body { get; set; }
            [JsonProperty("else")] public Else _else { get; set; }

            public class Else
            {
                public List<AST> body { get; set; }
            }
        }
        public class Write
        {
            public List<WriteContent> Arguments { get; set; }
            [JsonProperty("ln")] public bool ln { get; set; }

            public class WriteContent
            {
                [JsonProperty("variable")] public string Variable { get; set; }
                [JsonProperty("line")] public string Line { get; set; }
                [JsonProperty("Expr")] public WriteExpr Expression { get; set; }
                [JsonProperty("numeric")] public string Numeric { get; set; }
                public class WriteExpr
                {
                    [JsonProperty("value")] public List<string> Value { get; set; }
                    [JsonProperty("type")] public string Type { get; set; }
                }
            }
        }
        static public List<AST> Ast = new List<AST>();
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
    }
}

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
    public class Program
    {
        static public List<Dictionary<string, Dictionary<string, Dictionary<string, object>>>> InitNode = new List<Dictionary<string, Dictionary<string, Dictionary<string, object>>>>();

        [Obsolete]
        static private void Main(string[] args)
        {
            AntlrFileStream antlrInputStream = new AntlrFileStream("lang.txt",Encoding.UTF8) ;

            ExprLexer lexer = new ExprLexer(antlrInputStream);

            CommonTokenStream commonToken = new CommonTokenStream(lexer);

            ExprParser parser = new ExprParser(commonToken);
            
            IParseTree tree = parser.prog();

            MyVisitor visitor = new MyVisitor();
            if (parser.NumberOfSyntaxErrors == 0 )
            {
                WriteCorect("Lexer and Parser completed successfully");

                visitor.Visit(tree);

                string json = JsonConvert.SerializeObject(InitNode, Formatting.Indented);

                File.WriteAllText("ast.json", json);

                ast = JsonConvert.DeserializeObject<List<AST>>(json);

                LL.Gen();
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
            public LLVMValueRef ValueRef { get; set; }
        }
        public class AST
        {
            [JsonProperty("state")]
            public state State { get; set; }
            public void HandlingStatus(LLVMBasicBlockRef entry, ref Dictionary<LLVMBasicBlockRef, List<Dictionary<string, initialization>>> local)
            {
                if (State.Initialization != null) Instructions.Initialization(State.Initialization, ref local);
                else if (State.Writeln != null)
                {
                    foreach (var valuePrint in State.Writeln.VALUE)
                    {
                        if (valuePrint.Exists(x => x.Keys.First() == "expr"))
                        {
                            var exprValue = valuePrint.Find(x => x.Keys.First() == "expr");
                            Instructions.Display(LL.CalculatingTheExpression(exprValue["expr"], ref local, exprValue["value"][0])[0], exprValue["value"][0],  ref local);
                        }
                        else Instructions.Display(valuePrint[0][valuePrint[0].Keys.First()][0], valuePrint[0].Keys.First(),ref local);

                    }
                }
                else if (State.iF != null) Instructions._if(State.iF, entry,ref local);
                else if (State.whilE != null) Instructions._while(State.whilE, entry,ref local);
                else if (State.changeValue != null) Instructions.changeValue(State.changeValue,ref local);
                else if (State.doWhile != null) Instructions._doWhile(State.doWhile, entry,ref local);
                else if (State.FOR != null) Instructions._for(State.FOR, entry,ref local);
                else if (State.Function != null) Instructions.buildFunction(State.Function,entry);
                else if (State.CallFunction != null) Instructions.callFunction(State.CallFunction,ref local);
            }
        }
        public class function
        {
            public string ID { get; set; }
            public List<initialization> args { get; set; }
            public string type { get; set; }
            public List<AST> body { get; set; }
        }
        
        public class callFunction
        {
            public string ID { get; set; }
            public List<string> argc { get; set; }
        }
        public class state
        {
            [JsonProperty("callFunc")]
            public callFunction CallFunction { get; set; }
            [JsonProperty("function")] public function Function { get; set; }
            [JsonProperty("initialization")] public initialization Initialization { get; set; }
            [JsonProperty("writeln")] public write Writeln {  get; set; }
            [JsonProperty("if")] public IF iF { get; set; }
            public WHILE whilE {get; set; }
            public ChangeValue changeValue {  get; set; }
            [JsonProperty("DoWhile")] public doWhile doWhile { get; set; }
            [JsonProperty("for")] public FOR FOR { get; set; }
        }
        public class FOR
        {
            [JsonProperty("equation")] public equation Equation { get; set; }
            [JsonProperty("init")] public initialization Init {  get; set; }
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
        public class WHILE : IF{ }
        public class doWhile : IF { }
        public class IF
        {
            public List<string> left { get; set; }
            public List<string> right { get; set; }
            [JsonProperty("operator")] public string Operator { get; set; }
            public List<AST> body { get; set; }
            [JsonProperty("else")] public Dictionary<string,List<AST>> Else { get; set; }
        }
        public class write
        {
            [JsonProperty("value")] public List<List<Dictionary<string,List<string>>>> VALUE { get; set; }
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
    }
}

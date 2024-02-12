﻿using Antlr4.Runtime;
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
            /*[JsonProperty("END")]
            public string END { get; set; }*/
        }
        public class AST
        {
            [JsonProperty("state")]
            public state State { get; set; }
        }
        public class state
        {
            [JsonProperty("initialization")]
            public initialization Initialization { get; set; }
            [JsonProperty("writeln")]
            public write Writeln {  get; set; }
            [JsonProperty("if")]
            public IF iF { get; set; }
            public WHILE whilE {get; set; }
            public ChangeValue changeValue {  get; set; }
        }
        public class ChangeValue
        {
            public string ID { get; set; }
            public string type{ get; set; }
            public List<string> expr { get; set; }
        }
        public class WHILE : IF{ }
        public class IF
        {
            public List<string> left { get; set; }
            public List<string> right { get; set; }
            [JsonProperty("operator")]
            public string Operator { get; set; }
            public List<AST> body { get; set; }
            [JsonProperty("else")]
            public Dictionary<string,List<AST>> Else { get; set; }
        }
        public class write
        {
            [JsonProperty("value")]
            public List<List<Dictionary<string,List<string>>>> VALUE { get; set; }
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

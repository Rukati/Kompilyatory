using System;
using System.Collections;
using static Kompilyatory.Program;
using LLVMSharp;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace Kompilyatory
{

    public class LL
    {
        public static LLVMContextRef context = LLVM.ContextCreate();
        public static LLVMBuilderRef builder = LLVM.CreateBuilderInContext(context);
        public static LLVMModuleRef module = LLVM.ModuleCreateWithName("RUKATICOMPILATOREZSYSHARP");

        public class AreaOfVisibility
        {
            static public Stack<Blocks> Function = new Stack<Blocks>();
        }
        public class Blocks
        {
            public LLVMBasicBlockRef block { get; set; }
            public Dictionary<string, Initialization> variable = new Dictionary<string, Initialization>();
            public Initialization FindVariable(string name)
            {
                if (variable.TryGetValue(name, out var value)) return value;
                foreach (var block in AreaOfVisibility.Function)
                {
                    if (block.variable.TryGetValue(name, out value)) return value;
                }
                return null;
            }
        }
        public static void Gen()
        {
            LLVM.InitializeX86TargetInfo();
            LLVM.InitializeX86Target();
            LLVM.InitializeX86TargetMC();
            LLVM.InitializeX86AsmParser();
            LLVM.InitializeX86AsmPrinter();
            
            var functionType = LLVM.FunctionType(LLVM.VoidType(), new LLVMTypeRef[] { }, new LLVMBool(0));
            var mainFunction = LLVM.AddFunction(module, "main", functionType);
            var entry = LLVM.AppendBasicBlock(mainFunction, "main");
            LLVM.PositionBuilderAtEnd(builder, entry);

            LLVMTypeRef retType = LLVM.FunctionType(LLVMTypeRef.VoidType(),
                new LLVMTypeRef[] { LLVMTypeRef.PointerType(LLVMTypeRef.Int8Type(), 0), }, true);
            LLVM.AddFunction(module, "printf", retType);

            AreaOfVisibility.Function.Push(new Blocks(){block = LLVM.GetInsertBlock(LL.builder)});
            var blocks = AreaOfVisibility.Function.Peek();
            foreach (var item in Ast)
            {
                item.HandlingStatus(entry, ref blocks);
            }
            
            // Ничего не возвращаем
            LLVM.BuildRetVoid(builder);
            // Выводим модуль в консоль
            LLVM.DumpModule(module);

            // Записываем модуль в файл .ll
            LLVM.WriteBitcodeToFile(module, "output.ll");

            // Освобождаем ресурсы
            LLVM.DisposeBuilder(builder);
            LLVM.DisposeModule(module);
            LLVM.ContextDispose(context);
        }
        public static LLVMValueRef BuildVariable(ref Initialization initValue)
        {
            LLVMTypeRef _lLVMType = GetTypeRef(initValue.type);
            return LLVM.BuildAlloca(builder, _lLVMType, initValue.Id);
        }
        public static void StoreValue(ref Initialization initValue)
        {
            string value = initValue.expr[0];
            LLVM.BuildStore(builder, BuildValue(ref value,initValue.type), initValue.ValueRef);
        }
        public static LLVMTypeRef GetTypeRef(string type)
        {
            switch (type)
            {
                case "int":
                    return LLVMTypeRef.Int32Type();
                case "float":
                    return LLVMTypeRef.FloatType();
                default:
                    WriteWrong($"Incorrect data type \"{type}\"");
                    break;
            }

            return default(LLVMTypeRef);
        }
        public static LLVMValueRef BuildValue(ref string initValue, string type = "int")
        {
            LLVMTypeRef _lLVMType = GetTypeRef(type);
            LLVMValueRef value = default(LLVMValueRef);
            if (initValue[0] == '$')
            {
                foreach (var block in AreaOfVisibility.Function)
                {
                    if (block.variable.TryGetValue(initValue.Substring(1), out Initialization variableValue)) return LLVM.BuildLoad(builder,variableValue.ValueRef,"");
                }
            }
            if (type == "int" && !initValue.Contains('.'))
            {
                if (initValue[0] == '-')
                {
                    var positiveValue = LLVM.ConstInt(_lLVMType, ulong.Parse(initValue.Substring(1)),
                        new LLVMBool(1));

                    var zeroValue = LLVM.ConstInt(_lLVMType, 0, new LLVMBool(0));

                    value = LLVM.BuildSub(builder, zeroValue, positiveValue, "");
                }
                else
                    value = LLVM.ConstInt(_lLVMType,
                        ulong.Parse(initValue), new LLVMBool(0));
            }
            else if (type == "float")
            {
                value = LLVM.ConstReal(_lLVMType,
                    double.Parse(initValue.Replace('.', ',')));
            }
            else WriteWrong($"The wrong data type was selected for the value variable");

            return value;
        }
        /*public static LLVMValueRef _BuildEquation(List<string> left, List<string> right, string op,
            ref Dictionary<LLVMBasicBlockRef, List<Dictionary<string, Initialization>>> _valueLocaleVariable)
        {
            var insertBlock = LLVM.GetInsertBlock(builder);
            LLVMValueRef leftVariable = default(LLVMValueRef);
            LLVMValueRef rightVariable = default(LLVMValueRef);

            if (left.Count == 1)
            {
                LLVMValueRef constInt;
                if (left[0][0] == '$')
                {
                    left[0] = left[0].Trim('$');
                    var blockWithVariable = FindBlockWithVariable(left[0], insertBlock, ref _valueLocaleVariable).Item1;
                    
                    if (!blockWithVariable.Equals(default(LLVMBasicBlockRef)))
                    {
                        var tuple = _valueLocaleVariable[blockWithVariable]
                            .FirstOrDefault(item => item.ContainsKey(left[0]))[left[0]];
                        if (tuple.ValueRef.Pointer != IntPtr.Zero)
                        {
                            leftVariable = LLVM.BuildLoad(builder, tuple.ValueRef, "");
                            leftVariable = LLVM.BuildSIToFP(builder, leftVariable ,LLVM.FloatType(), "LeftValueInCompare");
                        }
                    }
                }
                else
                {
                    if (left[0][0] == '-')
                    {
                        var zeroValue = LLVM.ConstReal(LLVMTypeRef.FloatType(), 0);
                        var absoluteValue = LLVM.ConstReal(LLVMTypeRef.FloatType(), double.Parse(left[0].Replace('.',',')));
                        constInt = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
                    }
                    else
                    {
                        constInt = LLVM.ConstReal(LLVMTypeRef.FloatType(), double.Parse(left[0].Replace('.',',')));
                    }

                    leftVariable = LLVM.BuildAlloca(builder, LLVM.FloatType(), "LeftVariableInit");
                    LLVM.BuildStore(builder, constInt, leftVariable);
                    leftVariable = LLVM.BuildLoad(builder, leftVariable, "LeftValueInCompare");
                }
            }
            else
            {
                LLVMValueRef constDoubleValue;
                double parsedValueDouble = double.Parse(CalculatingTheExpression(left, ref _valueLocaleVariable,"float")[0].Replace('.',','));
                if (left[0][0] == '-')
                {
                    var zeroValue = LLVM.ConstReal(LLVM.FloatType(), 0);
                    var absoluteValue =
                        LLVM.ConstReal(LLVM.FloatType(), parsedValueDouble);

                    constDoubleValue = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
                }
                else constDoubleValue = LLVM.ConstReal(LLVM.FloatType(), parsedValueDouble);

                if (_valueLocaleVariable[insertBlock].FirstOrDefault(varib =>
                        varib.ContainsKey(left[0])).TryGetValue(left[0], out var tuple) &&
                    tuple.ValueRef.Pointer != IntPtr.Zero)
                    leftVariable = LLVM.BuildLoad(builder, tuple.ValueRef, "LeftValueInCompare");
                else
                {
                    leftVariable = LLVM.BuildAlloca(builder, LLVM.FloatType(), "");
                    LLVM.BuildStore(builder, constDoubleValue, leftVariable);
                    leftVariable = LLVM.BuildLoad(builder, leftVariable, "LeftValueInCompare");
                }
            }

            if (right.Count == 1)
            {
                LLVMValueRef constInt;
                if (right[0][0] == '$')
                {
                    right[0] = right[0].Trim('$');
                    var blockWithVariable =
                        FindBlockWithVariable(right[0], insertBlock, ref _valueLocaleVariable).Item1;

                    if (!blockWithVariable.Equals(default(LLVMBasicBlockRef)))
                    {
                        var tuple = _valueLocaleVariable[blockWithVariable]
                            .FirstOrDefault(item => item.ContainsKey(right[0]))[right[0]];
                        if (tuple.ValueRef.Pointer != IntPtr.Zero)
                        {
                            rightVariable = LLVM.BuildLoad(builder, tuple.ValueRef, "");
                            rightVariable = LLVM.BuildSIToFP(builder, rightVariable ,LLVM.FloatType(), "RightValueInCompare");
                        }
                    }
                }
                else
                {
                    if (right[0][0] == '-')
                    {
                        var zeroValue = LLVM.ConstReal(LLVM.FloatType(), 0);
                        var absoluteValue = LLVM.ConstReal(LLVM.FloatType(), double.Parse(right[0].Replace('.',',')));
                        constInt = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
                    }
                    else
                    {
                        constInt = LLVM.ConstReal(LLVM.FloatType(), double.Parse(right[0].Replace('.',',')));
                    }

                    rightVariable = LLVM.BuildAlloca(builder, LLVM.FloatType(), "");
                    LLVM.BuildStore(builder, constInt, rightVariable);
                    rightVariable = LLVM.BuildLoad(builder, rightVariable, "RightValueInCompare");
                }
            }
            else
            {
                LLVMValueRef constDuble;
                double parsedDoubleValue = double.Parse(CalculatingTheExpression(right, ref _valueLocaleVariable,"float")[0].Replace('.',','));
                if (right[0][0] == '-')
                {
                    var zeroValue = LLVM.ConstReal(LLVM.FloatType(), 0);
                    var absoluteValue =
                        LLVM.ConstReal(LLVM.FloatType(), parsedDoubleValue);

                    constDuble = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
                }
                else constDuble = LLVM.ConstReal(LLVM.FloatType(), parsedDoubleValue);

                if (_valueLocaleVariable[insertBlock].FirstOrDefault(varib =>
                        varib.ContainsKey(right[0])).TryGetValue(right[0], out var tuple) &&
                    tuple.ValueRef.Pointer != IntPtr.Zero)
                    rightVariable = LLVM.BuildLoad(builder, tuple.ValueRef, "RifgtValueInCompare");
                else
                {
                    rightVariable = LLVM.BuildAlloca(builder, LLVM.FloatType(), "");
                    LLVM.BuildStore(builder, constDuble, rightVariable);
                    rightVariable = LLVM.BuildLoad(builder, rightVariable, "RightValueInCompare");
                }
            }

            LLVMValueRef icmpVariable = default(LLVMValueRef);
            
            switch (op)
            {
                case "==":
                    icmpVariable = LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealOEQ, leftVariable, rightVariable,
                        "compare_eq");
                    break;
                case "!=":
                    icmpVariable = LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealONE, leftVariable, rightVariable,
                        "compare_ne");
                    break;
                case "<":
                    icmpVariable = LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealOLT, leftVariable, rightVariable,
                        "compare_lt");
                    break;
                case ">":
                    icmpVariable = LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealOGT, leftVariable, rightVariable,
                        "compare_gt");
                    break;
            }
            
            return icmpVariable;
        }*/
        public static LLVMValueRef CalculatingTheExpression(List<string> inputVarialbe, ref Blocks _valueLocaleVariable, string type = "int")
        {
            if (inputVarialbe.Count == 0)
                WriteWrong($"An uninitialized variable was used");
            
            if (inputVarialbe.Count == 1)
            {
                var variable = _valueLocaleVariable.FindVariable(inputVarialbe[0].Substring(1));
                if (inputVarialbe[0][0] == '$') return LLVM.BuildLoad(builder,variable.ValueRef, "");

                string value = variable.expr[0];
                return BuildValue(ref value);
            }

            string right = inputVarialbe[0];
            string left = inputVarialbe[1];

            while (inputVarialbe[2].Length >= 1 && Char.IsLetterOrDigit(char.Parse(inputVarialbe[2][0].ToString())))
            {
                int k = 0;
                for (;; k++)
                {
                    if (inputVarialbe[k].Length == 1)
                        if (!char.IsLetterOrDigit(inputVarialbe[k][0]))
                            break;
                }

                /*
                AST intervalExpr = new AST();
                intervalExpr.State = new State();
                intervalExpr.State.Initialization = new Initialization();
                intervalExpr.State.Initialization.expr = new List<string>();
                intervalExpr.State.Initialization.expr.AddRange(inputVarialbe.GetRange(k - 2, 3));
                intervalExpr.State.Initialization.type = type;
                */

                inputVarialbe.RemoveRange(k - 2, 3);
                inputVarialbe.Insert(k - 2,
                    CalculatingTheExpression(intervalExpr.State.Initialization.expr, ref _valueLocaleVariable,
                        type)[0]);
            }
            
            return BuildExpr(BuildValue(ref left,type),BuildValue(ref right,type),inputVarialbe[2]);
        }
        public static LLVMValueRef BuildExpr(LLVMValueRef op1, LLVMValueRef op2, string op)
        {
            switch (op)
            {
                case "+":
                    return LLVM.BuildAdd(builder, op1, op2, "add");
                case "-":
                    return LLVM.BuildSub(builder, op1, op2, "sub");
                case "*":
                    return LLVM.BuildMul(builder, op1, op2, "mul");
                case "/":
                    return LLVM.BuildSDiv(builder, op1, op2, "div");
                default:
                    throw new ArgumentException("Invalid operator: " + op);
            }
        }

        private static bool ContainsDecimal(string str)
        {
            return str.Contains(".");
        }
    }
}
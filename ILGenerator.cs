using System;
using System.Collections;
using static Kompilyatory.Program;
using LLVMSharp;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Kompilyatory
{

    public class LL
    {
        public static LLVMContextRef context = LLVM.ContextCreate();
        public static LLVMBuilderRef builder = LLVM.CreateBuilderInContext(context);
        public static LLVMModuleRef module = LLVM.ModuleCreateWithName("RUKATICOMPILATOREZSYSHARP");

        public class AreaOfVisibility
        {
            public Stack<Blocks> Function = new Stack<Blocks>();
            public Initialization FindVariable(string name)
            {
                foreach (var block in Function)
                {
                    if (block.variable.TryGetValue(name, out Initialization value)) return value;
                }
                return null;
            }
        }
        public class Blocks
        {
            public LLVMBasicBlockRef block { get; set; }
            public Dictionary<string, Initialization> variable;
        }

        public static void Gen()
        {
            LLVM.InitializeX86TargetInfo();
            LLVM.InitializeX86Target();
            LLVM.InitializeX86TargetMC();
            LLVM.InitializeX86AsmParser();
            LLVM.InitializeX86AsmPrinter();

            LLVMTypeRef retType = LLVM.FunctionType(LLVMTypeRef.VoidType(),
                new LLVMTypeRef[] { LLVMTypeRef.PointerType(LLVMTypeRef.Int8Type(), 0), }, true);
            LLVM.AddFunction(module, "printf", retType);

            foreach (var function in Ast)
            {
                if (function.State.Function == null) break;
                else
                {
                    foreach (var callFunction in Ast)
                    {
                        if (callFunction.State.CallFunction != null)
                        {
                            if (callFunction.State.CallFunction.ID == function.State.Function.ID && function.State.Function.type == "void")
                            {
                                Instructions.BuildFunction(function.State.Function);
                                break;
                            }
                        }
                        else if (callFunction.State.Initialization != null)
                        {
                            if (callFunction.State.Initialization.func.ID == function.State.Function.ID)
                            {
                                Instructions.BuildFunction(function.State.Function);
                                break;
                            }
                        }
                    }
                }
            }

            var functionType = LLVM.FunctionType(LLVM.VoidType(), new LLVMTypeRef[] { }, new LLVMBool(0));
            var mainFunction = LLVM.AddFunction(module, "main", functionType);
            var entry = LLVM.AppendBasicBlock(mainFunction, "main");
            LLVM.PositionBuilderAtEnd(builder, entry);
            
            AreaOfVisibility areaOfVisibility = new AreaOfVisibility();
            areaOfVisibility.Function.Push(new Blocks(){block = entry, variable = new Dictionary<string, Initialization>()});
            
            foreach (var item in Ast)
            {
                if (item.State.Function != null) continue;
                item.HandlingStatus(ref areaOfVisibility);
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
        public static void StoreValue(ref Initialization initValue, ref AreaOfVisibility areaOfVisibility)
        {
            string value = initValue.expr[0];
            var RefValue = BuildValue(ref value, ref areaOfVisibility, initValue.type);
            LLVM.BuildStore(builder, RefValue , initValue.VariableRef);
            initValue.ValueRef = RefValue;
        }
        public static LLVMTypeRef GetTypeRef(string type)
        {
            switch (type)
            {
                case "int":
                    return LLVMTypeRef.Int32Type();
                case "float":
                    return LLVMTypeRef.FloatType();
                case "void":
                    return LLVMTypeRef.VoidType();
                default:
                    WriteWrong($"Incorrect data type \"{type}\"");
                    break;
            }

            return default(LLVMTypeRef);
        }
        public static LLVMValueRef BuildValue(ref string initValue, ref AreaOfVisibility areaOfVisibility, string type = "int")
        {
            LLVMTypeRef _lLVMType = GetTypeRef(type);
            LLVMValueRef value = default(LLVMValueRef);
            if (initValue[0] == '$')
            {
                var variable = areaOfVisibility.FindVariable(initValue.Substring(1));
                if (variable == null)
                    WriteWrong($"Unknown variable \"{initValue.Substring(1)}\"");
                else
                    return variable.ValueRef;
            }

            if (type == "int")
            {
                if (initValue[0] == '-')
                {
                    double floatingNumber = double.Parse(initValue.Substring(1), CultureInfo.InvariantCulture);
                    ulong uLongNumber = (ulong)floatingNumber;
                    
                    var positiveValue = LLVM.ConstInt(_lLVMType, uLongNumber,
                        new LLVMBool(1));

                    var zeroValue = LLVM.ConstInt(_lLVMType, 0, new LLVMBool(0));

                    value = LLVM.BuildSub(builder, zeroValue, positiveValue, "");
                }
                else
                {
                    double floatingNumber = double.Parse(initValue, CultureInfo.InvariantCulture);
                    ulong uLongNumber = (ulong)floatingNumber; 
                    value = LLVM.ConstInt(_lLVMType, uLongNumber, new LLVMBool(0)); 
                }
            }
            else if (type == "float")
            {
                value = LLVM.ConstReal(_lLVMType,
                    double.Parse(initValue,CultureInfo.InvariantCulture));
            }
            else WriteWrong($"The wrong data type was selected for the value variable");

            return value;
        }
        public static LLVMValueRef _BuildEquation(List<string> left, List<string> right, string op,
            ref AreaOfVisibility valueLocaleVariable)
        {
            var insertBlock = LLVM.GetInsertBlock(builder);
            LLVMValueRef leftVariable = default(LLVMValueRef);
            LLVMValueRef rightVariable = default(LLVMValueRef);

            leftVariable = CalculatingTheExpression(left, ref valueLocaleVariable, "float");
            rightVariable = CalculatingTheExpression(right, ref valueLocaleVariable, "float");
            leftVariable = LLVM.BuildSIToFP(builder, leftVariable ,LLVM.FloatType(), "LeftValueInCompare");
            rightVariable = LLVM.BuildSIToFP(builder, rightVariable, LLVM.FloatType(), "RightValueInCompare");
            
            /*
            if (left[0][0] == '$')
            {
                left[0] = left[0].Trim('$');
                var variable = valueLocaleVariable.FindVariable(left[0]);
                if (variable != null)
                {
                    leftVariable = LLVM.BuildLoad(builder, variable.VariableRef, "");
                    leftVariable = LLVM.BuildSIToFP(builder, leftVariable ,LLVM.FloatType(), "LeftValueInCompare");
                }
                else
                    WriteWrong($"Unknown variable: {left[0]}");
            }
            else
            {
                if (left.Count == 1)
                {
                    string LeftValue = left[0];
                    leftVariable = BuildValue(ref LeftValue,ref valueLocaleVariable, "float");
                }
                else
                {
                    leftVariable = CalculatingTheExpression(left, ref valueLocaleVariable, "float");
                }
            }
            
            if (right[0][0] == '$')
            {
                right[0] = right[0].Trim('$');
                var variable = valueLocaleVariable.FindVariable(right[0]);
                if (variable != null)
                {
                    rightVariable = LLVM.BuildLoad(builder, variable.VariableRef, "");
                    rightVariable =
                        LLVM.BuildSIToFP(builder, rightVariable, LLVM.FloatType(), "LeftValueInCompare");
                }
                else 
                    WriteWrong($"Unknown variable: {right[0]}");

            }
            else
            {
                if (right.Count == 1)
                {
                    string LeftValue = right[0];
                    rightVariable = BuildValue(ref LeftValue,ref valueLocaleVariable, "float");
                }
                else
                {
                    rightVariable = CalculatingTheExpression(right, ref valueLocaleVariable, "float");
                }
            }
            */

            LLVMValueRef compare = default(LLVMValueRef);
            switch (op)
            {
                case "==":
                    compare = LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealOEQ, leftVariable, rightVariable,
                        "compare_eq");
                    break;
                case "!=":
                    compare = LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealONE, leftVariable, rightVariable,
                        "compare_ne");
                    break;
                case "<":
                    compare = LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealOLT, leftVariable, rightVariable,
                        "compare_lt");
                    break;
                case ">":
                    compare = LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealOGT, leftVariable, rightVariable,
                        "compare_gt");
                    break;
            }

            return compare;
        }
        public static Stack<LLVMValueRef> valueRefs = new Stack<LLVMValueRef>();
        public static LLVMValueRef CalculatingTheExpression(List<string> inputVarialbe, ref AreaOfVisibility valueLocaleVariable, string type = "int")
        {
            if (inputVarialbe.Count == 0)
                WriteWrong($"An uninitialized variable was used:");

            if (inputVarialbe.Count == 1)
            {
                if (inputVarialbe[0][0] == '$')
                {
                    var variable = valueLocaleVariable.FindVariable(inputVarialbe[0].Substring(1));
                    if (variable == null) WriteWrong($"Unknown variable: {inputVarialbe[0]}");
                    else if (variable.ValueRef.Pointer == IntPtr.Zero)
                        WriteWrong($"An uninitialized variable was used: {variable.Id}");
                    else return LLVM.BuildLoad(builder, variable.VariableRef, "");
                }
                else
                {
                    string value = inputVarialbe[0];
                    return BuildValue(ref value, ref valueLocaleVariable);
                }
            }

            string right;
            string left;
            string op;
            LLVMValueRef l;
            LLVMValueRef r;

            while (inputVarialbe.Count > 1)
            {
                int k = 0;
                for (;; k++)
                {
                    if (inputVarialbe[k].Length == 1)
                        if (!char.IsLetterOrDigit(inputVarialbe[k][0]))
                            break;
                }

                right = inputVarialbe[k - 2];
                left = inputVarialbe[k - 1];
                op = inputVarialbe[k - 0];

                inputVarialbe.RemoveRange(k - 2, 3);
                if (left[0] == '$')
                {
                    var variable = valueLocaleVariable.FindVariable(left.Substring(1));
                    if (variable == null) WriteWrong($"Unknown variable: {left.Substring(1)}");
                    else if (variable.ValueRef.Pointer == IntPtr.Zero) WriteWrong($"An uninitialized variable was used");
                    l = LLVM.BuildLoad(builder,variable.VariableRef , "");
                }
                else if (left.Contains('%'))
                    l = valueRefs.Pop();
                else
                {
                    left = GetValue(left);
                    l = BuildValue(ref left, ref valueLocaleVariable, type);
                }    
                if (right[0] == '$')
                {    
                    var variable = valueLocaleVariable.FindVariable(right.Substring(1));
                    if (variable == null) WriteWrong($"Unknown variable: {right.Substring(1)}");
                    else if (variable.ValueRef.Pointer == IntPtr.Zero) WriteWrong($"An uninitialized variable was used");
                    r = LLVM.BuildLoad(builder, variable.VariableRef, "");
                }
                else if (right.Contains('%'))
                    r = valueRefs.Pop();
                else
                {
                    right = GetValue(right);
                    r = BuildValue(ref right, ref valueLocaleVariable, type);
                }                
                valueRefs.Push(BuildExpr(l, r, op, type, ref valueLocaleVariable));
                inputVarialbe.Insert(k - 2, valueRefs.Peek().GetValueName());
            }
            
            return valueRefs.Peek();
        }
        public static LLVMValueRef BuildExpr(LLVMValueRef op1, LLVMValueRef op2, string op, string type, ref AreaOfVisibility areaOfVisibility)
        {
            if (op1.TypeOf().ToString() != op2.TypeOf().ToString())
            {
                op1 = LLVM.BuildSIToFP(builder, op1 ,LLVM.FloatType(), "");
                op2 = LLVM.BuildSIToFP(builder, op2 ,LLVM.FloatType(), "");
            }
            
            switch (op)
            {
                case "+":
                    if (type == "float")
                        return LLVM.BuildFAdd(builder, op1, op2, "fadd");
                    else
                        return LLVM.BuildAdd(builder, op1, op2, "add");
                case "-":
                    if (type == "float")
                        return LLVM.BuildFSub(builder, op1, op2, "fsub");
                    else
                        return LLVM.BuildSub(builder, op1, op2, "sub");
                case "*":
                    if (type == "float")
                        return LLVM.BuildFMul(builder, op1, op2, "fmul");
                    else
                        return LLVM.BuildMul(builder, op1, op2, "mul");
                case "/":
                    if (type == "float")
                        return LLVM.BuildFDiv(builder, op1, op2, "fdiv");
                    else
                        return LLVM.BuildSDiv(builder, op1, op2, "div");
                default:
                    throw new ArgumentException("Invalid operator: " + op);
            }
            
        }
        public static string GetValue(string valueName)
        {
            if (valueName.Contains('%'))
            {
                string[] parts = valueName.Split('='); 
                return parts[0].Trim();
            }
            int lastIndex = valueName.LastIndexOf(' ');
            if (lastIndex >= 0)
            {
                valueName = valueName.Substring(lastIndex + 1);
            }

            return valueName;
        }
    }
}
using System;
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

            LLVMTypeRef retType = LLVM.FunctionType(LLVMTypeRef.Int32Type(),
                new LLVMTypeRef[] { LLVMTypeRef.PointerType(LLVMTypeRef.Int8Type(), 0), }, true);
            LLVM.AddFunction(module, "printf", retType);
            var _valueLocaleVariable = new Dictionary<LLVMBasicBlockRef, List<Dictionary<string, initialization>>>()
            {
                { LLVM.GetInsertBlock(LL.builder), new List<Dictionary<string, initialization>>() }
            };
            foreach (var item in ast) item.HandlingStatus(entry, ref _valueLocaleVariable);

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
        public static LLVMValueRef _BuildEquation(List<string> left, List<string> right, string op,
            ref Dictionary<LLVMBasicBlockRef, List<Dictionary<string, initialization>>> _valueLocaleVariable)
        {
            var insertBlock = LLVM.GetInsertBlock(builder);
            LLVMValueRef leftVariable = default(LLVMValueRef);
            LLVMValueRef rightVariable = default(LLVMValueRef);

            if (left.Count == 1)
            {
                LLVMValueRef constInt;
                var blockWithVariable = FindBlockWithVariable(left[0], insertBlock, ref _valueLocaleVariable).Item1;

                if (!blockWithVariable.Equals(default(LLVMBasicBlockRef)))
                {
                    var tuple = _valueLocaleVariable[blockWithVariable]
                        .FirstOrDefault(item => item.ContainsKey(left[0]))[left[0]];
                    if (tuple.ValueRef.Pointer != IntPtr.Zero)
                    {
                        leftVariable = LLVM.BuildLoad(builder, tuple.ValueRef, "LeftValueInCompare");
                    }
                }
                else
                {
                    if (left[0][0] == '-')
                    {
                        var zeroValue = LLVM.ConstInt(LLVM.Int32Type(), 0, new LLVMBool(0));
                        var absoluteValue = LLVM.ConstInt(LLVM.Int32Type(), (ulong)Math.Abs(long.Parse(left[0])),
                            new LLVMBool(0));
                        constInt = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
                    }
                    else
                    {
                        constInt = LLVM.ConstInt(LLVM.Int32Type(), (ulong)long.Parse(left[0]), new LLVMBool(0));
                    }

                    leftVariable = LLVM.BuildAlloca(builder, LLVM.Int32Type(), "LeftVariableInit");
                    LLVM.BuildStore(builder, constInt, leftVariable);
                    leftVariable = LLVM.BuildLoad(builder, leftVariable, "LeftValueInCompare");
                }
            }
            else
            {
                LLVMValueRef constInt;
                long parsedValueInt = long.Parse(CalculatingTheExpression(left, ref _valueLocaleVariable)[0]);
                if (left[0][0] == '-')
                {
                    var zeroValue = LLVM.ConstInt(LLVM.Int32Type(), 0, new LLVMBool(0));
                    var absoluteValue =
                        LLVM.ConstInt(LLVM.Int32Type(), (ulong)Math.Abs(parsedValueInt), new LLVMBool(0));

                    constInt = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
                }
                else constInt = LLVM.ConstInt(LLVM.Int32Type(), (ulong)parsedValueInt, new LLVMBool(0));

                if (_valueLocaleVariable[insertBlock].FirstOrDefault(varib =>
                        varib.ContainsKey(left[0])).TryGetValue(left[0], out var tuple) &&
                    tuple.ValueRef.Pointer != IntPtr.Zero)
                    leftVariable = LLVM.BuildLoad(builder, tuple.ValueRef, "LeftValueInCompare");
                else
                {
                    leftVariable = LLVM.BuildAlloca(builder, LLVM.Int32Type(), "LeftVariableInit");
                    LLVM.BuildStore(builder, constInt, leftVariable);
                    leftVariable = LLVM.BuildLoad(builder, leftVariable, "LeftValueInCompare");
                }
            }

            if (right.Count == 1)
            {
                LLVMValueRef constInt;

                var blockWithVariable = FindBlockWithVariable(right[0], insertBlock, ref _valueLocaleVariable).Item1;

                if (!blockWithVariable.Equals(default(LLVMBasicBlockRef)))
                {
                    var tuple = _valueLocaleVariable[blockWithVariable]
                        .FirstOrDefault(item => item.ContainsKey(right[0]))[right[0]];
                    if (tuple.ValueRef.Pointer != IntPtr.Zero)
                    {
                        rightVariable = LLVM.BuildLoad(builder, tuple.ValueRef, "RightValueInCompare");
                    }
                }
                else
                {
                    if (right[0][0] == '-')
                    {
                        var zeroValue = LLVM.ConstInt(LLVM.Int32Type(), 0, new LLVMBool(0));
                        var absoluteValue = LLVM.ConstInt(LLVM.Int32Type(), (ulong)Math.Abs(long.Parse(right[0])),
                            new LLVMBool(0));
                        constInt = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
                    }
                    else
                    {
                        constInt = LLVM.ConstInt(LLVM.Int32Type(), (ulong)long.Parse(right[0]), new LLVMBool(0));
                    }

                    rightVariable = LLVM.BuildAlloca(builder, LLVM.Int32Type(), "RightVariableInit");
                    LLVM.BuildStore(builder, constInt, rightVariable);
                    rightVariable = LLVM.BuildLoad(builder, rightVariable, "RightValueInCompare");
                }
            }
            else
            {
                LLVMValueRef constInt;
                long parsedValueInt = long.Parse(CalculatingTheExpression(right, ref _valueLocaleVariable)[0]);
                if (right[0][0] == '-')
                {
                    var zeroValue = LLVM.ConstInt(LLVM.Int32Type(), 0, new LLVMBool(0));
                    var absoluteValue =
                        LLVM.ConstInt(LLVM.Int32Type(), (ulong)Math.Abs(parsedValueInt), new LLVMBool(0));

                    constInt = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
                }
                else constInt = LLVM.ConstInt(LLVM.Int32Type(), (ulong)parsedValueInt, new LLVMBool(0));

                if (_valueLocaleVariable[insertBlock].FirstOrDefault(varib =>
                        varib.ContainsKey(right[0])).TryGetValue(right[0], out var tuple) &&
                    tuple.ValueRef.Pointer != IntPtr.Zero)
                    rightVariable = LLVM.BuildLoad(builder, tuple.ValueRef, "RifgtValueInCompare");
                else
                {
                    rightVariable = LLVM.BuildAlloca(builder, LLVM.Int32Type(), "RighVariableInit");
                    LLVM.BuildStore(builder, constInt, rightVariable);
                    rightVariable = LLVM.BuildLoad(builder, rightVariable, "RightValueInCompare");
                }
            }

            LLVMValueRef icmpVariable = default(LLVMValueRef);
            switch (op)
            {
                case "==":
                    icmpVariable = LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntEQ, leftVariable, rightVariable,
                        "compare_eq");
                    break;
                case "!=":
                    icmpVariable = LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntNE, leftVariable, rightVariable,
                        "compare_ne");
                    break;
                case "<":
                    icmpVariable = LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntSLT, leftVariable, rightVariable,
                        "compare_lt");
                    break;
                case ">":
                    icmpVariable = LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntSGT, leftVariable, rightVariable,
                        "compare_gt");
                    break;
            }

            return icmpVariable;
        }
        public static Tuple<LLVMBasicBlockRef,LLVMValueRef> FindBlockWithVariable(in string variableName,
            LLVMBasicBlockRef insertBlock,
            ref Dictionary<LLVMBasicBlockRef, List<Dictionary<string, initialization>>> _valueLocaleVariable)
        {
            if (_valueLocaleVariable.ContainsKey(insertBlock))
            {
                var blockVariables = _valueLocaleVariable[insertBlock];
                foreach (var variablesDict in blockVariables)
                {
                    if (variablesDict.ContainsKey(variableName))
                    {
                        return new Tuple<LLVMBasicBlockRef, LLVMValueRef>(insertBlock,variablesDict[variableName].ValueRef);
                    }
                }
            }

            foreach (var block in _valueLocaleVariable.Keys.Except(new List<LLVMBasicBlockRef> { insertBlock }))
            {
                var blockVariables = _valueLocaleVariable[block];
                foreach (var variablesDict in blockVariables)
                {
                    if (variablesDict.ContainsKey(variableName))
                    {
                        return new Tuple<LLVMBasicBlockRef, LLVMValueRef>(insertBlock,variablesDict[variableName].ValueRef);
                    }
                }
            }

            WriteWrong($"Unknown variable \"{variableName}\"");
            return
                null; // Если переменная не найдена ни в одном блоке, возвращаем значение по умолчанию
        }
        public static List<string> CalculatingTheExpression(List<string> expr,
                     ref Dictionary<LLVMBasicBlockRef, List<Dictionary<string, initialization>>> _valueLocaleVariable,
                     string type = "int")
                 {
                     var insertBlock = LLVM.GetInsertBlock(builder);
                     Type targetType = null;
                     if (type == "int") targetType = typeof(int);
                     else if (type == "float") targetType = typeof(float);
         
                     if (expr.Count == 1)
                     {
                         expr[0] = expr[0].Replace(',', '.');
                         if (int.TryParse(expr[0], out int k) || (targetType == typeof(float) &&
                                                                  float.TryParse(
                                                                      ContainsDecimal(expr[0]) ? expr[0] : expr[0] + ".0",
                                                                      NumberStyles.Float, CultureInfo.InvariantCulture,
                                                                      out float n)))
                             return expr;
                         else if (char.IsLetter(expr[0][0]))
                         {
                             return _valueLocaleVariable[insertBlock].FirstOrDefault(dict => dict.ContainsKey(expr[0]))[expr[0]]
                                 .EXPR;
                         }
                     }
         
                     if (expr.Count == 0)
                         WriteWrong($"An uninitialized variable was used");
         
                     int j;
                     while (expr[2].Length >= 1 && Char.IsLetterOrDigit(char.Parse(expr[2][0].ToString())))
                     {
                         int k = 0;
                         for (;; k++)
                         {
                             if (expr[k].Length == 1)
                                 if (!char.IsLetterOrDigit(expr[k][0]))
                                     break;
                         }
         
                         AST intervalExpr = new AST();
                         intervalExpr.State = new state();
                         intervalExpr.State.Initialization = new initialization();
                         intervalExpr.State.Initialization.EXPR = new List<string>();
                         intervalExpr.State.Initialization.EXPR.AddRange(expr.GetRange(k - 2, 3));
                         intervalExpr.State.Initialization.TYPE = type;
         
                         expr.RemoveRange(k - 2, 3);
                         expr.Insert(k - 2,
                             CalculatingTheExpression(intervalExpr.State.Initialization.EXPR, ref _valueLocaleVariable,
                                 type)[0]);
                     }
         
                     string right = null;
                     string left = null;
         
                     AST tempAST = new AST();
                     tempAST.State = new state();
                     tempAST.State.Initialization = new initialization()
                     {
                         EXPR = new List<string>(),
                         TYPE = type,
                     };
                     if (targetType == typeof(int))
                     {
                         int i;
                         if (!int.TryParse(expr[0], out i))
                         {
                             try
                             {
                                 var blockWithVariable = FindBlockWithVariable(expr[0], insertBlock, ref _valueLocaleVariable).Item1;
         
                                 var RNAME = _valueLocaleVariable[blockWithVariable]
                                     .FirstOrDefault(item => item.ContainsKey(expr[0]))[expr[0]];
                                 if (i == 0 && RNAME == null) // i = 0 -> ExceptionParse | RNAME = null -> ExceptionFind
                                     WriteWrong($"The name \"  {expr[0]}  \" does not exist in the current context.");
                                 right = RNAME != null
                                     ? CalculatingTheExpression(RNAME.EXPR, ref _valueLocaleVariable, type)[0]
                                     : expr[0];
                             }
                             catch (Exception ex)
                             {
                                 WriteWrong($"The name \" {expr[0]} \" does not exist in the current context.");
                             }
                         }
                         else right = expr[0];
         
                         if (!int.TryParse(expr[1], out i))
                         {
                             try
                             {
                                 var blockWithVariable = FindBlockWithVariable(expr[1], insertBlock, ref _valueLocaleVariable).Item1;
         
                                 var LNAME = _valueLocaleVariable[blockWithVariable]
                                     .FirstOrDefault(item => item.ContainsKey(expr[1]))[expr[1]];
                                 if (i == 0 && LNAME == null) // i = 0 -> ExceptionParse | LNAME = null -> ExceptionFind
                                     WriteWrong($"The name \" {expr[1]} \" does not exist in the current context.");
                                 left = LNAME != null
                                     ? CalculatingTheExpression(LNAME.EXPR, ref _valueLocaleVariable, type)[0]
                                     : expr[1];
                             }
                             catch (Exception ex)
                             {
                                 WriteWrong($"The name \"{expr[1]}\" does not exist in the current context.");
                             }
                         }
                         else left = expr[1];
         
         
                         switch (char.Parse(expr[2]))
                         {
                             case '+':
                                 tempAST.State.Initialization.EXPR.Add((int.Parse(left) + int.Parse(right)).ToString());
                                 tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                                 return CalculatingTheExpression(tempAST.State.Initialization.EXPR, ref _valueLocaleVariable,
                                     type);
                             case '-':
                                 tempAST.State.Initialization.EXPR.Add((int.Parse(left) - int.Parse(right)).ToString());
                                 tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                                 return CalculatingTheExpression(tempAST.State.Initialization.EXPR, ref _valueLocaleVariable,
                                     type);
                             case '/':
                                 try
                                 {
                                     tempAST.State.Initialization.EXPR.Add((int.Parse(left) / int.Parse(right)).ToString());
                                 }
                                 catch (DivideByZeroException ex)
                                 {
                                     WriteWrong(ex.Message);
                                 }
         
                                 tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                                 return CalculatingTheExpression(tempAST.State.Initialization.EXPR, ref _valueLocaleVariable,
                                     type);
                             case '*':
                                 tempAST.State.Initialization.EXPR.Add((int.Parse(left) * int.Parse(right)).ToString());
                                 tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                                 return CalculatingTheExpression(tempAST.State.Initialization.EXPR, ref _valueLocaleVariable,
                                     type);
                         }
                     }
                     else if (targetType == typeof(float))
                     {
                         expr[0] = expr[0].Replace(',', '.');
                         float i;
                         if (!float.TryParse(ContainsDecimal(expr[0]) ? expr[0] : expr[0] + ".0", NumberStyles.Float,
                                 CultureInfo.InvariantCulture, out i))
                         {
                             try
                             {
                                 var blockWithVariable = FindBlockWithVariable(expr[0], insertBlock, ref _valueLocaleVariable).Item1;
         
                                 var RNAME = _valueLocaleVariable[blockWithVariable]
                                     .FirstOrDefault(item => item.ContainsKey(expr[0]))[expr[0]];
                                 if (i == 0 && RNAME == null) // i = 0 -> ExceptionParse | RNAME = null -> ExceptionFind
                                     WriteWrong($"The name \" {expr[0]} \" does not exist in the current context.");
                                 right = RNAME != null
                                     ? CalculatingTheExpression(RNAME.EXPR, ref _valueLocaleVariable, type)[0]
                                     : expr[0];
                             }
                             catch (Exception ex)
                             {
                                 WriteWrong($"The name \"{expr[0]}\" does not exist in the current context.");
                             }
                         }
                         else right = expr[0];
         
                         expr[1] = expr[1].Replace(',', '.');
                         if (!float.TryParse(ContainsDecimal(expr[1]) ? expr[1] : expr[1] + ".0", NumberStyles.Float,
                                 CultureInfo.InvariantCulture, out i))
                         {
                             try
                             {
                                 var blockWithVariable = FindBlockWithVariable(expr[1], insertBlock, ref _valueLocaleVariable).Item1;
         
                                 var LNAME = _valueLocaleVariable[blockWithVariable]
                                     .FirstOrDefault(item => item.ContainsKey(expr[1]))[expr[1]];
                                 if (i == 0 && LNAME == null) // i = 0 -> ExceptionParse | LNAME = null -> ExceptionFind
                                     WriteWrong($"The name \"  {expr[1]}  \" does not exist in the current context.");
                                 left = LNAME != null
                                     ? CalculatingTheExpression(LNAME.EXPR, ref _valueLocaleVariable, type)[0]
                                     : expr[1];
                             }
                             catch (Exception ex)
                             {
                                 WriteWrong($"The name \"{expr[1]}\" does not exist in the current context.");
                             }
                         }
                         else left = expr[1];
         
                         left = left.Replace(',', '.');
                         left = ContainsDecimal(left) ? left : left + ".0";
                         right = right.Replace(',', '.');
                         right = ContainsDecimal(right) ? right : right + ".0";
         
         
                         switch (char.Parse(expr[2]))
                         {
                             case '+':
                                 tempAST.State.Initialization.EXPR.Add(
                                     ((float)(float.Parse(left, NumberStyles.Float, CultureInfo.InvariantCulture) +
                                              float.Parse(right, NumberStyles.Float, CultureInfo.InvariantCulture))).ToString());
                                 tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                                 return CalculatingTheExpression(tempAST.State.Initialization.EXPR, ref _valueLocaleVariable,
                                     type);
                             case '-':
                                 tempAST.State.Initialization.EXPR.Add(
                                     ((float)(float.Parse(left, NumberStyles.Float, CultureInfo.InvariantCulture) -
                                              float.Parse(right, NumberStyles.Float, CultureInfo.InvariantCulture))).ToString());
                                 tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                                 return CalculatingTheExpression(tempAST.State.Initialization.EXPR, ref _valueLocaleVariable,
                                     type);
                             case '/':
                                 try
                                 {
                                     tempAST.State.Initialization.EXPR.Add(
                                         ((float)(float.Parse(left, NumberStyles.Float, CultureInfo.InvariantCulture) /
                                                  float.Parse(right, NumberStyles.Float, CultureInfo.InvariantCulture)))
                                         .ToString());
                                 }
                                 catch (DivideByZeroException ex)
                                 {
                                     WriteWrong(ex.Message);
                                 }
         
                                 tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                                 return CalculatingTheExpression(tempAST.State.Initialization.EXPR, ref _valueLocaleVariable,
                                     type);
                             case '*':
                                 tempAST.State.Initialization.EXPR.Add(
                                     ((float)(float.Parse(left, NumberStyles.Float, CultureInfo.InvariantCulture) *
                                              float.Parse(right, NumberStyles.Float, CultureInfo.InvariantCulture))).ToString());
                                 tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                                 return CalculatingTheExpression(tempAST.State.Initialization.EXPR, ref _valueLocaleVariable,
                                     type);
                         }
                     }
         
                     return null;
                 }
        private static bool ContainsDecimal(string str)
                 {
                     return str.Contains(".");
                 }
    }
}
using System;
using static Kompilyatory.Program;
using LLVMSharp;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using static Kompilyatory.LL;


namespace Kompilyatory
{
    public class Instructions
    {
        public static void CallFunction(CallFunction function,  Initialization variable, ref AreaOfVisibility local)
        {
            var getPuts = LLVM.GetNamedFunction(module, $"{function.ID}");
            if (getPuts.Pointer == IntPtr.Zero) WriteWrong($"Function \"{function.ID}\" does not exist");

            LLVMValueRef[] args = new LLVMValueRef[function.argc.Count];
            int index = 0;
            
            foreach (var argument in function.argc)
            {
                if (argument[0] == '$')
                {
                    var block = local.FindVariable(argument.Substring(1));
                    if (block == null) WriteWrong($"Unknown variable: {argument.Substring(1)}");
                    args[index] = block.ValueRef;
                }
                else
                {
                    LLVMValueRef functionValue = LLVM.GetNamedFunction(module, function.ID);
                    LLVMTypeRef functionType = LLVM.TypeOf(functionValue);
                    string str = argument;
                    args[index] = BuildValue(ref str, ref local);
                }
                index++;
            }

            if (variable != null)
            {
                if (!GetTypeRef(variable.type).Equals(getPuts.TypeOf().GetReturnType().GetElementType())) WriteWrong($"The data type of the return variable ({variable.Id}) must match the return type of the function ({function.ID})");

                variable.VariableRef = BuildVariable(ref variable);
                local.Function.Peek().variable.Add(variable.Id, variable);
                var build = LLVM.BuildCall(builder, getPuts, args, "");
                LLVM.BuildStore(builder,build,variable.VariableRef);
                variable.ValueRef = build;
            }
            else
            {
                LLVM.BuildCall(builder, getPuts, args, "");
            }
        }
        public static void BuildFunction(Function function)
        {
            LLVMTypeRef[] llvmTypeRefs = new LLVMTypeRef[function.args.Count];
            int index = 0;
            foreach (var variable in function.args)
            {
                llvmTypeRefs[index] = GetTypeRef(variable.type);
                index++;
            }

            LLVMTypeRef returnType = GetTypeRef(function.type);
            AreaOfVisibility Area = new AreaOfVisibility();
            
            var functionType = LLVM.FunctionType(returnType, llvmTypeRefs, false);
            var Function = LLVM.AddFunction(module, function.ID, functionType);
            var Block = LLVM.AppendBasicBlock(Function, function.ID);

            var block = new Blocks() { block = Block, variable = new Dictionary<string, Initialization>() };
            Area.Function.Push(block);
            LLVM.PositionBuilderAtEnd(builder, Block);
            
            for (int i = 0; i < function.args.Count; i++)
            {
                var paramValue = LLVM.GetParam(Function, (uint)i);
                LLVM.SetValueName(paramValue,$"argc_{i+1}");

                var variable = function.args[i];
                variable.VariableRef = BuildVariable(ref variable);
                variable.ValueRef = paramValue;
                LLVM.BuildStore(builder, paramValue, variable.VariableRef);
                block.variable.Add(function.args[i].Id,variable);
            }

            foreach (var item in function.body) item.HandlingStatus(ref Area);

            if (function.Return.Count() > 0)
            {
                if (function.type == "void")
                    WriteWrong(
                        $"Function \"{function.ID}\" can't return a value because it has a type \"{function.type}\"");
                if (function.Return.Count == 1)
                {
                    if (function.Return[0][0] == '$')
                    {
                        Initialization variableReturn = Area.FindVariable(function.Return[0].Substring(1));
                        if (variableReturn.type != function.type)
                            WriteWrong(
                                $"The data type of the return variable ({variableReturn.type}) must match the return type of the function ({function.type})");

                        LLVM.BuildRet(builder,variableReturn.ValueRef);
                    }
                    else
                    {
                        var ret = function.Return[0];
                        LLVM.BuildRet(builder,BuildValue(ref ret, ref Area, function.type));
                    }
                }
                else
                {
                    LLVM.BuildRet(builder,CalculatingTheExpression(function.Return, ref Area, function.type));
                }
            }
            else if (function.type == "void") LLVM.BuildRetVoid(builder);
            else WriteWrong($"Function return values do not match \"{function.ID}\"");
            
            LLVM.PositionBuilderAtEnd(builder, Block);
        }
        public static void ChangeValue(ChangeValue stateInfo,
            ref AreaOfVisibility valueLocaleVariable)
        {
            Initialization variable = valueLocaleVariable.FindVariable(stateInfo.ID);
            if (variable == null) WriteWrong($"Unknown variable: {stateInfo.ID}");
            LLVMTypeRef _lLVMType = GetTypeRef(variable.type);

            if (variable.type == "int")
            {
                if (stateInfo.expr.Count() == 1)
                {
                    if (stateInfo.expr[0][0] == '$')
                    {
                        var variableWithNewValue = valueLocaleVariable.FindVariable(stateInfo.expr[0].Substring(1));
                        if (variableWithNewValue.expr.Count == 0) WriteWrong($"An uninitialized variable was used");
                        if (variableWithNewValue.type != "int")
                        {
                            string initValue = variableWithNewValue.expr[0];
                            variable.ValueRef = BuildValue(ref initValue, ref valueLocaleVariable, "int");
                            LLVM.BuildStore(builder, variable.ValueRef, variable.VariableRef);
                            variable.expr = variableWithNewValue.expr;
                        }
                        else
                        {
                            variable.ValueRef = variableWithNewValue.ValueRef;
                            variable.expr = variableWithNewValue.expr;
                        }
                    }
                    else
                    {
                        variable.expr = stateInfo.expr;
                        string initValue = stateInfo.expr[0];
                        variable.ValueRef = BuildValue(ref initValue, ref valueLocaleVariable, variable.type);
                        LLVM.BuildStore(builder, variable.ValueRef, variable.VariableRef);
                    }
                }
                else
                {
                    var value = CalculatingTheExpression(stateInfo.expr, ref valueLocaleVariable, variable.type);

                    if (variable.type == "int") value = LLVM.BuildFPToSI(builder, value, GetTypeRef(variable.type), "");
                    else value = LLVM.BuildSIToFP(builder, value ,GetTypeRef(variable.type), "");
                    LLVM.BuildStore(builder, value, variable.VariableRef);
                    variable.ValueRef = value;
                }
            }
            else
            {
                if (stateInfo.expr.Count() == 1)
                {
                    if (stateInfo.expr[0][0] == '$')
                    {
                        var variableWithNewValue = valueLocaleVariable.FindVariable(stateInfo.expr[0].Substring(1));
                        if (variableWithNewValue.expr.Count == 0) WriteWrong($"An uninitialized variable was used");
                        if (variableWithNewValue.type != "float")
                        {
                            string initValue = variableWithNewValue.expr[0];
                            variable.ValueRef = BuildValue(ref initValue, ref valueLocaleVariable, "float");
                            LLVM.BuildStore(builder, variable.ValueRef, variable.VariableRef);
                            variable.expr = variableWithNewValue.expr;
                        }
                        else
                        {
                            variable.ValueRef = variableWithNewValue.ValueRef;
                            variable.expr = variableWithNewValue.expr;
                        }
                    }
                    else
                    {
                        variable.expr = stateInfo.expr;
                        string initValue = stateInfo.expr[0];
                        variable.ValueRef = BuildValue(ref initValue, ref valueLocaleVariable, variable.type);
                        LLVM.BuildStore(builder, variable.ValueRef, variable.VariableRef);
                    }
                }
                else
                {
                    var value = CalculatingTheExpression(stateInfo.expr, ref valueLocaleVariable, variable.type);

                    if (variable.type == "int") value = LLVM.BuildFPToSI(builder, value, GetTypeRef(variable.type), "");
                    else value = LLVM.BuildSIToFP(builder, value ,GetTypeRef(variable.type), "");
                    LLVM.BuildStore(builder, value, variable.VariableRef);
                    variable.ValueRef = value;
                }
            }
        }
        public static void _while(While stateWhile, ref AreaOfVisibility valueLocaleVariable)
        {
            LLVMBasicBlockRef conditionBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_cond");
            LLVMBasicBlockRef loopBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_loop");
            LLVMBasicBlockRef endBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_end");

            LLVM.BuildBr(builder, conditionBlock);

            LLVM.PositionBuilderAtEnd(builder, conditionBlock);

            LLVM.BuildCondBr(builder, _BuildEquation(stateWhile.left, stateWhile.right, stateWhile.Operator, ref valueLocaleVariable), loopBlock, endBlock);

            LLVM.PositionBuilderAtEnd(builder, loopBlock);
            Blocks blockIf = new Blocks() { block = loopBlock,variable = new Dictionary<string, Initialization>()};
            valueLocaleVariable.Function.Push(blockIf);
            foreach (AST astNode in stateWhile.body) astNode.HandlingStatus(ref valueLocaleVariable);
            valueLocaleVariable.Function.Pop();
            LLVM.BuildBr(builder, conditionBlock);

            LLVM.PositionBuilderAtEnd(builder, endBlock);
        }
        public static void _if(If stateIf, ref AreaOfVisibility valueLocaleVariable)
        {
            LLVMBasicBlockRef ifTrue = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(valueLocaleVariable.Function.Peek().block), "if_true");
            LLVMBasicBlockRef ifFalse = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(valueLocaleVariable.Function.Peek().block), "if_false");
            LLVMBasicBlockRef EndBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(valueLocaleVariable.Function.Peek().block), "endBlockIf");

            LLVM.BuildCondBr(builder,
                _BuildEquation(stateIf.left, stateIf.right, stateIf.Operator, ref valueLocaleVariable), ifTrue,
                ifFalse);

            // IF
            LLVM.PositionBuilderAtEnd(builder, ifTrue);
            Blocks blockIf = new Blocks() { block = ifTrue, variable = new Dictionary<string, Initialization>()};
            valueLocaleVariable.Function.Push(blockIf);
            foreach (AST astNode in stateIf.body) astNode.HandlingStatus(ref valueLocaleVariable);
            valueLocaleVariable.Function.Pop();
            LLVM.BuildBr(builder, EndBlock);

            // ELSE
            LLVM.PositionBuilderAtEnd(builder, ifFalse);
            Blocks blockElse = new Blocks() { block = ifFalse,variable = new Dictionary<string, Initialization>()}; valueLocaleVariable.Function.Push(blockElse);
            foreach (AST astNode in stateIf.Else["body"]) astNode.HandlingStatus(ref valueLocaleVariable);
            valueLocaleVariable.Function.Pop();
            LLVM.BuildBr(builder, EndBlock);

            LLVM.PositionBuilderAtEnd(builder, EndBlock);
        }
        public static void _doWhile( DoWhile stateDoWhile,
            ref AreaOfVisibility valueLocaleVariable)
        {
            LLVMBasicBlockRef conditionBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_cond");
            LLVMBasicBlockRef loopBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_loop");
            LLVMBasicBlockRef endBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_end");

            LLVM.BuildBr(builder, loopBlock);

            LLVM.PositionBuilderAtEnd(builder, loopBlock);
            Blocks blockIf = new Blocks() { block = loopBlock,variable = new Dictionary<string, Initialization>()};
            valueLocaleVariable.Function.Push(blockIf);
            foreach (AST astNode in stateDoWhile.body) astNode.HandlingStatus(ref valueLocaleVariable);
            valueLocaleVariable.Function.Pop();
            LLVM.BuildBr(builder, conditionBlock);

            LLVM.PositionBuilderAtEnd(builder, conditionBlock);

            LLVM.BuildCondBr(builder, _BuildEquation(stateDoWhile.left,stateDoWhile.right,stateDoWhile.Operator,ref valueLocaleVariable), loopBlock, endBlock);

            LLVM.PositionBuilderAtEnd(builder, endBlock);
        }
        public static void _for(For @for,
            ref AreaOfVisibility valueLocaleVariable)
        {

            LLVMBasicBlockRef conditionBlock =
                LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "for_cond");
            LLVMBasicBlockRef loopBlock =
                LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "for_loop");
            LLVMBasicBlockRef bodyBlock =
                LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "for_body");
            LLVMBasicBlockRef endBlock =
                LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "for_end");

            LLVM.BuildBr(builder, conditionBlock);
            LLVM.PositionBuilderAtEnd(builder, conditionBlock);

            Blocks blockIf = new Blocks()
                { block = conditionBlock, variable = new Dictionary<string, Initialization>() };
            valueLocaleVariable.Function.Push(blockIf);

            if (@for.Init.Id != "")
            {
                var variable = @for.Init;
                Initialization(ref variable, ref valueLocaleVariable);
            }

        LLVM.BuildBr(builder, loopBlock);
            LLVM.PositionBuilderAtEnd(builder, loopBlock);

            LLVM.BuildCondBr(builder, _BuildEquation(@for.Equation.left, @for.Equation.right, @for.Equation.Operator, ref valueLocaleVariable), bodyBlock, endBlock);
            
            Blocks blockBody = new Blocks() { block = bodyBlock,variable = new Dictionary<string, Initialization>()};
            LLVM.PositionBuilderAtEnd(builder, bodyBlock);
            valueLocaleVariable.Function.Push(blockBody);

            foreach (AST astNode in @for.body) astNode.HandlingStatus(ref valueLocaleVariable);
            ChangeValue(@for.changeValue, ref valueLocaleVariable);
            valueLocaleVariable.Function.Pop();
            valueLocaleVariable.Function.Pop();
            LLVM.BuildBr(builder, loopBlock);
            LLVM.PositionBuilderAtEnd(builder, endBlock);
        }
        public static void Initialization(ref Initialization initValue, ref AreaOfVisibility blocks)
        {
            var insertBlock = LLVM.GetInsertBlock(builder);
            
            if (blocks.FindVariable(initValue.Id) != null )
                WriteWrong($"A variable named \"{initValue.Id}\" already exists");
            
            
            if (initValue.func != null)
                if (initValue.func.ID != null) {CallFunction(initValue.func,initValue,ref blocks); return;}
                
      
            var variable = BuildVariable(ref initValue);
            initValue.VariableRef = variable;
            blocks.Function.Peek().variable.Add(initValue.Id, initValue );

            if (initValue.expr.Count == 0) initValue.expr = new List<string>();
            else if (initValue.expr.Count == 1)
            {
                if (initValue.expr[0][0] != '$')
                {
                    StoreValue(ref initValue,ref blocks);
                }
                else
                {
                    initValue.ValueRef = CalculatingTheExpression(initValue.expr, ref blocks, initValue.type);
                    initValue.expr = new List<string>();
                    initValue.expr.Add(GetValue(initValue.ValueRef.GetValueName()));
                    
                    LLVM.BuildStore(builder,initValue.ValueRef,initValue.VariableRef);
                }
            }
            else
            {
                initValue.ValueRef = CalculatingTheExpression(initValue.expr, ref blocks, initValue.type);
                initValue.expr = new List<string>();
                initValue.expr.Add(GetValue(initValue.ValueRef.GetValueName()));
                
                LLVM.BuildStore(builder,initValue.ValueRef,initValue.VariableRef);
            }

        }
        public static void Display(string outputString,in string typeOutputString, ref AreaOfVisibility valueLocaleVariable, ref List<LLVMValueRef> args ,string typeExpr = "int", Initialization variable = null)
        {
            LLVMValueRef formatString = default;

            if (typeOutputString == "variable")
            {
                
                if (variable.type == "float")
                {
                    var doubleValue = LLVM.BuildFPExt(builder, variable.ValueRef,
                        LLVM.DoubleType(), "");
                    args.Add(doubleValue);
                }
                else
                {
                    args.Add(LoadValue(variable.VariableRef));
                }
            }
            else if (typeOutputString == "expr")
            {
                args.Add(outputString.Contains('%') ? valueRefs.Pop() :  BuildValue(ref outputString, ref valueLocaleVariable,typeExpr));
            }
            else
            {
                args.Add(LLVM.BuildGlobalStringPtr(builder, $"{outputString}",
                    $"Writeln_value_{outputString}_string"));
                // args = new LLVMValueRef[] { formatString };
            }

            // var getPuts = LLVM.GetNamedFunction(module, "printf");
            // LLVM.BuildCall(builder, getPuts, args, $"");
        }
        private static LLVMValueRef LoadValue(LLVMValueRef val)
        {
            return LLVM.BuildLoad(builder, val, "");
        }
    }
}
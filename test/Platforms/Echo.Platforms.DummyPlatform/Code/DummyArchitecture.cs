using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
using Echo.Core.Code;
using Echo.Platforms.DummyPlatform.ControlFlow;

namespace Echo.Platforms.DummyPlatform.Code
{
    public class DummyArchitecture : IInstructionSetArchitecture<DummyInstruction>
    {
        public static DummyArchitecture Instance
        {
            get;
        } = new DummyArchitecture();

        public IStaticSuccessorResolver<DummyInstruction> SuccessorResolver
        {
            get;
        } = new DummyStaticSuccessorResolver();
        
        public long GetOffset(DummyInstruction instruction) =>
            instruction.Offset;

        public string GetMnemonic(DummyInstruction instruction) => 
            instruction.Mnemonic;

        public int GetOperandCount(DummyInstruction instruction) => 
            instruction.Operands.Count;

        public object GetOperand(DummyInstruction instruction, int index) => 
            instruction.Operands[index];

        public int GetSize(DummyInstruction instruction) => 1;

        public byte[] GetOpCodeBytes(DummyInstruction instruction) =>
            throw new System.NotImplementedException();

        public byte[] GetOperandBytes(DummyInstruction instruction) =>
            throw new System.NotImplementedException();

        public byte[] GetInstructionBytes(DummyInstruction instruction) =>
            throw new System.NotImplementedException();

        public InstructionFlowControl GetFlowControl(DummyInstruction instruction) =>
            instruction.OpCode switch
            {
                DummyOpCode.Jmp => InstructionFlowControl.CanBranch,
                DummyOpCode.JmpCond => InstructionFlowControl.CanBranch,
                DummyOpCode.Ret => InstructionFlowControl.IsTerminator,
                DummyOpCode.Switch => InstructionFlowControl.CanBranch,
                _ => InstructionFlowControl.Fallthrough
            };

        public int GetStackPushCount(DummyInstruction instruction) => 
            instruction.PushCount;

        public int GetStackPopCount(DummyInstruction instruction) =>
            instruction.PopCount;

        public IEnumerable<IVariable> GetReadVariables(DummyInstruction instruction) =>
            Enumerable.Empty<IVariable>();

        public IEnumerable<IVariable> GetWrittenVariables(DummyInstruction instruction) =>
            Enumerable.Empty<IVariable>();
    }
}
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Br"/> and <see cref="CilOpCodes.Br_S"/>
    /// operation codes.
    /// </summary>
    public class Br : BranchHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Br, CilCode.Br_S
        };

        /// <inheritdoc />
        protected override bool? VerifyCondition(ExecutionContext context, CilInstruction instruction) => true;
    }
}
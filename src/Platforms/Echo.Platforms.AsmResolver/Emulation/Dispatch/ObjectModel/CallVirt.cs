using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Callvirt"/> operation code.
    /// </summary>
    public class CallVirt : CallBase
    {
        private static readonly SignatureComparer Comparer = new SignatureComparer();

        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Callvirt
        };

        /// <inheritdoc />
        protected override MethodDevirtualizationResult DevirtualizeMethod(CilInstruction instruction, IList<ICliValue> arguments)
        {
            var method = ((IMethodDescriptor) instruction.Operand).Resolve();

            switch (arguments[0])
            {
                case { IsZero: true }:
                    return new MethodDevirtualizationResult(new NullReferenceException());
                
                case OValue { ReferencedObject: IDotNetValue referencedObject }:
                    var implementation = FindMethodImplementationInType(referencedObject.Type.Resolve(), method);
                    return implementation is null
                        ? new MethodDevirtualizationResult(new MissingMethodException())
                        : new MethodDevirtualizationResult(implementation);

                default:
                    return new MethodDevirtualizationResult(new InvalidProgramException());
            }
        }

        private static MethodDefinition FindMethodImplementationInType(TypeDefinition type, MethodDefinition baseMethod)
        {
            if (!baseMethod.IsVirtual)
                return baseMethod;

            var implementation = default(MethodDefinition);
            var declaringType = baseMethod.DeclaringType;
            while (type is {} && !Comparer.Equals(type, declaringType))
            {
                // Prioritize explicit interface implementations.
                if (declaringType.IsInterface)
                    implementation = TryFindExplicitInterfaceImplementationInType(type, baseMethod);

                // Try find any implicit implementations.
                implementation ??= TryFindImplicitImplementationInType(type, baseMethod);
                
                if (implementation is {})
                    break;
                
                // Move up type hierarchy tree.
                type = type.BaseType?.Resolve();
            }

            return implementation;
        }

        private static MethodDefinition TryFindImplicitImplementationInType(TypeDefinition type, MethodDefinition baseMethod)
        {
            foreach (var method in type.Methods)
            {
                if (method.IsVirtual
                    && method.IsReuseSlot
                    && method.Name == baseMethod.Name
                    && Comparer.Equals(method.Signature, baseMethod.Signature))
                {
                    return method;
                }
            }

            return null;
        }

        private static MethodDefinition TryFindExplicitInterfaceImplementationInType(TypeDefinition type, MethodDefinition baseMethod)
        {
            foreach (var impl in type.MethodImplementations)
            {
                if (Comparer.Equals(baseMethod, impl.Declaration))
                    return impl.Body.Resolve();
            }

            return null;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Blocks;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
using Echo.ControlFlow.Regions;
using Echo.ControlFlow.Regions.Detection;
using Echo.ControlFlow.Serialization.Blocks;
using Echo.Core.Code;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.ControlFlow.Tests.Serialization.Blocks
{
    public class BlockBuilderTest
    {
        [Fact]
        public void FlatGraphShouldProduceFlatBlock()
        {
            var instructions = new[]
            {
                DummyInstruction.Op(0, 0, 0),
                DummyInstruction.Op(1, 0, 0),
                DummyInstruction.Op(2, 0, 0),
                DummyInstruction.Op(3, 0, 0),
                DummyInstruction.Ret(4),
            };

            var cfgBuilder = new StaticFlowGraphBuilder<DummyInstruction>(
                DummyArchitecture.Instance,
                instructions,
                DummyArchitecture.Instance.SuccessorResolver);

            var cfg = cfgBuilder.ConstructFlowGraph(0);
            
            var blockBuilder = new BlockBuilder<DummyInstruction>();
            var rootScope = blockBuilder.ConstructBlocks(cfg);
            
            Assert.Single(rootScope.Blocks);
            Assert.IsAssignableFrom<BasicBlock<DummyInstruction>>(rootScope.Blocks[0]);
            Assert.Equal(instructions, ((BasicBlock<DummyInstruction>) rootScope.Blocks[0]).Instructions);
        }
        
        [Fact]
        public void BasicRegionShouldTranslateToSingleScopeBlock()
        {
            var instructions = new[]
            {
                DummyInstruction.Push(0, 1),
                DummyInstruction.JmpCond(1, 4),
                
                DummyInstruction.Op(2, 0, 0),
                DummyInstruction.Jmp(3, 4),
                
                DummyInstruction.Op(4, 0, 0),
                DummyInstruction.Ret(5),
            };

            var cfgBuilder = new StaticFlowGraphBuilder<DummyInstruction>(
                DummyArchitecture.Instance,
                instructions,
                DummyArchitecture.Instance.SuccessorResolver);

            var cfg = cfgBuilder.ConstructFlowGraph(0);
            
            var region = new BasicControlFlowRegion<DummyInstruction>();
            cfg.Regions.Add(region);
            region.Nodes.Add(cfg.Nodes[2]);
            
            var blockBuilder = new BlockBuilder<DummyInstruction>();
            var rootScope = blockBuilder.ConstructBlocks(cfg);
            
            Assert.Equal(3, rootScope.Blocks.Count);
            Assert.IsAssignableFrom<BasicBlock<DummyInstruction>>(rootScope.Blocks[0]);
            Assert.IsAssignableFrom<ScopeBlock<DummyInstruction>>(rootScope.Blocks[1]);
            Assert.IsAssignableFrom<BasicBlock<DummyInstruction>>(rootScope.Blocks[2]);
            
            var order = rootScope.GetAllBlocks().ToArray();
            Assert.Equal(
                new long[] {0, 2, 4}, 
                order.Select(b => b.Offset));
        }

        [Fact]
        public void Loop()
        {
            var instructions = new[]
            {
                DummyInstruction.Push(0, 1),
                
                DummyInstruction.Op(1, 0, 0),
                DummyInstruction.Op(2, 0, 0),
                DummyInstruction.JmpCond(3, 1),
                
                DummyInstruction.Op(4, 0, 0),
                DummyInstruction.Ret(5),
            };

            var cfgBuilder = new StaticFlowGraphBuilder<DummyInstruction>(
                DummyArchitecture.Instance,
                instructions,
                DummyArchitecture.Instance.SuccessorResolver);

            var cfg = cfgBuilder.ConstructFlowGraph(0);
            var blockBuilder = new BlockBuilder<DummyInstruction>();
            var rootScope = blockBuilder.ConstructBlocks(cfg);
            
            var order = rootScope.GetAllBlocks().ToArray();
            Assert.Equal(
                new long[] {0, 1, 4}, 
                order.Select(b => b.Offset));
        }

        [Fact]
        public void WeirdOrder()
        {
            var instructions = new[]
            {
                DummyInstruction.Op(0, 0, 0),
                DummyInstruction.Op(1, 0, 0),
                DummyInstruction.Op(2, 0, 0),
                DummyInstruction.Jmp(3, 6),
                
                DummyInstruction.Op(4, 0, 0),
                DummyInstruction.Ret(5),
                
                DummyInstruction.Op(6, 0, 0),
                DummyInstruction.Op(7, 0, 0),
                DummyInstruction.Jmp(8, 15),
                
                DummyInstruction.Op(9, 0, 0),
                DummyInstruction.Op(10, 0, 0),
                DummyInstruction.Jmp(11, 4),
                
                DummyInstruction.Op(12, 0, 0),
                DummyInstruction.Op(13, 0, 0),
                DummyInstruction.Jmp(14, 9),
                
                DummyInstruction.Op(15, 0, 0),
                DummyInstruction.Op(16, 0, 0),
                DummyInstruction.Jmp(17, 12),
            };

            var cfgBuilder = new StaticFlowGraphBuilder<DummyInstruction>(
                DummyArchitecture.Instance,
                instructions,
                DummyArchitecture.Instance.SuccessorResolver);
                
            var cfg = cfgBuilder.ConstructFlowGraph(0);
            var blockBuilder = new BlockBuilder<DummyInstruction>();
            var rootScope = blockBuilder.ConstructBlocks(cfg);
            
            var order = rootScope.GetAllBlocks().ToArray();
            Assert.Equal(
                new long[] {0,6,15,12,9,4} ,
                order.Select(b => b.Offset));
        }
        

        private static ControlFlowGraph<DummyInstruction> ConstructGraphWithEHRegions(IEnumerable<DummyInstruction> instructions, IEnumerable<ExceptionHandlerRange> ranges)
        {
            var architecture = DummyArchitecture.Instance;
            var builder = new StaticFlowGraphBuilder<DummyInstruction>(
                architecture,
                instructions,
                architecture.SuccessorResolver);

            var rangesArray = ranges as ExceptionHandlerRange[] ?? ranges.ToArray();
            var cfg = builder.ConstructFlowGraph(0, rangesArray);
            cfg.DetectExceptionHandlerRegions(rangesArray);
            
            return cfg;
        }
        
        [Fact]
        public void ExceptionHandler()
        {
            var instructions = new[]
            {
                DummyInstruction.Op(0, 0, 0),
                
                // try start
                DummyInstruction.Op(1, 0, 0),
                DummyInstruction.Jmp(2, 5),
                
                // handler start
                DummyInstruction.Op(3, 0, 0),
                DummyInstruction.Jmp(4, 5),
                
                DummyInstruction.Ret(5),
            };
            
            var ranges = new[]
            {
                new ExceptionHandlerRange(new AddressRange(1, 3), new AddressRange(3, 5)),
            };

            var cfg = ConstructGraphWithEHRegions(instructions, ranges);
            var blockBuilder = new BlockBuilder<DummyInstruction>();
            var rootScope = blockBuilder.ConstructBlocks(cfg);
            
            var order = rootScope.GetAllBlocks().ToArray();
            Assert.Equal(
                new long[] {0, 1, 3, 5}, 
                order.Select(b => b.Offset));
        }
    }
}
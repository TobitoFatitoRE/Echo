﻿using Echo.Core.Graphing;

namespace Echo.DataFlow
{
    /// <summary>
    /// Represents an edge between two nodes in a data flow graph (DFG). The origin of the node represents the dependant,
    /// and the target of the node represents the dependency.
    /// </summary>
    /// <typeparam name="TContents">The type of information to store in each data flow node.</typeparam>
    public readonly struct DataFlowEdge<TContents> : IEdge
    {
        /// <summary>
        /// Creates a new dependency edge between two nodes.
        /// </summary>
        /// <param name="origin">The dependent node.</param>
        /// <param name="target">The dependency node.</param>
        /// <param name="type">The type of dependency.</param>
        public DataFlowEdge(DataFlowNode<TContents> origin, DataFlowNode<TContents> target, DataDependencyType type)
        {
            Origin = origin;
            Target = target;
            Type = type;
        }
        
        /// <summary>
        /// Gets the node this edge starts at. This represents the dependent node. 
        /// </summary>
        public DataFlowNode<TContents> Origin
        {
            get;
        }
        
        INode IEdge.Origin => Origin;

        /// <summary>
        /// Gets the node that this edge points to in the data flow graph. THis represents the dependency node.
        /// </summary>
        public DataFlowNode<TContents> Target
        {
            get;
        }

        INode IEdge.Target => Target;

        /// <summary>
        /// Gets the type of dependency that this edge encodes.
        /// </summary>
        public DataDependencyType Type
        {
            get;
        }
    }
}
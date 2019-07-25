using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Echo.ControlFlow.Serialization.Dot
{
    /// <summary>
    /// Provides a mechanism for writing graphs to a character stream using the dot file format.
    /// </summary>
    public class DotWriter
    {
        private static readonly IDictionary<char, string> EscapedCharacters = new Dictionary<char, string>
        {
            ['\r'] = "\\\r",
            ['\n'] = "\\\n",
            ['"'] = "\\\"",
            ['\t'] = "\\\t",
        };

        /// <summary>
        /// Creates a new dot writer. 
        /// </summary>
        /// <param name="writer">The writer responsible for writing the output.</param>
        public DotWriter(TextWriter writer)
        {
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        /// <summary>
        /// Gets the writer that is used to write textual data to the output stream.
        /// </summary>
        protected TextWriter Writer
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether nodes in the output file should be explicitly defined before the
        /// edges are defined.
        /// </summary>
        public bool SeparateNodesAndEdges
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Gets or sets a value indicating whether statements in the output file should be separated by semicolons.
        /// </summary>
        public bool IncludeSemicolons
        {
            get;
            set;
        } = true;
        
        /// <summary>
        /// Writes a graph to the character stream.
        /// </summary>
        /// <param name="graph">The graph to write.</param>
        public void Write(IGraph graph)
        {
            WriteHeader();
            
            // Nodes
            var nodeIdentifiers = new Dictionary<INode, string>();
            foreach (var node in graph.GetNodes())
            {
                string identifier = nodeIdentifiers.Count.ToString();
                nodeIdentifiers[node] = identifier;
                
                if (SeparateNodesAndEdges
                    || !node.GetIncomingEdges().Any() && !node.GetOutgoingEdges().Any())
                {
                    Write(node, identifier);
                }
            }

            // Edges
            foreach (var edge in graph.GetEdges())
                Write(nodeIdentifiers, edge);

            WriteFooter();
        }

        /// <summary>
        /// Appends the header of a new graph to the output stream.
        /// </summary>
        protected virtual void WriteHeader()
        {
            Writer.WriteLine("digraph {");
        }

        /// <summary>
        /// Appends the footer of a graph to the output stream.
        /// </summary>
        private void WriteFooter()
        {
            Writer.WriteLine("}");
        }

        /// <summary>
        /// Appends a single node definition to the output stream.
        /// </summary>
        /// <param name="node">The node to append.</param>
        /// <param name="identifier">The identifier to use when referencing this node.</param>
        protected virtual void Write(INode node, string identifier)
        {
            WriteIdentifier(identifier);
            WriteSemicolon();
            Writer.WriteLine();
        }

        /// <summary>
        /// Appends an edge to the output stream.
        /// </summary>
        /// <param name="nodeIdentifiers">A mapping between nodes and their identifiers.</param>
        /// <param name="edge">The edge to append.</param>
        protected virtual void Write(IDictionary<INode, string> nodeIdentifiers, IEdge edge)
        {
            WriteIdentifier(nodeIdentifiers[edge.Origin]);
            Writer.Write(" -> ");
            WriteIdentifier(nodeIdentifiers[edge.Target]);

            switch (edge.Type)
            {
                case EdgeType.FallThrough:
                    break;
                case EdgeType.Conditional:
                    Writer.Write("[color=red]");
                    break;
                case EdgeType.Abnormal:
                    Writer.Write("[color=gray, style=dashed]");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            WriteSemicolon();
            
            Writer.WriteLine();
        }

        /// <summary>
        /// Appends a single identifier to the output stream.
        /// </summary>
        /// <param name="text">The identifier to write.</param>
        protected void WriteIdentifier(string text)
        {
            if (!NeedsEscaping(text))
            {
                Writer.Write(text);
            }
            else
            {
                Writer.Write('"');
                foreach (char c in text)
                    WriteEscapedCharacter(c);
                Writer.Write('"');
            }
        }
        
        /// <summary>
        /// Appends a semicolon to the output stream, depending on the value of <see cref="IncludeSemicolons"/>.
        /// </summary>
        protected void WriteSemicolon()
        {
            if (IncludeSemicolons)
                Writer.Write(';');
        }

        /// <summary>
        /// Determines whether an identifier requires escaping.
        /// </summary>
        /// <param name="text">The identifier to test.</param>
        /// <returns><c>True</c> if the identifier needs escaping, <c>false</c> otherwise.</returns>
        protected static bool NeedsEscaping(string text)
        {
            return text.ToCharArray().Any(c => EscapedCharacters.ContainsKey(c) || !char.IsLetterOrDigit(c));
        }

        /// <summary>
        /// Appends a single character to the output stream, and escapes it when necessary.
        /// </summary>
        /// <param name="c">The character to write.</param>
        protected void WriteEscapedCharacter(char c)
        {
            if (EscapedCharacters.TryGetValue(c, out string escaped))
                Writer.Write(escaped);
            else
                Writer.Write(c);
        }
        
    }
}
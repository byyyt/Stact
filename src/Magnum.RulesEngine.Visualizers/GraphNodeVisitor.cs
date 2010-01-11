// Copyright 2007-2008 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Magnum.RulesEngine.Visualizers
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Linq.Expressions;
	using System.Reflection;
	using CollectionExtensions;
	using ExecutionModel;
	using Microsoft.Glee.Drawing;
	using Microsoft.Glee.GraphViewerGdi;
	using QuickGraph;
	using QuickGraph.Algorithms.Search;
	using QuickGraph.Algorithms.ShortestPath;
	using QuickGraph.Glee;
	using Color=Microsoft.Glee.Drawing.Color;

	public class GraphNodeVisitor :
		AbstractModelVisitor<GraphNodeVisitor>
	{
		private readonly AdjacencyGraph<NodeVertex, Edge<NodeVertex>> _graph = new AdjacencyGraph<NodeVertex, Edge<NodeVertex>>();
		private readonly Dictionary<int, NodeVertex> _nodes = new Dictionary<int, NodeVertex>();
		private Dictionary<Type, Color> _colors;
		private NodeVertex _lastNodeVertex;
		private NodeVertex _root;
		private Stack<NodeVertex> _stack = new Stack<NodeVertex>();

		public GraphNodeVisitor()
		{
			_colors = new Dictionary<Type, Color>
				{
					{typeof (AlphaNode<>), Color.Red},
					{typeof(TypeNode<>), Color.Orange},
					{typeof (JoinNode<>), Color.Green},
					{typeof (ConditionNode<>), Color.Blue},
					{typeof (ActionNode<>), Color.Teal},
					{typeof (ConstantNode<>), Color.Magenta},
				};
		}

		public void GetGraph(int width, int height, string filename)
		{
			GleeGraphPopulator<NodeVertex, Edge<NodeVertex>> glee = _graph.CreateGleePopulator();

			glee.NodeAdded += NodeStyler;
			glee.EdgeAdded += EdgeStyler;
			glee.Compute();

			Graph gleeGraph = glee.GleeGraph;

			var renderer = new GraphRenderer(gleeGraph);
			renderer.CalculateLayout();

			var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			renderer.Render(bitmap);

			Trace.WriteLine("Saving graph to " + filename);

			bitmap.Save(filename, ImageFormat.Png);
		}

		public void ComputeDepthFirstSearch()
		{
			var algorithm = new DepthFirstSearchAlgorithm<NodeVertex, Edge<NodeVertex>>(_graph);

			algorithm.Compute();
		}

		public void ComputeShortestPath()
		{
			var algorithm = new DijkstraShortestPathAlgorithm<NodeVertex, Edge<NodeVertex>>(_graph, x => 1);

			algorithm.Compute();
		}

		protected override void IncreaseDepth()
		{
			if (_lastNodeVertex != null)
				_stack.Push(_lastNodeVertex);
		}

		protected override void DecreaseDepth()
		{
			if (_stack.Count > 0)
				_stack.Pop();
		}

		protected bool Visit(TypeDispatchNode node)
		{
			_lastNodeVertex = GetSink(node.GetHashCode(), () => "Rules", typeof (object), typeof (TypeDispatchNode));
			_root = _lastNodeVertex;

			if (_stack.Count > 0)
				_graph.AddEdge(new Edge<NodeVertex>(_stack.Peek(), _lastNodeVertex));

			return true;
		}

		protected bool Visit<T>(TypeNode<T> node)
		{
			_lastNodeVertex = GetSink(node.GetHashCode(), () => typeof (T).Name, typeof (T), typeof (TypeNode<>));

			if (_stack.Count > 0)
				_graph.AddEdge(new Edge<NodeVertex>(_stack.Peek(), _lastNodeVertex));

			return true;
		}

		protected bool Visit<T>(ConditionNode<T> node)
		{
			_lastNodeVertex = GetSink(node.GetHashCode(), () => GetExpressionBody(node.Expression), typeof (T), typeof (ConditionNode<>));

			if (_stack.Count > 0)
				_graph.AddEdge(new Edge<NodeVertex>(_stack.Peek(), _lastNodeVertex));

			return true;
		}

		protected bool Visit<T>(AlphaNode<T> node)
		{
			_lastNodeVertex = GetSink(node.GetHashCode(), () => "\u03B1", typeof(T), typeof(AlphaNode<>));

			if (_stack.Count > 0 && _stack.Peek().NodeType != typeof(JoinNode<>))
				_graph.AddEdge(new Edge<NodeVertex>(_stack.Peek(), _lastNodeVertex));

			return true;
		}

		protected bool Visit<T>(JoinNode<T> node)
		{
			_lastNodeVertex = GetSink(node.GetHashCode(), () => "J", typeof (T), typeof (JoinNode<>));

			if(_nodes.ContainsKey(node.RightActivation.GetHashCode()))
			{
				var sink = _nodes[node.RightActivation.GetHashCode()];

				_graph.AddEdge(new Edge<NodeVertex>(sink, _lastNodeVertex));
			}

			if (_stack.Count > 0)
				_graph.AddEdge(new Edge<NodeVertex>(_stack.Peek(), _lastNodeVertex));

			return true;
		}

		protected bool Visit<T>(ConstantNode<T> node)
		{
			_lastNodeVertex = GetSink(node.GetHashCode(), () => "C", typeof (T), typeof (ConstantNode<>));

			if (_stack.Count > 0)
				_graph.AddEdge(new Edge<NodeVertex>(_lastNodeVertex, _stack.Peek()));

			return true;
		}

		protected bool Visit<T>(ActionNode<T> node)
		{
			_lastNodeVertex = GetSink(node.GetHashCode(), () => GetExpressionBody(node.Expression), typeof (T), typeof (ActionNode<>));

			if (_stack.Count > 0)
				_graph.AddEdge(new Edge<NodeVertex>(_stack.Peek(), _lastNodeVertex));

			return true;
		}

		private string GetExpressionBody(Expression expression)
		{
			var lambda = expression as LambdaExpression;
			if (lambda != null)
				return lambda.Body.ToString();

			return expression.ToString();
		}

		private NodeVertex GetSink(int key, Func<string> getTitle, Type type, Type nodeType)
		{
			return _nodes.Retrieve(key, () =>
				{
					var newSink = new NodeVertex(getTitle(), type, nodeType);

					_graph.AddVertex(newSink);

					return newSink;
				});
		}

		private void NodeStyler(object sender, GleeVertexEventArgs<NodeVertex> args)
		{
			args.Node.Attr.Fillcolor = _colors.Retrieve(args.Vertex.NodeType, () => Color.Black);

			args.Node.Attr.Shape = Shape.Box;
			args.Node.Attr.Fontcolor = Color.White;
			args.Node.Attr.Fontsize = 8;
			args.Node.Attr.FontName = "Arial";
			args.Node.Attr.Label = args.Vertex.Name;
			args.Node.Attr.Padding = 1.2;
		}

		private static void EdgeStyler(object sender, GleeEdgeEventArgs<NodeVertex, Edge<NodeVertex>> e)
		{
			e.GEdge.EdgeAttr.Label = e.Edge.Source.ObjectType.Name;
			e.GEdge.EdgeAttr.FontName = "Tahoma";
			e.GEdge.EdgeAttr.Fontsize = 6;
		}
	}
}
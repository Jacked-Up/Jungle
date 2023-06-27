using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jungle.Editor
{
    public class JunglePortView : Port
    {
        private class JungleEdgeConnectorListener : IEdgeConnectorListener
        { 
            private GraphViewChange graphViewChange;
            private List<Edge> edgesToCreate;
            private List<GraphElement> edgesToDelete;
       
            public JungleEdgeConnectorListener()
            {
                edgesToCreate = new List<Edge>();
                edgesToDelete = new List<GraphElement>();
                graphViewChange.edgesToCreate = edgesToCreate;
            }
            
            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {
                
            }
            
            public void OnDrop(GraphView graphView, Edge edge)
            {
                edgesToCreate.Clear();
                edgesToCreate.Add(edge);
                edgesToDelete.Clear();
                if (edge.input.capacity == Capacity.Single)
                {
                    foreach (var connection in edge.input.connections)
                    {
                        if (connection != edge)
                        {
                            edgesToDelete.Add(connection);
                        }
                    }
                }
                if (edge.output.capacity == Capacity.Single)
                {
                    foreach (var connection in edge.output.connections)
                    {
                        if (connection != edge)
                        {
                            edgesToDelete.Add(connection);
                        }
                    }
                }

                edgesToDelete ??= new List<GraphElement>();
                if (edgesToDelete.Count > 0)
                {
                    graphView.DeleteElements(edgesToDelete);
                }
                if (graphView.graphViewChanged != null)
                {
                    edgesToCreate ??= new List<Edge>();
                    edgesToCreate = graphView.graphViewChanged(graphViewChange).edgesToCreate;
                }
                edgesToCreate ??= new List<Edge>();
                foreach (var edgeToCreate in edgesToCreate)
                {
                    graphView.AddElement(edgeToCreate);
                    edge.input.Connect(edgeToCreate);
                    edge.output.Connect(edgeToCreate);
                }
            }
        }
        
        protected JunglePortView(
            Orientation portOrientation,
            Direction portDirection,
            Capacity portCapacity,
            Type type) : base(portOrientation, portDirection, portCapacity, type)
        {
            
        }
        
        public new static JunglePortView Create<TEdge>(
            Orientation orientation,
            Direction direction,
            Capacity capacity,
            Type type)
            where TEdge : Edge, new()
        {
            var listener = new JungleEdgeConnectorListener();
            var element = new JunglePortView(orientation, direction, capacity, type)
            {
                m_EdgeConnector = new EdgeConnector<TEdge>(listener)
            };
            element.AddManipulator(element.m_EdgeConnector);
            return element;
        }
    }
}

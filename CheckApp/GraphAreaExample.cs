using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using GraphX.Common.Enums;
using GraphX.Common.Models;
using GraphX.Controls;
using GraphX.Logic.Models;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CheckApp
{
    public class GraphAreaExample : GraphArea<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>> 
    {
        public void GenerateGraph(Model model, bool EdgesToAdd = true)
        {
            var graph = new GraphExample();
            foreach (var ent in model.Entities)
            {
                var vertex = new DataVertex { Name = ent.Label, ID = ent.Id, Text = ent.Label };
                graph.AddVertex(vertex);
            }
            foreach (var hyp in model.Hyperedges)
            {
                var vertex = new DataVertex { Name = hyp.Label, ID = hyp.Id, Text = hyp.Label };
                graph.AddVertex(vertex);
            }
            var logicCore = new LogicCoreExample
            {
                Graph = graph
            };

            var vList = logicCore.Graph.Vertices.ToDictionary(x => x.ID);

            LogicCore = logicCore;

            LogicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.LinLog;
            SetVerticesMathShape(VertexShape.Circle);
            GenerateGraph(true);

            AddPoles(model);
            if (EdgesToAdd)
                AddEdges(model, graph, vList);

            SetVerticesDrag(true, true);
            UpdateLayout();
        }

        private void AddPoles(Model model)
        {
            foreach (var item in VertexList.Keys)
            {
                var poleCount = model.Vertices.First(x => x.Id == item.ID).Poles.Count;
                VertexList[item].SetConnectionPointsVisibility(false);
                VertexList[item].VertexConnectionPointsList.Clear();
                VertexList[item].VCPRoot.Children.Clear();
                VertexList[item].FontSize = 8;

                foreach (var pole in model.Vertices.First(x => x.Id == item.ID).Poles)
                {
                    var vcp = new GraphX.Controls.StaticVertexConnectionPoint { Id = (int)pole.Id, Tag = pole };
                    var ctrl = new Border { Margin = new Thickness(2, 2, 0, 2), Padding = new Thickness(0), Child = vcp };
                    VertexList[item].VCPRoot.Children.Add(ctrl);
                    VertexList[item].VertexConnectionPointsList.Add(vcp);
                }
            }
        }

        private void AddEdges(Model model, GraphExample graph, Dictionary<long, DataVertex> vList)
        {
            foreach (var hedge in model.HyperedgeConnectors)
            {
                foreach (var link in hedge.Links)
                {
                    graph.AddEdge(vList[(int)link.SourcePole.VertexOwner.Id], vList[(int)link.TargetPole.VertexOwner.Id], (int)link.SourcePole.Id, (int)link.TargetPole.Id);
                }
            }
            RelayoutGraph(true);
            foreach (var edge in EdgesList.Values)
            {
                edge.ShowArrows = false;
                edge.UpdateLayout();
                edge.DetachLabels();
            }
            SetEdgesDrag(true);
            SetEdgesDashStyle(GraphX.Controls.EdgeDashStyle.Solid);
        }
    }
    public class LogicCoreExample : GXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>
    {
    }

    public class DataVertex : VertexBase
    {
        public string Text { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Text;
        }

        public DataVertex() : this(string.Empty)
        {
        }

        public DataVertex(string text = "")
        {
            Text = string.IsNullOrEmpty(text) ? "New Vertex" : text;
        }
    }

    public class DataEdge : EdgeBase<DataVertex>, INotifyPropertyChanged
    {
        public DataEdge(DataVertex source, DataVertex target, double weight = 1)
            : base(source, target, weight)
        {
            Angle = 90;
        }

        public double Angle { get; set; }

        /// <summary>
        /// Node main description (header)
        /// </summary>
        private string _text;
        public string Text { get { return _text; } set { _text = value; OnPropertyChanged("Text"); } }

        public override string ToString()
        {
            return Text;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

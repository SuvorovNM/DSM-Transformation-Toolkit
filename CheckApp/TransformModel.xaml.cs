using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using GraphX.Common.Enums;
using GraphX.Logic.Models;
using QuickGraph;
using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Логика взаимодействия для ModelGraph.xaml
    /// </summary>
    public partial class TransformModel : UserControl
    {
        private Model LeftSubmodel { get; set; }
        private Model RightSubmodel { get; set; }
        public TransformModel(Model left, Model right)
        {
            InitializeComponent();
            LeftSubmodel = left;
            RightSubmodel = right;
            DataContext = this;
            Loaded += ControlLoaded;
            cbMathShape.Checked += CbMathShapeOnChecked;
            cbMathShape.Unchecked += CbMathShapeOnChecked;
            butAddVcp.Click += ButAddVcp_Click;
        }

        private void ButAddVcp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CbMathShapeOnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            foreach (var item in graphArea.VertexList.Values)
                item.VertexConnectionPointsList.ForEach(a => a.Shape = VertexShape.Circle);
            graphArea.UpdateAllEdges(true);
        }

        void ControlLoaded(object sender, RoutedEventArgs e)
        {
            GenerateGraph(graphArea, LeftSubmodel);
            //zoomControl.ZoomToFill();

            GenerateGraph(graphArea1, RightSubmodel);
            //zoomControl1.ZoomToFill();
        }

        private void GenerateGraph(GraphAreaExample graphArea, Model model)
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

            var vList = logicCore.Graph.Vertices.ToDictionary(x=>x.ID);

            graphArea.LogicCore = logicCore;

            graphArea.LogicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.LinLog;//ISOM KK CompoundFDP FR! LinLog
            graphArea.SetVerticesMathShape(VertexShape.Circle);
            graphArea.GenerateGraph(true);

            foreach (var item in graphArea.VertexList.Keys)
            {
                var poleCount = model.Vertices.First(x => x.Id == item.ID).Poles.Count;
                graphArea.VertexList[item].SetConnectionPointsVisibility(false);
                graphArea.VertexList[item].VertexConnectionPointsList.Clear();
                graphArea.VertexList[item].VCPRoot.Children.Clear();

                foreach (var pole in model.Vertices.First(x => x.Id == item.ID).Poles)
                {
                    var vcp = new GraphX.Controls.StaticVertexConnectionPoint { Id = (int)pole.Id, Tag = pole };
                    var ctrl = new Border { Margin = new Thickness(2, 2, 0, 2), Padding = new Thickness(0), Child = vcp };
                    graphArea.VertexList[item].VCPRoot.Children.Add(ctrl);
                    graphArea.VertexList[item].VertexConnectionPointsList.Add(vcp);
                }
            }
            foreach (var hedge in model.HyperedgeConnectors)
            {
                foreach (var link in hedge.Links)
                {
                    ShowcaseHelper.AddEdge(logicCore.Graph, vList[(int)link.SourcePole.VertexOwner.Id], vList[(int)link.TargetPole.VertexOwner.Id], (int)link.SourcePole.Id, (int)link.TargetPole.Id);
                }
            }
            graphArea.RelayoutGraph(true);
            foreach (var edge in graphArea.EdgesList.Values)
            {
                edge.ShowArrows = false;
                edge.UpdateLayout();
                edge.DetachLabels();
            }

            graphArea.SetVerticesDrag(true, true);
            graphArea.SetEdgesDrag(true);
            graphArea.SetEdgesDashStyle(GraphX.Controls.EdgeDashStyle.Solid);
            graphArea.UpdateLayout();
        }
    }
}

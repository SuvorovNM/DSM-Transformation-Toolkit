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
    public partial class ModelGraph : UserControl
    {
        private Model Model { get; set; }
        public ModelGraph(Model model)
        {
            InitializeComponent();
            Model = model;
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
            GenerateGraph();
        }

        private void GenerateGraph()
        {
            Random rnd = new Random();
            var graph = new GraphExample();
            foreach (var ent in Model.Entities)
            {
                var vertex = new DataVertex { Name = ent.Label, ID = ent.Id, Text = ent.Label };
                graph.AddVertex(vertex);
            }
            foreach(var hyp in Model.Hyperedges)
            {
                var vertex = new DataVertex { Name = hyp.Label, ID = hyp.Id, Text = hyp.Label };
                graph.AddVertex(vertex);
            }
            var logicCore = new LogicCoreExample
            {
                Graph = graph

            };

            var vList = logicCore.Graph.Vertices.ToList();

            //add edges
            ShowcaseHelper.AddEdge(logicCore.Graph, vList[0], vList[1], 3, 2);
            ShowcaseHelper.AddEdge(logicCore.Graph, vList[0], vList[2], 4, 2);

            ShowcaseHelper.AddEdge(logicCore.Graph, vList[1], vList[3], 3, 1);
            ShowcaseHelper.AddEdge(logicCore.Graph, vList[3], vList[5], 2, 3);

            ShowcaseHelper.AddEdge(logicCore.Graph, vList[2], vList[4], 4, 2);
            ShowcaseHelper.AddEdge(logicCore.Graph, vList[4], vList[5], 1, 4);

            ShowcaseHelper.AddEdge(logicCore.Graph, vList[5], vList[1], 1, 4);
            ShowcaseHelper.AddEdge(logicCore.Graph, vList[5], vList[2], 2, 3);

            graphArea.LogicCore = logicCore;

            graphArea.LogicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.EfficientSugiyama;
            graphArea.SetVerticesMathShape(VertexShape.Circle);            
            graphArea.GenerateGraph(true);

            foreach (var item in graphArea.VertexList.Values)
            {
                item.VertexConnectionPointsList.Clear();
            }
            //settings
            graphArea.SetVerticesDrag(true, true);
            graphArea.SetEdgesDrag(true);
            graphArea.UpdateLayout();
            zoomControl.ZoomToFill();
        }
    }
    public class LogicCoreExample : GXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>
    {
    }
}

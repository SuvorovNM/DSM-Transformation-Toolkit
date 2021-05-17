using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using DSM_Graph_Layer.HPGraphModel.ModelClasses.Transformations;
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
    /// Логика взаимодействия для TestTransform.xaml
    /// </summary>
    public partial class TestTransform : Window
    {
        private List<TransformationRule> Rules { get; set; }
        private int currentIndex;
        public TestTransform(List<TransformationRule> rules)
        {
            InitializeComponent();
            Rules = rules;
            currentIndex = 0;
            DataContext = this;
            NumberLabel.Content = currentIndex + 1;
        }

        void FillRules()
        {
            GenerateGraph(graphArea, Rules[currentIndex].LeftPart);
            zoomControl.ZoomToFill();
            if (zoomControl.ViewFinder != null)
                zoomControl.ViewFinder.Visibility = Visibility.Collapsed;

            GenerateGraph(graphArea1, Rules[currentIndex].RightPart);
            zoomControl1.ZoomToFill();
            if (zoomControl1.ViewFinder != null)
                zoomControl1.ViewFinder.Visibility = Visibility.Collapsed;
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

            var vList = logicCore.Graph.Vertices.ToDictionary(x => x.ID);

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
            graphArea.SetVerticesDrag(false, false);
            graphArea.UpdateLayout();
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                FillRules();
            }
            ValidateButtons();
            NumberLabel.Content = currentIndex + 1;
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex < Rules.Count - 1)
            {
                currentIndex++;
                FillRules();
            }
            ValidateButtons();
            NumberLabel.Content = currentIndex + 1;
        }

        private void ValidateButtons()
        {
            if (currentIndex == 0)
                UpButton.IsEnabled = false;
            else
                UpButton.IsEnabled = true;

            if (currentIndex == Rules.Count - 1)
                DownButton.IsEnabled = false;
            else
                DownButton.IsEnabled = true;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillRules();
            ValidateButtons();
        }
    }
}

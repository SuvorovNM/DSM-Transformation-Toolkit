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
    /// <summary>
    /// Элемент управления для отображения графа модели
    /// </summary>
    public class GraphAreaExample : GraphArea<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>> 
    {
        /// <summary>
        /// Генерация графа мо модели
        /// </summary>
        /// <param name="model">Выбранная модель</param>
        /// <param name="EdgesToAdd">Необходимо ли добавлять ребра: True, если необходимо</param>
        public void GenerateGraph(Model model, bool EdgesToAdd = true)
        {
            var graph = new BidirectionalGraph<DataVertex, DataEdge>();

            // Добавление вершин в граф
            foreach (var ent in model.Entities)
            {
                var vertex = new DataVertex { ID = ent.Id, Text = ent.Label };
                graph.AddVertex(vertex);
            }
            foreach (var hyp in model.Hyperedges)
            {
                var vertex = new DataVertex { ID = hyp.Id, Text = hyp.Label };
                graph.AddVertex(vertex);
            }
            var logicCore = new GXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>
            {
                Graph = graph
            };

            var vList = logicCore.Graph.Vertices.ToDictionary(x => x.ID);

            // Установка базовых параметров для отображения графа модели, включая алогоритм укладки
            LogicCore = logicCore;
            LogicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.LinLog;
            SetVerticesMathShape(VertexShape.Circle);
            SetVerticesDrag(true, true);
            GenerateGraph(true);

            // Добавление полюсов (Vertex Connection Points) к графу модели
            AddPoles(model);

            // Добавление ребер, если это необходимо
            if (EdgesToAdd)
                AddEdges(model, graph, vList);

            UpdateLayout();
        }

        /// <summary>
        /// Добавление полюсов (Vertex Connection Points) к графу модели по полюсам вершин выбранной модели
        /// </summary>
        /// <param name="model">Выбранная модель</param>
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

        /// <summary>
        /// Добавление ребер к графу модели на основе связей выбранной модели
        /// </summary>
        /// <param name="model">Выбранная модель</param>
        /// <param name="graph">Граф модели</param>
        /// <param name="vList">Список вершин</param>
        private void AddEdges(Model model, BidirectionalGraph<DataVertex, DataEdge> graph, Dictionary<long, DataVertex> vList)
        {
            foreach (var hedge in model.HyperedgeConnectors)
            {
                foreach (var link in hedge.Links)
                {
                    var edge = new DataEdge(vList[(int)link.SourcePole.VertexOwner.Id], vList[(int)link.TargetPole.VertexOwner.Id])
                    {
                        ID = link.Id,
                        SourceConnectionPointId = (int)link.SourcePole.Id,
                        TargetConnectionPointId = (int)link.TargetPole.Id,
                    };
                    graph.AddEdge(edge);
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

    /// <summary>
    /// Вершина отображаемого графа модели
    /// </summary>
    public class DataVertex : VertexBase
    {
        /// <summary>
        /// Текст вершины
        /// </summary>
        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }

        /// <summary>
        /// Инициализация вершины
        /// </summary>
        /// <param name="text">Отображаемый текст вершины</param>
        public DataVertex(string text = "")
        {
            Text = string.IsNullOrEmpty(text) ? "New Vertex" : text;
        }
    }

    /// <summary>
    /// Ребро отображаемого графа модели
    /// </summary>
    public class DataEdge : EdgeBase<DataVertex>
    {
        /// <summary>
        /// Инициализация ребра
        /// </summary>
        /// <param name="source">Исходная вершина</param>
        /// <param name="target">Целевая вершина</param>
        /// <param name="weight">Вес ребра</param>
        public DataEdge(DataVertex source, DataVertex target, double weight = 1)
            : base(source, target, weight)
        {
        }
    }
}

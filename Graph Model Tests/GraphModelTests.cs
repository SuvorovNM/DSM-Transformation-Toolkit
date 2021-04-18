using DSM_Graph_Layer;
using DSM_Graph_Layer.HPGraphModel;
using DSM_Graph_Layer.Enums;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Graph_Model_Tests
{
    public class GraphModelTests
    {
        HPGraph SourceGraph;
        [SetUp]
        public void Setup()
        {
            SourceGraph = new HPGraph();
            var vertex = CreateVertex(5);
            SourceGraph.AddVertex(vertex);
            var vertex1 = CreateVertex(10);
            SourceGraph.AddVertex(vertex1);
            var hEdge = CreateHyperEdge(vertex1.Poles.First());
            SourceGraph.AddHyperEdge(hEdge);

            var newHPGraph = CreateRandomGraph();
            vertex.AddDecomposition(newHPGraph);
        }

        [Test]
        public void AddNewVertexWithPolesTest()
        {
            var polesCount = 6;
            var startVertexCount = SourceGraph.Vertices.Count;

            var vertex = CreateVertex(polesCount);
            SourceGraph.AddVertex(vertex);

            Assert.AreEqual(polesCount + 1, vertex.Poles.Count);
            Assert.AreEqual(startVertexCount + 1, SourceGraph.Vertices.Count);
            Assert.IsTrue(vertex.OwnerGraph == SourceGraph);
        }       
        
        [Test]
        public void AddNewHyperEdgeTest()
        {
            var startEdgeCount = SourceGraph.Edges.Count;
            var vertex = CreateVertex(5);
            SourceGraph.AddVertex(vertex);

            var hEdge = CreateHyperEdge(vertex.Poles.First());
            SourceGraph.AddHyperEdge(hEdge);

            Assert.AreEqual(startEdgeCount + 1, SourceGraph.Edges.Count);
            Assert.AreEqual(hEdge.Links.Count, SourceGraph.Vertices[0].Poles.Count);
            Assert.AreEqual(hEdge.Poles.Count, SourceGraph.Vertices[0].Poles.Count + 1);
            Assert.IsTrue(hEdge.OwnerGraph == SourceGraph);
        }

        [Test]
        public void RemoveVertexTest()
        {
            var startVerticesCount = SourceGraph.Vertices.Count;
            var removedVerticeId = SourceGraph.Vertices[0].Id;

            SourceGraph.RemoveStructure(SourceGraph.Vertices[0]);

            Assert.AreEqual(startVerticesCount - 1, SourceGraph.Vertices.Count);
            Assert.IsTrue(!SourceGraph.Vertices.Any(x => x.Id == removedVerticeId));
            Assert.IsTrue(!SourceGraph.Edges.Any(x => x.Poles.Any(x=>x.VertexOwner.Id == removedVerticeId)));
        }

        [Test]
        public void RemoveEdgeTest()
        {
            var startEdgeCount = SourceGraph.Edges.Count;
            var removedEdgeId = SourceGraph.Edges[0].Id;
            var totalPolesCount = SourceGraph.Vertices.Sum(x => x.Poles.Count) + SourceGraph.ExternalPoles.Count;

            SourceGraph.RemoveStructure(SourceGraph.Edges[0]);

            Assert.AreEqual(startEdgeCount - 1, SourceGraph.Edges.Count);
            Assert.IsTrue(!SourceGraph.Edges.Any(x => x.Id == removedEdgeId));
            Assert.AreEqual(totalPolesCount, SourceGraph.Vertices.Sum(x => x.Poles.Count) + SourceGraph.ExternalPoles.Count);
        }

        [Test]
        public void AddPoleToVertexTest()
        {
            var sourceVertex = SourceGraph.Vertices[0];
            var startVertexPoleCount = sourceVertex.Poles.Count;
            var startPoleId = GraphEnumerator.currentPoleId;

            sourceVertex.AddPole(new Pole(PoleType.Input));
            sourceVertex.AddPole(new Pole(PoleType.Output));

            Assert.AreEqual(startVertexPoleCount + 2, sourceVertex.Poles.Count);
            Assert.IsTrue(sourceVertex.Poles.Exists(x => x.Id == startPoleId + 1 && x.VertexOwner == sourceVertex));
            Assert.IsTrue(sourceVertex.Poles.Exists(x => x.Id == startPoleId + 2 && x.VertexOwner == sourceVertex));
        }

        [Test]
        public void AddPoleAndLinkToEdgeTest()
        {
            var sourceEdge = SourceGraph.Edges[0];
            var startEdgePoleCount = sourceEdge.Poles.Count;
            var startLinkCount = sourceEdge.Links.Count;
            var addingPole = SourceGraph.Vertices[1].Poles.Last();
            var currentLinkId = GraphEnumerator.currentLinkId;

            sourceEdge.AddPole(addingPole);
            sourceEdge.AddLink(addingPole, sourceEdge.Poles.First());

            Assert.AreEqual(startEdgePoleCount + 1, sourceEdge.Poles.Count);
            Assert.AreEqual(startLinkCount + 1, sourceEdge.Links.Count);
            Assert.IsTrue(sourceEdge.Poles.Where(x=>x.Id == addingPole.Id).Count() == 1);
            Assert.IsTrue(sourceEdge.Links.Where(x => x.Id == currentLinkId + 1).Count() == 1);
        }

        [Test]
        public void RemovePoleFromVertexTest()
        {
            var sourceVertex = SourceGraph.Vertices[0];
            var startVertexPoleCount = sourceVertex.Poles.Count;
            var removedPole = sourceVertex.Poles[1];
            var containingEdges = SourceGraph.Edges.Where(x => x.Poles.Contains(removedPole));
            var containingLinks = SourceGraph.Edges.SelectMany(x => x.Links.Where(x => x.SourcePole == removedPole || x.TargetPole == removedPole));

            sourceVertex.RemovePole(removedPole);

            Assert.AreEqual(startVertexPoleCount - 1, sourceVertex.Poles.Count);
            Assert.IsTrue(!containingEdges.Any());
            Assert.IsTrue(!containingLinks.Any());
        }

        [Test]
        public void RemovePoleFromEdgeTest()
        {
            var sourceEdge = SourceGraph.Edges[0];
            var startEdgePoleCount = sourceEdge.Poles.Count;
            var removedPole = sourceEdge.Poles[2];
            var containingLinks = sourceEdge.Links.Where(x => x.SourcePole == removedPole || x.TargetPole == removedPole);

            sourceEdge.RemovePole(removedPole);

            Assert.AreEqual(startEdgePoleCount - 1, sourceEdge.Poles.Count);
            Assert.IsTrue(!containingLinks.Any());
        }

        [Test]
        public void RemoveAllPolesFromHyperEdgeTest()
        {
            var startEdgesCount = SourceGraph.Edges.Count;
            var vertex = CreateVertex(5);
            SourceGraph.AddVertex(vertex);
            var hEdge = CreateHyperEdge(vertex.Poles.First());
            SourceGraph.AddHyperEdge(hEdge);
            var totalPolesCount = SourceGraph.Vertices.Sum(x => x.Poles.Count) + SourceGraph.ExternalPoles.Count;

            var polesList = hEdge.Poles.ToList();
            foreach (var pole in polesList)
            {
                hEdge.RemovePole(pole);
            }

            Assert.Zero(hEdge.Poles.Count);
            Assert.Zero(hEdge.Links.Count);
            Assert.AreEqual(startEdgesCount, SourceGraph.Edges.Count);
            Assert.AreEqual(totalPolesCount, SourceGraph.Vertices.Sum(x => x.Poles.Count) + SourceGraph.ExternalPoles.Count);
        }

        [Test]
        public void RemoveAllPolesFromVertexTest()
        {
            var polesCount = 10;
            var startVerticesCount = SourceGraph.Vertices.Count;
            var vertex = CreateVertex(polesCount);
            SourceGraph.AddVertex(vertex);

            var polesList = vertex.Poles.ToList();
            foreach (var pole in polesList)
            {
                vertex.RemovePole(pole);
            }

            Assert.AreEqual(startVerticesCount, SourceGraph.Vertices.Count);
        }

        [Test]
        public void RemoveLinkFromHyperEdgeTest()
        {
            var sourceEdge = SourceGraph.Edges.First();
            var startLinkCount = sourceEdge.Links.Count;
            var removedLink = sourceEdge.Links[3];

            sourceEdge.RemoveLink(removedLink);

            Assert.AreEqual(startLinkCount - 1, sourceEdge.Links.Count);
            Assert.IsTrue(!sourceEdge.Links.Any(x => x.Id == removedLink.Id));
        }

        [Test]
        public void RemoveAllLinksFromHyperEdgeTest()
        {
            var hEdge = SourceGraph.Edges.First();
            var hyperedgesCount = SourceGraph.Edges.Count;

            foreach(var link in hEdge.Links.ToList())
            {
                hEdge.RemoveLink(link);
            }

            Assert.Zero(hEdge.Links.Count);
            Assert.AreEqual(hyperedgesCount - 1, SourceGraph.Edges.Count);
        }

        [Test]
        public void DefineDecompositionTest()
        {
            var newHPGraph = CreateRandomGraph();
            var decomposingVertex = SourceGraph.Vertices[1];
            var currentDecompositionsCount = decomposingVertex.Decompositions.Count;

            decomposingVertex.AddDecomposition(newHPGraph);

            Assert.AreEqual(currentDecompositionsCount + 1, decomposingVertex.Decompositions.Count);
            Assert.AreEqual(decomposingVertex.Decompositions.Last(), newHPGraph);
            Assert.AreEqual(newHPGraph.ParentGraph, SourceGraph);
        }

        [Test]
        public void RemoveDecompositionTest()
        {
            var decomposingVertex = SourceGraph.Vertices[0];
            var hpGraph = decomposingVertex.Decompositions.First();
            var decompositionCount = decomposingVertex.Decompositions.Count;

            decomposingVertex.RemoveDecomposition(hpGraph);

            Assert.AreEqual(decompositionCount - 1, decomposingVertex.Decompositions.Count);
            Assert.IsTrue(!decomposingVertex.Decompositions.Any(x => x.Id == hpGraph.Id));
        }

        [Test]
        public void CreateNewDecompositionForVertexTest()
        {
            var decomposingVertex = SourceGraph.Vertices[1];
            var decompositionCount = decomposingVertex.Decompositions.Count;

            var hpGraph = decomposingVertex.AddDecomposition();

            Assert.AreEqual(decompositionCount + 1, decomposingVertex.Decompositions.Count);
            Assert.IsTrue(hpGraph.ParentGraph == SourceGraph);
            Assert.AreEqual(hpGraph.ExternalPoles.Count, decomposingVertex.Poles.Count);
        }

        [Test]
        public void CreateNewDecompositionForEdgeTest()
        {
            var decomposingEdge = SourceGraph.Edges[0];
            var decompositionCount = decomposingEdge.Decompositions.Count;

            var hpGraph = decomposingEdge.AddDecomposition();

            Assert.AreEqual(decompositionCount + 1, decomposingEdge.Decompositions.Count);
            Assert.IsTrue(hpGraph.ParentGraph == SourceGraph);
            Assert.AreEqual(hpGraph.ExternalPoles.Count, decomposingEdge.Poles.Count);
        }

        private static Vertex CreateVertex(int polesCount)
        {
            var vertex = new Vertex();
            var polesList = new List<Pole> { vertex.Poles.FirstOrDefault() };
            for (int i = 0; i < polesCount; i++)
            {
                var pole = new Pole();
                polesList.Add(pole);
                vertex.AddPole(pole);
            }

            return vertex;
        }


        private Hyperedge CreateHyperEdge(Pole sourcePole)
        {
            var hEdge = new Hyperedge();
            hEdge.AddPole(sourcePole);
            foreach (var pole in SourceGraph.Vertices[0].Poles)
            {
                hEdge.AddPole(pole);
                hEdge.AddLink(sourcePole, pole);
            }

            return hEdge;
        }

        // TODO: Добавить внешние полюса!
        private HPGraph CreateRandomGraph()
        {
            var rnd = new Random();
            var graph = new HPGraph();
            for (int i = 0; i< rnd.Next(1, 100); i++)
            {
                var vertex = new Vertex();

                for (int j=0; j<rnd.Next(1,10); j++)
                {
                    var pole = new Pole();
                    vertex.AddPole(pole);
                }
                graph.AddVertex(vertex);
            }

            for (int i = 0; i < rnd.Next(1, 20); i++)
            {
                graph.AddExternalPole(new Pole());
            }

            for (int j = 0; j< rnd.Next(1, 10); j++)
            {
                var edge = new Hyperedge();
                for (int k = 0; k < rnd.Next(1, 10); k++)
                {
                    var vertex = graph.Vertices[rnd.Next(graph.Vertices.Count)];
                    var pole = vertex.Poles[rnd.Next(vertex.Poles.Count)];
                    edge.AddPole(pole);
                    if (edge.Poles.Count > 1)
                    {
                        edge.AddLink(edge.Poles[rnd.Next(edge.Poles.Count)], pole);
                    }
                }
                for (int k = 0; k < rnd.Next(1, 5); k++)
                {
                    var extPole = graph.ExternalPoles[rnd.Next(graph.ExternalPoles.Count)];
                    edge.AddPole(extPole);
                    if (edge.Poles.Count > 1)
                    {
                        edge.AddLink(extPole, edge.Poles[rnd.Next(edge.Poles.Count)]);
                    }
                }
                graph.AddHyperEdge(edge);
            }

            return graph;
        }
    }
}
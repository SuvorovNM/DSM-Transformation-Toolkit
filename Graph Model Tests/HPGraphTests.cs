using DSM_Graph_Layer;
using DSM_Graph_Layer.HPGraphModel;
using DSM_Graph_Layer.Enums;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using DSM_Graph_Layer.HPGraphModel.GraphClasses;

namespace Graph_Model_Tests
{
    public class HPGraphTests
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
            SourceGraph.AddExternalPole(new Pole());

            var newHPGraph = CreateRandomGraph();
            vertex.AddDecomposition(newHPGraph);
        }

        [Test]
        public void CreateNewRoleTest()
        {
            var testRole = new Role("Ассоциация");

            Assert.IsTrue(testRole.OppositeRole != null && testRole.OppositeRole.Label == testRole.Label && testRole.OppositeRole.OppositeRole == testRole);
        }

        #region Manipulations with verticies
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
        public void RemoveVertexTest()
        {
            var startVerticesCount = SourceGraph.Vertices.Count;
            var removedVerticeId = SourceGraph.Vertices[0].Id;

            SourceGraph.RemoveStructure(SourceGraph.Vertices[0]);

            Assert.AreEqual(startVerticesCount - 1, SourceGraph.Vertices.Count);
            Assert.IsTrue(!SourceGraph.Vertices.Any(x => x.Id == removedVerticeId));
            Assert.IsTrue(!SourceGraph.Edges.Any(x => x.Poles.Any(x => x.VertexOwner.Id == removedVerticeId)));
        }
        #endregion

        #region Manipulations with hyperedges
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
        #endregion

        #region Manipulations with poles
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
        #endregion

        #region Manipulations with links
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
        #endregion

        #region Manipulations with decompositions
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
        #endregion

        #region Subgraph matching
        [Test]
        public void SingleSubgraphSearchTest()
        {
            (var addedVertex, var addedVertex1, var hEdge) = CreateVerticesAndHedge();
            SourceGraph.AddVertex(addedVertex1);
            SourceGraph.AddVertex(addedVertex);
            SourceGraph.AddHyperEdge(hEdge);

            (var newVertex, var newVertex1, var newHedge) = CreateVerticesAndHedge();
            var newHPGraph = new HPGraph();
            newHPGraph.AddVertex(newVertex);
            newHPGraph.AddVertex(newVertex1);
            newHPGraph.AddHyperEdge(newHedge);

            var results = SourceGraph.FindIsomorphicSubgraphs(newHPGraph);

            Assert.IsTrue(results.Count == 1);
            Assert.IsTrue(results.Any(x => x.Vertices.Contains(addedVertex) && x.Vertices.Contains(addedVertex1) && x.Edges.Contains(hEdge)));
        }

        [Test]
        public void MultipleSubgraphSearchTest()
        {
            (var addedVertex, var addedVertex1, var hEdge) = CreateVerticesAndHedge();
            SourceGraph.AddVertex(addedVertex1);
            SourceGraph.AddVertex(addedVertex);
            SourceGraph.AddHyperEdge(hEdge);

            (var secondlyAddedVertex, var secondlyAddedVertex1, var secondlyAddedHedge) = CreateVerticesAndHedge();
            SourceGraph.AddVertex(secondlyAddedVertex);
            SourceGraph.AddVertex(secondlyAddedVertex1);
            SourceGraph.AddHyperEdge(secondlyAddedHedge);

            (var newVertex, var newVertex1, var newHedge) = CreateVerticesAndHedge();
            var newHPGraph = new HPGraph();
            newHPGraph.AddVertex(newVertex);
            newHPGraph.AddVertex(newVertex1);
            newHPGraph.AddHyperEdge(newHedge);

            var results = SourceGraph.FindIsomorphicSubgraphs(newHPGraph);

            Assert.IsTrue(results.Count == 2);
            Assert.IsTrue(results.Any(x => x.Vertices.Contains(addedVertex) && x.Vertices.Contains(addedVertex1) && x.Edges.Contains(hEdge)));
            Assert.IsTrue(results.Any(x => x.Vertices.Contains(secondlyAddedVertex) && x.Vertices.Contains(secondlyAddedVertex1) && x.Edges.Contains(secondlyAddedHedge)));
        }

        [Test]
        public void SingleSubgraphSearchWithIncidentHyperedgesTest()
        {
            (var addedVertex, var addedVertex1, var hedge1, var hedge2) = CreateVerticesAndIncidentHyperedges();
            SourceGraph.AddVertex(addedVertex1);
            SourceGraph.AddVertex(addedVertex);
            SourceGraph.AddHyperEdge(hedge1);
            SourceGraph.AddHyperEdge(hedge2);

            (var newVertex, var newVertex1, var newHedge1, var newHedge2) = CreateVerticesAndIncidentHyperedges();
            var newHPGraph = new HPGraph();
            newHPGraph.AddVertex(newVertex);
            newHPGraph.AddVertex(newVertex1);
            newHPGraph.AddHyperEdge(newHedge1);
            newHPGraph.AddHyperEdge(newHedge2);

            var results = SourceGraph.FindIsomorphicSubgraphs(newHPGraph);

            Assert.IsTrue(results.Count == 1);
            Assert.IsTrue(results.Any(x => x.Vertices.Contains(addedVertex) && x.Vertices.Contains(addedVertex1) && x.Edges.Contains(hedge1) && x.Edges.Contains(hedge2)));
        }

        [Test]
        public void MultipleSubgraphSearchWithIncidentHyperedgesTest()
        {
            (var addedVertex, var addedVertex1, var hedge1, var hedge2) = CreateVerticesAndIncidentHyperedges();
            SourceGraph.AddVertex(addedVertex1);
            SourceGraph.AddVertex(addedVertex);
            SourceGraph.AddHyperEdge(hedge1);
            SourceGraph.AddHyperEdge(hedge2);

            (var secondlyAddedVertex, var secondlyAddedVertex1, var secondlyHedge1, var secondlyHedge2) = CreateVerticesAndIncidentHyperedges();
            SourceGraph.AddVertex(secondlyAddedVertex1);
            SourceGraph.AddVertex(secondlyAddedVertex);
            SourceGraph.AddHyperEdge(secondlyHedge1);
            SourceGraph.AddHyperEdge(secondlyHedge2);

            (var newVertex, var newVertex1, var newHedge1, var newHedge2) = CreateVerticesAndIncidentHyperedges();
            var newHPGraph = new HPGraph();
            newHPGraph.AddVertex(newVertex);
            newHPGraph.AddVertex(newVertex1);
            newHPGraph.AddHyperEdge(newHedge1);
            newHPGraph.AddHyperEdge(newHedge2);

            var results = SourceGraph.FindIsomorphicSubgraphs(newHPGraph);

            Assert.IsTrue(results.Count == 2);
            Assert.IsTrue(results.Any(x => x.Vertices.Contains(addedVertex) && x.Vertices.Contains(addedVertex1) && x.Edges.Contains(hedge1) && x.Edges.Contains(hedge2)));
            Assert.IsTrue(results.Any(x => x.Vertices.Contains(secondlyAddedVertex) && x.Vertices.Contains(secondlyAddedVertex1) && x.Edges.Contains(secondlyHedge1) && x.Edges.Contains(secondlyHedge2)));
        }

        [Test]
        public void SingleIncompleteVertexSubgraphSearch() // TODO: Корректный ли тест??
        {
            var newHPGraph = new HPGraph();
            var vertex = new Vertex();
            newHPGraph.AddVertex(vertex);

            var results = SourceGraph.FindIsomorphicSubgraphs(newHPGraph);

            Assert.IsTrue(results.Count == SourceGraph.Vertices.Count);
        }
        #endregion

        #region Graph Transformations
        [Test]
        public void SingleSubgraphTransformationTest()
        {
            var startVertexCount = SourceGraph.Vertices.Count;
            var startHyperedgeCount = SourceGraph.Edges.Count;
            var polesCount = 100;

            (var addedVertex, var addedVertex1, var hEdge) = CreateVerticesAndHedge();
            SourceGraph.AddVertex(addedVertex1);
            SourceGraph.AddVertex(addedVertex);
            SourceGraph.AddHyperEdge(hEdge);

            (var newVertex, var newVertex1, var newHedge) = CreateVerticesAndHedge();
            var patternGraph = new HPGraph();
            patternGraph.AddVertex(newVertex);
            patternGraph.AddVertex(newVertex1);
            patternGraph.AddHyperEdge(newHedge);

            var vertex = new Vertex();
            for (int i = 1; i < polesCount; i++)
                vertex.AddPole(new Pole());
            var replacementGraph = new HPGraph();
            replacementGraph.AddVertex(vertex);

            SourceGraph.Transform(patternGraph, replacementGraph);

            Assert.IsTrue(SourceGraph.Vertices.Count == startVertexCount + 1);
            Assert.IsTrue(SourceGraph.Edges.Count == startHyperedgeCount);
            Assert.IsTrue(SourceGraph.Vertices.Any(x=>x.Poles.Count == polesCount));
        }

        [Test]
        public void MultipleSubgraphTransformationTest()
        {
            var startVertexCount = SourceGraph.Vertices.Count;
            var startHyperedgeCount = SourceGraph.Edges.Count;
            var polesCount = 100;

            (var addedVertex, var addedVertex1, var hEdge) = CreateVerticesAndHedge();
            SourceGraph.AddVertex(addedVertex1);
            SourceGraph.AddVertex(addedVertex);
            SourceGraph.AddHyperEdge(hEdge);

            (var secondlyAddedVertex, var secondlyAddedVertex1, var secondlyAddedHedge) = CreateVerticesAndHedge();
            SourceGraph.AddVertex(secondlyAddedVertex);
            SourceGraph.AddVertex(secondlyAddedVertex1);
            SourceGraph.AddHyperEdge(secondlyAddedHedge);

            (var newVertex, var newVertex1, var newHedge) = CreateVerticesAndHedge();
            var patternGraph = new HPGraph();
            patternGraph.AddVertex(newVertex);
            patternGraph.AddVertex(newVertex1);
            patternGraph.AddHyperEdge(newHedge);

            var vertex = new Vertex();
            for (int i = 1; i < polesCount; i++)
                vertex.AddPole(new Pole());
            var replacementGraph = new HPGraph();
            replacementGraph.AddVertex(vertex);

            SourceGraph.Transform(patternGraph, replacementGraph);

            Assert.IsTrue(SourceGraph.Vertices.Count == startVertexCount + 2);
            Assert.IsTrue(SourceGraph.Edges.Count == startHyperedgeCount);
            Assert.IsTrue(SourceGraph.Vertices.Count(x => x.Poles.Count == polesCount) == 2);
        }

        [Test]
        public void SingleSubgraphTransformationWithIncidentHyperedgesTest()
        {
            var startVertexCount = SourceGraph.Vertices.Count;
            var startHyperedgeCount = SourceGraph.Edges.Count;

            (var addedVertex, var addedVertex1, var hedge1, var hedge2) = CreateVerticesAndIncidentHyperedges();
            SourceGraph.AddVertex(addedVertex1);
            SourceGraph.AddVertex(addedVertex);
            SourceGraph.AddHyperEdge(hedge1);
            SourceGraph.AddHyperEdge(hedge2);

            (var newVertex, var newVertex1, var newHedge1, var newHedge2) = CreateVerticesAndIncidentHyperedges();
            var patternGraph = new HPGraph();
            patternGraph.AddVertex(newVertex);
            patternGraph.AddVertex(newVertex1);
            patternGraph.AddHyperEdge(newHedge1);
            patternGraph.AddHyperEdge(newHedge2);

            var replacementGraph = new HPGraph();
            var (vertex1, vertex2, hedge) = CreateVerticesAndHedge();
            replacementGraph.AddVertex(vertex1);
            replacementGraph.AddVertex(vertex2);
            replacementGraph.AddHyperEdge(hedge);

            SourceGraph.Transform(patternGraph, replacementGraph);

            Assert.IsTrue(SourceGraph.Vertices.Count == startVertexCount + 2);
            Assert.IsTrue(SourceGraph.Edges.Count == startHyperedgeCount + 1);
            Assert.IsTrue(SourceGraph.Vertices.Any(x=>x.Poles.Count == vertex1.Poles.Count));
            Assert.IsTrue(SourceGraph.Vertices.Any(x => x.Poles.Count == vertex2.Poles.Count));
            Assert.IsTrue(SourceGraph.Edges.Any(x => x.Poles.Count == hedge.Poles.Count));
            Assert.IsTrue(SourceGraph.Edges.Any(x => x.Links.Count == hedge.Links.Count));
        }

        [Test]
        public void MultipleSubgraphTransformationWithIncidentHyperedgesTest()
        {
            var startVertexCount = SourceGraph.Vertices.Count;
            var startHyperedgeCount = SourceGraph.Edges.Count;

            (var addedVertex, var addedVertex1, var hedge1, var hedge2) = CreateVerticesAndIncidentHyperedges();
            SourceGraph.AddVertex(addedVertex1);
            SourceGraph.AddVertex(addedVertex);
            SourceGraph.AddHyperEdge(hedge1);
            SourceGraph.AddHyperEdge(hedge2);

            (var secondlyAddedVertex, var secondlyAddedVertex1, var secondlyHedge1, var secondlyHedge2) = CreateVerticesAndIncidentHyperedges();
            SourceGraph.AddVertex(secondlyAddedVertex1);
            SourceGraph.AddVertex(secondlyAddedVertex);
            SourceGraph.AddHyperEdge(secondlyHedge1);
            SourceGraph.AddHyperEdge(secondlyHedge2);

            (var newVertex, var newVertex1, var newHedge1, var newHedge2) = CreateVerticesAndIncidentHyperedges();
            var patternGraph = new HPGraph();
            patternGraph.AddVertex(newVertex);
            patternGraph.AddVertex(newVertex1);
            patternGraph.AddHyperEdge(newHedge1);
            patternGraph.AddHyperEdge(newHedge2);

            var replacementGraph = new HPGraph();
            var (vertex1, vertex2, hedge) = CreateVerticesAndHedge();
            replacementGraph.AddVertex(vertex1);
            replacementGraph.AddVertex(vertex2);
            replacementGraph.AddHyperEdge(hedge);

            SourceGraph.Transform(patternGraph, replacementGraph);

            Assert.IsTrue(SourceGraph.Vertices.Count == startVertexCount + 4);
            Assert.IsTrue(SourceGraph.Edges.Count == startHyperedgeCount + 2);
            Assert.IsTrue(SourceGraph.Vertices.Count(x => x.Poles.Count == vertex1.Poles.Count)>=2);
            Assert.IsTrue(SourceGraph.Vertices.Count(x => x.Poles.Count == vertex2.Poles.Count)>=2);
            Assert.IsTrue(SourceGraph.Edges.Count(x => x.Poles.Count == hedge.Poles.Count)>=2);
            Assert.IsTrue(SourceGraph.Edges.Count(x => x.Links.Count == hedge.Links.Count)>=2);
        }

        [Test]
        public void MultipleSubgraphTransformationWithIncidentHyperedgesAndIncompleteVerticesTest()
        {
            var startVertexCount = SourceGraph.Vertices.Count;
            var startHyperedgeCount = SourceGraph.Edges.Count;

            (var addedVertex, var addedVertex1, var hedge1, var hedge2) = CreateVerticesAndIncidentHyperedges();
            SourceGraph.AddVertex(addedVertex1);
            SourceGraph.AddVertex(addedVertex);
            SourceGraph.AddHyperEdge(hedge1);
            SourceGraph.AddHyperEdge(hedge2);

            (var secondlyAddedVertex, var secondlyAddedVertex1, var secondlyHedge1, var secondlyHedge2) = CreateVerticesAndIncidentHyperedges();
            SourceGraph.AddVertex(secondlyAddedVertex1);
            SourceGraph.AddVertex(secondlyAddedVertex);
            SourceGraph.AddHyperEdge(secondlyHedge1);
            SourceGraph.AddHyperEdge(secondlyHedge2);

            (var newVertex, var newVertex1, var newHedge1, var newHedge2) = CreateVerticesAndIncidentHyperedges();
            newVertex.IsIncomplete = true;
            newVertex.Poles.RemoveAll(x => !x.EdgeOwners.Any());

            var patternGraph = new HPGraph();
            patternGraph.AddVertex(newVertex);
            patternGraph.AddVertex(newVertex1);
            patternGraph.AddHyperEdge(newHedge1);
            patternGraph.AddHyperEdge(newHedge2);

            var replacementGraph = new HPGraph();
            var newIncomplete = new VertexForTransformation(newVertex);
            var (vertex1, vertex2, hedge) = CreateSubgraphWithIncompleteVertex(newIncomplete);
            replacementGraph.AddVertex(newIncomplete);
            replacementGraph.AddVertex(vertex1);
            replacementGraph.AddVertex(vertex2);
            replacementGraph.AddHyperEdge(hedge);

            SourceGraph.Transform(patternGraph, replacementGraph);

            Assert.IsTrue(SourceGraph.Vertices.Count == startVertexCount + 6);
            Assert.IsTrue(SourceGraph.Edges.Count == startHyperedgeCount + 2);
            Assert.IsTrue(SourceGraph.Vertices.Contains(addedVertex));
            Assert.IsTrue(SourceGraph.Vertices.Contains(secondlyAddedVertex));
            Assert.IsTrue(SourceGraph.Vertices.Count(x => x.Poles.Count == vertex1.Poles.Count) >= 2);
            Assert.IsTrue(SourceGraph.Vertices.Count(x => x.Poles.Count == vertex2.Poles.Count) >= 2);
            Assert.IsTrue(SourceGraph.Edges.Count(x => x.Poles.Count == hedge.Poles.Count) >= 2);
            Assert.IsTrue(SourceGraph.Edges.Count(x => x.Links.Count == hedge.Links.Count) >= 2);
        }

        [Test]
        public void MultipleSubgraphTransformationWithIncidentHyperedgesAndIncompleteVerticesWithEdgesNotExistingInPatternTest()
        {
            var startVertexCount = SourceGraph.Vertices.Count;
            var startHyperedgeCount = SourceGraph.Edges.Count;

            (var addedVertex, var addedVertex1, var hedge1, var hedge2) = CreateVerticesAndIncidentHyperedges();
            SourceGraph.AddVertex(addedVertex);
            SourceGraph.AddVertex(addedVertex1);
            SourceGraph.AddHyperEdge(hedge1);
            SourceGraph.AddHyperEdge(hedge2);
            var nonExistingInPatternHyperedge = new Hyperedge();
            nonExistingInPatternHyperedge.AddPole(addedVertex.Poles.First());
            nonExistingInPatternHyperedge.AddPole(SourceGraph.ExternalPoles.First());
            nonExistingInPatternHyperedge.AddLink(addedVertex.Poles.First(), SourceGraph.ExternalPoles.First());
            SourceGraph.AddHyperEdge(nonExistingInPatternHyperedge);

            (var secondlyAddedVertex, var secondlyAddedVertex1, var secondlyHedge1, var secondlyHedge2) = CreateVerticesAndIncidentHyperedges();
            SourceGraph.AddVertex(secondlyAddedVertex);
            SourceGraph.AddVertex(secondlyAddedVertex1);
            SourceGraph.AddHyperEdge(secondlyHedge1);
            SourceGraph.AddHyperEdge(secondlyHedge2);
            var nonExistingInPatternHyperedge1 = new Hyperedge();
            nonExistingInPatternHyperedge1.AddPole(secondlyAddedVertex.Poles.Last());
            nonExistingInPatternHyperedge1.AddPole(SourceGraph.Vertices.First().Poles.First());
            nonExistingInPatternHyperedge1.AddLink(secondlyAddedVertex.Poles.Last(), SourceGraph.Vertices.First().Poles.First());
            SourceGraph.AddHyperEdge(nonExistingInPatternHyperedge1);

            (var newVertex, var newVertex1, var newHedge1, var newHedge2) = CreateVerticesAndIncidentHyperedges();
            newVertex.IsIncomplete = true;
            newVertex.Poles.RemoveAll(x => !x.EdgeOwners.Any());

            var patternGraph = new HPGraph();
            patternGraph.AddVertex(newVertex);
            patternGraph.AddVertex(newVertex1);
            patternGraph.AddHyperEdge(newHedge1);
            patternGraph.AddHyperEdge(newHedge2);

            var replacementGraph = new HPGraph();
            var newIncomplete = new VertexForTransformation(newVertex);
            var (vertex1, vertex2, hedge) = CreateSubgraphWithIncompleteVertex(newIncomplete);
            replacementGraph.AddVertex(newIncomplete);
            replacementGraph.AddVertex(vertex1);
            replacementGraph.AddVertex(vertex2);
            replacementGraph.AddHyperEdge(hedge);

            SourceGraph.Transform(patternGraph, replacementGraph);

            Assert.IsTrue(SourceGraph.Vertices.Count == startVertexCount + 6);
            Assert.IsTrue(SourceGraph.Edges.Count == startHyperedgeCount + 4);
            Assert.IsTrue(SourceGraph.Vertices.Contains(addedVertex));
            Assert.IsTrue(SourceGraph.Vertices.Contains(secondlyAddedVertex));
            Assert.IsTrue(SourceGraph.Edges.Contains(nonExistingInPatternHyperedge));
            Assert.IsTrue(SourceGraph.Edges.Contains(nonExistingInPatternHyperedge1));
            Assert.IsTrue(SourceGraph.Vertices.Count(x => x.Poles.Count == vertex1.Poles.Count) >= 2);
            Assert.IsTrue(SourceGraph.Vertices.Count(x => x.Poles.Count == vertex2.Poles.Count) >= 2);
            Assert.IsTrue(SourceGraph.Edges.Count(x => x.Poles.Count == hedge.Poles.Count) >= 2);
            Assert.IsTrue(SourceGraph.Edges.Count(x => x.Links.Count == hedge.Links.Count) >= 2);
        }
        #endregion
        private static (Vertex,Vertex,Hyperedge) CreateSubgraphWithIncompleteVertex(VertexForTransformation incompleteVertex)
        {
            var addedVertex = CreateVertex(51);
            var addedVertex1 = CreateVertex(52);
            var hEdge = new Hyperedge();

            foreach (var pole in addedVertex.Poles)
            {
                hEdge.AddPole(pole);
            }
            foreach (var pole in addedVertex1.Poles)
            {
                hEdge.AddPole(pole);
            }
            foreach (var pole in incompleteVertex.Poles)
            {
                hEdge.AddPole(pole);
            }

            var firstPole = hEdge.Poles[0];
            var secondPole = hEdge.Poles[1];

            foreach (var pole in hEdge.Poles.Skip(2))
            {
                if (firstPole != pole)
                {
                    hEdge.AddLink(pole, firstPole);
                    hEdge.AddLink(secondPole, pole);
                }
            }
            foreach(var pole in incompleteVertex.Poles)
            {
                foreach(var pole1 in hEdge.Poles.SkipLast(incompleteVertex.Poles.Count))
                {
                    hEdge.AddLink(pole, pole1);
                }
            }

            return (addedVertex, addedVertex1, hEdge);
        }

        private static (VertexForTransformation, VertexForTransformation, Hyperedge, Hyperedge) CreateVerticesAndIncidentHyperedges()
        {
            var addedVertex = CreateVertex(5);
            var addedVertex1 = CreateVertex(10);
            var hedge1 = new Hyperedge();
            var hedge2 = new Hyperedge();
            foreach (var pole in addedVertex.Poles.Skip(2))
            {
                hedge1.AddPole(pole);
            }
            foreach (var pole in addedVertex.Poles.SkipLast(2))
            {
                hedge2.AddPole(pole);
            }

            foreach (var pole in addedVertex1.Poles.Skip(3))
            {
                hedge1.AddPole(pole);
            }
            foreach (var pole in addedVertex1.Poles.SkipLast(3))
            {
                hedge2.AddPole(pole);
            }

            var firstPole = hedge1.Poles[0];
            var secondPole = hedge1.Poles[1];
            foreach (var pole in hedge1.Poles.Skip(2))
            {
                if (firstPole != pole)
                {
                    hedge1.AddLink(pole, firstPole);
                    hedge1.AddLink(secondPole, pole);
                }
            }

            firstPole = hedge2.Poles[1];
            secondPole = hedge2.Poles[2];
            foreach (var pole in hedge2.Poles.SkipLast(1))
            {
                if (firstPole != pole)
                {
                    hedge2.AddLink(pole, firstPole);
                    hedge2.AddLink(secondPole, pole);
                }
            }

            return (addedVertex, addedVertex1, hedge1, hedge2);
        }

        private static (Vertex, Vertex, Hyperedge) CreateVerticesAndHedge()
        {
            var addedVertex = CreateVertex(10);
            var addedVertex1 = CreateVertex(15);
            var hEdge = new Hyperedge();
            foreach(var pole in addedVertex.Poles)
            {
                hEdge.AddPole(pole);
            }
            foreach(var pole in addedVertex1.Poles)
            {
                hEdge.AddPole(pole);
            }

            var firstPole = hEdge.Poles[0];
            var secondPole = hEdge.Poles[1];

            foreach (var pole in hEdge.Poles.Skip(2))
            {
                if (firstPole != pole)
                {
                    hEdge.AddLink(pole, firstPole);
                    hEdge.AddLink(secondPole, pole);
                }
            }

            return (addedVertex, addedVertex1, hEdge);
        }

        private static VertexForTransformation CreateVertex(int polesCount)
        {
            var vertex = new VertexForTransformation();
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

        private HPGraph CreateRandomGraph()
        {
            var rnd = new Random();
            var graph = new HPGraph();
            for (int i = 0; i< 10; i++)
            {
                var vertex = new Vertex();

                for (int j=0; j<15; j++)
                {
                    var pole = new Pole();
                    vertex.AddPole(pole);
                }
                graph.AddVertex(vertex);
            }

            for (int i = 0; i < 10; i++)
            {
                graph.AddExternalPole(new Pole());
            }

            for (int j = 0; j< 3; j++)
            {
                var edge = new Hyperedge();
                for (int k = 0; k < 10; k++)
                {
                    var vertex = graph.Vertices[k];
                    var pole = vertex.Poles[k / (j+1)];
                    edge.AddPole(pole);
                    if (edge.Poles.Count > 1)
                    {
                        edge.AddLink(edge.Poles[k-1], pole);
                    }
                }
                for (int k = 0; k < 3; k++)
                {
                    var extPole = graph.ExternalPoles[k];
                    edge.AddPole(extPole);
                    if (edge.Poles.Count > 1)
                    {
                        edge.AddLink(extPole, edge.Poles[j]);
                    }
                }
                graph.AddHyperEdge(edge);
            }

            return graph;
        }
    }
}
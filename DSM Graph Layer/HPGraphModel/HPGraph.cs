using System;
using System.Collections.Generic;
using System.Linq;
using DSM_Graph_Layer.HPGraphModel.IsomorphicSubgraphMatching;

namespace DSM_Graph_Layer.HPGraphModel
{
    public class HPGraph : ElementWithId
    {
        public HPGraph ParentGraph { get; set; }
        public List<Pole> ExternalPoles { get; }
        public List<Hyperedge> Edges { get; }
        public List<Vertex> Vertices { get; }

        public HPGraph()
        {
            GraphEnumerator.SetNextId(this);
            ParentGraph = null;
            ExternalPoles = new List<Pole>();
            Edges = new List<Hyperedge>();
            Vertices = new List<Vertex>();
        }

        public HPGraph(HPGraph parentGraph, List<Pole> externalPoles = null)
        {
            GraphEnumerator.SetNextId(this);
            ParentGraph = parentGraph;
            Edges = new List<Hyperedge>();
            Vertices = new List<Vertex>();
            ExternalPoles = new List<Pole>();
            if (externalPoles != null)
            {
                // Пока определено через клонирование
                foreach (var item in externalPoles)
                {
                    var pole = (Pole)item.Clone();
                    ExternalPoles.Add(pole);
                }
            }

        }

        public void AddExternalPole(Pole p)
        {
            if (!ExternalPoles.Any(x => x.Id == p.Id))
            {
                p.GraphOwner = this;
                ExternalPoles.Add(p);
            }

        }

        public void RemoveExternalPole(Pole p)
        {
            if (p.VertexOwner.OwnerGraph != this)
            {
                throw new Exception("Данный полюс невозможно удалить из текущего графа!");
            }
            else
            {
                if (ExternalPoles.Any(x => x.Id == p.Id))
                {
                    RemoveAllLinksForPole(p);
                    foreach (var edge in Edges.Where(x => x.Poles.Contains(p)))
                    {
                        edge.RemovePole(p);
                    }
                    ExternalPoles.Remove(p);
                }
            }
        }

        public void AddVertex(Vertex v)
        {
            if (!Vertices.Any(x => x.Id == v.Id))
            {
                v.OwnerGraph = this;
                Vertices.Add(v);
            }
        }

        public void AddHyperEdge(Hyperedge e) // TODO: сделать проверку на дуги/полюса
        {
            if (!Edges.Any(x => x.Id == e.Id) && e.Links.Any() && e.Poles.Any() && !e.Poles.Any(x => x.GraphOwner != this && x.GraphOwner != null))
            {
                e.OwnerGraph = this;
                Edges.Add(e);
            }
        }

        public void RemoveStructure(Structure str)
        {
            if (str is Vertex)
                RemoveVertex(str as Vertex);
            else if (str is Hyperedge)
                RemoveHyperEdge(str as Hyperedge);
            else
                throw new Exception("Попытка удаления нераспознанной структуры из графа!");
        }

        private void RemoveVertex(Vertex v)
        {
            if (Vertices.Any(x => x.Id == v.Id))
            {
                foreach (var pole in v.Poles.ToList())
                {
                    RemoveInternalPole(pole);
                }
                Vertices.Remove(v);
            }
        }

        private void RemoveHyperEdge(Hyperedge e)
        {
            if (Edges.Any(x => x.Id == e.Id))
            {
                foreach (var pole in e.Poles)
                {
                    pole.EdgeOwners.Remove(e);
                }
                Edges.Remove(e);
            }
        }

        public void RemoveInternalPole(Pole p)
        {
            // TODO: уточнить проверку
            if (p.VertexOwner == null || p.VertexOwner.OwnerGraph != this)
            {
                throw new Exception("Данный полюс невозможно удалить из текущего графа!");
            }
            else
            {
                RemoveAllLinksForPole(p);
                foreach (var edge in Edges.Where(x => x.Poles.Contains(p)))
                {
                    edge.RemovePole(p);
                }
                p.VertexOwner.RemovePole(p);
            }
        }

        public void RemoveAllLinksForPole(Pole p)
        {
            var links = Edges.SelectMany(x => x.Links).Where(x => x.SourcePole == p || x.TargetPole == p).ToList();
            foreach (var link in links)
            {
                var edge = link.EdgeOwner;
                edge.RemoveLink(link);
            }
        }

        public bool IsSubgraph(HPGraph g)
        {
            throw new NotImplementedException();
        }

        public void Transform(HPGraph leftPart, HPGraph rightPart)
        {
            var subgraphFinder = new IsomorphicVertexFinder(this, leftPart);
            subgraphFinder.Recurse();

            var results = subgraphFinder.GeneratedAnswers;
            try
            {
                foreach (var (vertices, edges, poles) in results)
                {
                    var hpGraph = new HPGraph();
                    hpGraph.Vertices.AddRange(vertices.Values);
                    hpGraph.ExternalPoles.AddRange(poles.Values.Where(x => x.VertexOwner == null));
                    hpGraph.Edges.AddRange(edges.Values);

                    DeleteSubgraph(hpGraph, vertices);
                    var insertedGraph = InitializeSubgraph(rightPart);
                    var dict = poles.ToDictionary(x => x.Key.Id, x => x.Value);
                    AddSubgraph(insertedGraph, dict);
                }
            }
            finally { }
        }

        public List<HPGraph> FindIsomorphicSubgraphs(HPGraph g)
        {
            var subgraphFinder = new IsomorphicVertexFinder(this, g);
            subgraphFinder.Recurse();

            var results = subgraphFinder.GeneratedAnswers;

            var hpGraphList = new List<HPGraph>();
            foreach (var (vertices, edges, poles) in results)
            {
                var hpGraph = new HPGraph();
                hpGraph.Vertices.AddRange(vertices.Values);
                hpGraph.ExternalPoles.AddRange(poles.Values.Where(x => x.VertexOwner == null));
                hpGraph.Edges.AddRange(edges.Values);

                hpGraphList.Add(hpGraph);
            }

            return hpGraphList;
        }

        private void DeleteSubgraph(HPGraph subgraph, Dictionary<Vertex,Vertex> matching)
        {
            foreach (var edge in subgraph.Edges)
            {
                this.RemoveStructure(edge);
            }

            var verticesForDeletion = subgraph.Vertices.Except(matching.Where(x => x.Key as VertexForTransformation != null && (x.Key as VertexForTransformation).IsIncomplete).Select(y => y.Value));
            foreach (var vertex in verticesForDeletion)
            {
                this.RemoveStructure(vertex);
            }

            var polesForDeletion = subgraph.ExternalPoles.Where(x => !x.EdgeOwners.Any());
            foreach (var pole in polesForDeletion)
            {
                this.RemoveExternalPole(pole);
            }
        }

        private void AddSubgraph(HPGraph subgraph, Dictionary<long, Pole> matching)
        {
            foreach (var pole in subgraph.ExternalPoles)
            {
                AddExternalPole(pole);
            }

            var verticesForInsertion = subgraph.Vertices.Where(x => x as VertexForTransformation == null || !(x as VertexForTransformation).IsIncomplete);
            foreach (var vertex in verticesForInsertion)
            {
                AddVertex(vertex);
            }

            foreach(var edge in subgraph.Edges.ToList())
            {
                foreach(var pole in edge.Poles.ToList())
                {
                    if (pole.VertexOwner as VertexForTransformation != null && (pole.VertexOwner as VertexForTransformation).IsIncomplete)
                    {
                        edge.AddPole(matching[pole.Id]);
                        foreach(var link in edge.Links.Where(x=>x.SourcePole == pole).ToList())
                        {
                            link.SourcePole = matching[pole.Id];
                        }
                        foreach(var link in edge.Links.Where(x=>x.TargetPole == pole).ToList())
                        {
                            link.TargetPole = matching[pole.Id];
                        }
                    }
                }
                edge.Poles.RemoveAll(x => x.VertexOwner as VertexForTransformation != null && (x.VertexOwner as VertexForTransformation).IsIncomplete);
                AddHyperEdge(edge);
            }
        }
        private HPGraph InitializeSubgraph(HPGraph subgraph)
        {
            var newGraph = new HPGraph();
            var poleMatching = new Dictionary<Pole, Pole>();
            foreach(var extPole in subgraph.ExternalPoles)
            {
                var newExtPole = new Pole(extPole.Type);
                newGraph.AddExternalPole(newExtPole);
                poleMatching.TryAdd(extPole, newExtPole);
            }
            foreach(var vertex in subgraph.Vertices.Where(x=> (x as VertexForTransformation == null) || !(x as VertexForTransformation).IsIncomplete))
            {
                var newVertex = new VertexForTransformation(false);
                poleMatching.TryAdd(vertex.Poles.First(), newVertex.Poles.First());

                foreach(var pole in vertex.Poles.Skip(1))
                {
                    var newIntPole = new Pole(pole.Type);
                    newVertex.AddPole(newIntPole);
                    poleMatching.TryAdd(pole, newIntPole);
                }
                newGraph.AddVertex(newVertex);
            }
            foreach(var vertex in subgraph.Vertices.Where(x => (x as VertexForTransformation != null) && (x as VertexForTransformation).IsIncomplete))
            {
                newGraph.AddVertex(vertex);
                foreach(var pole in vertex.Poles)
                {
                    poleMatching.TryAdd(pole, pole);
                }
            }
            foreach(var hedge in subgraph.Edges)
            {
                var newHedge = new Hyperedge();

                foreach(var pole in hedge.Poles)
                {
                    newHedge.AddPole(poleMatching[pole]);
                }
                foreach(var link in hedge.Links)
                {
                    newHedge.AddLink(poleMatching[link.SourcePole], poleMatching[link.TargetPole], link.Type);
                }
                newGraph.AddHyperEdge(newHedge);
            }

            return newGraph;
        }
    }
}

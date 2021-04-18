using System;
using System.Collections.Generic;
using System.Linq;

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
            if (!Edges.Any(x => x.Id == e.Id) && e.Links.Any() && e.Poles.Any() && !e.Poles.Any(x=>x.GraphOwner != this && x.GraphOwner != null))
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
                Edges.Remove(e);
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
            throw new NotImplementedException();
        }

        public HPGraph FindIsomorphicSubgraph(HPGraph g)
        {
            throw new NotImplementedException();
        }


    }
}

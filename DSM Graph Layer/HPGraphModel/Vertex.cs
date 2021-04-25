using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel
{
    public class Vertex : Structure
    {
        public Vertex() : base()
        {
            var vertexPole = new Pole();
            vertexPole.VertexOwner = this;
            Poles.Add(vertexPole);
        }

        public override void AddPole(Pole p)
        {
            if (!Poles.Any(x => x.Id == p.Id))
            {
                p.VertexOwner = this;
                Poles.Add(p);
            }
        }

        public override void RemovePole(Pole p)
        {
            var graph = p.VertexOwner.OwnerGraph;
            graph.RemoveAllLinksForPole(p);
            foreach (var edge in graph.Edges.Where(x => x.Poles.Contains(p)))
            {
                edge.RemovePole(p);
            }
            Poles.Remove(p);

            if (!Poles.Any())
                OwnerGraph.RemoveStructure(this);
        }
    }

    public class VertexForTransformation : Vertex
    {
        public VertexForTransformation(bool isIncomplete = false) :base()
        {
            IsIncomplete = isIncomplete;
        }
        public VertexForTransformation(VertexForTransformation vertex)
        {
            IsIncomplete = vertex.IsIncomplete;
            Id = vertex.Id;
            Poles = new List<Pole>();
            foreach (var pole in vertex.Poles)
            {
                var p = new Pole(pole);
                p.VertexOwner = this;
                Poles.Add(p);
            }
            SemanticType = vertex.SemanticType;
            vertex.Decompositions = vertex.Decompositions;
        }

        public bool IsIncomplete { get; set; }
    }
}

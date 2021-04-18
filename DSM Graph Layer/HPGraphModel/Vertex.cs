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
}

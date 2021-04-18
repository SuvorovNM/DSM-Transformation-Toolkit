using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel
{
    public abstract class Structure : ElementWithId
    {
        public HPGraph OwnerGraph { get; set; }
        public List<Pole> Poles { get; }
        public string SemanticType { get; set; }
        public List<HPGraph> Decompositions { get; }

        public Structure()
        {
            GraphEnumerator.SetNextId(this);
            Poles = new List<Pole>();
            Decompositions = new List<HPGraph>();
        }

        public abstract void AddPole(Pole p);

        // TODO: если получится, сделать другую передачу для удаления
        public virtual void RemovePole(Pole p)
        {
            if (Poles.Any(x => x.Id == p.Id))
                Poles.Remove(p);

            if (!Poles.Any())
                OwnerGraph.RemoveStructure(this);
        }

        public void AddDecomposition(HPGraph graph)
        {
            if (!Decompositions.Any(x => x.Id == graph.Id))
            {
                graph.ParentGraph = OwnerGraph;
                Decompositions.Add(graph);
            }
        }

        public HPGraph AddDecomposition()
        {
            var hpGraph = new HPGraph(OwnerGraph, Poles);
            Decompositions.Add(hpGraph);

            return hpGraph;
        }

        public void RemoveDecomposition(HPGraph graph)
        {
            if (Decompositions.Any(x => x.Id == graph.Id))
            {
                graph.ParentGraph = null;
                Decompositions.Remove(graph);
            }
        }
    }
}

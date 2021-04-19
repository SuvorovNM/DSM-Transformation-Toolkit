using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.IsomorphicSubgraphMatching
{
    public class IsomorphicEdgeFinder : IIsomorphicElementFinder<Hyperedge>
    {
        public IsomorphicEdgeFinder(
            HPGraph source, 
            HPGraph target, 
            Dictionary<Vertex, Vertex> coreSourceV, 
            Dictionary<Vertex, Vertex> coreTargetV)
        {
            HPGraphSource = source;
            HPGraphTarget = target;
            CoreSourceV = coreSourceV;
            CoreTargetV = coreTargetV;

            CoreSource = new Dictionary<Hyperedge, Hyperedge>();
            ConnSource = new Dictionary<Hyperedge, long>();
            foreach (var edge in HPGraphSource.Edges)
            {
                CoreSource.Add(edge, null);
                ConnSource.Add(edge, 0);
            }

            CoreTarget = new Dictionary<Hyperedge, Hyperedge>();
            ConnTarget = new Dictionary<Hyperedge, long>();
            foreach (var edge in HPGraphTarget.Edges)
            {
                CoreTarget.Add(edge, null);
                ConnTarget.Add(edge, 0);
            }
        }

        public Dictionary<Hyperedge, Hyperedge> CoreSource { get; set; }
        public Dictionary<Hyperedge, Hyperedge> CoreTarget { get; set; }
        public Dictionary<Hyperedge, long> ConnSource { get; set; } // Может и не нужно
        public Dictionary<Hyperedge, long> ConnTarget { get; set; } // Может и не нужно
        private HPGraph HPGraphSource { get; }
        private HPGraph HPGraphTarget { get; }
        private Dictionary<Vertex, Vertex> CoreSourceV { get; set; }
        private Dictionary<Vertex, Vertex> CoreTargetV { get; set; }


        public bool CheckFisibiltyRules(Hyperedge source, Hyperedge target)
        {
            throw new NotImplementedException();
        }

        public List<(Hyperedge, Hyperedge)> GetAllCandidatePairs()
        {
            throw new NotImplementedException();
        }

        public (Dictionary<Hyperedge, Hyperedge>, Dictionary<Pole,Pole>) Recurse()
        {
            throw new NotImplementedException();
        }

        public void RestoreVectors()
        {
            throw new NotImplementedException();
        }

        public void UpdateVectors(Hyperedge source, Hyperedge target)
        {
            throw new NotImplementedException();
        }
    }
}

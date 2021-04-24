using System;
using System.Collections.Generic;
using System.Linq;
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
        public Dictionary<Pole, Pole> PolCorr { get; set; }

        public void Recurse(long step = 1, Hyperedge source = null, Hyperedge target = null)
        {
            if (CoreTarget.Values.All(x => x != null))
            {
                var incidentEdges = GroupByIncidence();
                foreach ((var sourceGroup, var targetGroup) in incidentEdges)
                {
                    var poleFinder = new IsomorphicPoleFinder();
                    var polCorr = poleFinder.Recurse();

                    if (!TryAppendToPolesMatching(polCorr))
                    {
                        PolCorr = new Dictionary<Pole, Pole>();
                        break;
                    }
                }
            }
            else
            {
                var pairs = GetAllCandidatePairs();
                foreach((var potentialSource, var potentialTarget) in pairs)
                {
                    UpdateVectors(step, potentialSource, potentialTarget);
                    Recurse(step + 1, potentialSource, potentialTarget);
                }
            }

            RestoreVectors(step, source, target);
        }

        public void RestoreVectors(long step, Hyperedge source, Hyperedge target)
        {
            CoreSource[source] = null;
            CoreTarget[target] = null;

            /*foreach (var item in HPGraphSource.Edges)
            {
                if (ConnSource[item] == step)
                    ConnSource[item] = 0;
            }
            foreach(var item in HPGraphTarget.Edges)
            {
                if (ConnTarget[item] == step)
                    ConnTarget[item] = 0;
            }*/
        }

        public void UpdateVectors(long step, Hyperedge source, Hyperedge target)
        {
            CoreSource[source] = target;
            CoreTarget[target] = source;
        }
        public bool CheckFisibiltyRules(Hyperedge source, Hyperedge target)
        {
            throw new NotImplementedException();
        }

        public List<(Hyperedge, Hyperedge)> GetAllCandidatePairs()
        {
            var candidateSourceEdges = HPGraphSource.Edges.Where(x => CoreSource[x] == null);
            var candidateTargetEdges = HPGraphTarget.Edges.Where(x => CoreTarget[x] == null);

            var resultList = new List<(Hyperedge, Hyperedge)>();
            foreach(var source in candidateSourceEdges)
            {
                var sourceVertices = source.Poles.Select(x => x.VertexOwner).Distinct();
                foreach(var target in candidateTargetEdges.Where(x=>x.Poles.Count==source.Poles.Count))
                {
                    var targetVertices = target.Poles.Select(x => x.VertexOwner).Distinct();

                    var correctness = true;
                    foreach(var vertice in targetVertices)
                    {
                        correctness &= sourceVertices.Contains(CoreTargetV[vertice]) && CoreSourceV[CoreTargetV[vertice]] == vertice;
                    }
                    foreach(var vertice in sourceVertices)
                    {
                        correctness &= targetVertices.Contains(CoreSourceV[vertice]) && CoreTargetV[CoreSourceV[vertice]] == vertice;
                    }

                    if (correctness)
                        resultList.Add((source, target));
                }
            }

            return resultList;
        }

        private List<(Hyperedge, Hyperedge)> GroupByIncidence()
        {
            var groupList = new List<List<Hyperedge>>();

            var possibleEdges = HPGraphTarget.Edges.Except(groupList.SelectMany(x => x.Select(y => y)));
            // Target edges
            while (possibleEdges.Any())
            {
                var group = new List<Hyperedge>();
                var firstHEdge = possibleEdges.First();
                group.Add(firstHEdge);

                var anyChanges = false;
                do
                {
                    var oldCount = group.Count;
                    foreach (var pole in group.SelectMany(x => x.Poles))
                    {
                        group.Union(pole.EdgeOwners);
                    }
                    anyChanges = oldCount != group.Count;
                } while (anyChanges);

                groupList.Add(group);
            }

            var incidenceList = new List<(Hyperedge, Hyperedge)>();
            foreach(var group in groupList)
            {
                var hEdge = new Hyperedge();
                var matchedHEdge = new Hyperedge();
                foreach(var item in group)
                {
                    hEdge.Links.AddRange(item.Links);
                    hEdge.Poles.Union(item.Poles);

                    matchedHEdge.Links.AddRange(CoreTarget[item].Links);
                    matchedHEdge.Poles.Union(CoreTarget[item].Poles);
                }
                incidenceList.Add((hEdge, matchedHEdge));
            }

            return incidenceList;
        }

        private bool TryAppendToPolesMatching(Dictionary<Pole, Pole> polesMatching)
        {
            foreach ((var key, var val) in polesMatching)
            {
                if (PolCorr[key] == null)
                    PolCorr[key] = val;
                else
                    return false;
            }
            return true;
        }
    }
}

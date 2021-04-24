using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.IsomorphicSubgraphMatching
{
    public class IsomorphicVertexFinder : IIsomorphicElementFinder<Vertex>
    {
        public IsomorphicVertexFinder(HPGraph source, HPGraph target)
        {
            HPGraphSource = source;
            HPGraphTarget = target;


            CoreSource = new Dictionary<Vertex, Vertex>();
            ConnSource = new Dictionary<Vertex, long>();
            foreach (var vertex in HPGraphSource.Vertices)
            {
                CoreSource.Add(vertex, null);
                ConnSource.Add(vertex, 0);
            }

            CoreTarget = new Dictionary<Vertex, Vertex>();
            ConnTarget = new Dictionary<Vertex, long>();
            foreach (var vertex in HPGraphTarget.Vertices)
            {
                CoreTarget.Add(vertex, null);
                ConnTarget.Add(vertex, 0);
            }

            GeneratedAnswers = new List<(Dictionary<Vertex, Vertex>, Dictionary<Hyperedge, Hyperedge>, Dictionary<Pole, Pole>)>();
        }
        public Dictionary<Vertex, Vertex> CoreSource { get; set; }
        public Dictionary<Vertex, Vertex> CoreTarget { get; set; }
        public Dictionary<Vertex, long> ConnSource { get; set; }
        public Dictionary<Vertex, long> ConnTarget { get; set; }
        private HPGraph HPGraphSource { get; }
        private HPGraph HPGraphTarget { get; }
        private List<(Dictionary<Vertex, Vertex>, Dictionary<Hyperedge, Hyperedge>, Dictionary<Pole, Pole>)> GeneratedAnswers;

        public void Recurse(long step = 1, Vertex source = null, Vertex target = null)
        {
            // TODO: Продумать множественный изоморфизм?
            if (CoreTarget.Values.All(x => x != null))
            {
                // Учитывать ли несколько вариаций изорфности ребер? - вероятно нет
                var edgeFinder = new IsomorphicEdgeFinder(HPGraphSource, HPGraphTarget, CoreSource, CoreTarget);
                edgeFinder.Recurse();//(var edgeCorr, var polCorr) = 
                var edgeCorr = edgeFinder.CoreTarget;
                var polCorr = edgeFinder.PolCorr;

                if (polCorr != null)
                {
                    AppendUnlinkedMatches(polCorr);
                    GeneratedAnswers.Add((CoreTarget, edgeCorr, polCorr));
                }
            }
            else
            {
                var possiblePairs = GetAllCandidatePairs();
                foreach ((var potentialSource, var potentialTarget) in possiblePairs)
                {
                    if (CheckFisibiltyRules(potentialSource, potentialTarget))
                    {
                        UpdateVectors(step, potentialSource, potentialTarget);
                        Recurse(step + 1, potentialSource, potentialTarget);
                    }
                }
            }

            RestoreVectors(step, source, target);
        }

        public void RestoreVectors(long step, Vertex source, Vertex target)
        {
            CoreSource[source] = null;
            CoreTarget[target] = null;

            foreach (var item in HPGraphSource.Vertices)
            {
                if (ConnSource[item] == step)
                    ConnSource[item] = 0;
            }
            foreach (var item in HPGraphTarget.Vertices)
            {
                if (ConnTarget[item] == step)
                    ConnTarget[item] = 0;
            }
        }

        public void UpdateVectors(long step, Vertex source, Vertex target)
        {
            CoreSource[source] = target;
            CoreTarget[target] = source;

            // TODO: По возможности заменить на что-то более адекватное
            var connectedToSourceVertices = GetConnectedVertices(source);
            foreach (var vertex in connectedToSourceVertices)
            {
                if (ConnSource[vertex] == 0)
                    ConnSource[vertex] = step;
            }

            var connectedToTargetVertices = GetConnectedVertices(target);
            foreach (var vertex in connectedToTargetVertices)
            {
                if (ConnTarget[vertex] == 0)
                    ConnTarget[vertex] = step;
            }
        }

        public List<(Vertex, Vertex)> GetAllCandidatePairs()
        {
            var candidateSourceVertices = HPGraphSource.Vertices.Where(x => CoreSource[x] == null && ConnSource[x] != 0);
            var candidateTargetVertices = HPGraphTarget.Vertices.Where(x => CoreTarget[x] == null && ConnTarget[x] != 0);
            if (!candidateSourceVertices.Any() || !candidateTargetVertices.Any())
            {
                candidateSourceVertices = HPGraphSource.Vertices.Where(x => CoreSource[x] == null);
                candidateTargetVertices = HPGraphTarget.Vertices.Where(x => CoreTarget[x] == null);
            }

            var resultPairList = new List<(Vertex, Vertex)>();
            foreach (var source in candidateSourceVertices)
            {
                foreach (var target in candidateTargetVertices)
                {
                    resultPairList.Add((source, target));
                }
            }

            return resultPairList;
        }

        public bool CheckFisibiltyRules(Vertex source, Vertex target)
        {
            return CheckConsistencyRule(source, target) && CheckOneLookAhead(source, target) && CheckTwoLookAhead(source, target);
        }

        private bool CheckConsistencyRule(Vertex source, Vertex target)
        {
            var matchedConnectedToSource = GetConnectedVertices(source).Where(x => CoreSource[x] != null);
            var matchedConnectedToTarget = GetConnectedVertices(target).Where(x => CoreTarget[x] != null);
            var result = true;

            foreach (var vertex in matchedConnectedToSource)
            {
                result &= matchedConnectedToTarget.Any(x => CoreTarget[x] == vertex);
            }
            foreach (var vertex in matchedConnectedToTarget)
            {
                result &= matchedConnectedToSource.Any(x => CoreSource[x] == vertex);
            }

            return result;
        }

        private bool CheckOneLookAhead(Vertex source, Vertex target)
        {
            var unmatchedConnectedToSource = GetConnectedVertices(source).Where(x => CoreSource[x] == null && ConnSource[x] != 0);
            var unmatchedConnectedToTarget = GetConnectedVertices(target).Where(x => CoreTarget[x] == null && ConnTarget[x] != 0);

            return unmatchedConnectedToSource.Count() >= unmatchedConnectedToTarget.Count();
        }

        private bool CheckTwoLookAhead(Vertex source, Vertex target)
        {
            var unmatchedConnectedToSourceNotConnectedToGraph = GetConnectedVertices(source).Where(x => CoreSource[x] == null && ConnSource[x] == 0);
            var unmatchedConnectedToTargetNotConnectedToGraph = GetConnectedVertices(target).Where(x => CoreTarget[x] == null && ConnTarget[x] == 0);

            return unmatchedConnectedToSourceNotConnectedToGraph.Count() > unmatchedConnectedToTargetNotConnectedToGraph.Count();
        }

        /// <summary>
        /// Связать несвязные полюса (по внешним полюсам пройтись?)
        /// Учитывать ли множественный изоморфизм? - нет. Обоснование: трансформации могут исполниться над одними и теми же вершинами множество раз
        /// Сейчас сопоставляются лишь первые попавшиеся
        /// </summary>
        /// <param name="polCorr"></param>
        /// <returns></returns>
        private void AppendUnlinkedMatches(Dictionary<Pole, Pole> polCorr)
        {
            var sourceExternalPoles = HPGraphSource.ExternalPoles.Except(polCorr.Keys).ToList();
            var targetExternalPoles = HPGraphTarget.ExternalPoles.Except(polCorr.Values).ToList();

            for (int i = 0; i < targetExternalPoles.Count(); i++)
            {
                polCorr.Add(sourceExternalPoles[i], targetExternalPoles[i]);
            }
        }

        private IEnumerable<Vertex> GetConnectedVertices(Vertex vertex)
        {
            return vertex.Poles.SelectMany(x => x.EdgeOwners).SelectMany(x => x.Poles.Select(x => x.VertexOwner)).Distinct();
        }
    }
}

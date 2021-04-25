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
        public List<(Dictionary<Vertex, Vertex> vertices, Dictionary<Hyperedge, Hyperedge> edges, Dictionary<Pole, Pole> poles)> GeneratedAnswers { get; set; }

        public void Recurse(long step = 1, Vertex source = null, Vertex target = null)
        {
            // TODO: Продумать множественный изоморфизм?
            if (CoreTarget.Values.All(x => x != null) && ValidateVertexIsomorphism())
            {
                // Учитывать ли несколько вариаций изорфности ребер? - вероятно нет
                var edgeFinder = new IsomorphicEdgeFinder(HPGraphSource, HPGraphTarget, CoreSource, CoreTarget);
                edgeFinder.Recurse();
                if (edgeFinder.GeneratedAnswers.Any())
                {
                    var edgeCorr = edgeFinder.GeneratedAnswers[0].edges;
                    var polCorr = edgeFinder.GeneratedAnswers[0].poles;

                    if (polCorr != null)
                    {
                        AppendUnlinkedMatches(polCorr);
                        GeneratedAnswers.Add((new Dictionary<Vertex, Vertex>(CoreTarget), edgeCorr, polCorr));
                    }
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
            if (source == null || target == null)
                return;
            RestoreVectors(step, source, target);
        }

        public void RestoreVectors(long step, Vertex source, Vertex target)
        {
            CoreSource[source] = null;
            CoreTarget[target] = null;

            foreach (var item in HPGraphSource.Vertices)
            {
                if (ConnSource[item] == step - 1)
                    ConnSource[item] = 0;
            }
            foreach (var item in HPGraphTarget.Vertices)
            {
                if (ConnTarget[item] == step - 1)
                    ConnTarget[item] = 0;
            }
        }

        public void UpdateVectors(long step, Vertex source, Vertex target)
        {
            CoreSource[source] = target;
            CoreTarget[target] = source;

            if (ConnSource[source] == 0)
                ConnSource[source] = step;
            if (ConnTarget[target] == 0)
                ConnTarget[target] = step;

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
                foreach (var target in candidateTargetVertices.Where(x=>source.Poles.Count >= x.Poles.Count))
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

            return unmatchedConnectedToSourceNotConnectedToGraph.Count() >= unmatchedConnectedToTargetNotConnectedToGraph.Count();
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
            var sourceExternalPoles = HPGraphSource.ExternalPoles.Except(polCorr.Values).ToList();
            var targetExternalPoles = HPGraphTarget.ExternalPoles.Except(polCorr.Keys).ToList();

            for (int i = 0; i < targetExternalPoles.Count(); i++)
            {
                polCorr.Add(sourceExternalPoles[i], targetExternalPoles[i]);
            }

            foreach(var targetV in HPGraphTarget.Vertices)
            {
                var targetInternalPoles = targetV.Poles.Except(polCorr.Keys).ToList();
                var sourceInternalPoles = CoreTarget[targetV].Poles.Except(polCorr.Values).ToList();

                for (int i = 0; i < targetInternalPoles.Count(); i++)
                {
                    polCorr.Add(sourceInternalPoles[i], targetInternalPoles[i]);
                }
            }
        }

        private IEnumerable<Vertex> GetConnectedVertices(Vertex vertex)
        {
            return vertex.Poles.SelectMany(x => x.EdgeOwners).SelectMany(x => x.Poles.Select(x => x.VertexOwner)).Distinct();
        }

        private bool ValidateVertexIsomorphism()
        {
            var createdVertexAnswers = GeneratedAnswers.Select(x => x.vertices);

            foreach(var dict in createdVertexAnswers)
            {
                if (dict.All(x => CoreTarget[x.Key] == x.Value))
                    return false;
            }
            return true;
        }
    }
}

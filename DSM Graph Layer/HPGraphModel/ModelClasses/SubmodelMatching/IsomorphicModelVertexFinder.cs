using DSM_Graph_Layer.HPGraphModel.GraphClasses;
using DSM_Graph_Layer.HPGraphModel.GraphClasses.IsomorphicSubgraphMatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses.SubmodelMatching
{
    public class IsomorphicModelVertexFinder : IsomorphicVertexFinder
    {
        public IsomorphicModelVertexFinder(Model source, Model target) : base(source, target)
        {

        }

        public override void Recurse(long step = 1, Vertex source = null, Vertex target = null)
        {
            if (CoreTarget.Values.All(x => x != null) && ValidateVertexIsomorphism())
            {
                // Переход на уровень гиперребер
                var edgeFinder = new IsomorphicModelHyperedgeConnectorFinder(HPGraphSource as Model, HPGraphTarget as Model, CoreSource, CoreTarget);
                edgeFinder.Recurse();

                // Если ответы нашлись, то получить матрицы соответствий, добавить несвязанные полюса к матрице соответствий полюсов и добавить ответ в список изоморфных подграфов
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

        protected override List<(Vertex, Vertex)> GetAllCandidatePairs()
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
                foreach (var target in candidateTargetVertices.Where(x => source.Poles.Count >= x.Poles.Count && x.GetType() == source.GetType()))
                {
                    if (source.GetType() == typeof(EntityVertex))
                    {
                        var check = true;
                        check &= (source as EntityVertex).Label == (target as EntityVertex).Label;
                        foreach (var attr in (target as EntityVertex).Attributes)
                        {
                            check &= (source as EntityVertex).Attributes.Any(x => x.DataType == attr.DataType && x.DataValue == attr.DataValue);
                        }
                        if (check)
                            resultPairList.Add((source, target));
                    }
                    else if (source.GetType() == typeof(HyperedgeRelation))
                    {
                        var check = true;
                        check &= (source as HyperedgeVertex).Label == (target as HyperedgeVertex).Label;
                        foreach (var attr in (target as HyperedgeVertex).Attributes)
                        {
                            check &= (source as HyperedgeVertex).Attributes.Any(x => x.DataType == attr.DataType && x.DataValue == attr.DataValue);
                        }
                        if (check)
                            resultPairList.Add((source, target));
                    }
                }
            }

            return resultPairList;
        }
    }
}

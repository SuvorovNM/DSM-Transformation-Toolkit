using DSM_Graph_Layer.HPGraphModel.GraphClasses;
using DSM_Graph_Layer.HPGraphModel.GraphClasses.IsomorphicSubgraphMatching;
using DSM_Graph_Layer.HPGraphModel.ModelClasses.SubmodelMatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses.SubmetamodelMatching
{
    class IsomorphicMetamodelHyperedgeConnectorFinder : IsomorphicEdgeFinder
    {
        public IsomorphicMetamodelHyperedgeConnectorFinder(Model source,
            Model target,
            Dictionary<Vertex, Vertex> coreSourceV,
            Dictionary<Vertex, Vertex> coreTargetV) : base(source, target, coreSourceV, coreTargetV)
        {

        }

        public override bool Recurse(long step = 1, Hyperedge source = null, Hyperedge target = null)
        {
            if (CoreTarget.Values.All(x => x != null))
            {
                // Группировка гиперребер по принципу инцидентности
                var incidentEdges = GroupByIncidence();
                foreach ((var sourceGroup, var targetGroup) in incidentEdges)
                {
                    // Переход на уровень полюсов и поиск полного изоморфизма
                    var poleFinder = new IsomorphicMetamodelPoleFinder(sourceGroup, targetGroup, CoreSourceV, CoreTargetV, CoreSource, CoreTarget);
                    poleFinder.Recurse();
                    var polCorr = poleFinder.CoreTarget;

                    // Попытка добавить соответствия в матрицу PolCorr
                    if (!TryAppendToPolesMatching(polCorr))
                    {
                        PolCorr = new Dictionary<Pole, Pole>();
                        break;
                    }
                }
                // Следует также учитывать и случаи, когда гиперребра в графе отсутствуют
                if (PolCorr.Any() || HPGraphTarget.Edges.Count == 0)
                {
                    GeneratedAnswers.Add((new Dictionary<Hyperedge, Hyperedge>(CoreTarget), new Dictionary<Pole, Pole>(PolCorr)));
                    return true;
                }
            }
            else
            {
                var pairs = GetAllCandidatePairs();
                foreach ((var potentialSource, var potentialTarget) in pairs)
                {
                    // TODO: возможно, стоит добавить проверку
                    UpdateVectors(step, potentialSource, potentialTarget);
                    if (Recurse(step + 1, potentialSource, potentialTarget))
                        return true;
                }
            }

            if (source == null || target == null)
                return false;
            RestoreVectors(step, source, target);
            return false;
        }

        protected override List<(Hyperedge, Hyperedge)> GetAllCandidatePairs()
        {
            var candidateSourceEdges = HPGraphSource.Edges.Where(x => CoreSource[x] == null).Select(x => x as RelationsPortsHyperedge);
            var candidateTargetEdges = HPGraphTarget.Edges.Where(x => CoreTarget[x] == null).Select(x => x as RelationsPortsHyperedge);

            var resultList = new List<(Hyperedge, Hyperedge)>();
            foreach (var source in candidateSourceEdges)
            {
                // Получить вершины гиперребер и провести проверку на соответствия, установленные при поиске изоморфизма на уровне вершин
                // Вершины пар гиперребер могут быть не полностью изоморфны
                var sourceVertices = GetVerticesForHyperedge(source);
                foreach (var target in candidateTargetEdges.Where(x => x.CorrespondingHyperedgeVertex == source.CorrespondingHyperedgeVertex.BaseElement))
                {
                    var targetVertices = GetVerticesForHyperedge(target);

                    var correctness = true;
                    foreach (var vertice in targetVertices)
                    {
                        correctness &= sourceVertices.Contains(CoreTargetV[vertice]) && CoreSourceV[CoreTargetV[vertice]] == vertice;
                    }

                    if (correctness)
                        resultList.Add((source, target));
                }
            }

            return resultList;
        }
    }
}

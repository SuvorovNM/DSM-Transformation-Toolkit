using DSM_Graph_Layer.HPGraphModel.GraphClasses;
using DSM_Graph_Layer.HPGraphModel.GraphClasses.IsomorphicSubgraphMatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses.SubmodelMatching
{
    /// <summary>
    /// Класс поиска подмоделей в модели на уровне гиперребер
    /// </summary>
    class IsomorphicModelEdgeFinder : IsomorphicEdgeFinder
    {
        public IsomorphicModelEdgeFinder(Model source,
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
                    var poleFinder = new IsomorphicModelPoleFinder(sourceGroup, targetGroup, CoreSourceV, CoreTargetV, CoreSource, CoreTarget);
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
    }
}

using DSM_Graph_Layer.HPGraphModel.GraphClasses;
using DSM_Graph_Layer.HPGraphModel.GraphClasses.IsomorphicSubgraphMatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses.SubmetamodelMatching
{
    /// <summary>
    /// Класс поиска экземпляров полюсов в модели
    /// </summary>
    class IsomorphicMetamodelPoleFinder : IsomorphicPoleFinder
    {
        public IsomorphicMetamodelPoleFinder(Hyperedge hyperEdgeSource,
            Hyperedge hyperEdgeTarget,
            Dictionary<Vertex, Vertex> coreSourceV,
            Dictionary<Vertex, Vertex> coreTargetV,
            Dictionary<Hyperedge, Hyperedge> coreSourceW,
            Dictionary<Hyperedge, Hyperedge> coreTargetW) : base(hyperEdgeSource, hyperEdgeTarget, coreSourceV, coreTargetV, coreSourceW, coreTargetW)
        {

        }

        /// <summary>
        /// Получение всех пар-кандидатов полюсов.
        /// Так как информация о вершинах может быть не 1:1 (петля с объектом метамодели могут преобразоваться в 2 связанных объекта модели), проверку на вершины необходимо опустить
        /// </summary>
        protected override List<(Pole, Pole)> GetAllCandidatePairs()
        {
            var sourceCandidatePoles = HyperedgeSource.Poles.Where(x => CoreSource[x] == null && ConnSource[x] != 0);
            var targetCandidatePoles = HyperedgeTarget.Poles.Where(x => CoreTarget[x] == null && ConnTarget[x] != 0);

            if (!sourceCandidatePoles.Any() || !targetCandidatePoles.Any())
            {
                sourceCandidatePoles = HyperedgeSource.Poles.Where(x => CoreSource[x] == null);
                targetCandidatePoles = HyperedgeTarget.Poles.Where(x => CoreTarget[x] == null);
            }

            var resultPairList = new List<(Pole, Pole)>();

            // Пары полюсов: (экземпляр порта/отношения; порт/отношение)
            foreach (var sourcePole in sourceCandidatePoles)
            {
                foreach (var targetPole in targetCandidatePoles
                                            .Where(x => x.GetType() == sourcePole.GetType()))
                {
                    var checkCorrectness = true;
                    if (sourcePole.GetType() == typeof(EntityPort))
                    {
                        checkCorrectness = (sourcePole as EntityPort).BaseElement == targetPole;
                    }
                    else if (sourcePole.GetType() == typeof(HyperedgeRelation))
                    {
                        checkCorrectness = (sourcePole as HyperedgeRelation).BaseElement == targetPole;
                    }

                    if (checkCorrectness)
                    {
                        foreach (var edge in targetPole.EdgeOwners.Intersect(CoreTargetW.Keys))
                            checkCorrectness &= sourcePole.EdgeOwners.Contains(CoreTargetW[edge]);

                        if (checkCorrectness)
                            resultPairList.Add((sourcePole, targetPole));
                    }
                }
            }

            return resultPairList;
        }
    }
}

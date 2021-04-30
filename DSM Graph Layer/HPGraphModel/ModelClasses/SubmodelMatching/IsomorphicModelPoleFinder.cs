using DSM_Graph_Layer.HPGraphModel.GraphClasses;
using DSM_Graph_Layer.HPGraphModel.GraphClasses.IsomorphicSubgraphMatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses.SubmodelMatching
{
    class IsomorphicModelPoleFinder : IsomorphicPoleFinder
    {
        public IsomorphicModelPoleFinder(Hyperedge hyperEdgeSource,
            Hyperedge hyperEdgeTarget,
            Dictionary<Vertex, Vertex> coreSourceV,
            Dictionary<Vertex, Vertex> coreTargetV,
            Dictionary<Hyperedge, Hyperedge> coreSourceW,
            Dictionary<Hyperedge, Hyperedge> coreTargetW) : base(hyperEdgeSource, hyperEdgeTarget, coreSourceV, coreTargetV, coreSourceW, coreTargetW)
        {

        }

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

            // Пары полюсов - те полюса, которые принадлежат паре изоморфных вершин и содержат аналогичные гиперребра
            foreach (var sourcePole in sourceCandidatePoles)
            {
                // TODO: Можно потом добавить еще проверку на количество связей, если потребуется
                foreach (var targetPole in targetCandidatePoles
                                            .Where(x => CoreSourceV[sourcePole.VertexOwner] == x.VertexOwner && 
                                                    sourcePole.EdgeOwners.Count >= x.EdgeOwners.Count &&
                                                    x.GetType() == sourcePole.GetType())) // TODO: Проверка на тип в данном случае может быть избыточной
                {
                    var checkCorrectness = true;
                    foreach (var edge in targetPole.EdgeOwners.Intersect(CoreTargetW.Keys))
                        checkCorrectness &= sourcePole.EdgeOwners.Contains(CoreTargetW[edge]);

                    if (sourcePole.GetType() == typeof(EntityPort))
                    {
                        foreach(var role in (targetPole as EntityPort).AcceptedRoles)
                        {
                            checkCorrectness &= (sourcePole as EntityPort).AcceptedRoles.Any(x => x.Name == role.Name);
                        }
                        foreach(var attr in (targetPole as EntityPort).Attributes)
                        {
                            checkCorrectness &= (sourcePole as EntityPort).Attributes.Any(x => x.DataType == attr.DataType && x.DataValue == attr.DataValue);
                        }
                    }
                    else if (sourcePole.GetType() == typeof(HyperedgeRelation))
                    {
                        foreach(var attr in (targetPole as HyperedgeRelation).Attributes)
                        {
                            checkCorrectness &= (sourcePole as HyperedgeRelation).Attributes.Any(x => x.DataType == attr.DataType && x.DataValue == attr.DataValue);
                        }
                        checkCorrectness &= (sourcePole as HyperedgeRelation).RelationRole.Name == (targetPole as HyperedgeRelation).RelationRole.Name;
                    }

                    if (checkCorrectness)
                        resultPairList.Add((sourcePole, targetPole));
                }
            }

            return resultPairList;
        }
    }
}

using DSM_Graph_Layer.HPGraphModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.GraphClasses.IsomorphicSubgraphMatching
{
    /// <summary>
    /// Класс для поиска изоморфизма на уровне полюсов
    /// </summary>
    public class IsomorphicPoleFinder : IIsomorphicElementFinder<Pole>
    {
        /// <summary>
        /// Инициализировать экземпляр класса
        /// </summary>
        /// <param name="hyperEdgeSource">Гиперребро исходного графа</param>
        /// <param name="hyperEdgeTarget">Гиперребро графа-паттерна</param>
        /// <param name="coreSourceV">Матрица соответствий для вершин исходного графа</param>
        /// <param name="coreTargetV">Матрица соответствий для вершин графа-паттерна</param>
        /// <param name="coreSourceW">Матрица соответствий для гиперребер исходного графа</param>
        /// <param name="coreTargetW">Матрица соответствий для гиперребер графа-паттерна</param>
        public IsomorphicPoleFinder(
            Hyperedge hyperEdgeSource,
            Hyperedge hyperEdgeTarget,
            Dictionary<Vertex, Vertex> coreSourceV,
            Dictionary<Vertex, Vertex> coreTargetV,
            Dictionary<Hyperedge, Hyperedge> coreSourceW,
            Dictionary<Hyperedge, Hyperedge> coreTargetW
            )
        {
            HyperedgeSource = hyperEdgeSource;
            HyperedgeTarget = hyperEdgeTarget;
            CoreSourceV = coreSourceV;
            CoreTargetV = coreTargetV;
            CoreSourceW = coreSourceW;
            CoreTargetW = coreTargetW;

            CoreSource = new Dictionary<Pole, Pole>();
            ConnSource = new Dictionary<Pole, long>();
            foreach (var pole in hyperEdgeSource.Poles)
            {
                CoreSource[pole] = null;
                ConnSource[pole] = 0;
            }

            CoreTarget = new Dictionary<Pole, Pole>();
            ConnTarget = new Dictionary<Pole, long>();
            foreach (var pole in hyperEdgeTarget.Poles)
            {
                CoreTarget[pole] = null;
                ConnTarget[pole] = 0;
            }
        }
        /// <summary>
        /// Гиперребро исходного графа
        /// </summary>
        protected Hyperedge HyperedgeSource { get; set; }
        /// <summary>
        /// Гиперребро графа-паттерна
        /// </summary>
        protected Hyperedge HyperedgeTarget { get; set; }
        /// <summary>
        /// Матрица соответствий для вершин исходного графа
        /// </summary>
        protected Dictionary<Vertex, Vertex> CoreSourceV { get; set; }
        /// <summary>
        /// Матрица соответствий для вершин графа-паттерна
        /// </summary>
        protected Dictionary<Vertex, Vertex> CoreTargetV { get; set; }
        /// <summary>
        /// Матрица соответствий для гиперребер исходного графа
        /// </summary>
        protected Dictionary<Hyperedge, Hyperedge> CoreSourceW { get; set; }
        /// <summary>
        /// Матрица соответствий для гиперребер графа-паттерна
        /// </summary>
        protected Dictionary<Hyperedge, Hyperedge> CoreTargetW { get; set; }

        /// <summary>
        /// Основная операция - рекурсивный поиск изоморфных полюсов гиперребер
        /// </summary>
        /// <param name="step">Шаг</param>
        /// <param name="source">Полюс исходного гиперребра, добавленный на предыдущем шаге</param>
        /// <param name="target">Полюс гиперребра графа-паттерна, добавленный на предыдущем шаге</param>
        /// <returns></returns>
        public virtual bool Recurse(long step = 1, Pole source = null, Pole target = null)
        {
            if (CoreTarget.Values.All(x => x != null))
            {
                return true;
            }

            var pairs = GetAllCandidatePairs();
            foreach ((var potentialSource, var potentialTarget) in pairs)
            {
                if (CheckFisibiltyRules(potentialSource, potentialTarget))
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

        protected override void RestoreVectors(long step, Pole source, Pole target)
        {
            CoreSource[source] = null;
            CoreTarget[target] = null;

            foreach (var item in HyperedgeSource.Poles)
            {
                if (ConnSource[item] == step - 1)
                    ConnSource[item] = 0;
            }
            foreach (var item in HyperedgeTarget.Poles)
            {
                if (ConnTarget[item] == step - 1)
                    ConnTarget[item] = 0;
            }
        }

        protected override void UpdateVectors(long step, Pole source, Pole target)
        {
            CoreSource[source] = target;
            CoreTarget[target] = source;

            if (ConnSource[source] == 0)
                ConnSource[source] = step;
            if (ConnTarget[target] == 0)
                ConnTarget[target] = step;

            var connectedToSourcePoles = GetConnectedPoles(source, HyperedgeSource);
            foreach (var pole in connectedToSourcePoles)
            {
                if (ConnSource[pole] == 0)
                    ConnSource[pole] = step;
            }

            var connectedToTargetPoles = GetConnectedPoles(target, HyperedgeTarget);
            foreach (var pole in connectedToTargetPoles)
            {
                if (ConnTarget[pole] == 0)
                    ConnTarget[pole] = step;
            }
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
                                            .Where(x => CoreSourceV[sourcePole.VertexOwner] == x.VertexOwner && sourcePole.EdgeOwners.Count >= x.EdgeOwners.Count))
                {
                    var checkCorrectness = true;
                    foreach (var edge in targetPole.EdgeOwners.Intersect(CoreTargetW.Keys))
                        checkCorrectness &= sourcePole.EdgeOwners.Contains(CoreTargetW[edge]);

                    if (checkCorrectness)
                        resultPairList.Add((sourcePole, targetPole));
                }
            }

            return resultPairList;
        }

        protected override bool CheckFisibiltyRules(Pole source, Pole target)
        {
            return CheckConsistencyRule(source, target) && CheckOneLookAhead(source, target) && CheckTwoLookAhead(source, target);
        }

        private bool CheckConsistencyRule(Pole source, Pole target)
        {
            var matchedConnectedToSource = GetConnectedPoles(source, HyperedgeSource).Where(x => CoreSource[x] != null);
            var matchedConnectedToTarget = GetConnectedPoles(target, HyperedgeTarget).Where(x => CoreTarget[x] != null);
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

        private bool CheckOneLookAhead(Pole source, Pole target)
        {
            var unmatchedConnectedToSource = GetConnectedPoles(source, HyperedgeSource).Where(x => CoreSource[x] == null && ConnSource[x] != 0);
            var unmatchedConnectedToTarget = GetConnectedPoles(target, HyperedgeTarget).Where(x => CoreTarget[x] == null && ConnTarget[x] != 0);

            return unmatchedConnectedToSource.Count() >= unmatchedConnectedToTarget.Count();
        }

        private bool CheckTwoLookAhead(Pole source, Pole target)
        {
            var unmatchedConnectedToSourceNotConnectedToGraph = GetConnectedPoles(source, HyperedgeSource).Where(x => CoreSource[x] == null && ConnSource[x] == 0);
            var unmatchedConnectedToTargetNotConnectedToGraph = GetConnectedPoles(target, HyperedgeTarget).Where(x => CoreTarget[x] == null && ConnTarget[x] == 0);

            return unmatchedConnectedToSourceNotConnectedToGraph.Count() >= unmatchedConnectedToTargetNotConnectedToGraph.Count();
        }

        /// <summary>
        /// Получить полюса, связанные с выбранным
        /// </summary>
        /// <param name="p">Выбранный полюс</param>
        /// <param name="w">Гиперребро, в котором производится поиск</param>
        /// <returns>Коллекция связанных полюсов</returns>
        protected IEnumerable<Pole> GetConnectedPoles(Pole p, Hyperedge w)
        {
            var connectedPolesList = new List<Pole>();

            foreach (var link in w.Links)
            {
                if (link.SourcePole == p)
                    connectedPolesList.Add(link.TargetPole);
                else if (link.TargetPole == p)
                    connectedPolesList.Add(link.SourcePole);
            }

            return connectedPolesList;
        }
    }
}

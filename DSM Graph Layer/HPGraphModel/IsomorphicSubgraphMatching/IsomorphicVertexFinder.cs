using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.IsomorphicSubgraphMatching
{
    /// <summary>
    /// Класс для поиска изоморфизма на уровне вершин
    /// </summary>
    public class IsomorphicVertexFinder : IIsomorphicElementFinder<Vertex>
    {
        /// <summary>
        /// Инициализация экземпляра класса
        /// </summary>
        /// <param name="source">Исходный граф</param>
        /// <param name="target">Граф-паттерн</param>
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
        /// <summary>
        /// Исходный граф
        /// </summary>
        private HPGraph HPGraphSource { get; }
        /// <summary>
        /// Граф-паттерн
        /// </summary>
        private HPGraph HPGraphTarget { get; }
        /// <summary>
        /// Список найденных изоморфных подграфов в виде словарей соответствий (см. CoreTarget)
        /// </summary>
        public List<(Dictionary<Vertex, Vertex> vertices, Dictionary<Hyperedge, Hyperedge> edges, Dictionary<Pole, Pole> poles)> GeneratedAnswers { get; set; }

        /// <summary>
        /// Основная функция - рекурсивный поиск изоморфных подграфов.
        /// Множественный изоморфизм на данный момент определен лишь для уровня вершин: на уровне гиперребер и полюсов определяется первый попавшийся изоморфизм и осуществляется выход из рекурсий
        /// </summary>
        /// <param name="step">Шаг</param>
        /// <param name="source">Вершина исходного графа, добавленная на прошлом шаге</param>
        /// <param name="target">Вершина графа-паттерна, добавленная на прошлом шаге</param>
        public void Recurse(long step = 1, Vertex source = null, Vertex target = null)
        {            
            if (CoreTarget.Values.All(x => x != null) && ValidateVertexIsomorphism())
            {
                // Переход на уровень гиперребер
                var edgeFinder = new IsomorphicEdgeFinder(HPGraphSource, HPGraphTarget, CoreSource, CoreTarget);
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

            // TODO: По возможности найти более оптимальный подход к поиску связанных вершин
            var connectedToSourceVertices = GetConnectedVertices(source, HPGraphSource);
            foreach (var vertex in connectedToSourceVertices)
            {
                if (ConnSource[vertex] == 0)
                    ConnSource[vertex] = step;
            }

            var connectedToTargetVertices = GetConnectedVertices(target, HPGraphTarget);
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
            var matchedConnectedToSource = GetConnectedVertices(source, HPGraphSource).Where(x => CoreSource[x] != null);
            var matchedConnectedToTarget = GetConnectedVertices(target, HPGraphTarget).Where(x => CoreTarget[x] != null);
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
            var unmatchedConnectedToSource = GetConnectedVertices(source, HPGraphSource).Where(x => CoreSource[x] == null && ConnSource[x] != 0);
            var unmatchedConnectedToTarget = GetConnectedVertices(target, HPGraphTarget).Where(x => CoreTarget[x] == null && ConnTarget[x] != 0);

            return unmatchedConnectedToSource.Count() >= unmatchedConnectedToTarget.Count();
        }

        private bool CheckTwoLookAhead(Vertex source, Vertex target)
        {
            var unmatchedConnectedToSourceNotConnectedToGraph = GetConnectedVertices(source, HPGraphSource).Where(x => CoreSource[x] == null && ConnSource[x] == 0);
            var unmatchedConnectedToTargetNotConnectedToGraph = GetConnectedVertices(target, HPGraphTarget).Where(x => CoreTarget[x] == null && ConnTarget[x] == 0);

            return unmatchedConnectedToSourceNotConnectedToGraph.Count() >= unmatchedConnectedToTargetNotConnectedToGraph.Count();
        }

        /// <summary>
        /// Связать несвязные полюса (по внешним полюсам пройтись + полюса вершин, которые не были соотнесены)
        /// Учитывать ли множественный изоморфизм? - нет. Обоснование: трансформации могут исполниться над одними и теми же вершинами множество раз
        /// Сейчас сопоставляются лишь первые попавшиеся
        /// </summary>
        /// <param name="polCorr">Матрица соответствий полюсов</param>
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

        /// <summary>
        /// Получить вершины, связанные с выбранной вершиной
        /// </summary>
        /// <param name="vertex">Вершина, для которой определяются связанные вершины</param>
        /// <param name="graph">Граф, в котором осуществляется поиск</param>
        /// <returns>Связанные вершины</returns>
        private IEnumerable<Vertex> GetConnectedVertices(Vertex vertex, HPGraph graph)
        {
            return graph.Vertices.Where(x => x.Poles.SelectMany(x => x.EdgeOwners).Intersect(vertex.Poles.SelectMany(x => x.EdgeOwners)).Count()>0);
        }


        /// <summary>
        /// Валидация для исключения дубликатов ответов, разница в которых лишь в разном порядке обхода вершин
        /// </summary>
        /// <returns>True, если валидация прошла успешно</returns>
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

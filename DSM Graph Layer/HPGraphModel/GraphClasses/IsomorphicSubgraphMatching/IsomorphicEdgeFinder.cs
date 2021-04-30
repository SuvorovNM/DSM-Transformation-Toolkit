using DSM_Graph_Layer.HPGraphModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.GraphClasses.IsomorphicSubgraphMatching
{
    /// <summary>
    /// Класс для поиска изомоформизмов на уровне гиперребер
    /// </summary>
    public class IsomorphicEdgeFinder : IIsomorphicElementFinder<Hyperedge>
    {
        /// <summary>
        /// Инициализировать экземпляр класса
        /// </summary>
        /// <param name="source">Исходный граф</param>
        /// <param name="target">Граф-паттерн</param>
        /// <param name="coreSourceV">Матрица соответствий для вершин исходного графа</param>
        /// <param name="coreTargetV">Матрица соответствий для вершин графа-паттерна</param>
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

            PolCorr = new Dictionary<Pole, Pole>();
            GeneratedAnswers = new List<(Dictionary<Hyperedge, Hyperedge> edges, Dictionary<Pole, Pole> poles)>();
        }
        /// <summary>
        /// Исходный граф
        /// </summary>
        protected HPGraph HPGraphSource { get; }
        /// <summary>
        /// Граф-паттерн
        /// </summary>
        protected HPGraph HPGraphTarget { get; }
        /// <summary>
        /// Матрица соответствий для вершин исходного графа
        /// </summary>
        protected Dictionary<Vertex, Vertex> CoreSourceV { get; set; }
        /// <summary>
        /// Матрица соответствий для вершин графа-паттерна
        /// </summary>
        protected Dictionary<Vertex, Vertex> CoreTargetV { get; set; }
        /// <summary>
        /// Матрица соответствий для полюсов гиперребер
        /// </summary>
        public Dictionary<Pole, Pole> PolCorr { get; set; }
        /// <summary>
        /// Список полученных изоморфизмов
        /// </summary>
        public List<(Dictionary<Hyperedge, Hyperedge> edges, Dictionary<Pole, Pole> poles)> GeneratedAnswers { get; set; }

        /// <summary>
        /// Основная операция - рекурсивный поиск изоморфных гиперребер
        /// </summary>
        /// <param name="step">Шаг</param>
        /// <param name="source">Гиперребро исходного графа, добавленное на предыдущем шаге</param>
        /// <param name="target">Гиперребро графа-паттерна, добавленное на предыдущем шаге</param>
        /// <returns></returns>
        public virtual bool Recurse(long step = 1, Hyperedge source = null, Hyperedge target = null)
        {
            if (CoreTarget.Values.All(x => x != null))
            {
                // Группировка гиперребер по принципу инцидентности
                var incidentEdges = GroupByIncidence();
                foreach ((var sourceGroup, var targetGroup) in incidentEdges)
                {
                    // Переход на уровень полюсов и поиск полного изоморфизма
                    var poleFinder = new IsomorphicPoleFinder(sourceGroup, targetGroup, CoreSourceV, CoreTargetV, CoreSource, CoreTarget);
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

        protected override void RestoreVectors(long step, Hyperedge source, Hyperedge target)
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

        protected override void UpdateVectors(long step, Hyperedge source, Hyperedge target)
        {
            CoreSource[source] = target;
            CoreTarget[target] = source;
        }
        protected override bool CheckFisibiltyRules(Hyperedge source, Hyperedge target)
        {
            throw new NotImplementedException();
        }

        protected override List<(Hyperedge, Hyperedge)> GetAllCandidatePairs()
        {
            var candidateSourceEdges = HPGraphSource.Edges.Where(x => CoreSource[x] == null);
            var candidateTargetEdges = HPGraphTarget.Edges.Where(x => CoreTarget[x] == null);

            var resultList = new List<(Hyperedge, Hyperedge)>();
            foreach (var source in candidateSourceEdges)
            {
                // Получить вершины гиперребер и провести проверку на соответствия, установленные при поиске изоморфизма на уровне вершин
                // Вершины пар гиперребер должны быть полностью изоморфны
                var sourceVertices = GetVerticesForHyperedge(source);
                foreach (var target in candidateTargetEdges.Where(x => x.Poles.Count == source.Poles.Count))
                {
                    var targetVertices = GetVerticesForHyperedge(target);

                    var correctness = true;
                    foreach (var vertice in targetVertices)
                    {
                        correctness &= sourceVertices.Contains(CoreTargetV[vertice]) && CoreSourceV[CoreTargetV[vertice]] == vertice;
                    }
                    foreach (var vertice in sourceVertices)
                    {
                        correctness &= targetVertices.Contains(CoreSourceV[vertice]) && CoreTargetV[CoreSourceV[vertice]] == vertice;
                    }

                    if (correctness)
                        resultList.Add((source, target));
                }
            }

            return resultList;
        }
        /// <summary>
        /// Получить вершины, инцидентные гиперребру
        /// </summary>
        /// <param name="source">Гиперребро</param>
        /// <returns>Коллекция вершин</returns>
        protected static IEnumerable<Vertex> GetVerticesForHyperedge(Hyperedge source)
        {
            return source.Poles.Select(x => x.VertexOwner).Distinct();
        }

        /// <summary>
        /// Сгруппировать гиперребра по признаку инцидентности
        /// </summary>
        /// <returns>Список сгруппированных гиперребер</returns>
        protected List<(Hyperedge, Hyperedge)> GroupByIncidence()
        {
            var groupList = new List<List<Hyperedge>>();

            // Получить группы
            var possibleEdges = HPGraphTarget.Edges.Except(groupList.SelectMany(x => x.Select(y => y)));
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
                        group = group.Union(pole.EdgeOwners.Intersect(HPGraphTarget.Edges)).ToList();
                    }
                    anyChanges = oldCount != group.Count;
                } while (anyChanges);

                groupList.Add(group);
            }

            // Преобразовать группы в гиперребра, объединив связи и полюса гиперребер из группы
            var incidenceList = new List<(Hyperedge, Hyperedge)>();
            foreach (var group in groupList)
            {
                var hEdge = new Hyperedge();
                var matchedHedge = new Hyperedge();
                var poles = new List<Pole>();
                var matchedPoles = new List<Pole>();
                foreach (var item in group)
                {
                    hEdge.Links.AddRange(item.Links);
                    poles.AddRange(item.Poles);

                    matchedHedge.Links.AddRange(CoreTarget[item].Links);
                    matchedPoles.AddRange(CoreTarget[item].Poles);
                }
                hEdge.Poles.AddRange(poles.Distinct());
                matchedHedge.Poles.AddRange(matchedPoles.Distinct());
                incidenceList.Add((matchedHedge, hEdge));
            }

            return incidenceList;
        }

        /// <summary>
        /// Осуществить попытку добавления полюсов в матрицу соответствий.
        /// Если на месте PolCorr[i] уже есть полюс, отличающийся от полюса на месте PolCorr[i], метод завершается с результатом false.
        /// </summary>
        /// <param name="polesMatching">Добавляемая матрица соответствий</param>
        /// <returns>Результат попытки добавления - True, если добавление прошло успешно</returns>
        protected bool TryAppendToPolesMatching(Dictionary<Pole, Pole> polesMatching)
        {
            foreach ((var key, var val) in polesMatching)
            {
                if (PolCorr.ContainsKey(key) && PolCorr[key] != val)
                {
                    return false;
                }
                else
                {
                    PolCorr[key] = val;
                }
            }
            return true;
        }
    }
}

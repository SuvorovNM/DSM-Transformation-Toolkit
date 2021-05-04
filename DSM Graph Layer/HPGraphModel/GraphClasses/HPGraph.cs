using System;
using System.Collections.Generic;
using System.Linq;
using DSM_Graph_Layer.HPGraphModel.GraphClasses.IsomorphicSubgraphMatching;

namespace DSM_Graph_Layer.HPGraphModel.GraphClasses
{
    public class HPGraph : ElementWithId
    {
        /// <summary>
        /// Родительский граф - используется при декомпозиции
        /// </summary>
        public HPGraph ParentGraph { get; set; }
        /// <summary>
        /// Внешние полюса графа
        /// </summary>
        public List<Pole> ExternalPoles { get; set; }
        /// <summary>
        /// Гиперребра графа
        /// </summary>
        public List<Hyperedge> Edges { get; set; }
        /// <summary>
        /// Вершины графа
        /// </summary>
        public List<Vertex> Vertices { get; set; }

        /// <summary>
        /// Создать новый гиперграф с полюсами
        /// </summary>
        public HPGraph()
        {
            GraphEnumerator.SetNextId(this);
            ParentGraph = null;
            ExternalPoles = new List<Pole>();
            Edges = new List<Hyperedge>();
            Vertices = new List<Vertex>();
        }

        /// <summary>
        /// Создать новый гиперграф с полюсами, являющийся декомпозицией выбранного графа
        /// </summary>
        /// <param name="parentGraph">Родительский граф</param>
        /// <param name="externalPoles">Внешние полюса создаваемого гиперграфа с полюсами</param>
        public HPGraph(HPGraph parentGraph, List<Pole> externalPoles = null)
        {
            GraphEnumerator.SetNextId(this);
            ParentGraph = parentGraph;
            Edges = new List<Hyperedge>();
            Vertices = new List<Vertex>();
            ExternalPoles = new List<Pole>();
            if (externalPoles != null)
            {
                // Пока определено через клонирование
                foreach (var item in externalPoles)
                {
                    var pole = (Pole)item.Clone();
                    ExternalPoles.Add(pole);
                }
            }

        }

        /// <summary>
        /// Добавить внешний полюс к графу
        /// </summary>
        /// <param name="p">Добавляемый полюс</param>
        public void AddExternalPole(Pole p)
        {
            if (!ExternalPoles.Any(x => x.Id == p.Id))
            {
                p.GraphOwner = this;
                ExternalPoles.Add(p);
            }

        }

        /// <summary>
        /// Удалить внешний полюс из графа, а также все его вхождения в гиперребра
        /// </summary>
        /// <param name="p">Удаляемый полюс</param>
        public void RemoveExternalPole(Pole p)
        {
            if (p.VertexOwner.OwnerGraph != this)
            {
                throw new Exception("Данный полюс невозможно удалить из текущего графа!");
            }
            else
            {
                if (ExternalPoles.Any(x => x.Id == p.Id))
                {
                    RemoveAllLinksForPole(p);
                    foreach (var edge in Edges.Where(x => x.Poles.Contains(p)))
                    {
                        edge.RemovePole(p);
                    }
                    ExternalPoles.Remove(p);
                }
            }
        }

        /// <summary>
        /// Добавление структуры
        /// </summary>
        /// <param name="str">Добавляемая структура</param>
        public void AddStructure(Structure str)
        {
            if (str is Vertex)
                AddVertex(str as Vertex);
            else if (str is Hyperedge)
                AddHyperEdge(str as Hyperedge);
            else
                throw new Exception("Попытка добавления нераспознанной структуры в граф!");
        }

        /// <summary>
        /// Добавить вершину в граф (для вершины должны быть определены полюса)
        /// </summary>
        /// <param name="v">Добавляемая вершина</param>
        private void AddVertex(Vertex v)
        {
            if (!Vertices.Any(x => x.Id == v.Id))
            {
                v.OwnerGraph = this;
                Vertices.Add(v);
            }
        }

        /// <summary>
        /// Добавить гиперребро в граф (для гиперребра должны быть определены полюса и связи)
        /// </summary>
        /// <param name="e">Добавляемое гиперребро</param>
        private void AddHyperEdge(Hyperedge e)
        {
            if (!Edges.Any(x => x.Id == e.Id) && e.Links.Any() && e.Poles.Any() && !e.Poles.Any(x => x.GraphOwner != this && x.GraphOwner != null))
            {
                e.OwnerGraph = this;
                Edges.Add(e);
            }
        }

        /// <summary>
        /// Удалить структуру (вершину или гиперребро) из графа
        /// </summary>
        /// <param name="str">Удаляемая структура</param>
        public void RemoveStructure(Structure str)
        {
            if (str is Vertex)
                RemoveVertex(str as Vertex);
            else if (str is Hyperedge)
                RemoveHyperEdge(str as Hyperedge);
            else
                throw new Exception("Попытка удаления нераспознанной структуры из графа!");
        }

        /// <summary>
        /// Удалить вершину из графа
        /// </summary>
        /// <param name="v">Удаляемая вершина</param>
        private void RemoveVertex(Vertex v)
        {
            if (Vertices.Any(x => x.Id == v.Id))
            {
                foreach (var pole in v.Poles.ToList())
                {
                    RemoveInternalPole(pole);
                }
                Vertices.Remove(v);
            }
        }

        /// <summary>
        /// Удалить гиперребро из графа
        /// </summary>
        /// <param name="e">Удаляемое гиперребро</param>
        private void RemoveHyperEdge(Hyperedge e)
        {
            if (Edges.Any(x => x.Id == e.Id))
            {
                foreach (var pole in e.Poles)
                {
                    pole.EdgeOwners.Remove(e);
                }
                Edges.Remove(e);
            }
        }

        /// <summary>
        /// Удалить внутренний полюс гиперграфа
        /// </summary>
        /// <param name="p">Удаляемый полюс</param>
        private void RemoveInternalPole(Pole p)
        {
            if (p.VertexOwner == null || p.VertexOwner.OwnerGraph != this)
            {
                throw new Exception("Данный полюс невозможно удалить из текущего графа!");
            }
            else
            {
                RemoveAllLinksForPole(p);
                foreach (var edge in Edges.Where(x => x.Poles.Contains(p)))
                {
                    edge.RemovePole(p);
                }
                p.VertexOwner.RemovePole(p);
            }
        }

        /// <summary>
        /// Удалить все связи, источником или приемником которых является полюс
        /// </summary>
        /// <param name="p">Выбранный полюс</param>
        public void RemoveAllLinksForPole(Pole p)
        {
            var links = Edges.SelectMany(x => x.Links).Where(x => x.SourcePole == p || x.TargetPole == p).ToList();
            foreach (var link in links)
            {
                var edge = link.EdgeOwner;
                edge.RemoveLink(link);
            }
        }

        /// <summary>
        /// Осуществление трансформации.
        /// Для трансформации в левой и правой части могут быть определены неполные вершины, которые не будут изменяться.
        /// У таких вершин должны быть одинаковые ID и кол-во полюсов
        /// </summary>
        /// <param name="leftPart">Левая часть правила</param>
        /// <param name="rightPart">Правая часть правила</param>
        public void Transform(HPGraph leftPart, HPGraph rightPart)
        {
            var subgraphFinder = new IsomorphicVertexFinder(this, leftPart);
            subgraphFinder.Recurse();

            var results = subgraphFinder.GeneratedAnswers;
            try
            {
                foreach (var (vertices, edges, poles) in results)
                {
                    var hpGraph = new HPGraph();
                    hpGraph.Vertices.AddRange(vertices.Values);
                    hpGraph.ExternalPoles.AddRange(poles.Values.Where(x => x.VertexOwner == null));
                    hpGraph.Edges.AddRange(edges.Values);

                    DeleteSubgraph(hpGraph, vertices);
                    var insertedGraph = InitializeSubgraph(rightPart);
                    var dict = poles.ToDictionary(x => x.Key.Id, x => x.Value);
                    AddSubgraph(insertedGraph, dict);
                }
            }
            finally 
            { 
                // Может быть добавлено логирование или уведомление пользователя о наличии ошибок при трансформации
                // Блок finally нужен для возможности продолжения серии трансформаций даже при получении ошибки
            }
        }

        /// <summary>
        /// Найти изоморфный подграф в текущем графе
        /// </summary>
        /// <param name="g">Паттерн, по которому осуществляется поиск</param>
        /// <returns>Список подграфов, изоморфных паттерну</returns>
        public List<HPGraph> FindIsomorphicSubgraphs(HPGraph g)
        {
            var subgraphFinder = new IsomorphicVertexFinder(this, g);
            subgraphFinder.Recurse();

            var results = subgraphFinder.GeneratedAnswers;

            var hpGraphList = new List<HPGraph>();
            foreach (var (vertices, edges, poles) in results)
            {
                var hpGraph = new HPGraph();
                hpGraph.Vertices.AddRange(vertices.Values);
                hpGraph.ExternalPoles.AddRange(poles.Values.Where(x => x.VertexOwner == null));
                hpGraph.Edges.AddRange(edges.Values);

                hpGraphList.Add(hpGraph);
            }

            return hpGraphList;
        }

        /// <summary>
        /// Удалить подграф из текущего графа.
        /// Неполные вершины не удаляются
        /// </summary>
        /// <param name="subgraph">Удаляемый подграф</param>
        /// <param name="matching">Словарь соответствия вершин, полученный при поиске изоморфных подграфов</param>
        private void DeleteSubgraph(HPGraph subgraph, Dictionary<Vertex, Vertex> matching)
        {
            // Удаление гиперребер
            foreach (var edge in subgraph.Edges)
            {
                RemoveStructure(edge);
            }

            // Удаление полных вершин
            var verticesForDeletion = subgraph.Vertices
                .Except(matching.Where(x => x.Key as VertexForTransformation != null && (x.Key as VertexForTransformation).IsIncomplete)
                .Select(y => y.Value));
            foreach (var vertex in verticesForDeletion)
            {
                RemoveStructure(vertex);
            }

            // Удаление внешних полюсов
            var polesForDeletion = subgraph.ExternalPoles.Where(x => !x.EdgeOwners.Any());
            foreach (var pole in polesForDeletion)
            {
                RemoveExternalPole(pole);
            }
        }

        /// <summary>
        /// Добавить подграф в текущий граф.
        /// Неполные вершины не добавляются
        /// </summary>
        /// <param name="subgraph">Добавляемый подграф</param>
        /// <param name="matching">Соответствие ID полюса в графе-паттерне и изоморфного ему полюса в текущем графе (используется для неполных вершин)</param>
        private void AddSubgraph(HPGraph subgraph, Dictionary<long, Pole> matching)
        {
            // Добавление внешний полюсов
            foreach (var pole in subgraph.ExternalPoles)
            {
                AddExternalPole(pole);
            }

            // Добавление полных вершин
            var verticesForInsertion = subgraph.Vertices.Where(x => x as VertexForTransformation == null || !(x as VertexForTransformation).IsIncomplete);
            foreach (var vertex in verticesForInsertion)
            {
                AddVertex(vertex);
            }

            // Добавление гиперребер
            foreach (var edge in subgraph.Edges.ToList())
            {
                foreach (var pole in edge.Poles.ToList())
                {
                    if (pole.VertexOwner as VertexForTransformation != null && (pole.VertexOwner as VertexForTransformation).IsIncomplete)
                    {
                        edge.AddPole(matching[pole.Id]);
                        foreach (var link in edge.Links.Where(x => x.SourcePole == pole).ToList())
                        {
                            link.SourcePole = matching[pole.Id];
                        }
                        foreach (var link in edge.Links.Where(x => x.TargetPole == pole).ToList())
                        {
                            link.TargetPole = matching[pole.Id];
                        }
                    }
                }
                edge.Poles.RemoveAll(x => x.VertexOwner as VertexForTransformation != null && (x.VertexOwner as VertexForTransformation).IsIncomplete);
                AddHyperEdge(edge);
            }
        }

        /// <summary>
        /// Инициализация нового подграфа для его добавления в граф.
        /// Операция необходима, так как при нахождении нескольких подграфов, изоморфных паттерну, будет осуществлена попытка добавления элементов подграфов с одинаковыми ID.
        /// Инициализация присваивает новые ID и обходит данное ограничение
        /// </summary>
        /// <param name="subgraph">Граф из правой части правила</param>
        /// <returns>Новый подграф, готовый для добавления</returns>
        private HPGraph InitializeSubgraph(HPGraph subgraph)
        {
            var newGraph = new HPGraph();
            var poleMatching = new Dictionary<Pole, Pole>();

            // Создание внешних полюсов
            foreach (var extPole in subgraph.ExternalPoles)
            {
                var newExtPole = new Pole(extPole.Type);
                newGraph.AddExternalPole(newExtPole);
                poleMatching.TryAdd(extPole, newExtPole);
            }

            // Создание полных вершин
            foreach (var vertex in subgraph.Vertices.Where(x => x as VertexForTransformation == null || !(x as VertexForTransformation).IsIncomplete))
            {
                var newVertex = new VertexForTransformation(false);
                poleMatching.TryAdd(vertex.Poles.First(), newVertex.Poles.First());

                foreach (var pole in vertex.Poles.Skip(1))
                {
                    var newIntPole = new Pole(pole.Type);
                    newVertex.AddPole(newIntPole);
                    poleMatching.TryAdd(pole, newIntPole);
                }
                newGraph.AddVertex(newVertex);
            }

            // Добавление существующих неполных вершин
            foreach (var vertex in subgraph.Vertices.Where(x => x as VertexForTransformation != null && (x as VertexForTransformation).IsIncomplete))
            {
                newGraph.AddVertex(vertex);
                foreach (var pole in vertex.Poles)
                {
                    poleMatching.TryAdd(pole, pole);
                }
            }

            // Добавление гиперребер
            foreach (var hedge in subgraph.Edges)
            {
                var newHedge = new Hyperedge();

                foreach (var pole in hedge.Poles)
                {
                    newHedge.AddPole(poleMatching[pole]);
                }
                foreach (var link in hedge.Links)
                {
                    newHedge.AddLink(poleMatching[link.SourcePole], poleMatching[link.TargetPole], link.Type);
                }
                newGraph.AddHyperEdge(newHedge);
            }

            return newGraph;
        }
    }
}

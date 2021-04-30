using DSM_Graph_Layer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.GraphClasses
{
    /// <summary>
    /// Вершина HP-графа
    /// </summary>
    public class Vertex : Structure
    {
        /// <summary>
        /// Инициализировать вершину - по умолчанию создается с единственным полюсом
        /// </summary>
        public Vertex() : base()
        {
            var vertexPole = new Pole();
            vertexPole.VertexOwner = this;
            Poles.Add(vertexPole);
        }

        /// <summary>
        /// Добавить полюс к вершине
        /// </summary>
        /// <param name="p">Добавляемый полюс</param>
        public override void AddPole(Pole p)
        {
            if (!Poles.Any(x => x.Id == p.Id))
            {
                p.VertexOwner = this;
                Poles.Add(p);
            }
        }

        /// <summary>
        /// Удалить полюс из вершины
        /// </summary>
        /// <param name="p">Удаляемый полюс</param>
        public override void RemovePole(Pole p)
        {
            var graph = p.VertexOwner.OwnerGraph;
            graph.RemoveAllLinksForPole(p);
            foreach (var edge in graph.Edges.Where(x => x.Poles.Contains(p)))
            {
                edge.RemovePole(p);
            }
            Poles.Remove(p);

            if (!Poles.Any())
                OwnerGraph.RemoveStructure(this);
        }
    }

    /// <summary>
    /// Подкласс вершины, необходимый для произведения трансформаций (содержит определение неполных вершин)
    /// </summary>
    public class VertexForTransformation : Vertex
    {
        /// <summary>
        /// Инициализация вершины для трансформации
        /// </summary>
        /// <param name="isIncomplete">Является ли вершина неполной (True, если да)</param>
        public VertexForTransformation(bool isIncomplete = false) : base()
        {
            IsIncomplete = isIncomplete;
        }
        /// <summary>
        /// Сделать дубликат вершины для трансформации
        /// </summary>
        /// <param name="vertex">Исходная вершина</param>
        public VertexForTransformation(VertexForTransformation vertex)
        {
            IsIncomplete = vertex.IsIncomplete;
            Id = vertex.Id;
            Poles = new List<Pole>();
            foreach (var pole in vertex.Poles)
            {
                var p = new Pole(pole);
                p.VertexOwner = this;
                Poles.Add(p);
            }
            SemanticType = vertex.SemanticType;
            vertex.Decompositions = vertex.Decompositions;
        }

        /// <summary>
        /// Является ли вершина неполной
        /// </summary>
        public bool IsIncomplete { get; set; }
    }
}

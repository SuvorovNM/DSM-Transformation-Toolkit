using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.GraphClasses
{
    [Serializable]
    /// <summary>
    /// Структура HP-графа: вершина или гиперребро
    /// </summary>
    public abstract class Structure : ElementWithId
    {
        /// <summary>
        /// Граф, которому принадлежит структура
        /// </summary>
        public HPGraph OwnerGraph { get; set; }
        /// <summary>
        /// Полюса структуры
        /// </summary>
        public List<Pole> Poles { get; set; }
        /// <summary>
        /// Семантический тип структуры
        /// </summary>
        public string SemanticType { get; set; }
        /// <summary>
        /// Декомпозиции структуры
        /// </summary>
        public List<HPGraph> Decompositions { get; set; }

        /// <summary>
        /// Инициализировать структуру и присвоить уникальный ID
        /// </summary>
        public Structure()
        {
            GraphEnumerator.SetNextId(this);
            Poles = new List<Pole>();
            Decompositions = new List<HPGraph>();
        }

        /// <summary>
        /// Добавить полюс к структуре
        /// </summary>
        /// <param name="p">Добавляемый полюс</param>
        public abstract void AddPole(Pole p);

        /// <summary>
        /// Удалить полюс из структуры
        /// </summary>
        /// <param name="p">Удаляемый полюс</param>
        public abstract void RemovePole(Pole p);

        /// <summary>
        /// Добавить декомпозицию для структуры
        /// </summary>
        /// <param name="graph">Новый граф, которым декомпозируется структура</param>
        public void AddDecomposition(HPGraph graph)
        {
            if (!Decompositions.Any(x => x.Id == graph.Id))
            {
                graph.ParentGraph = OwnerGraph;
                Decompositions.Add(graph);
            }
        }

        /// <summary>
        /// Создать новую декомпозицию для структуры через отображение внутренних полюсов структуры на внешние полюса HP-графа
        /// </summary>
        /// <returns>Созданная декомпозиция</returns>
        public HPGraph AddDecomposition()
        {
            var hpGraph = new HPGraph(OwnerGraph, Poles);
            Decompositions.Add(hpGraph);

            return hpGraph;
        }

        /// <summary>
        /// Удалить ссылку на декомпозицию структуры
        /// </summary>
        /// <param name="graph">Удаляемая декомпозиция</param>
        public void RemoveDecomposition(HPGraph graph)
        {
            if (Decompositions.Any(x => x.Id == graph.Id))
            {
                graph.ParentGraph = null;
                Decompositions.Remove(graph);
            }
        }
    }
}

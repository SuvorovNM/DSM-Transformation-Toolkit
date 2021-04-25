using System;
using System.Collections.Generic;
using System.Text;
using DSM_Graph_Layer.HPGraphModel;

namespace DSM_Graph_Layer
{
    /// <summary>
    /// Генератор ID для графов и их элементов
    /// </summary>
    public static class GraphEnumerator
    {
        public static long currentGraphId = 0;
        public static long currentStructureId = 0;
        public static long currentPoleId = 0;
        public static long currentLinkId = 0;
        /// <summary>
        /// Получить следующий ID для графа
        /// </summary>
        /// <param name="hpGraph">Экземпляр графа</param>
        public static void SetNextId(HPGraph hpGraph)
        {
            currentGraphId++;
            hpGraph.Id = currentGraphId;
        }
        /// <summary>
        /// Получить следующий ID для структуры
        /// </summary>
        /// <param name="structure">Экземпляр структуры</param>
        public static void SetNextId(Structure structure)
        {
            currentStructureId++;
            structure.Id = currentStructureId;
        }
        /// <summary>
        /// Получить следующий ID для полюса
        /// </summary>
        /// <param name="pole">Экземпляр полюса</param>
        public static void SetNextId(Pole pole)
        {
            currentPoleId++;
            pole.Id = currentPoleId;
        }
        /// <summary>
        /// Получить следующий ID для связи
        /// </summary>
        /// <param name="link">Экземпляр связи</param>
        public static void SetNextId(Link link)
        {
            currentLinkId++;
            link.Id = currentLinkId;
        }
    }
}

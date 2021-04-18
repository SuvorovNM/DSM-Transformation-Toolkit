using DSM_Graph_Layer.HPGraphModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer
{
    public class IsomorphicSubgraphFinder
    {
        /// <summary>
        /// Отображение i-й вершины исходного графа на вершину vCoreSource[i] графа-паттерна
        /// </summary>
        private long[] vCoreSource;

        /// <summary>
        /// Отображение i-й вершины графа-паттерна на вершину vCoreTarget[i] исходного графа
        /// </summary>
        private long[] vCoreTarget;

        /// <summary>
        /// Достижимые вершины из текущих вершин исходного графа (vConnSource[i] - номер шага, на котором вершина была добавлена в массив)
        /// </summary>
        private long[] vConnSource;

        /// <summary>
        /// Достижимые вершины из текущих вершин графа-паттерна (vConnTarget[i] - номер шага, на котором вершина была добавлена в массив)
        /// </summary>
        private long[] vConnTarget;


        /// <summary>
        /// Отображение i-го гиперребра исходного графа на wCoreSource[i] гиперребра графа-паттерна
        /// </summary>
        private long[] wCoreSource;
        /// <summary>
        /// Отображение i-го гиперребра графа-паттерна на wCoreSource[i] гиперребра исходного графа
        /// </summary>
        private long[] wCoreTarget;

        /// <summary>
        /// wSteps[i] - индекс гиперребра исходного графа, добавленный на i-ом шаге
        /// </summary>
        private long[] wSteps;


        /// <summary>
        /// Отображение i-го полюса исходного графа на полюс pCoreSource[i] графа-паттерна
        /// </summary>
        private long[] pCoreSource;

        /// <summary>
        /// Отображение i-го полюса графа-паттерна на полюс pCoreTarget[i] исходного графа
        /// </summary>
        private long[] pCoreTarget;
        /// <summary>
        /// Достижимые полюса из текущих полюсов исходного графа (pConnSource[i] - номер шага, на котором i-ый полюс была добавлена в массив)
        /// </summary>
        private long[] pConnSource;
        /// <summary>
        /// Достижимые полюса из текущих полюсов графа-паттерна (pConnTarget[i] - номер шага, на котором i-ый полюс была добавлена в массив)
        /// </summary>
        private long[] pConnTarget;

        public IsomorphicSubgraphFinder(HPGraph sourceG, HPGraph targetG)
        {
            SourceGraph = sourceG;
            TargetGraph = targetG;
        }
        private HPGraph SourceGraph { get; set; }
        private HPGraph TargetGraph { get; set; }

    }
}

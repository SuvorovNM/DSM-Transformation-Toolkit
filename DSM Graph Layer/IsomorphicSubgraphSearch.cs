using DSM_Graph_Layer.HPGraphModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer
{
    public class IsomorphicSubgraphFinder
    {
        private const long nullValue = 0;
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
            GeneratedAnswers = new List<(long[], long[], long[])>();

            // TODO: переопределить индексы? создать словарь соответствий? !!!!!! Учеть полюса
            vCoreSource = new long[sourceG.Vertices.Count];
            vCoreTarget = new long[targetG.Vertices.Count];
            vCoreSource = new long[sourceG.Vertices.Count];
            vConnTarget = new long[targetG.Vertices.Count];

            wCoreSource = new long[sourceG.Edges.Count];
            wCoreTarget = new long[targetG.Edges.Count];
            wSteps = new long[targetG.Edges.Count];
        }
        private HPGraph SourceGraph { get; set; }
        private HPGraph TargetGraph { get; set; }
        public List<(long[], long[], long[])> GeneratedAnswers;

        public void RecurseV() // TODO: может добавить шаг?
        {
            if (vCoreTarget.All(x => x != nullValue))
            {
                var polCorr = RecurseW();
                if (polCorr != null)
                {
                    var finalCorr = AppendUnlinkedMatches(polCorr);
                    // TODO: Уточнить о несвязных вершинах
                    GeneratedAnswers.Add((vCoreTarget, wCoreTarget, finalCorr));
                }
                else
                {
                    RestoreVectorsV();
                }
            }
            else
            {
                var possiblePairs = GetAllCandidatePairsV();
                foreach(var pair in possiblePairs)
                {
                    if (CheckFisibilityRulesV(pair))
                    {
                        UpdateVectorsV(pair);
                        RecurseV();
                    }
                }
                RestoreVectorsV();
            }
        }

        public long[] RecurseW() // TODO: Output?
        {
            if (wCoreTarget.All(x=>x != nullValue))
            {
                var incidentEdges = GroupByIncidence();
                foreach(var pair in incidentEdges)
                {
                    var polCorr = RecurseP(pair); // TODO: добавить проверку на принадлежность соответствующим гиперребрам при сравнении
                    if (!TryAppendToPoleMatching(polCorr)) 
                    {
                        ClearPoleVectors();
                        break;
                    }
                }
                return pCoreTarget; // А нужно ли возвращать вектор?
            }

            var possiblePairs = GetAllCandidatePairsW(); // TODO: Add feasibility rules или определить их в GetAllCandidatePairsW (наверное лучше тут)?
            foreach (var pair in possiblePairs)
            {
                //TODO: возможно стоит здесь также осуществлять поиск подграфа
                var poles = RecurseP(pair);
                if (poles != null)
                {
                    UpdateVectorsW(pair);
                    RecurseW();
                }
            }

            RestoreVectorsW();
            return null; // Уточнить положение return
        }

        // Похоже придется вынести в отдельный класс
        public long[] RecurseP((Hyperedge sourceEdge, Hyperedge targetEdge) edgePair, List<long> polCorr = null) // TODO: Output?
        {
            if (polCorr == null)
            {
                polCorr = new List<long>(edgePair.targetEdge.Poles.Count);
            }
            if (polCorr.All(x => x != nullValue))
                return polCorr.ToArray();

            var possiblePairs = GetAllCandidatePairsP();
            foreach (var pair in possiblePairs)
            {
                if (CheckFisibilityRulesP(pair))
                {
                    throw new NotImplementedException();
                }
            }
        }

        public long[] AppendUnlinkedMatches(long[] polCorr)
        {
            throw new NotImplementedException();
        }

        public void RestoreVectorsV()
        {
            throw new NotImplementedException();
        }

        public void RestoreVectorsW()
        {
            throw new NotImplementedException();
        }

        public void UpdateVectorsV((Vertex sourceVertex, Vertex targetVertex) pair)
        {
            throw new NotImplementedException();
        }
        public void UpdateVectorsW((Hyperedge sourceEdge, Hyperedge targetEdge) pair)
        {
            throw new NotImplementedException();
        }

        public List<(Vertex,Vertex)> GetAllCandidatePairsV()
        {
            throw new NotImplementedException();
        }
        public List<(Hyperedge, Hyperedge)> GetAllCandidatePairsW()
        {
            throw new NotImplementedException();
        }
        public List<(Pole, Pole)> GetAllCandidatePairsP()
        {
            throw new NotImplementedException();
        }

        public bool CheckFisibilityRulesV((Vertex sourceVertex, Vertex targetVertex) pair)
        {
            throw new NotImplementedException();
        }

        public bool CheckFisibilityRulesP((Pole sourcePole, Pole targetPole) pair)
        {
            throw new NotImplementedException();
        }

        public List<(Hyperedge, Hyperedge)> GroupByIncidence()
        {
            // TODO: Расшифровать все гиперребра и объединить Links и Poles
            throw new NotImplementedException();
        }

        public bool TryAppendToPoleMatching(long[] polCorr)
        {
            throw new NotImplementedException();
        }

        public void ClearPoleVectors()
        {

        }
    }
}

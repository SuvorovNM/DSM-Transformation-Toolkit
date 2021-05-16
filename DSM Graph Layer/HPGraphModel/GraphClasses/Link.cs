using DSM_Graph_Layer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.GraphClasses
{
    [Serializable]
    public class Link : ElementWithId
    {
        /// <summary>
        /// Полюс-источник
        /// </summary>
        public Pole SourcePole { get; set; }
        /// <summary>
        /// Полюс-приемник
        /// </summary>
        public Pole TargetPole { get; set; }
        /// <summary>
        /// Тип связи
        /// </summary>
        public LinkType Type { get; set; }
        /// <summary>
        /// Гиперребро, содержащее связь
        /// </summary>
        public Hyperedge EdgeOwner { get; set; }
        /// <summary>
        /// Инициализировать связь
        /// </summary>
        /// <param name="sourcePole">Полюс-источник</param>
        /// <param name="targetPole">Полюс-приемник</param>
        /// <param name="type">Тип связи</param>
        public Link(Pole sourcePole, Pole targetPole, LinkType type = LinkType.Edge)
        {
            GraphEnumerator.SetNextId(this);
            if (sourcePole.CanBeInput() && targetPole.CanBeOutput())
            {
                if (type == LinkType.Edge)
                {
                    if (!sourcePole.CanBeBoth() || !targetPole.CanBeBoth())
                    {
                        throw new Exception("Невозможно создать отношение между данными полюсами!");
                    }
                }
                SourcePole = sourcePole;
                TargetPole = targetPole;
                Type = type;
            }
            else
            {
                throw new Exception("Невозможно создать отношение между данными полюсами!");
            }
        }
    }
}

using DSM_Graph_Layer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel
{
    public class Link : ElementWithId
    {
        public Pole SourcePole { get; set; }
        public Pole TargetPole { get; set; }
        public LinkType Type { get; set; }
        public Hyperedge EdgeOwner { get; set; }
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

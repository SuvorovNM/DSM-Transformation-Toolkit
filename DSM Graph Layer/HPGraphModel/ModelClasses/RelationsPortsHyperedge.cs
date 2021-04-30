using DSM_Graph_Layer.HPGraphModel.GraphClasses;
using DSM_Graph_Layer.HPGraphModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    public class RelationsPortsHyperedge : Hyperedge
    {
        public RelationsPortsHyperedge BaseElement { get; set; }
        public List<RelationsPortsHyperedge> Instances { get; set; }
        public List<EntityPort> Ports
        {
            get
            {
                return Poles.Where(x => x as EntityPort != null).Select(x => x as EntityPort).ToList();
            }
        }
        public List<HyperedgeRelation> Relations
        {
            get
            {
                return Poles.Where(x => x as HyperedgeRelation != null).Select(x => x as HyperedgeRelation).ToList();
            }
        }
        public HyperedgeVertex CorrespondingHyperedgeVertex
        {
            get
            {
                return Relations.First().HyperedgeOwner;
            }
        }

        public void AddConnection(HyperedgeRelation rel, EntityPort p) // TODO: Сделать RemoveConnection?
        {
            if (p.AcceptedRoles.Contains(rel.RelationRole) &&
                (rel.BaseElement == null || p.BaseElement == null || p.BaseElement.Relations.Contains(rel.BaseElement)))
            {
                if (!Relations.Contains(rel))
                    AddRelation(rel);
                if (!Ports.Contains(p))
                    AddPort(p);
                AddLink(rel, p);
                rel.CorrespondingPort = p;
            }
            else
                throw new Exception("Невозможно простроить связь - порт не может принимать отношение с выбранной ролью!");
        }
        public void AddRelation(HyperedgeRelation rel)
        {
            AddPole(rel);
        }
        public void AddPort(EntityPort port)
        {
            AddPole(port);
        }
    }
}

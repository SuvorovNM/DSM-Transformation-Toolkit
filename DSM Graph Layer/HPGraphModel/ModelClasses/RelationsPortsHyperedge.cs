using DSM_Graph_Layer.HPGraphModel.GraphClasses;
using DSM_Graph_Layer.HPGraphModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    [Serializable]
    /// <summary>
    /// Гиперребро, связывающее порты с отношениями гиперребра
    /// </summary>
    public class RelationsPortsHyperedge : Hyperedge
    {
        /// <summary>
        /// Список портов, инцидентных гиперребру-коннектору
        /// </summary>
        public List<EntityPort> Ports
        {
            get
            {
                return Poles.Where(x => x as EntityPort != null).Select(x => x as EntityPort).ToList();
            }
        }
        /// <summary>
        /// Список отношений, инцидентных гиперребру-коннектору
        /// </summary>
        public List<HyperedgeRelation> Relations
        {
            get
            {
                return Poles.Where(x => x as HyperedgeRelation != null).Select(x => x as HyperedgeRelation).ToList();
            }
        }
        /// <summary>
        /// Вершина-гиперребро, соответствующая гиперребру-коннектору
        /// </summary>
        public HyperedgeVertex CorrespondingHyperedgeVertex
        {
            get
            {
                return Relations.First().HyperedgeOwner;
            }
        }
        /// <summary>
        /// Добавить связь между отношением и портом
        /// </summary>
        /// <param name="rel">Отношение гиперребра</param>
        /// <param name="p">Порт</param>
        public void AddConnection(HyperedgeRelation rel, EntityPort p)
        {
            if (p.AcceptedRoles.Select(x=>x.Label).Contains(rel.RelationRole.Label))
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
        /// <summary>
        /// Удалить связь между отношением и портом
        /// </summary>
        /// <param name="rel">Отношение гиперребра</param>
        /// <param name="p">Порт</param>
        public void RemoveConnection(HyperedgeRelation rel, EntityPort p)
        {
            var link = Links.First(x => x.SourcePole == rel && x.TargetPole == p || x.SourcePole == p && x.TargetPole == rel);
            if (link!= null)
            {
                Links.Remove(link);
            }
            rel.CorrespondingPort = null;
        }
        /// <summary>
        /// Добавить отношение
        /// </summary>
        /// <param name="rel">Отношение</param>
        private void AddRelation(HyperedgeRelation rel)
        {
            AddPole(rel);
        }
        /// <summary>
        /// Добавить порт
        /// </summary>
        /// <param name="port">Порт</param>
        private void AddPort(EntityPort port)
        {
            AddPole(port);
        }
    }
}

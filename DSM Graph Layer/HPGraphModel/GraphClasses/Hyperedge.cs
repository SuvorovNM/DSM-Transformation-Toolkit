using DSM_Graph_Layer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.GraphClasses
{
    [Serializable]
    /// <summary>
    /// Гиперребро HP-графа
    /// </summary>
    public class Hyperedge : Structure
    {
        /// <summary>
        /// Связи гиперребра
        /// </summary>
        public List<Link> Links { get; set; }
        public Hyperedge() : base()
        {
            Links = new List<Link>();
        }

        /// <summary>
        /// Добавить полюс к гиперребру и добавить ссылку на гиперребро у полюса
        /// </summary>
        /// <param name="p">Добавляемый полюс</param>
        public override void AddPole(Pole p)
        {
            if (!Poles.Any(x => x.Id == p.Id) && (p.VertexOwner != null || p.GraphOwner != null))
            {
                p.EdgeOwners.Add(this);
                Poles.Add(p);
            }
        }

        /// <summary>
        /// Удалить полюс из гиперребра и удалить ссылку на гиперребро у полюса.
        /// Если в гиперребре больше не осталось полюсов, то гиперребро удаляется.
        /// </summary>
        /// <param name="p">Удаляемый полюс</param>
        public override void RemovePole(Pole p)
        {
            if (Poles.Any(x => x.Id == p.Id))
            {
                RemoveLinksForPole(p);
                p.EdgeOwners.Remove(this);
                Poles.Remove(p);
            }

            if (!Poles.Any())
                OwnerGraph.RemoveStructure(this);
        }

        /// <summary>
        /// Добавить связь в гиперребро
        /// </summary>
        /// <param name="source">Полюс-источник</param>
        /// <param name="target">Полюс-приемник</param>
        /// <param name="type">Тип связи</param>
        /// <returns>Добавленная связь</returns>
        public Link AddLink(Pole source, Pole target, LinkType type = LinkType.Edge)
        {
            if (Poles.Contains(source) && Poles.Contains(target))
            {
                var link = new Link(source, target, type);
                link.EdgeOwner = this;
                Links.Add(link);
                return link;
            }
            else
            {
                throw new Exception("Выбранные полюса не принадлежат гиперребру!");
            }
        }

        /// <summary>
        /// Добавить связь в гиперребро
        /// </summary>
        /// <param name="link">Добавляемая связь</param>
        public void AddLink(Link link)
        {
            if (!Links.Any(x => x.Id == link.Id) && Poles.Contains(link.SourcePole) && Poles.Contains(link.TargetPole))
            {
                link.EdgeOwner = this;
                Links.Add(link);
            }
        }

        /// <summary>
        /// Удалить связь из гиперребра
        /// </summary>
        /// <param name="link">Удаляемая связь</param>
        public void RemoveLink(Link link)
        {
            if (Links.Any(x => x.Id == link.Id))
                Links.Remove(link);

            if (!Links.Any())
                OwnerGraph.RemoveStructure(this);
        }

        /// <summary>
        /// Удалить связи, в которых участвует выбранный полюс
        /// </summary>
        /// <param name="p">Выбранный полюс</param>
        private void RemoveLinksForPole(Pole p)
        {
            var links = Links.Where(x => x.SourcePole == p || x.TargetPole == p).ToList();
            foreach (var link in links)
            {
                RemoveLink(link);
            }
        }
    }
}

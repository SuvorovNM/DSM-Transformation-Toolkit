using DSM_Graph_Layer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel
{
    public class Hyperedge : Structure
    {
        public List<Link> Links { get; set; }
        public Hyperedge() : base()
        {
            Links = new List<Link>();
        }

        public override void AddPole(Pole p)
        {
            if (!Poles.Any(x => x.Id == p.Id) && (p.VertexOwner!=null || p.GraphOwner!=null))
                Poles.Add(p);
        }

        public override void RemovePole(Pole p)
        {
            if (Poles.Any(x => x.Id == p.Id))
            {
                RemoveLinksForPole(p);
                Poles.Remove(p);
            }               

            if (!Poles.Any())
                OwnerGraph.RemoveStructure(this);
        }

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

        public void AddLink(Link link)
        {
            if (!Links.Any(x => x.Id == link.Id) && Poles.Contains(link.SourcePole) && Poles.Contains(link.TargetPole))
            {
                link.EdgeOwner = this;
                Links.Add(link);
            }
        }

        public void RemoveLink(Link link)
        {
            if (Links.Any(x => x.Id == link.Id))
                Links.Remove(link);

            if (!Links.Any())
                OwnerGraph.RemoveStructure(this);
        }

        private void RemoveLinksForPole(Pole p)
        {
            var links = Links.Where(x => x.SourcePole == p || x.TargetPole == p).ToList();
            foreach(var link in links)
            {
                RemoveLink(link);
            }
        }
    }
}

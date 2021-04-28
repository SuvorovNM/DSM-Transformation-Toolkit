using DSM_Graph_Layer.HPGraphModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    public class Model : HPGraph, ILabeledElement, IAttributedElement, IMetamodelingElement<Model>
    {
        public Model(string label = "")
        {
            Label = label;
            Roles = new List<Role>();            
            Attributes = new List<ElementAttribute>();
            Instances = new List<Model>();
        }
        public Model(Model parentGraph, string label = "", List<Pole> externalPoles = null) : base(parentGraph, externalPoles)
        {
            Roles = parentGraph.Roles;
            Label = label;
            Attributes = new List<ElementAttribute>();
            Instances = new List<Model>();
        }

        public List<Role> Roles { get; }
        public string Label { get; set; }
        public List<ElementAttribute> Attributes { get; set; }
        public List<EntityVertex> Entities 
        { 
            get
            {
                return Vertices.Where(x => x as EntityVertex != null).Select(x => x as EntityVertex).ToList();
            } 
        }
        public List<HyperedgeVertex> Hyperedges
        {
            get
            {
                return Vertices.Where(x => x as HyperedgeVertex != null).Select(x => x as HyperedgeVertex).ToList();
            }
        }

        public Model BaseElement { get; set; }
        public List<Model> Instances { get; set; }

        /// <summary>
        /// Добавить новую пару ролей к графу
        /// </summary>
        /// <param name="r">Добавляемая роль</param>
        public void AddNewRolePairToGraph(Role r) // TODO: возможно не стоит сразу добавлять Opposite
        {
            if (!Roles.Contains(r))
                Roles.Add(r);
            if (r != r?.OppositeRole && !Roles.Contains(r?.OppositeRole))
                Roles.Add(r.OppositeRole);
        }

        /// <summary>
        /// Добавить новую роль к графу
        /// </summary>
        /// <param name="name">Наименование роли</param>
        /// <returns>Добавленная роль</returns>
        public Role AddNewRoleToGraph(string name)
        {
            var r = new Role(name);
            Roles.Add(r);

            return r;
        }

        public void SetLabel(string label)
        {
            Label = label;
        }

        public void AddNewEntityVertex(EntityVertex entity) // TODO: Разобраться с инициализацией элементов в моделях, основанных на метамоделях
        {
            AddVertex(entity);
            /*foreach(var instance in Instances)
            {
                var entInstance = entity.Instantiate(entity.Label);
                instance.AddNewEntityVertex(entInstance);
            }*/
        }
        public void RemoveEntityVertex(EntityVertex entity)
        {
            if (Entities.Contains(entity))
            {
                foreach(var port in entity.Ports)
                {
                    entity.RemovePortFromEntity(port);
                }
                entity.BaseElement?.DeleteInstance(entity);

                foreach(var instance in Instances)
                {
                    var entities = entity.Instances.Where(x => x.OwnerGraph == instance);
                    foreach(var entityInst in entities)
                    {
                        instance.RemoveEntityVertex(entityInst);
                    }
                }
                RemoveStructure(entity);
            }            
        }

        public void AddNewHyperedgeVertex(HyperedgeVertex hyperedge) // TODO: Определить момент добавления/изменения гиперребра
        {
            AddVertex(hyperedge);
            /*foreach(var instance in Instances)
            {
                var hedgeInstance = hyperedge.Instantiate(hyperedge.Label);
                instance.AddNewHyperedgeVertex(hedgeInstance);
            }*/
        }
        public void RemoveHyperedgeVertex(HyperedgeVertex hyperedge)
        {
            if (Hyperedges.Contains(hyperedge))
            {
                foreach(var rel in hyperedge.Relations)
                {
                    hyperedge.RemoveRelationFromHyperedge(rel);
                }
                hyperedge.BaseElement?.DeleteInstance(hyperedge);

                foreach(var instance in Instances)
                {
                    var hyperedges = hyperedge.Instances.Where(x => x.OwnerGraph == instance);
                    foreach(var hyperedgeInst in hyperedges)
                    {
                        instance.RemoveHyperedgeVertex(hyperedgeInst);
                    }
                }
                RemoveStructure(hyperedge);
            }
        }
        public HyperedgeVertex AddNewRelation((EntityPort port, Role role) relFirst, (EntityPort port, Role role) relSecond)
        {
            var hyperedge = (HyperedgeVertex) null;            
            if (BaseElement == null)
            {
                hyperedge = new HyperedgeVertex();
                AddNewHyperedgeVertex(hyperedge);                
            }
            else
            {
                var appropriateHyperedges = BaseElement.Hyperedges
                    .Where(x => x.Relations.Select(y => y.RelationRole).Contains(relFirst.role) && x.Relations.Select(y => y.RelationRole).Contains(relSecond.role));

                if (appropriateHyperedges.Any())
                {
                    hyperedge = appropriateHyperedges.First().Instantiate("");
                    AddNewHyperedgeVertex(hyperedge);
                }
            }

            if (hyperedge != null)
            {
                var rel1 = new HyperedgeRelation(relFirst.role);
                var rel2 = new HyperedgeRelation(relSecond.role);
                rel1.SetOppositeRelation(rel2);

                hyperedge.AddRelationPairToHyperedge(rel1);

                var relationPortsLinks = hyperedge.CorrespondingHyperedge == null ? new RelationsPortsHyperedge() : hyperedge.CorrespondingHyperedge;
                relationPortsLinks.AddConnection(rel1, relFirst.port);
                relationPortsLinks.AddConnection(rel2, relSecond.port);

                AddHyperEdge(relationPortsLinks);
            }
            return hyperedge;
        }

        public void AddLinkBetweenPortAndRelation(EntityPort p, HyperedgeRelation rel)
        {
            var relationPortsLinks = rel.HyperEdge.CorrespondingHyperedge == null ? new RelationsPortsHyperedge() : rel.HyperEdge.CorrespondingHyperedge;
            relationPortsLinks.AddConnection(rel, p);
            AddHyperEdge(relationPortsLinks);
        }

        public void SetBaseElement(Model baseElement)
        {
            BaseElement = baseElement;
            baseElement.Instances.Add(this);
        }

        public Model Instantiate(string label)
        {
            var newModel = new Model(label);
            newModel.SetBaseElement(this);

            foreach (var attribute in Attributes)
            {
                newModel.Attributes.Add(new ElementAttribute(attribute.DataValue, ""));
            }
            newModel.Roles.AddRange(Roles); // Возможно, это и не нужно
            newModel.ParentGraph = ParentGraph; // Возможно, это и не нужно

            return newModel;
        }

        public void DeleteInstance(Model instance)
        {
            if (Instances.Contains(instance))
            {
                instance.BaseElement = null;
                Instances.Remove(instance);

                foreach (var ent in instance.Entities.ToList())
                {
                    ent.BaseElement.DeleteInstance(ent);
                }
                foreach (var hyperedge in instance.Hyperedges.ToList())
                {
                    hyperedge.BaseElement.DeleteInstance(hyperedge);
                }
            }
        }
    }
}

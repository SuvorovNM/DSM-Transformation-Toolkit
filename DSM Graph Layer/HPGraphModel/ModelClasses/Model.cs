using DSM_Graph_Layer.HPGraphModel.GraphClasses;
using DSM_Graph_Layer.HPGraphModel.Interfaces;
using DSM_Graph_Layer.HPGraphModel.ModelClasses.SubmodelMatching;
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

        public HyperedgeVertex AddHyperedgeWithRelation(EntityVertex source, EntityVertex target, Role role)
        {
            var sourcePort = source.Ports.Where(x => x.AcceptedRoles.Contains(role)).First();
            var targetPort = target.Ports.Where(x => x.AcceptedRoles.Contains(role.OppositeRole)).First();

            return AddHyperedgeWithRelation(sourcePort, targetPort, role);
        }

        public HyperedgeVertex AddHyperedgeWithRelation(EntityPort source, EntityPort target, Role role)
        {
            var hyperedge = (HyperedgeVertex) null;
            var rel1 = (HyperedgeRelation) null;
            var rel2 = (HyperedgeRelation) null;
            if (BaseElement == null)
            {
                hyperedge = new HyperedgeVertex();
                AddNewHyperedgeVertex(hyperedge);

                rel1 = new HyperedgeRelation(role);
                rel2 = new HyperedgeRelation(role.OppositeRole);
                hyperedge.AddRelationPairToHyperedge(rel1, rel2);
            }
            else
            {
                var appropriateHyperedges = BaseElement.Hyperedges
                    .Where(x => x.Relations.Select(y => y.RelationRole).Contains(role) && x.Relations.Select(y => y.RelationRole).Contains(role.OppositeRole))
                    .Where(x=>x.Relations.Select(y=>y.CorrespondingPort).Contains(source.BaseElement) && x.Relations.Select(y => y.CorrespondingPort).Contains(target.BaseElement));
                // TODO: убрать странный костыль
                appropriateHyperedges = appropriateHyperedges.Where(x => x.Relations.Where(y => y.CorrespondingPort == source.BaseElement && y.OppositeRelation.CorrespondingPort == target.BaseElement).Any());

                if (appropriateHyperedges.Any())
                {
                    hyperedge = appropriateHyperedges.First().Instantiate("");
                    AddNewHyperedgeVertex(hyperedge);

                    rel1 = hyperedge.Relations.Where(x => x.RelationRole == role).First();
                    rel2 = hyperedge.Relations.Where(x => x.RelationRole == role.OppositeRole).First();
                }
            }
            if (hyperedge != null && rel1 != null && rel2 != null)
            {
                var relationPortsLinks = hyperedge.CorrespondingHyperedge == null ? new RelationsPortsHyperedge() : hyperedge.CorrespondingHyperedge;
                relationPortsLinks.AddConnection(rel1, source);
                relationPortsLinks.AddConnection(rel2, target);

                AddHyperEdge(relationPortsLinks);
            }
            return hyperedge;
        }

        private void AddLinkBetweenPortAndRelation(EntityPort p, HyperedgeRelation rel)
        {
            var relationPortsLinks = rel.HyperedgeOwner.CorrespondingHyperedge == null ? new RelationsPortsHyperedge() : rel.HyperedgeOwner.CorrespondingHyperedge;
            relationPortsLinks.AddConnection(rel, p);
            AddHyperEdge(relationPortsLinks);
        }

        public void AddRelationToHyperedge(HyperedgeVertex hedge, EntityPort source, EntityPort target, Role r)
        {
            var sourceRel = hedge.Relations.Where(x => x.RelationRole == r && x.CorrespondingPort == null);
            var targetRel = hedge.Relations.Where(x => x.RelationRole == r.OppositeRole && x.CorrespondingPort == null);
            if (sourceRel.Any() && targetRel.Any())
            {
                AddLinkBetweenPortAndRelation(source, sourceRel.First());
                AddLinkBetweenPortAndRelation(target, targetRel.First());
            }
        }
        public void AddRelationToHyperedge(HyperedgeVertex hedge, EntityVertex source, EntityVertex target, Role r)
        {
            var sourceRel = hedge.Relations.Where(x => x.RelationRole == r && x.CorrespondingPort == null);
            var targetRel = hedge.Relations.Where(x => x.RelationRole == r.OppositeRole && x.CorrespondingPort == null);

            var sourcePort = source.Ports.Where(x => x.AcceptedRoles.Contains(r));
            var targetPort = target.Ports.Where(x => x.AcceptedRoles.Contains(r.OppositeRole));

            if (sourceRel.Any() && targetRel.Any() && sourcePort.Any() && targetPort.Any())
            {
                AddLinkBetweenPortAndRelation(sourcePort.First(), sourceRel.First());
                AddLinkBetweenPortAndRelation(targetPort.First(), targetRel.First());
            }
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

        public List<Model> FindIsomorphicModels(Model model)
        {
            var subgraphFinder = new IsomorphicModelVertexFinder(this, model);
            subgraphFinder.Recurse();

            var results = subgraphFinder.GeneratedAnswers;

            var modelList = new List<Model>();
            foreach (var (vertices, edges, poles) in results)
            {
                var submodel = new Model();
                submodel.Vertices.AddRange(vertices.Values);
                submodel.ExternalPoles.AddRange(poles.Values.Where(x => x.VertexOwner == null));
                submodel.Edges.AddRange(edges.Values);

                modelList.Add(submodel);
            }

            return modelList;
        }
    }
}

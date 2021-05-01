using DSM_Graph_Layer.HPGraphModel.GraphClasses;
using DSM_Graph_Layer.HPGraphModel.Interfaces;
using DSM_Graph_Layer.HPGraphModel.ModelClasses.SubmetamodelMatching;
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
            Transformations = new Dictionary<Model, List<TransformationRule>>();
        }
        public Model(Model parentGraph, string label = "", List<Pole> externalPoles = null) : base(parentGraph, externalPoles)
        {
            Roles = parentGraph.Roles;
            Label = label;
            Attributes = new List<ElementAttribute>();
            Instances = new List<Model>();
            Transformations = new Dictionary<Model, List<TransformationRule>>();
        }
        public Model(
            IEnumerable<EntityVertex> entities,
            IEnumerable<HyperedgeVertex> hyperedges,
            IEnumerable<RelationsPortsHyperedge> hyperedgeConnectors = null,
            IEnumerable<Pole> externalPoles = null)
        {
            Vertices = entities == null ? new List<Vertex>() : new List<Vertex>(entities);
            if (hyperedges != null)
                Vertices.AddRange(hyperedges);
            Edges = hyperedgeConnectors == null ? new List<Hyperedge>() : new List<Hyperedge>(hyperedgeConnectors);
            ExternalPoles = externalPoles == null ? new List<Pole>() : new List<Pole>(externalPoles);
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
        public List<RelationsPortsHyperedge> HyperedgeConnectors
        {
            get
            {
                return Edges.Where(x => x as RelationsPortsHyperedge != null).Select(x => x as RelationsPortsHyperedge).ToList();
            }
        }
        public Model BaseElement { get; set; }
        public List<Model> Instances { get; set; }
        public Dictionary<Model, List<TransformationRule>> Transformations { get; set; }

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
                foreach (var port in entity.Ports)
                {
                    entity.RemovePortFromEntity(port);
                }
                entity.BaseElement?.DeleteInstance(entity);

                foreach (var instance in Instances)
                {
                    var entities = entity.Instances.Where(x => x.OwnerGraph == instance);
                    foreach (var entityInst in entities)
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
                foreach (var rel in hyperedge.Relations)
                {
                    hyperedge.RemoveRelationFromHyperedge(rel);
                }
                hyperedge.BaseElement?.DeleteInstance(hyperedge);

                foreach (var instance in Instances)
                {
                    var hyperedges = hyperedge.Instances.Where(x => x.OwnerGraph == instance);
                    foreach (var hyperedgeInst in hyperedges)
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
            var hyperedge = (HyperedgeVertex)null;
            var rel1 = (HyperedgeRelation)null;
            var rel2 = (HyperedgeRelation)null;
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
                    .Where(x => x.Relations.Select(y => y.CorrespondingPort).Contains(source.BaseElement) && x.Relations.Select(y => y.CorrespondingPort).Contains(target.BaseElement));
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

        public List<Model> FindIsomorphicSubmodels(Model model)
        {
            var submodelFinder = new IsomorphicModelVertexFinder(this, model);
            submodelFinder.Recurse();

            var results = submodelFinder.GeneratedAnswers;

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

        public List<Model> FindAllInstancesOfPartialMetamodel(Model partialMetamodel)
        {
            var submetamodelFinder = new IsomorphicMetamodelVertexFinder(this, partialMetamodel);
            submetamodelFinder.Recurse();

            var results = submetamodelFinder.GeneratedAnswers;

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

        public bool AddTransformationRule(Model targetModel, TransformationRule rule)
        {
            if (!Transformations.TryGetValue(targetModel, out var rules))
                rules = InitializeTransformationRuleSequence(targetModel);

            if (rule.RightPart.Vertices.All(x => x.OwnerGraph == targetModel) && rule.RightPart.Hyperedges.All(x => x.OwnerGraph == targetModel))
            {
                rules.Add(rule);
                return true;
            }

            return false;
        }
        public bool RemoveTransformationRule(Model targetModel, TransformationRule rule)
        {
            if (!Transformations.TryGetValue(targetModel, out var rules) || !rules.Contains(rule))
                return false;

            rules.Remove(rule);
            return true;
        }

        public Model ExecuteTransformations(Model targetMetamodel)
        {
            if (BaseElement == null)
                throw new Exception("Для выполнения трансформации необходимо перейти на уровень модели!");
            if (!BaseElement.Transformations.TryGetValue(targetMetamodel, out var rules))
                throw new Exception("Данное преобразование не было задано!");

            var newModel = targetMetamodel.Instantiate(targetMetamodel.Label);

            var correspondingVerticies = new Dictionary<EntityVertex, List<EntityVertex>>();
            foreach (var rule in rules)
            {
                newModel = ExecuteTransformationRule(newModel, rule, correspondingVerticies);
            }

            return newModel;
        }
        private Model ExecuteTransformationRule(Model targetModel, TransformationRule rule, Dictionary<EntityVertex, List<EntityVertex>> correspondingVerticies)
        {
            var submetamodelSearchResults = FindAllInstancesOfPartialMetamodel(rule.LeftPart);
            foreach (var searchResult in submetamodelSearchResults)
            {
                var addedEntities = CreateEntityVerticies(targetModel, rule, correspondingVerticies, searchResult);
                var addedHyperedges = CreateHyperedgeVerticies(targetModel, rule);

                //var currentIncomplete = searchResult.Entities.Where(x => rule.LeftPart.IncompleteVertices.Contains(x.BaseElement)).ToList();
                var currentIncomplete = searchResult.Entities.SelectMany(x => x.ConnectedVertices).Distinct().Except(searchResult.Entities).ToList();
                CreateHyperedgeConnections(targetModel, rule, correspondingVerticies, addedEntities, addedHyperedges, currentIncomplete);
            }

            return targetModel;
        }

        private static void CreateHyperedgeConnections(Model targetModel, TransformationRule rule, Dictionary<EntityVertex, List<EntityVertex>> correspondingVerticies, List<EntityVertex> addedEntities, List<HyperedgeVertex> addedHyperedges, List<EntityVertex> incompletes)
        {
            // Создание связующих гиперребер
            foreach (var hyperedgeConn in rule.RightPart.HyperedgeConnectors)
            {
                var correspondingHyperedgeInstance = addedHyperedges.First(x => x.BaseElement == hyperedgeConn.CorrespondingHyperedgeVertex);
                var hyperedgeConnectorInstance = new RelationsPortsHyperedge();

                var connectedPortInstances = addedEntities.SelectMany(x => x.Ports).Where(y => hyperedgeConn.Ports.Contains(y.BaseElement));
                if (connectedPortInstances.Count() == hyperedgeConn.Ports.Count)
                {
                    // Если количество портов совпало, значит просто создаем на их основе гиперребро
                    CreateSingleHyperedgeConnector(targetModel, hyperedgeConn, correspondingHyperedgeInstance, hyperedgeConnectorInstance, connectedPortInstances);
                }
                else
                {
                    // Если не совпало, необходимо учесть неполные вершины
                    var verticies = hyperedgeConn.Ports.Select(y => y.EntityOwner).Distinct();
                    foreach (var v in incompletes) // СДЕЛАТЬ ПРОЩЕ!
                    {
                        if (correspondingVerticies.TryGetValue(v, out var targetVerticies))
                        {
                            addedEntities.AddRange(targetVerticies.Where(x => verticies.Contains(x.BaseElement)));
                        }
                    }

                    connectedPortInstances = addedEntities.SelectMany(x => x.Ports).Where(y => hyperedgeConn.Ports.Contains(y.BaseElement));
                    CreateSingleHyperedgeConnector(targetModel, hyperedgeConn, correspondingHyperedgeInstance, hyperedgeConnectorInstance, connectedPortInstances);
                }
            }
        }

        private static void CreateSingleHyperedgeConnector(Model targetModel, RelationsPortsHyperedge hyperedgeConn, HyperedgeVertex correspondingHyperedgeInstance, RelationsPortsHyperedge hyperedgeConnectorInstance, IEnumerable<EntityPort> connectedPortInstances)
        {
            foreach (var port in connectedPortInstances)
            {
                hyperedgeConnectorInstance.AddPole(port);
            }

            foreach (var rel in correspondingHyperedgeInstance.Relations)
            {
                hyperedgeConnectorInstance.AddPole(rel);
                var port = hyperedgeConn.Relations.First(x => rel.BaseElement == x).CorrespondingPort;

                // Вынужденная недетерминированность - способ по возможности избегать генерации петель
                EntityPort portInst;
                if (connectedPortInstances.Any(x => x.BaseElement == port && !hyperedgeConnectorInstance.Links.Any(y => y.TargetPole == x)))
                {
                    portInst = connectedPortInstances.First(x => x.BaseElement == port && !hyperedgeConnectorInstance.Links.Any(y => y.TargetPole == x));
                }
                else
                {
                    portInst = connectedPortInstances.First(x => x.BaseElement == port);
                }

                hyperedgeConnectorInstance.AddConnection(rel, portInst);
            }

            targetModel.AddHyperEdge(hyperedgeConnectorInstance);
        }

        private List<HyperedgeVertex> CreateHyperedgeVerticies(Model targetModel, TransformationRule rule)
        {
            var addedHyperedges = new List<HyperedgeVertex>();
            // Создание вершин-гиперребер
            foreach (var hyperedge in rule.RightPart.Hyperedges)
            {
                var hyperedgeInstance = hyperedge.Instantiate(hyperedge.Label);// Надо учитывать соотнесение информации (labels, attrs) с левой и правой частей
                targetModel.AddNewHyperedgeVertex(hyperedgeInstance);
                addedHyperedges.Add(hyperedgeInstance);
            }
            return addedHyperedges;
        }

        private List<EntityVertex> CreateEntityVerticies(Model targetModel, TransformationRule rule, Dictionary<EntityVertex, List<EntityVertex>> correspondingVerticies, Model searchResult)
        {
            var addedEntities = new List<EntityVertex>();
            // Создание вершин-сущностей
            foreach (var entity in rule.RightPart.Entities.Except(rule.RightPart.IncompleteVertices))
            {
                var entityInstance = entity.Instantiate(entity.Label);
                targetModel.AddNewEntityVertex(entityInstance);
                addedEntities.Add(entityInstance);
                // TODO: продумать соотнесение полюсов
            }
            foreach (var ent in rule.LeftPart.Entities)//.IncompleteVertices
            {
                var entInst = searchResult.Entities.First(y => y.BaseElement == ent);
                if (!correspondingVerticies.TryGetValue(entInst, out var targetV))
                {
                    correspondingVerticies.Add(entInst, addedEntities);
                }
                else
                {
                    targetV.AddRange(addedEntities);
                }
            }

            return addedEntities;
        }

        private List<TransformationRule> InitializeTransformationRuleSequence(Model targetModel)
        {
            var transformationRules = new List<TransformationRule>();
            Transformations.Add(targetModel, transformationRules);

            return transformationRules;
        }
    }

    public class ModelForTransformation : Model
    {
        public List<EntityVertex> IncompleteVertices { get; set; }

        public ModelForTransformation(IEnumerable<EntityVertex> entities,
            IEnumerable<EntityVertex> incompleteEntities,
            IEnumerable<HyperedgeVertex> hyperedges,
            IEnumerable<RelationsPortsHyperedge> hyperedgeConnectors = null,
            IEnumerable<Pole> externalPoles = null) : base(entities, hyperedges, hyperedgeConnectors, externalPoles)
        {
            Vertices = entities == null ? new List<Vertex>() : new List<Vertex>(entities);
            IncompleteVertices = incompleteEntities == null ? new List<EntityVertex>() : new List<EntityVertex>(incompleteEntities);
            if (hyperedges != null)
                Vertices.AddRange(hyperedges);
            Edges = hyperedgeConnectors == null ? new List<Hyperedge>() : new List<Hyperedge>(hyperedgeConnectors);
            ExternalPoles = externalPoles == null ? new List<Pole>() : new List<Pole>(externalPoles);
        }
    }
}

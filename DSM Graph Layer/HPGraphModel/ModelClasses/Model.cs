using DSM_Graph_Layer.HPGraphModel.GraphClasses;
using DSM_Graph_Layer.HPGraphModel.Interfaces;
using DSM_Graph_Layer.HPGraphModel.ModelClasses.SubmetamodelMatching;
using DSM_Graph_Layer.HPGraphModel.ModelClasses.SubmodelMatching;
using DSM_Graph_Layer.HPGraphModel.ModelClasses.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    [Serializable]
    /// <summary>
    /// Непосредственно модель, с которой пользователь производит взаимодействие
    /// </summary>
    public class Model : HPGraph, IAttributedElement, IMetamodelingElement<Model>
    {
        /// <summary>
        /// Инициализация модели
        /// </summary>
        /// <param name="label">Наименование модели</param>
        public Model(string label = "")
        {
            Label = label;
            Roles = new List<Role>();
            Attributes = new List<ElementAttribute>();
            Instances = new List<Model>();
            Transformations = new Dictionary<Model, List<TransformationRule>>();
        }
        /// <summary>
        /// Создание модели на основе родительской - т.е. создание декомпозиции элемента модели
        /// </summary>
        /// <param name="parentGraph">Родительская модель</param>
        /// <param name="label">Наименование создаваемой модели</param>
        /// <param name="externalPoles">Список внешних полюсов</param>
        public Model(Model parentGraph, string label = "", List<Pole> externalPoles = null) : base(parentGraph, externalPoles)
        {
            Roles = parentGraph.Roles;
            Label = label;
            Attributes = new List<ElementAttribute>();
            Instances = new List<Model>();
            Transformations = new Dictionary<Model, List<TransformationRule>>();
        }
        /// <summary>
        /// Создания модели/подмодели с заданными множествами элементов (поле GraphOwner не переназначается)
        /// </summary>
        /// <param name="entities">Сущности</param>
        /// <param name="hyperedges">Гиперребра-вершины</param>
        /// <param name="hyperedgeConnectors">Гиперребра-коннекторы</param>
        /// <param name="externalPoles">Внешние полюса</param>
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
        public Model BaseElement { get; set; }
        public List<Model> Instances { get; set; }
        public string Label { get; set; }
        public List<ElementAttribute> Attributes { get; set; }
        /// <summary>
        /// Роли, доступные в модели
        /// </summary>
        public List<Role> Roles { get; }
        /// <summary>
        /// Словарь трансформаций - трансформации описываются на уровне метамодели, а применяются на уровне модели
        /// </summary>
        public Dictionary<Model, List<TransformationRule>> Transformations { get; set; }
        /// <summary>
        /// Сущности модели
        /// </summary>
        public List<EntityVertex> Entities
        {
            get
            {
                return Vertices.Where(x => x as EntityVertex != null).Select(x => x as EntityVertex).ToList();
            }
        }
        /// <summary>
        /// Гиперребра-вершины модели
        /// </summary>
        public List<HyperedgeVertex> Hyperedges
        {
            get
            {
                return Vertices.Where(x => x as HyperedgeVertex != null).Select(x => x as HyperedgeVertex).ToList();
            }
        }
        /// <summary>
        /// Гиперребра-коннекторы модели
        /// </summary>
        public List<RelationsPortsHyperedge> HyperedgeConnectors
        {
            get
            {
                return Edges.Where(x => x as RelationsPortsHyperedge != null).Select(x => x as RelationsPortsHyperedge).ToList();
            }
        }

        /// <summary>
        /// Добавить новую пару ролей к графу (включая OppositeRole)
        /// </summary>
        /// <param name="r">Добавляемая роль</param>
        public void AddNewRolePairToGraph(Role r)
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

        /// <summary>
        /// Добавить новую сущность в модель
        /// </summary>
        /// <param name="entity">Добавляемая сущность</param>
        public void AddNewEntityVertex(EntityVertex entity)
        {
            AddStructure(entity);
        }

        /// <summary>
        /// Удалить сущность из модели
        /// </summary>
        /// <param name="entity">Удаляемая сущность</param>
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

        /// <summary>
        /// Добавление вершины-гиперребра в модель
        /// </summary>
        /// <param name="hyperedge">Добавляемое гиперребро</param>
        public void AddNewHyperedgeVertex(HyperedgeVertex hyperedge)
        {
            AddStructure(hyperedge);
        }

        /// <summary>
        /// Удаление вершины-гиперребра из модели
        /// </summary>
        /// <param name="hyperedge">Удаляемое гиперребро</param>
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

        /// <summary>
        /// Добавить новую вершину-гиперребро и простроить связи заданной роли между сущностями
        /// </summary>
        /// <param name="source">Исходная сущность</param>
        /// <param name="target">Целевая сущность</param>
        /// <param name="role">Роль отношения</param>
        /// <returns>Созданная вершина-гиперребро</returns>
        public HyperedgeVertex AddHyperedgeWithRelation(EntityVertex source, EntityVertex target, Role role)
        {
            var sourcePort = source.Ports.Where(x => x.AcceptedRoles.Contains(role)).First();
            var targetPort = target.Ports.Where(x => x.AcceptedRoles.Contains(role.OppositeRole)).First();

            return AddHyperedgeWithRelation(sourcePort, targetPort, role);
        }

        /// <summary>
        /// Добавить новую-вершину-гиперребро и построить связи заданной роли между портами сущностей
        /// </summary>
        /// <param name="source">Исходный порт</param>
        /// <param name="target">Целевой порт</param>
        /// <param name="role">Роль отношения</param>
        /// <returns>Созданная вершина-гиперребро</returns>
        public HyperedgeVertex AddHyperedgeWithRelation(EntityPort source, EntityPort target, Role role)
        {
            var hyperedge = (HyperedgeVertex)null;
            var rel1 = (HyperedgeRelation)null;
            var rel2 = (HyperedgeRelation)null;

            // Если текущий уровень - уровень метамодели, то создавать новое гиперребро с заданными отношениями
            if (BaseElement == null)
            {
                hyperedge = new HyperedgeVertex();
                AddNewHyperedgeVertex(hyperedge);

                rel1 = new HyperedgeRelation(role);
                rel2 = new HyperedgeRelation(role.OppositeRole);
                hyperedge.AddRelationPairToHyperedge(rel1, rel2);
            }
            // Если текущий уровень - уровень модели, то искать прообразы подходящих гиперребер
            else
            {
                var appropriateHyperedges = BaseElement.Hyperedges
                    .Where(x => x.Relations.Select(y => y.RelationRole).Contains(role) && x.Relations.Select(y => y.RelationRole).Contains(role.OppositeRole))
                    .Where(x => x.Relations.Where(y => y.CorrespondingPort == source.BaseElement && y.OppositeRelation.CorrespondingPort == target.BaseElement).Any());

                // Если прообраз найден, то создать экземпляр первого добавленного в метамодель гиперребра
                if (appropriateHyperedges.Any())
                {
                    hyperedge = appropriateHyperedges.First().Instantiate(appropriateHyperedges.First()?.Label ?? "");
                    AddNewHyperedgeVertex(hyperedge);

                    rel1 = hyperedge.Relations.Where(x => x.RelationRole == role && x.CorrespondingPort == null).First();
                    rel2 = rel1.OppositeRelation;
                }
            }
            // Создать связующиее гиперребро, если оно еще не было создано
            if (hyperedge != null && rel1 != null && rel2 != null)
            {
                var relationPortsLinks = hyperedge.CorrespondingHyperedge == null ? new RelationsPortsHyperedge() : hyperedge.CorrespondingHyperedge;
                relationPortsLinks.AddConnection(rel1, source);
                relationPortsLinks.AddConnection(rel2, target);

                AddStructure(relationPortsLinks);
            }
            return hyperedge;
        }        

        public void SetBaseElement(Model baseElement)
        {
            if (BaseElement != null)
                BaseElement.DeleteInstance(this);
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
            newModel.Roles.AddRange(Roles);
            newModel.ParentGraph = ParentGraph;

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

        /// <summary>
        /// Найти подмодели, изоморфные паттерну
        /// </summary>
        /// <param name="model">Паттерн</param>
        /// <returns>Список изоморфных подмоделей</returns>
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

        /// <summary>
        /// Найти все экземпляры подмоделей, изоморфные подметамодели (части метамодели)
        /// </summary>
        /// <param name="partialMetamodel">Часть метамодели (паттерн)</param>
        /// <returns>Список изоморфных подмоделей</returns>
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

        /// <summary>
        /// Добавить новое правило трансформации для преобразования в заданную метамодель
        /// </summary>
        /// <param name="targetModel">Целевая метамодель</param>
        /// <param name="rule">Правило трансформации</param>
        /// <returns>Результат добавления</returns>
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
        /// <summary>
        /// Удалить правило трансформации для заданной метамодели
        /// </summary>
        /// <param name="targetModel">Целевая метамодель</param>
        /// <param name="rule">Правило трансформации</param>
        /// <returns>Результат удаления</returns>
        public bool RemoveTransformationRule(Model targetModel, TransformationRule rule)
        {
            if (!Transformations.TryGetValue(targetModel, out var rules) || !rules.Contains(rule))
                return false;

            rules.Remove(rule);
            return true;
        }

        /// <summary>
        /// Переопределить правила трансформации для целевой метамодель (если она была перезагружена)
        /// </summary>
        /// <param name="newTarget">Новая целевая метамодель</param>
        public void RedefineTransformation(Model newTarget)
        {
            if (Transformations.Where(x => x.Key.Id == newTarget.Id).Any())
            {
                var transformation = Transformations.FirstOrDefault(x => x.Key.Id == newTarget.Id);
                foreach (var rule in transformation.Value)
                {
                    var vertexIds = rule.RightPart.Entities.Select(x => x.Id);
                    var hyperedgeIds = rule.RightPart.Hyperedges.Select(x => x.Id);
                    var poleIds = rule.RightPart.ExternalPoles.Select(x => x.Id);

                    var newRightPart = new ModelForTransformation(newTarget.Entities.Where(x => vertexIds.Contains(x.Id)), newTarget.Hyperedges.Where(x => hyperedgeIds.Contains(x.Id)), newTarget.ExternalPoles.Where(x => poleIds.Contains(x.Id)));
                    rule.RightPart = newRightPart;

                    AddTransformationRule(newTarget, rule);
                }
                Transformations.Remove(transformation.Key);
            }
        }

        /// <summary>
        /// Выполнить трансформации для преобразования в модель, основанную на целевой метамодели
        /// </summary>
        /// <param name="targetMetamodel"></param>
        /// <returns></returns>
        public Model ExecuteTransformations(Model targetMetamodel)
        {
            if (BaseElement == null)
                throw new Exception("Для выполнения трансформации необходимо перейти на уровень модели!");
            if (!BaseElement.Transformations.TryGetValue(targetMetamodel, out var rules))
                throw new Exception("Данное преобразование не было задано!");

            var newModel = targetMetamodel.Instantiate(targetMetamodel.Label);

            var modelTransformator = new ModelTransformator(newModel);
            foreach (var rule in rules)
            {
                var submetamodelSearchResults = FindAllInstancesOfPartialMetamodel(rule.LeftPart);
                foreach (var searchResult in submetamodelSearchResults)
                    modelTransformator.ExecuteTransformation(searchResult, rule);
            }

            return newModel;
        }        

        private List<TransformationRule> InitializeTransformationRuleSequence(Model targetModel)
        {
            var transformationRules = new List<TransformationRule>();
            Transformations.Add(targetModel, transformationRules);

            return transformationRules;
        }
    }
}

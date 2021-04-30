using DSM_Graph_Layer;
using DSM_Graph_Layer.HPGraphModel;
using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using DSM_Graph_Layer.Enums;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using DSM_Graph_Layer.HPGraphModel.ModelClasses.SubmodelMatching;

namespace Graph_Model_Tests
{
    class MetamodelingTests
    {
        Model Metamodel;
        [SetUp]        
        public void Setup()
        {
            Metamodel = new Model("Entity-Relation Diagram");

            var sourceRole = new Role("Источник связи");
            var targetRole = new Role(sourceRole, "Приемник связи");
            Metamodel.AddNewRolePairToGraph(targetRole);
            var attributeOwnerRole = new Role("Владелец атрибута");
            var attributeServantRole = new Role(attributeOwnerRole, "Атрибут");
            Metamodel.AddNewRolePairToGraph(attributeServantRole);

            var entity = new EntityVertex("Сущность");
            var entityLinksPort = new EntityPort("Связи", new[] { targetRole, sourceRole });
            var entityAttrPort = new EntityPort("Атрибуты", new[] { attributeOwnerRole });
            entity.AddPortToEntity(entityLinksPort);
            entity.AddPortToEntity(entityAttrPort);
            entity.Attributes.Add(new ElementAttribute("string", "Имя"));
            entity.Attributes.Add(new ElementAttribute("string", "Описание"));
            Metamodel.AddNewEntityVertex(entity);

            var link = new EntityVertex("Связь");
            var linkEntityLinksPort = new EntityPort("Связи с сущностями", new[] { sourceRole });
            var linkAttributesPort = new EntityPort("Атрибуты", new[] { attributeOwnerRole });
            link.AddPortToEntity(linkEntityLinksPort);
            link.AddPortToEntity(linkAttributesPort);
            link.Attributes.Add(new ElementAttribute("string", "Имя"));
            link.Attributes.Add(new ElementAttribute("string", "Описание"));
            Metamodel.AddNewEntityVertex(link);

            var attr = new EntityVertex("Атрибут");
            var attrLinksPort = new EntityPort("Связи атрибутов с элементами", new[] { attributeServantRole });
            attr.AddPortToEntity(attrLinksPort);
            attr.Attributes.Add(new ElementAttribute("string", "Имя"));
            attr.Attributes.Add(new ElementAttribute("string", "Тип"));
            attr.Attributes.Add(new ElementAttribute("string", "Описание"));
            Metamodel.AddNewEntityVertex(attr);

            var entityLink = Metamodel.AddHyperedgeWithRelation(entityLinksPort, linkEntityLinksPort, targetRole);
            entityLink.SetLabel("Сущность_Связь");

            var entityAttr = Metamodel.AddHyperedgeWithRelation(entityAttrPort,attrLinksPort, attributeOwnerRole);
            entityAttr.SetLabel("Принадлежит");

            var linkAttr = Metamodel.AddHyperedgeWithRelation(linkAttributesPort, attrLinksPort, attributeOwnerRole);
            linkAttr.SetLabel("Принадлежит");

            var entityEntity = Metamodel.AddHyperedgeWithRelation(entityLinksPort, entityLinksPort, sourceRole);
            entityEntity.SetLabel("Суперсущность_Подсущность");
        }
        #region Initialization
        [Test]
        public void ModelInitializationTest()
        {
            var startInstancesCount = Metamodel.Instances.Count;
            var dataValue = "Предметная область";

            Metamodel.Attributes.Add(new ElementAttribute("string", dataValue));
            var model = Metamodel.Instantiate("Экзамен");

            Assert.AreEqual(startInstancesCount + 1, Metamodel.Instances.Count);
            Assert.IsTrue(model.BaseElement == Metamodel);
            Assert.IsTrue(model.Attributes.Any(x => x.DataType == dataValue));
        }

        [Test]
        public void EntityInitializationTest()
        {
            var entities = Metamodel.Entities;
            List<EntityVertex> instances = CreateAllEntityInstances();

            Assert.IsTrue(instances.Count == entities.Count);
            for (int j = 0; j < instances.Count; j++)
            {
                Assert.IsTrue(instances[j].BaseElement == entities[j]);
                Assert.IsTrue(instances[j].Attributes.Any(x => x.DataType == "StringVal") && instances[j].Attributes.Any(x => x.DataType == "IntVal"));
                Assert.IsTrue(instances[j].Ports.Count == entities[j].Ports.Count);
                for (int k = 0; k < instances[j].Ports.Count; k++)
                {
                    Assert.IsTrue(instances[j].Ports[k].BaseElement == entities[j].Ports[k]);
                    Assert.IsTrue(instances[j].Ports[k].AcceptedRoles.SequenceEqual(entities[j].Ports[k].AcceptedRoles));
                }
            }
        }

        [Test]
        public void HyperedgeInitializationTest()
        {
            List<HyperedgeVertex> instances = CreateAllHyperedgeInstances();

            Assert.IsTrue(instances.Count == Metamodel.Hyperedges.Count);
            for (int j = 0; j < instances.Count; j++)
            {
                Assert.IsTrue(instances[j].BaseElement == Metamodel.Hyperedges[j]);
                Assert.IsTrue(instances[j].Attributes.Any(x => x.DataType == "StringVal") && instances[j].Attributes.Any(x => x.DataType == "IntVal"));
                Assert.IsTrue(instances[j].Relations.Count == Metamodel.Hyperedges[j].Relations.Count);
                for (int k = 0; k < instances[j].Relations.Count; k++)
                {
                    Assert.IsTrue(instances[j].Relations[k].BaseElement == Metamodel.Hyperedges[j].Relations[k]);
                    Assert.IsTrue(instances[j].Relations[k].RelationRole == Metamodel.Hyperedges[j].Relations[k].RelationRole);
                }
            }
        }

        [Test]
        public void ConnectionsBetweenVerticiesInitializationTest()
        {
            Model model = Metamodel.Instantiate("Модель");

            var entInstances = CreateAllEntityInstances();            
            foreach (var instance in entInstances)
            {
                model.AddNewEntityVertex(instance);
            }

            var startHyperedgeVertexCount = model.Hyperedges.Count;
            var startHyperedgeConnectorCount = model.Edges.Count;

            var ent = entInstances.Where(x => x.BaseElement.Label == "Сущность").First();
            var attr = entInstances.Where(x => x.BaseElement.Label == "Атрибут").First();

            var entAttr = ent.Ports.Where(x => x.BaseElement.Label == "Атрибуты").First();
            var attrEntity = attr.Ports.Where(x => x.BaseElement.Label == "Связи атрибутов с элементами").First();
            var roleOwner = model.Roles.Where(x => x.Name == "Владелец атрибута").First();
            var roleServant = roleOwner.OppositeRole;

            model.AddHyperedgeWithRelation(entAttr, attrEntity, roleOwner);

            Assert.AreEqual(startHyperedgeVertexCount + 1, model.Hyperedges.Count);
            Assert.AreEqual(startHyperedgeConnectorCount + 1, model.Edges.Count);
            Assert.IsTrue(entAttr.Relations.Count == 1 && entAttr.EdgeOwners.Count == 1);
            Assert.IsTrue(attrEntity.Relations.Count == 1 && attrEntity.EdgeOwners.Count == 1);
            Assert.IsTrue(entAttr.EdgeOwners.First().Links.Count == 2);
        }
        #endregion

        #region Manipulations with ports
        [Test]
        public void AddPortToMetamodelEntityTest()
        {
            var instance = Metamodel.Entities.First().Instantiate("Тест");
            var instancePortsCount = instance.Ports.Count;

            var addedPort = new EntityPort("Добавленный порт", new[] { Metamodel.Roles.First() });
            Metamodel.Entities.First().AddPortToEntity(addedPort);

            Assert.AreEqual(instancePortsCount + 1, instance.Ports.Count);
            Assert.IsTrue(instance.Ports.Any(x => x.BaseElement == addedPort));
            Assert.IsTrue(Metamodel.Entities.First().Ports.Contains(addedPort));
        }

        [Test]
        public void RemovePortFromMetamodelEntityTest()
        {
            var model = Metamodel.Instantiate("Test model");
            var entity = Metamodel.Entities.First();
            var addedPort = new EntityPort("Добавленный порт", new[] { Metamodel.Roles.First() });
            entity.AddPortToEntity(addedPort);

            var instance = entity.Instantiate("Test Instance");
            model.AddNewEntityVertex(instance);
            var instancePortsCount = instance.Ports.Count;

            entity.RemovePortFromEntity(addedPort);

            Assert.AreEqual(instancePortsCount - 1, instance.Ports.Count);
            Assert.IsTrue(!instance.Ports.Any(x => x.BaseElement == addedPort));
            Assert.IsTrue(!Metamodel.Entities.Any(x => x.Ports.Contains(addedPort)));
        }

        [Test]
        public void RemovePortFromMetamodelEntityWithLinksTest()
        {
            var model = Metamodel.Instantiate("Test model");
            var entity = Metamodel.Entities.First();
            var removedPort = entity.Ports.First();
            var connectedEntity = removedPort.Relations.First().OppositeRelation.CorrespondingPort.EntityOwner;

            var entityInstance = entity.Instantiate("Test instance");
            var connectedToEntityInstance = connectedEntity.Instantiate("Test instance 1");
            model.AddNewEntityVertex(entityInstance);
            model.AddNewEntityVertex(connectedToEntityInstance);
            var hedgeInstance = model.AddHyperedgeWithRelation(entityInstance, connectedToEntityInstance, removedPort.Relations.First().RelationRole);

            entity.RemovePortFromEntity(removedPort);

            Assert.Zero(model.Edges.Count);
            Assert.Zero(model.Hyperedges.Count);
            Assert.IsTrue(!entity.Ports.Any(x => x.BaseElement == removedPort));
            Assert.IsTrue(!model.Hyperedges.Contains(hedgeInstance.BaseElement));
        }

        [Test]
        public void RemovePortFromOnePortMetamodelEntityWithoutLinksTest()
        {
            var model = Metamodel.Instantiate("Test model");
            var targetEntity = new EntityVertex("Test entity");
            targetEntity.AddPortToEntity(new EntityPort("Test port", new[] { Metamodel.Roles.First() }));
            Metamodel.AddNewEntityVertex(targetEntity);
            var removedPort = targetEntity.Ports.First();

            var instance = targetEntity.Instantiate("Test instance");
            model.AddNewEntityVertex(instance);
            var instancePortsCount = instance.Ports.Count;
            var instancesCount = model.Entities.Count;
            var metamodelVerticiesCount = Metamodel.Entities.Count;

            targetEntity.RemovePortFromEntity(removedPort);

            Assert.AreEqual(instancePortsCount - 1, instance.Ports.Count);
            Assert.IsTrue(!instance.Ports.Any(x => x.BaseElement == removedPort));
            Assert.AreEqual(instancesCount - 1, model.Entities.Count);
            Assert.AreEqual(metamodelVerticiesCount - 1, Metamodel.Entities.Count);
            Assert.IsTrue(!Metamodel.Entities.Contains(targetEntity));
        }
        #endregion

        #region Manipulations with relations
        [Test]
        public void AddRelationToMetamodelTest()
        {
            var instance = Metamodel.Hyperedges.First().Instantiate("Test");
            var instanceRelsCount = instance.Relations.Count;

            var addedRelation1 = new HyperedgeRelation(Metamodel.Roles.First(), "TestRel1");
            var addedRelation2 = new HyperedgeRelation(Metamodel.Roles.First().OppositeRole, "TestRel2");
            Metamodel.Hyperedges.First().AddRelationPairToHyperedge(addedRelation1, addedRelation2);

            Assert.AreEqual(instanceRelsCount + 2, instance.Relations.Count);
            Assert.IsTrue(instance.Relations.Any(x => x.BaseElement == addedRelation1) && instance.Relations.Any(x=>x.BaseElement== addedRelation2));
            Assert.IsTrue(instance.Relations.Where(x => x.BaseElement == addedRelation1).First().OppositeRelation == instance.Relations.Where(x => x.BaseElement == addedRelation2).First());
            Assert.IsTrue(Metamodel.Hyperedges.First().Relations.Contains(addedRelation1) && Metamodel.Hyperedges.First().Relations.Contains(addedRelation2));
        }

        [Test]
        public void RemoveUnlinkedRelationFromMetamodelTest()
        {
            var model = Metamodel.Instantiate("Model");
            var instance = Metamodel.Hyperedges.First().Instantiate("Test");
            model.AddNewHyperedgeVertex(instance);
            var instanceRelsCount = instance.Relations.Count;

            var addedRelation1 = new HyperedgeRelation(Metamodel.Roles.First(), "TestRel1");
            var addedRelation2 = new HyperedgeRelation(Metamodel.Roles.First().OppositeRole, "TestRel2");
            Metamodel.Hyperedges.First().AddRelationPairToHyperedge(addedRelation1, addedRelation2);

            Metamodel.Hyperedges.First().RemoveRelationFromHyperedge(addedRelation1);

            Assert.AreEqual(instanceRelsCount, instance.Relations.Count);
            Assert.IsTrue(!instance.Relations.Any(x => x.BaseElement == addedRelation1) && !instance.Relations.Any(x => x.BaseElement == addedRelation2));
            Assert.IsTrue(!Metamodel.Hyperedges.First().Relations.Contains(addedRelation1) && !Metamodel.Hyperedges.First().Relations.Contains(addedRelation2));
        }

        [Test]
        public void RemoveLinkedRelationFromMetamodelTest()
        {
            var addedRelation1 = new HyperedgeRelation(Metamodel.Roles.First(), "TestRel1");
            var addedRelation2 = new HyperedgeRelation(Metamodel.Roles.First().OppositeRole, "TestRel2");
            Metamodel.Hyperedges.First().AddRelationPairToHyperedge(addedRelation1, addedRelation2);

            var model = Metamodel.Instantiate("Model");
            var removedRel = Metamodel.Hyperedges.First().Relations.First();

            var connectedEntity1 = removedRel.CorrespondingPort.EntityOwner;
            var connectedEntity2 = removedRel.OppositeRelation.CorrespondingPort.EntityOwner;

            var hyperedgeInstance = Metamodel.Hyperedges.First().Instantiate("Test instance");
            var connectedEntityInstance1 = connectedEntity1.Instantiate("Entity instance 1");
            var connectedEntityInstance2 = connectedEntity2.Instantiate("Entity instance 2");

            model.AddNewEntityVertex(connectedEntityInstance1);
            model.AddNewEntityVertex(connectedEntityInstance2);
            model.AddNewHyperedgeVertex(hyperedgeInstance);
            model.AddRelationToHyperedge(hyperedgeInstance, connectedEntity1, connectedEntity2, removedRel.RelationRole);
            var instanceRelsCount = hyperedgeInstance.Relations.Count;

            Metamodel.Hyperedges.First().RemoveRelationFromHyperedge(removedRel);

            Assert.AreEqual(instanceRelsCount - 2, hyperedgeInstance.Relations.Count);
            Assert.IsTrue(!hyperedgeInstance.Relations.Any(x => x.BaseElement == removedRel) && !hyperedgeInstance.Relations.Any(x => x.BaseElement == removedRel.OppositeRelation));
            Assert.IsTrue(!Metamodel.Hyperedges.First().Relations.Contains(removedRel) && !Metamodel.Hyperedges.First().Relations.Contains(removedRel.OppositeRelation));
            Assert.IsTrue(Metamodel.Hyperedges.Contains(hyperedgeInstance.BaseElement));
            Assert.IsTrue(model.Hyperedges.Contains(hyperedgeInstance));
        }

        [Test]
        public void RemoveLinkedRelationFromSinglePairMetamodelHyperedgeTest()
        {
            var model = Metamodel.Instantiate("Test model");
            var hyperedge = Metamodel.Hyperedges.First();
            var removedRel = hyperedge.Relations.First();
            var connectedEntity1 = removedRel.CorrespondingPort.EntityOwner;
            var connectedEntity2 = removedRel.OppositeRelation.CorrespondingPort.EntityOwner;

            var hyperedgeInstance = hyperedge.Instantiate("Test instance");
            var connectedEntityInstance1 = connectedEntity1.Instantiate("Entity instance 1");
            var connectedEntityInstance2 = connectedEntity2.Instantiate("Entity instance 2");

            model.AddNewEntityVertex(connectedEntityInstance1);
            model.AddNewEntityVertex(connectedEntityInstance2);
            model.AddNewHyperedgeVertex(hyperedgeInstance);
            model.AddRelationToHyperedge(hyperedgeInstance, connectedEntity1, connectedEntity2, removedRel.RelationRole);
            var hyperedgeVertexCountInModel = model.Hyperedges.Count;
            var hyperedgeConnectorsCountInModel = model.Edges.Count;

            hyperedge.RemoveRelationFromHyperedge(removedRel);

            Assert.AreEqual(hyperedgeVertexCountInModel - 1, model.Hyperedges.Count);
            Assert.AreEqual(hyperedgeConnectorsCountInModel - 1, model.Edges.Count);
            Assert.IsTrue(!Metamodel.Hyperedges.Contains(hyperedge));
            Assert.IsTrue(!model.Hyperedges.Any(x => x.BaseElement == hyperedge));
        }
        #endregion

        #region Manipulations with entities
        [Test]
        public void MetamodelEntityDeletionTest()
        {
            var model = Metamodel.Instantiate("TestModel");
            var entityInst = Metamodel.Entities.Where(x => x.Label == "Сущность").First().Instantiate("TestEntityInstance");
            var attrInst = Metamodel.Entities.Where(x => x.Label == "Атрибут").First().Instantiate("TestAttrInstance");

            var sourceRole = Metamodel.Roles.Where(x => x.Name == "Источник связи").First();
            var attrRole = Metamodel.Roles.Where(x => x.Name == "Владелец атрибута").First();

            model.AddNewEntityVertex(entityInst);
            model.AddNewEntityVertex(attrInst);
            model.AddHyperedgeWithRelation(entityInst, entityInst, sourceRole);
            model.AddHyperedgeWithRelation(entityInst, attrInst, attrRole);
            var entityModelCount = model.Entities.Count;
            var entityMetamodelCount = Metamodel.Entities.Count;

            Metamodel.RemoveEntityVertex(entityInst.BaseElement);

            Assert.AreEqual(entityModelCount - 1, model.Entities.Count);
            Assert.AreEqual(entityMetamodelCount - 1, Metamodel.Entities.Count);
            Assert.Zero(model.Edges.Count);
            Assert.IsTrue(!model.Entities.Contains(entityInst));
            Assert.IsTrue(!Metamodel.Entities.Contains(entityInst.BaseElement));
        }

        [Test]
        public void ModelEntityDeletionTest()
        {
            var model = Metamodel.Instantiate("TestModel");
            var entityInst = Metamodel.Entities.Where(x => x.Label == "Сущность").First().Instantiate("TestEntityInstance");
            var attrInst = Metamodel.Entities.Where(x => x.Label == "Атрибут").First().Instantiate("TestAttrInstance");

            var sourceRole = Metamodel.Roles.Where(x => x.Name == "Источник связи").First();
            var attrRole = Metamodel.Roles.Where(x => x.Name == "Владелец атрибута").First();

            model.AddNewEntityVertex(entityInst);
            model.AddNewEntityVertex(attrInst);
            model.AddHyperedgeWithRelation(entityInst, entityInst, sourceRole);
            model.AddHyperedgeWithRelation(entityInst, attrInst, attrRole);
            var entityModelCount = model.Entities.Count;
            var entityMetamodelCount = Metamodel.Entities.Count;

            model.RemoveEntityVertex(entityInst);

            Assert.AreEqual(entityModelCount - 1, model.Entities.Count);
            Assert.AreEqual(entityMetamodelCount, Metamodel.Entities.Count);
            Assert.Zero(model.Edges.Count);
            Assert.IsTrue(!model.Entities.Contains(entityInst));
            Assert.Zero(Metamodel.Entities.Where(x => x.Label == "Сущность").First().Instances.Count);
        }
        #endregion

        #region Manipulations with hyperedges
        [Test]
        public void MetamodelHyperedgeDeletionTest()
        {
            var model = Metamodel.Instantiate("TestModel");
            var entityInst = Metamodel.Entities.Where(x => x.Label == "Сущность").First().Instantiate("TestEntityInstance");
            var attrInst = Metamodel.Entities.Where(x => x.Label == "Атрибут").First().Instantiate("TestAttrInstance");

            var sourceRole = Metamodel.Roles.Where(x => x.Name == "Источник связи").First();
            var attrRole = Metamodel.Roles.Where(x => x.Name == "Владелец атрибута").First();

            model.AddNewEntityVertex(entityInst);
            model.AddNewEntityVertex(attrInst);
            var hyperedgeInst = model.AddHyperedgeWithRelation(entityInst, attrInst, attrRole);
            var hyperedgeModelCount = model.Hyperedges.Count;
            var hyperedgeMetamodelCount = Metamodel.Hyperedges.Count;

            Metamodel.RemoveHyperedgeVertex(hyperedgeInst.BaseElement);

            Assert.AreEqual(hyperedgeModelCount - 1, model.Hyperedges.Count);
            Assert.AreEqual(hyperedgeMetamodelCount - 1, Metamodel.Hyperedges.Count);
            Assert.Zero(model.Edges.Count);
            Assert.Zero(entityInst.Ports.Sum(x => x.Relations.Count));
            Assert.IsTrue(!model.Hyperedges.Contains(hyperedgeInst));
            Assert.IsTrue(!Metamodel.Hyperedges.Contains(hyperedgeInst.BaseElement));
        }

        [Test]
        public void ModelHyperedgeDeletionTest()
        {
            var model = Metamodel.Instantiate("TestModel");
            var entityInst = Metamodel.Entities.Where(x => x.Label == "Сущность").First().Instantiate("TestEntityInstance");
            var attrInst = Metamodel.Entities.Where(x => x.Label == "Атрибут").First().Instantiate("TestAttrInstance");

            var sourceRole = Metamodel.Roles.Where(x => x.Name == "Источник связи").First();
            var attrRole = Metamodel.Roles.Where(x => x.Name == "Владелец атрибута").First();

            model.AddNewEntityVertex(entityInst);
            model.AddNewEntityVertex(attrInst);
            var hyperedgeInst = model.AddHyperedgeWithRelation(entityInst, attrInst, attrRole);
            var hyperedgeModelCount = model.Hyperedges.Count;
            var hyperedgeMetamodelCount = Metamodel.Hyperedges.Count;

            model.RemoveHyperedgeVertex(hyperedgeInst);

            Assert.AreEqual(hyperedgeModelCount - 1, model.Hyperedges.Count);
            Assert.AreEqual(hyperedgeMetamodelCount, Metamodel.Hyperedges.Count);
            Assert.Zero(model.Edges.Count);
            Assert.Zero(entityInst.Ports.Sum(x => x.Relations.Count));
            Assert.Zero(model.Hyperedges.Sum(x => x.Instances.Count));
            Assert.IsTrue(!model.Hyperedges.Contains(hyperedgeInst));
        }
        #endregion

        #region Submodel matching
        [Test]
        public void SingleEntitySubmodelMatchingTest()
        {
            var entity = new EntityVertex("Сущность");
            entity.AddPortToEntity(new EntityPort("Связи"));
            var model = new Model("Pattern");
            model.AddNewEntityVertex(entity);

            var answers = Metamodel.FindIsomorphicSubmodels(model);

            Assert.IsTrue(answers.Count == 1);
            Assert.IsTrue(answers.First().Entities.Count == 1);
            Assert.Zero(answers.First().Hyperedges.Count);
            Assert.IsTrue(answers.First().Entities.First()==Metamodel.Entities.First());
        }

        [Test]
        public void SingleHyperedgeSubmodelMatchingTest()
        {
            var sourceRole = new Role("Источник связи");
            var targetRole = new Role(sourceRole, "Приемник связи");

            var hyperedge = new HyperedgeVertex("Сущность_Связь");
            hyperedge.AddRelationPairToHyperedge(new HyperedgeRelation(sourceRole), new HyperedgeRelation(targetRole));
            var model = new Model("Pattern");
            model.AddNewHyperedgeVertex(hyperedge);

            var answers = Metamodel.FindIsomorphicSubmodels(model);

            Assert.IsTrue(answers.Count == 1);
            Assert.Zero(answers.First().Entities.Count);
            Assert.IsTrue(answers.First().Hyperedges.Count == 1);
            Assert.IsTrue(answers.First().Hyperedges.First() == Metamodel.Hyperedges.First());
        }

        [Test]
        public void MultipleEntitiesSubmodelMatchingTest()
        {
            var entity = new EntityVertex("Сущность");
            entity.AddPortToEntity(new EntityPort("Связи", new[] { new Role("Приемник связи") }));
            var link = new EntityVertex("Связь");
            link.AddPortToEntity(new EntityPort("Связи с сущностями", new[] { new Role("Источник связи") }));

            var model = new Model("Pattern");
            model.AddNewEntityVertex(entity);
            model.AddNewEntityVertex(link);

            var answers = Metamodel.FindIsomorphicSubmodels(model);

            Assert.IsTrue(answers.Count == 1);
            Assert.IsTrue(answers.First().Entities.Count == 2);
            Assert.Zero(answers.First().Hyperedges.Count);
            Assert.IsTrue(answers.First().Entities.Any(x => x.Label == entity.Label) && answers.First().Entities.Any(x => x.Label == link.Label));
        }

        [Test]
        public void MultipleHyperedgesSubmodelMatchingTest()
        {
            var sourceRole = new Role("Источник связи");
            var targetRole = new Role(sourceRole, "Приемник связи");
            var attributeOwnerRole = new Role("Владелец атрибута");
            var attributeServantRole = new Role(attributeOwnerRole, "Атрибут");

            var hyperedge1 = new HyperedgeVertex("Сущность_Связь");
            hyperedge1.AddRelationPairToHyperedge(new HyperedgeRelation(sourceRole), new HyperedgeRelation(targetRole));
            var hyperedge2 = new HyperedgeVertex("Принадлежит");
            hyperedge2.AddRelationPairToHyperedge(new HyperedgeRelation(attributeOwnerRole), new HyperedgeRelation(attributeServantRole));
            var model = new Model("Pattern");
            model.AddNewHyperedgeVertex(hyperedge1);
            model.AddNewHyperedgeVertex(hyperedge2);

            var answers = Metamodel.FindIsomorphicSubmodels(model);

            Assert.IsTrue(answers.Count == 2);
            Assert.Zero(answers.First().Entities.Count);
            Assert.Zero(answers.Last().Entities.Count);
            Assert.IsTrue(answers.First().Hyperedges.Count == 2);
            Assert.IsTrue(answers.Last().Hyperedges.Count == 2);
            Assert.IsTrue(answers.First().Hyperedges.Any(x => x.Label == hyperedge1.Label) && answers.First().Hyperedges.Any(x => x.Label == hyperedge2.Label));
            Assert.IsTrue(answers.Last().Hyperedges.Any(x => x.Label == hyperedge1.Label) && answers.Last().Hyperedges.Any(x => x.Label == hyperedge2.Label));
        }

        [Test]
        public void MultipleHyperedgeInclusionsMatchingTest()
        {
            var attributeOwnerRole = new Role("Владелец атрибута");
            var attributeServantRole = new Role(attributeOwnerRole, "Атрибут");

            var hyperedge = new HyperedgeVertex("Принадлежит");
            hyperedge.AddRelationPairToHyperedge(new HyperedgeRelation(attributeOwnerRole), new HyperedgeRelation(attributeServantRole));
            var model = new Model("Pattern");
            model.AddNewHyperedgeVertex(hyperedge);

            var answers = Metamodel.FindIsomorphicSubmodels(model);

            Assert.IsTrue(answers.Count == 2);
            Assert.IsTrue(answers.All(x=>x.Entities.Count==0));
            Assert.IsTrue(answers.All(x=>x.Hyperedges.Count == 1));
        }

        [Test]
        public void ConnectedEntityiesSubmodelMatchingTest()
        {
            var reciever = new Role("Приемник связи");
            var source = new Role("Источник связи");
            reciever.OppositeRole = source;

            var entity = new EntityVertex("Сущность");
            entity.AddPortToEntity(new EntityPort("Связи", new[] { reciever }));
            var link = new EntityVertex("Связь");
            link.AddPortToEntity(new EntityPort("Связи с сущностями", new[] { source }));

            var model = new Model("Pattern");
            model.AddNewEntityVertex(entity);
            model.AddNewEntityVertex(link);
            var entityLink = model.AddHyperedgeWithRelation(entity, link, reciever);
            entityLink.SetLabel("Сущность_Связь");

            var answers = Metamodel.FindIsomorphicSubmodels(model);

            Assert.IsTrue(answers.Count == 1);
            Assert.IsTrue(answers.First().Entities.Count == 2);
            Assert.IsTrue(answers.First().Hyperedges.Count == 1);
            Assert.IsTrue(answers.First().Edges.Count == 1);
            Assert.IsTrue(answers.First().Entities.Any(x => x.Label == entity.Label) && answers.First().Entities.Any(x => x.Label == link.Label));
            Assert.IsTrue(answers.First().Edges.First().Links.Count == 2);
        }

        [Test]
        public void FullIsomorphismMatchingTest()
        {
            var answers = Metamodel.FindIsomorphicSubmodels(Metamodel);

            Assert.IsTrue(answers.Count == 1);
            Assert.AreEqual(Metamodel.Entities.Count, answers.First().Entities.Count);
            Assert.AreEqual(Metamodel.Hyperedges.Count, answers.First().Hyperedges.Count);
            Assert.AreEqual(Metamodel.Edges.Count, answers.First().Edges.Count);
        }
        #endregion

        #region Submetamodel matching
        [Test]
        public void SingleEntitySubmetamodelMatchingTest()
        {
            var model = Metamodel.Instantiate("Test model");
            var entities = CreateAllEntityInstances();
            foreach(var item in entities)
            {
                model.AddNewEntityVertex(item);
            }

            var hedges = CreateAllHyperedgeInstances();
            foreach(var item in hedges)
            {
                model.AddNewHyperedgeVertex(item);
            }

            var submetamodel = new Model(new[] { Metamodel.Entities.First() }, null);

            var answers = model.FindAllInstancesOfPartialMetamodel(submetamodel);

            Assert.IsTrue(answers.Count == 1);
            Assert.IsTrue(answers.First().Entities.Count == 1);
            Assert.Zero(answers.First().Hyperedges.Count);
            Assert.IsTrue(answers.First().Entities.First().BaseElement == Metamodel.Entities.First());
        }

        [Test]
        public void SingleHyperedgeSubmetamodelMatchingTest()
        {
            var model = Metamodel.Instantiate("Test model");
            var entities = CreateAllEntityInstances();
            foreach (var item in entities)
            {
                model.AddNewEntityVertex(item);
            }

            var hedges = CreateAllHyperedgeInstances();
            foreach (var item in hedges)
            {
                model.AddNewHyperedgeVertex(item);
            }

            var submetamodel = new Model(null, new[] { Metamodel.Hyperedges.First() });

            var answers = model.FindAllInstancesOfPartialMetamodel(submetamodel);

            Assert.IsTrue(answers.Count == 1);
            Assert.IsTrue(answers.First().Hyperedges.Count == 1);
            Assert.Zero(answers.First().Entities.Count);
            Assert.IsTrue(answers.First().Hyperedges.First().BaseElement == Metamodel.Hyperedges.First());
        }

        [Test]
        public void MultipleEntitiesSubmetamodelMatchingTest()
        {
            var model = Metamodel.Instantiate("Test model");
            var entities = CreateAllEntityInstances();
            foreach (var item in entities)
            {
                model.AddNewEntityVertex(item);
            }

            var hedges = CreateAllHyperedgeInstances();
            foreach (var item in hedges)
            {
                model.AddNewHyperedgeVertex(item);
            }

            var submetamodel = new Model(new[] { Metamodel.Entities.First(), Metamodel.Entities.Last() }, null);

            var answers = model.FindAllInstancesOfPartialMetamodel(submetamodel);

            Assert.IsTrue(answers.Count == 1);
            Assert.IsTrue(answers.First().Entities.Count == 2);
            Assert.Zero(answers.First().Hyperedges.Count);
            Assert.IsTrue(answers.First().Entities.First().BaseElement == Metamodel.Entities.First() && answers.First().Entities.Last().BaseElement == Metamodel.Entities.Last());
        }

        [Test]
        public void MultipleHyperedgesSubmetamodelMatchingTest()
        {
            var model = Metamodel.Instantiate("Test model");
            var entities = CreateAllEntityInstances();
            foreach (var item in entities)
            {
                model.AddNewEntityVertex(item);
            }

            var hedges = CreateAllHyperedgeInstances();
            foreach (var item in hedges)
            {
                model.AddNewHyperedgeVertex(item);
            }

            var submetamodel = new Model(null, new[] { Metamodel.Hyperedges.First(), Metamodel.Hyperedges.Last() });

            var answers = model.FindAllInstancesOfPartialMetamodel(submetamodel);

            Assert.IsTrue(answers.Count == 1);
            Assert.IsTrue(answers.First().Hyperedges.Count == 2);
            Assert.Zero(answers.First().Entities.Count);
            Assert.IsTrue(answers.First().Hyperedges.First().BaseElement == Metamodel.Hyperedges.First() && answers.First().Hyperedges.Last().BaseElement == Metamodel.Hyperedges.Last());
        }

        [Test]
        public void MultipleInstancesMatchingTest()
        {
            var model = Metamodel.Instantiate("TestModel");
            var entityInst = Metamodel.Entities.Where(x => x.Label == "Сущность").First().Instantiate("TestEntityInstance");
            var attrInst1 = Metamodel.Entities.Where(x => x.Label == "Атрибут").First().Instantiate("TestAttrInstance 1");
            var attrInst2 = Metamodel.Entities.Where(x => x.Label == "Атрибут").First().Instantiate("TestAttrInstance 2");
            var attrRole = Metamodel.Roles.Where(x => x.Name == "Владелец атрибута").First();

            model.AddNewEntityVertex(entityInst);
            model.AddNewEntityVertex(attrInst1);
            model.AddNewEntityVertex(attrInst2);
            var hyperedgeInst1 = model.AddHyperedgeWithRelation(entityInst, attrInst1, attrRole);
            var hyperedgeInst2 = model.AddHyperedgeWithRelation(entityInst, attrInst2, attrRole);
            var hyperedgeModelCount = model.Hyperedges.Count;
            var hyperedgeMetamodelCount = Metamodel.Hyperedges.Count;

            var submetamodel = new Model(new[] { entityInst.BaseElement, attrInst1.BaseElement }, new[] { hyperedgeInst1.BaseElement}, new[] { hyperedgeInst1.BaseElement.CorrespondingHyperedge });
            
            var answers = model.FindAllInstancesOfPartialMetamodel(submetamodel);

            Assert.IsTrue(answers.Count == 2);
            Assert.IsTrue(answers.All(x => x.Edges.Count == 1 && x.Entities.Count == 2 && x.Hyperedges.Count == 1));
            Assert.IsTrue(answers.All(x => x.Entities.Any(y => y.Label == "TestEntityInstance") && x.Entities.Any(y => y.Label.StartsWith("TestAttrInstance"))));
        }

        [Test]
        public void MatchingNonExistentElementsTest()
        {
            var model = Metamodel.Instantiate("TestModel");

            var entityInst = Metamodel.Entities.Where(x => x.Label == "Сущность").First().Instantiate("TestEntityInstance");
            var attrInst1 = Metamodel.Entities.Where(x => x.Label == "Атрибут").First().Instantiate("TestAttrInstance 1");
            var attrInst2 = Metamodel.Entities.Where(x => x.Label == "Атрибут").First().Instantiate("TestAttrInstance 2");
            var attrRole = Metamodel.Roles.Where(x => x.Name == "Владелец атрибута").First();

            model.AddNewEntityVertex(entityInst);
            model.AddNewEntityVertex(attrInst1);
            model.AddNewEntityVertex(attrInst2);
            var hyperedgeInst1 = model.AddHyperedgeWithRelation(entityInst, attrInst1, attrRole);
            var hyperedgeInst2 = model.AddHyperedgeWithRelation(entityInst, attrInst2, attrRole);

            var relEntity = Metamodel.Entities.Where(x => x.Label == "Связь").First();
            var attrEntity = Metamodel.Entities.Where(x => x.Label == "Атрибут").First();
            var hyperedge = Metamodel.Hyperedges.Where(x => x.Label == "Принадлежит" && x.Relations.Any(y => y.CorrespondingPort.VertexOwner == relEntity)).First();

            var submetamodel = new Model(new[] { relEntity, attrEntity }, new[] { hyperedge }, new[] { hyperedge.CorrespondingHyperedge });

            var answers = model.FindAllInstancesOfPartialMetamodel(submetamodel);

            Assert.Zero(answers.Count);
        }
        #endregion

        private List<HyperedgeVertex> CreateAllHyperedgeInstances()
        {
            var instances = new List<HyperedgeVertex>();
            int i = 0;
            foreach (var hyperedge in Metamodel.Hyperedges)
            {
                hyperedge.Attributes.Add(new ElementAttribute("string", "StringVal"));
                hyperedge.Attributes.Add(new ElementAttribute("int", "IntVal"));
                var instance = hyperedge.Instantiate($"Test {i++}");
                instances.Add(instance);
            }

            return instances;
        }

        private List<EntityVertex> CreateAllEntityInstances()
        {
            var instances = new List<EntityVertex>();

            int i = 0;
            foreach (var entity in Metamodel.Entities)
            {
                entity.Attributes.Add(new ElementAttribute("string", "StringVal"));
                entity.Attributes.Add(new ElementAttribute("int", "IntVal"));
                var instance = entity.Instantiate($"Test {i++}");
                instances.Add(instance);
            }

            return instances;
        }
    }
}

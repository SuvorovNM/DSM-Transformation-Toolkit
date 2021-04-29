﻿using DSM_Graph_Layer;
using DSM_Graph_Layer.HPGraphModel;
using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using DSM_Graph_Layer.Enums;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;

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
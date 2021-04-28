using DSM_Graph_Layer;
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

            var entityLink = Metamodel.AddNewRelation((entityLinksPort, targetRole), (linkEntityLinksPort, sourceRole));
            entityLink.SetLabel("Сущность_Связь");

            var entityAttr = Metamodel.AddNewRelation((entityAttrPort, attributeOwnerRole), (attrLinksPort, attributeServantRole));
            entityAttr.SetLabel("Принадлежит");

            var linkAttr = Metamodel.AddNewRelation((linkAttributesPort, attributeOwnerRole), (attrLinksPort, attributeServantRole));
            entityAttr.SetLabel("Принадлежит");

            var entityEntity = Metamodel.AddNewRelation((entityLinksPort, targetRole), (entityLinksPort, sourceRole));
            entityAttr.SetLabel("Суперсущность_Подсущность");
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
            var instances = new List<EntityVertex>();

            int i = 0;
            foreach(var entity in entities)
            {
                entity.Attributes.Add(new ElementAttribute("string", "StringVal"));
                entity.Attributes.Add(new ElementAttribute("int", "45"));
                var instance = entity.Instantiate($"Test {i}");
                instances.Add(instance);
            }

            Assert.IsTrue(instances.Count == entities.Count);
            for (int j=0; j<instances.Count; j++)
            {
                Assert.IsTrue(instances[j].BaseElement == entities[j]);
                Assert.IsTrue(instances[j].Attributes.Any(x => x.DataType == "StringVal") && instances[j].Attributes.Any(x => x.DataType == "45"));
                Assert.IsTrue(instances[j].Ports.Count == entities[j].Ports.Count);
                for (int k = 0; k< instances[j].Ports.Count; k++)
                {
                    Assert.IsTrue(instances[j].Ports[k].BaseElement == entities[j].Ports[k]);
                    Assert.IsTrue(instances[j].Ports[k].AcceptedRoles.SequenceEqual(entities[j].Ports[k].AcceptedRoles));
                }
            }
        }
    }
}

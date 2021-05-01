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
    class TransformationTests
    {
        //Model MetamodelSource;
        //Model MetamodelTarget;
        [SetUp]
        public void Setup()
        {
            //MetamodelSource = CreateEntityRelationMetamodel();
        }

        [Test]
        public void TransformationRuleCreationTest()
        {
            var sourceModel = CreateEntityRelationMetamodel();
            var targetModel = CreateClassDiagramMetamodel();
            var pattern = GetSourceAttributeSubmodel(sourceModel);
            var rightPart = GetClassAssociationSubmodel(targetModel);

            var transformationRule = new TransformationRule(pattern, rightPart, "TestRule");
            sourceModel.AddTransformationRule(targetModel, transformationRule);

            Assert.IsTrue(sourceModel.Transformations.Count == 1);
            Assert.IsTrue(sourceModel.Transformations[targetModel].Count == 1);
            Assert.IsTrue(sourceModel.Transformations[targetModel].First() == transformationRule);
        }

        [Test]
        public void SingeTransformationExecutionWithSingleSubmodelInclusionTest()
        {
            var sourceModel = CreateEntityRelationMetamodel();
            var targetModel = CreateClassDiagramMetamodel();
            var pattern = GetSourceAttributeSubmodel(sourceModel);
            var rightPart = GetClassAssociationSubmodel(targetModel);

            var transformationRule = new TransformationRule(pattern, rightPart, "TestRule");
            sourceModel.AddTransformationRule(targetModel, transformationRule);

            var model = sourceModel.Instantiate("ER Diagram Instance");
            var teacher = sourceModel.Entities.First(x => x.Label == "Сущность").Instantiate("Учитель");
            var fio = sourceModel.Entities.First(x => x.Label == "Атрибут").Instantiate("ФИО");
            model.AddNewEntityVertex(teacher);
            model.AddNewEntityVertex(fio);
            model.AddHyperedgeWithRelation(teacher, fio, sourceModel.Roles.First(x => x.Name == "Владелец атрибута"));

            var resultModel = model.ExecuteTransformations(targetModel);

            Assert.IsTrue(resultModel.Entities.Count == 1);
            Assert.IsTrue(resultModel.Hyperedges.Count == 1);
            Assert.IsTrue(resultModel.HyperedgeConnectors.Count == 1);
        }

        [Test]
        public void SingeTransformationExecutionWithMultipleSubmodelInclusionTest()
        {
            var sourceModel = CreateEntityRelationMetamodel();
            var targetModel = CreateClassDiagramMetamodel();
            var pattern = GetSourceAttributeSubmodel(sourceModel);
            var rightPart = GetClassAssociationSubmodel(targetModel);

            var transformationRule = new TransformationRule(pattern, rightPart, "TestRule");
            sourceModel.AddTransformationRule(targetModel, transformationRule);

            var model = sourceModel.Instantiate("ER Diagram Instance");
            var teacher = sourceModel.Entities.First(x => x.Label == "Сущность").Instantiate("Учитель");
            var fio = sourceModel.Entities.First(x => x.Label == "Атрибут").Instantiate("ФИО");
            model.AddNewEntityVertex(teacher);
            model.AddNewEntityVertex(fio);
            model.AddHyperedgeWithRelation(teacher, fio, sourceModel.Roles.First(x => x.Name == "Владелец атрибута"));

            var student = sourceModel.Entities.First(x => x.Label == "Сущность").Instantiate("Ученик");
            var stage = sourceModel.Entities.First(x => x.Label == "Атрибут").Instantiate("Класс");
            model.AddNewEntityVertex(student);
            model.AddNewEntityVertex(stage);
            model.AddHyperedgeWithRelation(student, stage, sourceModel.Roles.First(x => x.Name == "Владелец атрибута"));

            var resultModel = model.ExecuteTransformations(targetModel);

            Assert.IsTrue(resultModel.Entities.Count == 2);
            Assert.IsTrue(resultModel.Hyperedges.Count == 2);
            Assert.IsTrue(resultModel.HyperedgeConnectors.Count == 2);
        }

        [Test]
        public void MultipleTransformationExecutionWithMultipleSubmodelInclusionTest()
        {
            var sourceModel = CreateEntityRelationMetamodel();
            var targetModel = CreateClassDiagramMetamodel();

            var pattern1 = GetSourceAttributeSubmodel(sourceModel);
            var rightPart1 = GetClassSubmodel(targetModel);
            var pattern2 = GetLinkSubmodel(sourceModel);
            var rightPart2 = GetAssociationSubmodel(targetModel);

            var transformationRule1 = new TransformationRule(pattern1, rightPart1, "TestRule 1");
            var transformationRule2 = new TransformationRule(pattern2, rightPart2, "TestRule 2");
            sourceModel.AddTransformationRule(targetModel, transformationRule1);
            sourceModel.AddTransformationRule(targetModel, transformationRule2);

            var model = sourceModel.Instantiate("ER Diagram Instance");
            var teacher = sourceModel.Entities.First(x => x.Label == "Сущность").Instantiate("Учитель");
            var fio = sourceModel.Entities.First(x => x.Label == "Атрибут").Instantiate("ФИО");
            model.AddNewEntityVertex(teacher);
            model.AddNewEntityVertex(fio);
            model.AddHyperedgeWithRelation(teacher, fio, sourceModel.Roles.First(x => x.Name == "Владелец атрибута"));

            var student = sourceModel.Entities.First(x => x.Label == "Сущность").Instantiate("Ученик");
            var stage = sourceModel.Entities.First(x => x.Label == "Атрибут").Instantiate("Класс");
            model.AddNewEntityVertex(student);
            model.AddNewEntityVertex(stage);
            model.AddHyperedgeWithRelation(student, stage, sourceModel.Roles.First(x => x.Name == "Владелец атрибута"));

            var educating = sourceModel.Entities.First(x => x.Label == "Связь").Instantiate("Обучает");
            model.AddNewEntityVertex(educating);
            model.AddHyperedgeWithRelation(teacher, educating, sourceModel.Roles.First(x => x.Name == "Приемник связи"));
            model.AddHyperedgeWithRelation(student, educating, sourceModel.Roles.First(x => x.Name == "Приемник связи"));

            var resultModel = model.ExecuteTransformations(targetModel);
            var check = resultModel.Entities.First().ConnectedVertices;

            Assert.IsTrue(resultModel.Entities.Count == 2);
            Assert.IsTrue(resultModel.Hyperedges.Count == 1);
            Assert.IsTrue(resultModel.HyperedgeConnectors.Count == 1);
            Assert.IsTrue(resultModel.Entities.First().ConnectedVertices.First() == resultModel.Entities.Last());
        }

        private static ModelForTransformation GetClassAssociationSubmodel(Model targetModel)
        {
            var targetClass = targetModel.Entities.First(x => x.Label == "Класс");
            var targetHyperedge = targetModel.Hyperedges.First(x => x.Label == "Ассоциация");
            var targetHyperedgeConnector = targetHyperedge.CorrespondingHyperedge;
            var rightPart = new ModelForTransformation(new[] { targetClass }, null, new[] { targetHyperedge }, new[] { targetHyperedgeConnector });
            return rightPart;
        }

        private static ModelForTransformation GetClassSubmodel(Model targetModel)
        {
            var targetClass = targetModel.Entities.First(x => x.Label == "Класс");
            var rightPart = new ModelForTransformation(new[] { targetClass }, null, null);
            return rightPart;
        }

        private static ModelForTransformation GetAssociationSubmodel(Model targetModel)
        {
            var targetClass = targetModel.Entities.First(x => x.Label == "Класс");

            var targetHyperedge = targetModel.Hyperedges.First(x => x.Label == "Ассоциация");
            var targetHyperedgeConnector = targetHyperedge.CorrespondingHyperedge;
            var rightPart = new ModelForTransformation(new[] { targetClass }, new[] { targetClass }, new[] { targetHyperedge }, new[] { targetHyperedgeConnector });
            return rightPart;
        }

        private static ModelForTransformation GetSourceAttributeSubmodel(Model sourceModel)
        {
            var sourceEntity = sourceModel.Entities.First(x => x.Label == "Сущность");
            var sourceAttr = sourceModel.Entities.First(x => x.Label == "Атрибут");
            var sourceHyperedge = sourceModel.Hyperedges.First(x => x.Label == "Принадлежит" && x.Relations.Any(x => x.CorrespondingPort.EntityOwner == sourceEntity));
            var sourceHyperedgeConnector = sourceHyperedge.CorrespondingHyperedge;
            var pattern = new ModelForTransformation(new[] { sourceEntity, sourceAttr },null, new[] { sourceHyperedge }, new[] { sourceHyperedgeConnector });
            return pattern;
        }

        private static ModelForTransformation GetLinkSubmodel(Model sourceModel)
        {
            var sourceEntity = sourceModel.Entities.First(x => x.Label == "Сущность");

            var linkEntity = sourceModel.Entities.First(x => x.Label == "Связь");
            /*var sourceHyperedge = sourceModel.Hyperedges.First(x => x.Label == "Сущность_Связь");
            var sourceHyperedgeConnector = sourceHyperedge.CorrespondingHyperedge;*/

            var pattern = new ModelForTransformation(new[] { linkEntity }, null, null);//new[] { sourceHyperedge}, new[] { sourceHyperedgeConnector}
            return pattern;
        }

        private Model CreateEntityRelationMetamodel()
        {
            var model = new Model("Entity-Relation Diagram");

            var sourceRole = new Role("Источник связи");
            var targetRole = new Role(sourceRole, "Приемник связи");
            model.AddNewRolePairToGraph(targetRole);
            var attributeOwnerRole = new Role("Владелец атрибута");
            var attributeServantRole = new Role(attributeOwnerRole, "Атрибут");
            model.AddNewRolePairToGraph(attributeServantRole);

            var entity = new EntityVertex("Сущность");
            var entityLinksPort = new EntityPort("Связи", new[] { targetRole, sourceRole });
            var entityAttrPort = new EntityPort("Атрибуты", new[] { attributeOwnerRole });
            entity.AddPortToEntity(entityLinksPort);
            entity.AddPortToEntity(entityAttrPort);
            entity.Attributes.Add(new ElementAttribute("string", "Имя"));
            entity.Attributes.Add(new ElementAttribute("string", "Описание"));
            model.AddNewEntityVertex(entity);

            var link = new EntityVertex("Связь");
            var linkEntityLinksPort = new EntityPort("Связи с сущностями", new[] { sourceRole });
            var linkAttributesPort = new EntityPort("Атрибуты", new[] { attributeOwnerRole });
            link.AddPortToEntity(linkEntityLinksPort);
            link.AddPortToEntity(linkAttributesPort);
            link.Attributes.Add(new ElementAttribute("string", "Имя"));
            link.Attributes.Add(new ElementAttribute("string", "Описание"));
            model.AddNewEntityVertex(link);

            var attr = new EntityVertex("Атрибут");
            var attrLinksPort = new EntityPort("Связи атрибутов с элементами", new[] { attributeServantRole });
            attr.AddPortToEntity(attrLinksPort);
            attr.Attributes.Add(new ElementAttribute("string", "Имя"));
            attr.Attributes.Add(new ElementAttribute("string", "Тип"));
            attr.Attributes.Add(new ElementAttribute("string", "Описание"));
            model.AddNewEntityVertex(attr);

            var entityLink = model.AddHyperedgeWithRelation(entityLinksPort, linkEntityLinksPort, targetRole);
            entityLink.SetLabel("Сущность_Связь");

            var entityAttr = model.AddHyperedgeWithRelation(entityAttrPort, attrLinksPort, attributeOwnerRole);
            entityAttr.SetLabel("Принадлежит");

            var linkAttr = model.AddHyperedgeWithRelation(linkAttributesPort, attrLinksPort, attributeOwnerRole);
            linkAttr.SetLabel("Принадлежит");

            var entityEntity = model.AddHyperedgeWithRelation(entityLinksPort, entityLinksPort, sourceRole);
            entityEntity.SetLabel("Суперсущность_Подсущность");

            return model;
        }

        private Model CreateClassDiagramMetamodel()
        {
            var model = new Model("Connected Entities Diagram");
            var associationRole = new Role("Ассоциация");
            model.AddNewRolePairToGraph(associationRole);

            var entity = new EntityVertex("Класс");
            var entityLinksPort = new EntityPort("Связи", new[] { associationRole });
            entity.AddPortToEntity(entityLinksPort);
            entity.Attributes.Add(new ElementAttribute("string", "Имя"));
            entity.Attributes.Add(new ElementAttribute("string", "Описание"));
            entity.Attributes.Add(new ElementAttribute("string", "Свойства"));
            entity.Attributes.Add(new ElementAttribute("string", "Методы"));
            model.AddNewEntityVertex(entity);

            var entityLink = model.AddHyperedgeWithRelation(entityLinksPort, entityLinksPort, associationRole);
            entityLink.SetLabel("Ассоциация");

            return model;
        }
    }
}

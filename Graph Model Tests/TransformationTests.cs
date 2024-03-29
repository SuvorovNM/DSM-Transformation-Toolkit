﻿using DSM_Graph_Layer;
using DSM_Graph_Layer.HPGraphModel;
using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using DSM_Graph_Layer.Enums;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using DSM_Graph_Layer.HPGraphModel.ModelClasses.SubmodelMatching;
using Graph_Model_Tests.Metamodels;
using DSM_Graph_Layer.HPGraphModel.ModelClasses.Transformations;

namespace Graph_Model_Tests
{
    class TransformationTests
    {
        EntityRelationDiagram erDiagram;
        ClassDiagram classDiagram;
        HyperedgeBasedBusDiagram hbBusDiagram;
        EntityBasedBusDiagram ebBusDiagram;
        [SetUp]
        public void Setup()
        {
            erDiagram = new EntityRelationDiagram();
            classDiagram = new ClassDiagram();
            hbBusDiagram = new HyperedgeBasedBusDiagram();
            ebBusDiagram = new EntityBasedBusDiagram();
        }

        [Test]
        public void TransformationRuleCreationTest()
        {
            var sourceModel = erDiagram.Metamodel;
            var targetModel = classDiagram.Metamodel;
            var pattern = erDiagram.GetEntitySubmodel();
            var rightPart = classDiagram.GetClassAssociationSubmodel();

            var transformationRule = new TransformationRule(pattern, rightPart, "TestRule");
            sourceModel.AddTransformationRule(targetModel, transformationRule);

            Assert.IsTrue(sourceModel.Transformations.Count == 1);
            Assert.IsTrue(sourceModel.Transformations[targetModel].Count == 1);
            Assert.IsTrue(sourceModel.Transformations[targetModel].First() == transformationRule);
        }

        [Test]
        public void SingeTransformationExecutionWithSingleSubmodelInclusionTest()
        {
            var sourceModel = erDiagram.Metamodel;
            var targetModel = classDiagram.Metamodel;
            var pattern = erDiagram.GetEntitySubmodel();
            var rightPart = classDiagram.GetClassAssociationSubmodel();

            var transformationRule = new TransformationRule(pattern, rightPart, "TestRule");
            sourceModel.AddTransformationRule(targetModel, transformationRule);

            var model = sourceModel.Instantiate("ER Diagram Instance");
            var teacher = sourceModel.Entities.First(x => x.Label == "Сущность").Instantiate("Учитель");
            var fio = sourceModel.Entities.First(x => x.Label == "Атрибут").Instantiate("ФИО");
            model.AddNewEntityVertex(teacher);
            model.AddNewEntityVertex(fio);
            model.AddHyperedgeWithRelation(teacher, fio, sourceModel.Roles.First(x => x.Label == "Владелец атрибута"));

            var resultModel = model.ExecuteTransformations(targetModel);

            Assert.IsTrue(resultModel.Entities.Count == 1);
            Assert.IsTrue(resultModel.Hyperedges.Count == 1);
            Assert.IsTrue(resultModel.HyperedgeConnectors.Count == 1);
        }

        [Test]
        public void SingeTransformationExecutionWithMultipleSubmodelInclusionTest()
        {
            var sourceModel = erDiagram.Metamodel;
            var targetModel = classDiagram.Metamodel;
            var pattern = erDiagram.GetEntitySubmodel();
            var rightPart = classDiagram.GetClassAssociationSubmodel();

            var transformationRule = new TransformationRule(pattern, rightPart, "TestRule");
            sourceModel.AddTransformationRule(targetModel, transformationRule);

            var model = sourceModel.Instantiate("ER Diagram Instance");
            var teacher = sourceModel.Entities.First(x => x.Label == "Сущность").Instantiate("Учитель");
            var fio = sourceModel.Entities.First(x => x.Label == "Атрибут").Instantiate("ФИО");
            model.AddNewEntityVertex(teacher);
            model.AddNewEntityVertex(fio);
            model.AddHyperedgeWithRelation(teacher, fio, sourceModel.Roles.First(x => x.Label == "Владелец атрибута"));

            var student = sourceModel.Entities.First(x => x.Label == "Сущность").Instantiate("Ученик");
            var stage = sourceModel.Entities.First(x => x.Label == "Атрибут").Instantiate("Класс");
            model.AddNewEntityVertex(student);
            model.AddNewEntityVertex(stage);
            model.AddHyperedgeWithRelation(student, stage, sourceModel.Roles.First(x => x.Label == "Владелец атрибута"));

            var resultModel = model.ExecuteTransformations(targetModel);

            Assert.IsTrue(resultModel.Entities.Count == 2);
            Assert.IsTrue(resultModel.Hyperedges.Count == 2);
            Assert.IsTrue(resultModel.HyperedgeConnectors.Count == 2);
        }

        [Test]
        public void MultipleTransformationExecutionWithMultipleSubmodelInclusionsTest()
        {
            var sourceModel = erDiagram.Metamodel;
            var targetModel = classDiagram.Metamodel;

            var pattern1 = erDiagram.GetEntitySubmodel();
            var rightPart1 = classDiagram.GetClassSubmodel();
            var pattern2 = erDiagram.GetLinkSubmodel();
            var rightPart2 = classDiagram.GetAssociationSubmodel();            

            var transformationRule1 = new TransformationRule(pattern1, rightPart1, "TestRule 1");
            var transformationRule2 = new TransformationRule(pattern2, rightPart2, "TestRule 2");
            sourceModel.AddTransformationRule(targetModel, transformationRule1);
            sourceModel.AddTransformationRule(targetModel, transformationRule2);

            var model = sourceModel.Instantiate("ER Diagram Instance");
            var teacher = sourceModel.Entities.First(x => x.Label == "Сущность").Instantiate("Учитель");
            var fio = sourceModel.Entities.First(x => x.Label == "Атрибут").Instantiate("ФИО");
            model.AddNewEntityVertex(teacher);
            model.AddNewEntityVertex(fio);
            model.AddHyperedgeWithRelation(teacher, fio, sourceModel.Roles.First(x => x.Label == "Владелец атрибута"));

            var student = sourceModel.Entities.First(x => x.Label == "Сущность").Instantiate("Ученик");
            var stage = sourceModel.Entities.First(x => x.Label == "Атрибут").Instantiate("Класс");
            model.AddNewEntityVertex(student);
            model.AddNewEntityVertex(stage);
            model.AddHyperedgeWithRelation(student, stage, sourceModel.Roles.First(x => x.Label == "Владелец атрибута"));

            var educating = sourceModel.Entities.First(x => x.Label == "Связь").Instantiate("Обучает");
            var price = sourceModel.Entities.First(x => x.Label == "Атрибут").Instantiate("Бесплатно");
            model.AddNewEntityVertex(educating);
            model.AddNewEntityVertex(price);
            model.AddHyperedgeWithRelation(teacher, educating, sourceModel.Roles.First(x => x.Label == "Приемник связи"));
            model.AddHyperedgeWithRelation(student, educating, sourceModel.Roles.First(x => x.Label == "Приемник связи"));

            var resultModel = model.ExecuteTransformations(targetModel);

            Assert.IsTrue(resultModel.Entities.Count == 2);
            Assert.IsTrue(resultModel.Hyperedges.Count == 1);
            Assert.IsTrue(resultModel.HyperedgeConnectors.Count == 1);
            Assert.IsTrue(resultModel.Entities.First().ConnectedVertices.First() == resultModel.Entities.Last());
        }

        [Test]
        public void MultipleTransformationExecutionWithHyperedgeTransformationsTest()
        {
            var sourceModel = erDiagram.Metamodel;
            var targetModel = classDiagram.Metamodel;

            var pattern1 = erDiagram.GetEntitySubmodel();
            var rightPart1 = classDiagram.GetClassSubmodel();
            var pattern2 = erDiagram.GetLinkSubmodel();
            var rightPart2 = classDiagram.GetAssociationSubmodel();
            var pattern3 = erDiagram.GetSuperentitySubentitySubmodel();
            var rightPart3 = classDiagram.GetInheritanceSubmodel();

            var transformationRule1 = new TransformationRule(pattern1, rightPart1, "TestRule 1");
            var transformationRule2 = new TransformationRule(pattern2, rightPart2, "TestRule 2");
            var transformationRule3 = new TransformationRule(pattern3, rightPart3, "TestRule 3");
            sourceModel.AddTransformationRule(targetModel, transformationRule1);
            sourceModel.AddTransformationRule(targetModel, transformationRule2);
            sourceModel.AddTransformationRule(targetModel, transformationRule3);

            var model = sourceModel.Instantiate("ER Diagram Instance");
            var person = sourceModel.Entities.First(x => x.Label == "Сущность").Instantiate("Человек");
            var fio = sourceModel.Entities.First(x => x.Label == "Атрибут").Instantiate("ФИО");
            model.AddNewEntityVertex(person);
            model.AddNewEntityVertex(fio);
            model.AddHyperedgeWithRelation(person, fio, sourceModel.Roles.First(x => x.Label == "Владелец атрибута"));

            var teacher = sourceModel.Entities.First(x => x.Label == "Сущность").Instantiate("Учитель");
            var post = sourceModel.Entities.First(x => x.Label == "Атрибут").Instantiate("Должность");
            model.AddNewEntityVertex(teacher);
            model.AddNewEntityVertex(post);
            model.AddHyperedgeWithRelation(teacher, post, sourceModel.Roles.First(x => x.Label == "Владелец атрибута"));
            model.AddHyperedgeWithRelation(teacher, person, sourceModel.Roles.First(x => x.Label == "Источник связи"));

            var student = sourceModel.Entities.First(x => x.Label == "Сущность").Instantiate("Ученик");
            var stage = sourceModel.Entities.First(x => x.Label == "Атрибут").Instantiate("Класс");
            model.AddNewEntityVertex(student);
            model.AddNewEntityVertex(stage);
            model.AddHyperedgeWithRelation(student, stage, sourceModel.Roles.First(x => x.Label == "Владелец атрибута"));
            model.AddHyperedgeWithRelation(student, person, sourceModel.Roles.First(x => x.Label == "Источник связи"));

            var educating = sourceModel.Entities.First(x => x.Label == "Связь").Instantiate("Обучает");
            var price = sourceModel.Entities.First(x => x.Label == "Атрибут").Instantiate("Бесплатно");
            model.AddNewEntityVertex(educating);
            model.AddNewEntityVertex(price);
            model.AddHyperedgeWithRelation(teacher, educating, sourceModel.Roles.First(x => x.Label == "Приемник связи"));
            model.AddHyperedgeWithRelation(student, educating, sourceModel.Roles.First(x => x.Label == "Приемник связи"));

            var resultModel = model.ExecuteTransformations(targetModel);

            Assert.IsTrue(resultModel.Entities.Count == 3);
            Assert.IsTrue(resultModel.Hyperedges.Count == 3);
            Assert.IsTrue(resultModel.Edges.Count == 3);
            Assert.IsTrue(resultModel.Hyperedges.Count(x => x.Label == "Наследование" && x.CorrespondingHyperedge.Links.Count==2)==2);
            Assert.IsTrue(resultModel.Hyperedges.Count(x => x.Label == "Ассоциация" && x.CorrespondingHyperedge.Links.Count == 2) ==1);
        }

        [Test]
        public void ComplexHyperedgeToVertexTransformationTest()
        {
            var sourceModel = hbBusDiagram.Metamodel;
            var targetModel = ebBusDiagram.Metamodel;

            hbBusDiagram.AddTransformationsToTargetModel(targetModel);
            var busDiagram = hbBusDiagram.GetSampleModel();

            var resultModel = busDiagram.ExecuteTransformations(targetModel);

            Assert.IsTrue(resultModel.Entities.Count == 4);
            Assert.IsTrue(resultModel.HyperedgeConnectors.Count == 3);
            Assert.IsTrue(resultModel.Hyperedges.Count == 3);
            Assert.IsTrue(resultModel.Hyperedges.Count(x => x.Label == "Bus To Processor") == 1);
            Assert.IsTrue(resultModel.Hyperedges.Count(x => x.Label == "Bus To RAM") == 1);
            Assert.IsTrue(resultModel.Hyperedges.Count(x => x.Label == "Bus To Periphery") == 1);
        }

        [Test]
        public void VertexToComplexHyperedgeTransformationTest()
        {
            var sourceModel = ebBusDiagram.Metamodel;
            var targetModel = hbBusDiagram.Metamodel;

            ebBusDiagram.AddTransformationsToTargetModel(targetModel);
            var busDiagram = ebBusDiagram.GetSampleModel();

            var resultModel = busDiagram.ExecuteTransformations(targetModel);

            Assert.IsTrue(resultModel.Entities.Count == 3);
            Assert.IsTrue(resultModel.HyperedgeConnectors.Count == 1);
            Assert.IsTrue(resultModel.Hyperedges.Count == 1);
        }

        [Test]
        public void FullModelTransformationTest()
        {
            var sourceModel = erDiagram.Metamodel;
            var targetModel = classDiagram.Metamodel;

            var pattern1 = erDiagram.GetEntitySubmodel();
            var rightPart1 = classDiagram.GetClassSubmodel();
            var pattern2 = erDiagram.GetLinkSubmodel();
            var rightPart2 = classDiagram.GetAssociationSubmodel();
            var pattern3 = erDiagram.GetSuperentitySubentitySubmodel();
            var rightPart3 = classDiagram.GetInheritanceSubmodel();

            var transformationRule1 = new TransformationRule(pattern1, rightPart1, "TestRule 1");
            var transformationRule2 = new TransformationRule(pattern2, rightPart2, "TestRule 2");
            var transformationRule3 = new TransformationRule(pattern3, rightPart3, "TestRule 3");
            sourceModel.AddTransformationRule(targetModel, transformationRule1);
            sourceModel.AddTransformationRule(targetModel, transformationRule2);
            sourceModel.AddTransformationRule(targetModel, transformationRule3);

            var model = erDiagram.GetSampleModel();

            var resultModel = model.ExecuteTransformations(targetModel);

            Assert.IsTrue(resultModel.Entities.Count == 4);
            Assert.IsTrue(resultModel.Hyperedges.Count == 5);
            Assert.IsTrue(resultModel.Edges.Count == 5);
            Assert.IsTrue(resultModel.Hyperedges.Count(x => x.Label == "Наследование" && x.CorrespondingHyperedge.Links.Count == 2) == 2);
            Assert.IsTrue(resultModel.Hyperedges.Count(x => x.Label == "Ассоциация" && x.CorrespondingHyperedge.Links.Count == 2) == 3);
        }       
    }
}

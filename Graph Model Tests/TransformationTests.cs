using DSM_Graph_Layer;
using DSM_Graph_Layer.HPGraphModel;
using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using DSM_Graph_Layer.Enums;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using DSM_Graph_Layer.HPGraphModel.ModelClasses.SubmodelMatching;
using Graph_Model_Tests.Metamodels;

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
            var pattern = erDiagram.GetSourceAttributeSubmodel();
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
            var pattern = erDiagram.GetSourceAttributeSubmodel();
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
            var pattern = erDiagram.GetSourceAttributeSubmodel();
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

            var pattern1 = erDiagram.GetSourceAttributeSubmodel();
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

            var pattern1 = erDiagram.GetSourceAttributeSubmodel();
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

            var pattern1 = hbBusDiagram.GetCpuSubmodel();
            var rightPart1 = ebBusDiagram.GetCpuSubmodel();
            var pattern2 = hbBusDiagram.GetRamSubmodel();
            var rightPart2 = ebBusDiagram.GetRamSubmodel();
            var pattern3 = hbBusDiagram.GetPeripherySubmodel();
            var rightPart3 = ebBusDiagram.GetPeripherySubmodel();
            var pattern4 = hbBusDiagram.GetBusHyperedgeSubmodel();
            var rightPart4 = ebBusDiagram.GetBusHyperedgeSubmodel();

            var transformationRule1 = new TransformationRule(pattern1, rightPart1, "TestRule 1");
            var transformationRule2 = new TransformationRule(pattern2, rightPart2, "TestRule 2");
            var transformationRule3 = new TransformationRule(pattern3, rightPart3, "TestRule 3");
            var transformationRule4 = new TransformationRule(pattern4, rightPart4, "TestRule 4");
            sourceModel.AddTransformationRule(targetModel, transformationRule1);
            sourceModel.AddTransformationRule(targetModel, transformationRule2);
            sourceModel.AddTransformationRule(targetModel, transformationRule3);
            sourceModel.AddTransformationRule(targetModel, transformationRule4);

            var busDiagram = sourceModel.Instantiate("Test Bus Diagram");
            var proc = sourceModel.Entities.First(x => x.Label == "CPU").Instantiate("Intel Core i5-2400");
            busDiagram.AddNewEntityVertex(proc);
            var ram = sourceModel.Entities.First(x => x.Label == "RAM").Instantiate("DDR4 PC4-28800U");
            busDiagram.AddNewEntityVertex(ram);
            var periphery = sourceModel.Entities.First(x => x.Label == "Periphery").Instantiate("Монитор HP 25x");
            busDiagram.AddNewEntityVertex(periphery);

            var busLink = busDiagram.AddHyperedgeWithRelation(proc, ram, sourceModel.Roles.First());
            busDiagram.AddRelationToHyperedge(busLink, ram, periphery, sourceModel.Roles.First());
            busDiagram.AddRelationToHyperedge(busLink, periphery, proc, sourceModel.Roles.First());

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

            var pattern1 = ebBusDiagram.GetCpuSubmodel();
            var rightPart1 = hbBusDiagram.GetCpuSubmodel();
            var pattern2 = ebBusDiagram.GetRamSubmodel();
            var rightPart2 = hbBusDiagram.GetRamSubmodel();
            var pattern3 = ebBusDiagram.GetPeripherySubmodel();
            var rightPart3 = hbBusDiagram.GetPeripherySubmodel();
            var pattern4 = ebBusDiagram.GetBusEntitySubmodel();
            var rightPart4 = hbBusDiagram.GetBusHyperedgeSubmodel();

            var transformationRule1 = new TransformationRule(pattern1, rightPart1, "TestRule 1");
            var transformationRule2 = new TransformationRule(pattern2, rightPart2, "TestRule 2");
            var transformationRule3 = new TransformationRule(pattern3, rightPart3, "TestRule 3");
            var transformationRule4 = new TransformationRule(pattern4, rightPart4, "TestRule 4");
            sourceModel.AddTransformationRule(targetModel, transformationRule1);
            sourceModel.AddTransformationRule(targetModel, transformationRule2);
            sourceModel.AddTransformationRule(targetModel, transformationRule3);
            sourceModel.AddTransformationRule(targetModel, transformationRule4);

            var busDiagram = sourceModel.Instantiate("Test Bus Diagram");
            var proc = sourceModel.Entities.First(x => x.Label == "CPU").Instantiate("Intel Core i5-2400");
            busDiagram.AddNewEntityVertex(proc);
            var ram = sourceModel.Entities.First(x => x.Label == "RAM").Instantiate("DDR4 PC4-28800U");
            busDiagram.AddNewEntityVertex(ram);
            var periphery = sourceModel.Entities.First(x => x.Label == "Periphery").Instantiate("Монитор HP 25x");
            busDiagram.AddNewEntityVertex(periphery);
            var bus = sourceModel.Entities.First(x => x.Label == "Bus").Instantiate("Шина компьютера");
            busDiagram.AddNewEntityVertex(bus);

            busDiagram.AddHyperedgeWithRelation(proc, bus, sourceModel.Roles.First(x=>x.Label== "Processor connection"));
            busDiagram.AddHyperedgeWithRelation(ram, bus, sourceModel.Roles.First(x=>x.Label== "RAM Connection"));
            busDiagram.AddHyperedgeWithRelation(periphery, bus, sourceModel.Roles.First(x=>x.Label== "Periphery connection"));

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

            var pattern1 = erDiagram.GetSourceAttributeSubmodel();
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
            var stage = sourceModel.Entities.First(x => x.Label == "Атрибут").Instantiate("Направление");
            model.AddNewEntityVertex(student);
            model.AddNewEntityVertex(stage);
            model.AddHyperedgeWithRelation(student, stage, sourceModel.Roles.First(x => x.Label == "Владелец атрибута"));
            model.AddHyperedgeWithRelation(student, person, sourceModel.Roles.First(x => x.Label == "Источник связи"));

            var examCard = sourceModel.Entities.First(x => x.Label == "Сущность").Instantiate("Билет");
            var question = sourceModel.Entities.First(x => x.Label == "Атрибут").Instantiate("Вопрос");
            model.AddNewEntityVertex(examCard);
            model.AddNewEntityVertex(question);
            model.AddHyperedgeWithRelation(examCard, question, sourceModel.Roles.First(x => x.Label == "Владелец атрибута"));

            var educating = sourceModel.Entities.First(x => x.Label == "Связь").Instantiate("Сдавать экзамен");
            model.AddNewEntityVertex(educating);
            model.AddHyperedgeWithRelation(teacher, educating, sourceModel.Roles.First(x => x.Label == "Приемник связи"));
            model.AddHyperedgeWithRelation(student, educating, sourceModel.Roles.First(x => x.Label == "Приемник связи"));

            var gettingCard = sourceModel.Entities.First(x => x.Label == "Связь").Instantiate("Тянет");
            model.AddNewEntityVertex(gettingCard);
            model.AddHyperedgeWithRelation(student, gettingCard, sourceModel.Roles.First(x => x.Label == "Приемник связи"));
            model.AddHyperedgeWithRelation(examCard, gettingCard, sourceModel.Roles.First(x => x.Label == "Приемник связи"));

            var creatingCard = sourceModel.Entities.First(x => x.Label == "Связь").Instantiate("Составляет");
            model.AddNewEntityVertex(creatingCard);
            model.AddHyperedgeWithRelation(teacher, creatingCard, sourceModel.Roles.First(x => x.Label == "Приемник связи"));
            model.AddHyperedgeWithRelation(examCard, creatingCard, sourceModel.Roles.First(x => x.Label == "Приемник связи"));

            var resultModel = model.ExecuteTransformations(targetModel);

            Assert.IsTrue(resultModel.Entities.Count == 4);
            Assert.IsTrue(resultModel.Hyperedges.Count == 5);
            Assert.IsTrue(resultModel.Edges.Count == 5);
            Assert.IsTrue(resultModel.Hyperedges.Count(x => x.Label == "Наследование" && x.CorrespondingHyperedge.Links.Count == 2) == 2);
            Assert.IsTrue(resultModel.Hyperedges.Count(x => x.Label == "Ассоциация" && x.CorrespondingHyperedge.Links.Count == 2) == 3);
        }       
    }
}

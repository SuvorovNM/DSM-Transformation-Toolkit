using DSM_Graph_Layer.HPGraphModel;
using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using DSM_Graph_Layer.HPGraphModel.ModelClasses.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Graph_Model_Tests.Metamodels
{
    public class EntityRelationDiagram : AbstractDiagram
    {
        public EntityRelationDiagram() : base()
        {
        }
        protected override Model CreateDiagram()
        {
            var model = new Model("Entity-Relation Diagram");

            var sourceRole = new Role("Источник связи");
            var targetRole = new Role(sourceRole, "Приемник связи");
            model.AddNewRolePairToGraph(targetRole);
            var attributeOwnerRole = new Role("Владелец атрибута");
            var attributeServantRole = new Role(attributeOwnerRole, "Атрибут");
            model.AddNewRolePairToGraph(attributeServantRole);

            var entity = new EntityVertex("Сущность");
            var entityLinksPort = new EntityPort("Связи", new[] { targetRole });
            var entityAttrPort = new EntityPort("Атрибуты", new[] { attributeOwnerRole });
            var entityInhPort = new EntityPort("Наследование", new[] { sourceRole });
            entity.AddPortToEntity(entityLinksPort);
            entity.AddPortToEntity(entityAttrPort);
            entity.AddPortToEntity(entityInhPort);
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

            var entityEntity = model.AddHyperedgeWithRelation(entityInhPort, entityLinksPort, sourceRole);
            entityEntity.SetLabel("Суперсущность_Подсущность");

            return model;
        }

        public Model GetSampleModel()
        {
            var model = Metamodel.Instantiate("ER Diagram Instance");
            var person = Metamodel.Entities.First(x => x.Label == "Сущность").Instantiate("Человек");
            var fio = Metamodel.Entities.First(x => x.Label == "Атрибут").Instantiate("ФИО");
            model.AddNewEntityVertex(person);
            model.AddNewEntityVertex(fio);
            model.AddHyperedgeWithRelation(person, fio, Metamodel.Roles.First(x => x.Label == "Владелец атрибута"));

            var teacher = Metamodel.Entities.First(x => x.Label == "Сущность").Instantiate("Учитель");
            var post = Metamodel.Entities.First(x => x.Label == "Атрибут").Instantiate("Должность");
            model.AddNewEntityVertex(teacher);
            model.AddNewEntityVertex(post);
            model.AddHyperedgeWithRelation(teacher, post, Metamodel.Roles.First(x => x.Label == "Владелец атрибута"));
            model.AddHyperedgeWithRelation(teacher, person, Metamodel.Roles.First(x => x.Label == "Источник связи"));

            var student = Metamodel.Entities.First(x => x.Label == "Сущность").Instantiate("Ученик");
            var stage = Metamodel.Entities.First(x => x.Label == "Атрибут").Instantiate("Направление");
            model.AddNewEntityVertex(student);
            model.AddNewEntityVertex(stage);
            model.AddHyperedgeWithRelation(student, stage, Metamodel.Roles.First(x => x.Label == "Владелец атрибута"));
            model.AddHyperedgeWithRelation(student, person, Metamodel.Roles.First(x => x.Label == "Источник связи"));

            var examCard = Metamodel.Entities.First(x => x.Label == "Сущность").Instantiate("Билет");
            var question = Metamodel.Entities.First(x => x.Label == "Атрибут").Instantiate("Вопрос");
            model.AddNewEntityVertex(examCard);
            model.AddNewEntityVertex(question);
            model.AddHyperedgeWithRelation(examCard, question, Metamodel.Roles.First(x => x.Label == "Владелец атрибута"));

            var educating = Metamodel.Entities.First(x => x.Label == "Связь").Instantiate("Сдавать экзамен");
            model.AddNewEntityVertex(educating);
            model.AddHyperedgeWithRelation(teacher, educating, Metamodel.Roles.First(x => x.Label == "Приемник связи"));
            model.AddHyperedgeWithRelation(student, educating, Metamodel.Roles.First(x => x.Label == "Приемник связи"));

            var gettingCard = Metamodel.Entities.First(x => x.Label == "Связь").Instantiate("Тянет");
            model.AddNewEntityVertex(gettingCard);
            model.AddHyperedgeWithRelation(student, gettingCard, Metamodel.Roles.First(x => x.Label == "Приемник связи"));
            model.AddHyperedgeWithRelation(examCard, gettingCard, Metamodel.Roles.First(x => x.Label == "Приемник связи"));

            var creatingCard = Metamodel.Entities.First(x => x.Label == "Связь").Instantiate("Составляет");
            model.AddNewEntityVertex(creatingCard);
            model.AddHyperedgeWithRelation(teacher, creatingCard, Metamodel.Roles.First(x => x.Label == "Приемник связи"));
            model.AddHyperedgeWithRelation(examCard, creatingCard, Metamodel.Roles.First(x => x.Label == "Приемник связи"));

            return model;
        }

        public ModelForTransformation GetEntitySubmodel()
        {
            var sourceEntity = Metamodel.Entities.First(x => x.Label == "Сущность");

            var pattern = new ModelForTransformation(new[] { sourceEntity}, null);
            return pattern;
        }
        public ModelForTransformation GetLinkSubmodel()
        {
            var linkEntity = Metamodel.Entities.First(x => x.Label == "Связь");

            var pattern = new ModelForTransformation(new[] { linkEntity }, null, null);
            return pattern;
        }
        public ModelForTransformation GetSuperentitySubentitySubmodel()
        {
            var inhLink = Metamodel.Hyperedges.First(x => x.Label == "Суперсущность_Подсущность");

            var pattern = new ModelForTransformation(null, new[] { inhLink });
            return pattern;
        }
    }
}

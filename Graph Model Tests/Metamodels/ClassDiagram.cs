using DSM_Graph_Layer.HPGraphModel;
using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using DSM_Graph_Layer.HPGraphModel.ModelClasses.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Graph_Model_Tests.Metamodels
{
    class ClassDiagram : AbstractDiagram
    {
        public ClassDiagram() : base()
        {

        }
        protected override Model CreateDiagram()
        {
            var model = new Model("Connected Entities Diagram");
            var associationRole = new Role("Ассоциация");
            model.AddNewRolePairToGraph(associationRole);
            var inheritanceRole = new Role("Наследование-родитель");
            var inheritanceRole1 = new Role(inheritanceRole, "Наследование потомок");
            model.AddNewRolePairToGraph(inheritanceRole);

            var entity = new EntityVertex("Класс");
            var entityLinksPort = new EntityPort("Связи", new[] { associationRole, inheritanceRole, inheritanceRole1 });
            entity.AddPortToEntity(entityLinksPort);
            entity.Attributes.Add(new ElementAttribute("string", "Имя"));
            entity.Attributes.Add(new ElementAttribute("string", "Описание"));
            entity.Attributes.Add(new ElementAttribute("string", "Свойства"));
            entity.Attributes.Add(new ElementAttribute("string", "Методы"));
            model.AddNewEntityVertex(entity);

            var entityLink = model.AddHyperedgeWithRelation(entityLinksPort, entityLinksPort, associationRole);
            entityLink.SetLabel("Ассоциация");
            var entityInh = model.AddHyperedgeWithRelation(entityLinksPort, entityLinksPort, inheritanceRole);
            entityInh.SetLabel("Наследование");

            return model;
        }


        public ModelForTransformation GetClassAssociationSubmodel()
        {
            var targetClass = Metamodel.Entities.First(x => x.Label == "Класс");
            var targetHyperedge = Metamodel.Hyperedges.First(x => x.Label == "Ассоциация");
            var targetHyperedgeConnector = targetHyperedge.CorrespondingHyperedge;
            var rightPart = new ModelForTransformation(new[] { targetClass }, null, new[] { targetHyperedge }, new[] { targetHyperedgeConnector });
            return rightPart;
        }

        public ModelForTransformation GetClassSubmodel()
        {
            var targetClass = Metamodel.Entities.First(x => x.Label == "Класс");
            var rightPart = new ModelForTransformation(new[] { targetClass }, null, null);
            return rightPart;
        }

        public ModelForTransformation GetAssociationSubmodel()
        {
            var targetClass = Metamodel.Entities.First(x => x.Label == "Класс");

            var targetHyperedge = Metamodel.Hyperedges.First(x => x.Label == "Ассоциация");
            var targetHyperedgeConnector = targetHyperedge.CorrespondingHyperedge;
            var rightPart = new ModelForTransformation(new[] { targetClass }, new[] { targetClass }, new[] { targetHyperedge }, new[] { targetHyperedgeConnector });
            return rightPart;
        }

        public ModelForTransformation GetInheritanceSubmodel()
        {
            var targetClass = Metamodel.Entities.First(x => x.Label == "Класс");
            var targetHyperedge = Metamodel.Hyperedges.First(x => x.Label == "Наследование");
            var targetHyperedgeConnector = targetHyperedge.CorrespondingHyperedge;

            var rightPart = new ModelForTransformation(new[] { targetClass }, new[] { targetClass }, new[] { targetHyperedge }, new[] { targetHyperedgeConnector });
            return rightPart;
        }
    }
}

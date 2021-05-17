using DSM_Graph_Layer.HPGraphModel;
using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using DSM_Graph_Layer.HPGraphModel.ModelClasses.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Graph_Model_Tests.Metamodels
{
    public class HyperedgeBasedBusDiagram : AbstractDiagram
    {
        public HyperedgeBasedBusDiagram() : base()
        {
            Metamodel = CreateDiagram();
        }
        public HyperedgeBasedBusDiagram(Model metamodel)
        {
            Metamodel = metamodel;
        }
        protected override Model CreateDiagram()
        {
            var model = new Model("Bus Diagram with hyperedge in center");
            var connectionRole = new Role("Subbus");
            model.AddNewRolePairToGraph(connectionRole);

            var proc = new EntityVertex("CPU");
            var procPort = new EntityPort("CPU connecton", new[] { connectionRole });
            proc.AddPortToEntity(procPort);
            proc.Attributes.Add(new ElementAttribute("Double", "Частота"));
            proc.Attributes.Add(new ElementAttribute("Int32", "Количество ядер"));
            model.AddNewEntityVertex(proc);

            var ram = new EntityVertex("RAM");
            var ramPort = new EntityPort("RAM connection", new[] { connectionRole });
            ram.AddPortToEntity(ramPort);
            ram.Attributes.Add(new ElementAttribute("Int32", "Объем"));
            model.AddNewEntityVertex(ram);

            var periphery = new EntityVertex("Periphery");
            var peripheryPort = new EntityPort("Periphery connection", new[] { connectionRole });
            periphery.AddPortToEntity(peripheryPort);
            periphery.Attributes.Add(new ElementAttribute("string", "Название"));
            model.AddNewEntityVertex(periphery);

            var busLink = model.AddHyperedgeWithRelation(proc, ram, connectionRole);
            busLink.AddRelationToHyperedge(ram, periphery, connectionRole);
            busLink.AddRelationToHyperedge(periphery, proc, connectionRole);
            busLink.SetLabel("Bus");

            return model;
        }

        public ModelForTransformation GetCpuSubmodel()
        {
            var targetClass = Metamodel.Entities.First(x => x.Label == "CPU");

            var rightPart = new ModelForTransformation(new[] { targetClass }, null, null);
            return rightPart;
        }
        public ModelForTransformation GetRamSubmodel()
        {
            var targetClass = Metamodel.Entities.First(x => x.Label == "RAM");

            var rightPart = new ModelForTransformation(new[] { targetClass }, null, null);
            return rightPart;
        }
        public ModelForTransformation GetPeripherySubmodel()
        {
            var targetClass = Metamodel.Entities.First(x => x.Label == "Periphery");

            var rightPart = new ModelForTransformation(new[] { targetClass }, null, null);
            return rightPart;
        }

        public ModelForTransformation GetBusHyperedgeSubmodel()
        {
            var targetClass = Metamodel.Hyperedges.First(x => x.Label == "Bus");

            var rightPart = new ModelForTransformation(null, new[] { targetClass });
            return rightPart;
        }
        public Model GetSampleModel()
        {
            var busDiagram = Metamodel.Instantiate("Test Bus Diagram");
            var proc = Metamodel.Entities.First(x => x.Label == "CPU").Instantiate("Intel Core i5-2400");
            busDiagram.AddNewEntityVertex(proc);
            var ram = Metamodel.Entities.First(x => x.Label == "RAM").Instantiate("DDR4 PC4-28800U");
            busDiagram.AddNewEntityVertex(ram);
            var periphery = Metamodel.Entities.First(x => x.Label == "Periphery").Instantiate("Монитор HP 25x");
            busDiagram.AddNewEntityVertex(periphery);

            var busLink = busDiagram.AddHyperedgeWithRelation(proc, ram, Metamodel.Roles.First());
            busLink.AddRelationToHyperedge(ram, periphery, Metamodel.Roles.First());
            busLink.AddRelationToHyperedge(periphery, proc, Metamodel.Roles.First());

            return busDiagram;
        }
        public void AddTransformationsToTargetModel(Model ebBusDiagram)
        {
            var entityBasedDiagram = new EntityBasedBusDiagram(ebBusDiagram);
            var targetModel = entityBasedDiagram.Metamodel;

            var pattern1 = GetCpuSubmodel();
            var rightPart1 = entityBasedDiagram.GetCpuSubmodel();
            var pattern2 = GetRamSubmodel();
            var rightPart2 = entityBasedDiagram.GetRamSubmodel();
            var pattern3 = GetPeripherySubmodel();
            var rightPart3 = entityBasedDiagram.GetPeripherySubmodel();
            var pattern4 = GetBusHyperedgeSubmodel();
            var rightPart4 = entityBasedDiagram.GetBusHyperedgeSubmodel();

            var transformationRule1 = new TransformationRule(pattern1, rightPart1, "TestRule 1");
            var transformationRule2 = new TransformationRule(pattern2, rightPart2, "TestRule 2");
            var transformationRule3 = new TransformationRule(pattern3, rightPart3, "TestRule 3");
            var transformationRule4 = new TransformationRule(pattern4, rightPart4, "TestRule 4");
            Metamodel.AddTransformationRule(targetModel, transformationRule1);
            Metamodel.AddTransformationRule(targetModel, transformationRule2);
            Metamodel.AddTransformationRule(targetModel, transformationRule3);
            Metamodel.AddTransformationRule(targetModel, transformationRule4);
        }
    }
}

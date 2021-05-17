using DSM_Graph_Layer.HPGraphModel;
using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using DSM_Graph_Layer.HPGraphModel.ModelClasses.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Graph_Model_Tests.Metamodels
{
    public class EntityBasedBusDiagram : AbstractDiagram
    {
        public EntityBasedBusDiagram() : base()
        {
            Metamodel = CreateDiagram();
        }
        public EntityBasedBusDiagram(Model metamodel)
        {
            Metamodel = metamodel;
        }
        protected override Model CreateDiagram()
        {
            var model = new Model("Bus Diagram with entity in center");
            var processorConnectionRole = new Role("Processor connection");
            model.AddNewRolePairToGraph(processorConnectionRole);
            var ramConnectionRole = new Role("RAM Connection");
            model.AddNewRolePairToGraph(ramConnectionRole);
            var peripheryConnectionRole = new Role("Periphery connection");
            model.AddNewRolePairToGraph(peripheryConnectionRole);

            var proc = new EntityVertex("CPU");
            var procPort = new EntityPort("CPU connecton", new[] { processorConnectionRole });
            proc.AddPortToEntity(procPort);
            proc.Attributes.Add(new ElementAttribute("Double", "Частота"));
            proc.Attributes.Add(new ElementAttribute("Int32", "Количество ядер"));
            model.AddNewEntityVertex(proc);

            var ram = new EntityVertex("RAM");
            var ramPort = new EntityPort("RAM connection", new[] { ramConnectionRole });
            ram.AddPortToEntity(ramPort);
            ram.Attributes.Add(new ElementAttribute("Int32", "Объем"));
            model.AddNewEntityVertex(ram);

            var periphery = new EntityVertex("Periphery");
            var peripheryPort = new EntityPort("Periphery connection", new[] { peripheryConnectionRole });
            periphery.AddPortToEntity(peripheryPort);
            periphery.Attributes.Add(new ElementAttribute("string", "Название"));
            model.AddNewEntityVertex(periphery);

            var bus = new EntityVertex("Bus");
            var busToProcPort = new EntityPort("Connection To CPU", new[] { processorConnectionRole });
            var busToRamPort = new EntityPort("Connection To RAM", new[] { ramConnectionRole });
            var busToPeripheryPort = new EntityPort("Connection To Periphery", new[] { peripheryConnectionRole });
            bus.AddPortToEntity(busToProcPort);
            bus.AddPortToEntity(busToRamPort);
            bus.AddPortToEntity(busToPeripheryPort);
            model.AddNewEntityVertex(bus);

            var busProcessorLink = model.AddHyperedgeWithRelation(proc, bus, processorConnectionRole);
            busProcessorLink.SetLabel("Bus To Processor");
            var busRamLink = model.AddHyperedgeWithRelation(ram, bus, ramConnectionRole);
            busRamLink.SetLabel("Bus To RAM");
            var busPeripheryLink = model.AddHyperedgeWithRelation(periphery, bus, peripheryConnectionRole);
            busPeripheryLink.SetLabel("Bus To Periphery");

            return model;
        }

        public ModelForTransformation GetCpuSubmodel()
        {
            var targetClass = Metamodel.Entities.First(x => x.Label == "CPU");

            var pattern = new ModelForTransformation(new[] { targetClass }, null, null);
            return pattern;
        }

        public ModelForTransformation GetRamSubmodel()
        {
            var targetClass = Metamodel.Entities.First(x => x.Label == "RAM");

            var pattern = new ModelForTransformation(new[] { targetClass }, null, null);
            return pattern;
        }
        public ModelForTransformation GetPeripherySubmodel()
        {
            var targetClass = Metamodel.Entities.First(x => x.Label == "Periphery");

            var pattern = new ModelForTransformation(new[] { targetClass }, null, null);
            return pattern;
        }

        public ModelForTransformation GetBusHyperedgeSubmodel()
        {
            var targetClass = Metamodel.Entities.First(x => x.Label == "Bus");
            var busToCpuHedge = Metamodel.Hyperedges.First(x => x.Label == "Bus To Processor");
            var busToRamHedge = Metamodel.Hyperedges.First(x => x.Label == "Bus To RAM");
            var busToPeripheryHedge = Metamodel.Hyperedges.First(x => x.Label == "Bus To Periphery");

            var pattern = new ModelForTransformation(new[] { targetClass }, new[] { busToCpuHedge, busToRamHedge, busToPeripheryHedge });
            return pattern;
        }

        public ModelForTransformation GetBusEntitySubmodel()
        {
            var targetClass = Metamodel.Entities.First(x => x.Label == "Bus");
            
            var pattern = new ModelForTransformation(new[] { targetClass }, null, null);
            return pattern;
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
            var bus = Metamodel.Entities.First(x => x.Label == "Bus").Instantiate("Шина компьютера");
            busDiagram.AddNewEntityVertex(bus);

            busDiagram.AddHyperedgeWithRelation(proc, bus, Metamodel.Roles.First(x => x.Label == "Processor connection"));
            busDiagram.AddHyperedgeWithRelation(ram, bus, Metamodel.Roles.First(x => x.Label == "RAM Connection"));
            busDiagram.AddHyperedgeWithRelation(periphery, bus, Metamodel.Roles.First(x => x.Label == "Periphery connection"));

            return busDiagram;
        }

        public void AddTransformationsToTargetModel(Model hbBusDiagram)
        {
            var hyperedgeBasedDiagram = new HyperedgeBasedBusDiagram(hbBusDiagram);
            var targetModel = hyperedgeBasedDiagram.Metamodel;

            var pattern1 = GetCpuSubmodel();
            var rightPart1 = hyperedgeBasedDiagram.GetCpuSubmodel();
            var pattern2 = GetRamSubmodel();
            var rightPart2 = hyperedgeBasedDiagram.GetRamSubmodel();
            var pattern3 = GetPeripherySubmodel();
            var rightPart3 = hyperedgeBasedDiagram.GetPeripherySubmodel();
            var pattern4 = GetBusEntitySubmodel();
            var rightPart4 = hyperedgeBasedDiagram.GetBusHyperedgeSubmodel();

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

using DSM_Graph_Layer.HPGraphModel;
using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Graph_Model_Tests.Metamodels
{
    class EntityBasedBusDiagram : AbstractDiagram
    {
        public EntityBasedBusDiagram() : base()
        {
            Metamodel = CreateDiagram();
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

            var pattern = new ModelForTransformation(new[] { targetClass }, null, new[] { busToCpuHedge, busToRamHedge, busToPeripheryHedge }, new[] { busToCpuHedge.CorrespondingHyperedge, busToRamHedge.CorrespondingHyperedge, busToPeripheryHedge.CorrespondingHyperedge });
            return pattern;
        }

        public ModelForTransformation GetBusEntitySubmodel()
        {
            var targetClass = Metamodel.Entities.First(x => x.Label == "Bus");
            
            var pattern = new ModelForTransformation(new[] { targetClass }, null, null);
            return pattern;
        }
    }
}

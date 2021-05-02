using DSM_Graph_Layer.HPGraphModel;
using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using DSM_Graph_Layer.HPGraphModel.ModelClasses.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Graph_Model_Tests.Metamodels
{
    class HyperedgeBasedBusDiagram : AbstractDiagram
    {
        public HyperedgeBasedBusDiagram() : base()
        {
            Metamodel = CreateDiagram();
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

            var rightPart = new ModelForTransformation(null, null, new[] { targetClass }, new[] { targetClass.CorrespondingHyperedge });
            return rightPart;
        }

    }
}

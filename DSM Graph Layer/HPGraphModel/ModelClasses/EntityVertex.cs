using DSM_Graph_Layer.HPGraphModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    class EntityVertex : Vertex, ILabeledElement, IAttributedElement, IMetamodelingElement<EntityVertex>
    {
        public EntityVertex(string label = "") : base()
        {
            Attributes = new List<ElementAttribute>();
            Label = label;
        }
        public List<EntityPort> Ports
        {
            get
            {
                return Poles.Select(x => x as EntityPort).ToList();
            }
        }
        public string Label { get; set; }
        public List<ElementAttribute> Attributes { get; set; }
        public EntityVertex BaseElement { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<EntityVertex> Instances { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void SetLabel(string label)
        {
            Label = label;
        }
        public void AddPortToEntity(EntityPort port)
        {
            throw new NotImplementedException();
        }
        public void RemovePortFromEntity(EntityPort port)
        {
            throw new NotImplementedException();
        }

        public void SetBaseElement(EntityVertex baseElement)
        {
            throw new NotImplementedException();
        }

        public EntityVertex Instantiate()
        {
            throw new NotImplementedException();
        }

        public void DeleteInstance(EntityVertex instance)
        {
            throw new NotImplementedException();
        }
    }
}

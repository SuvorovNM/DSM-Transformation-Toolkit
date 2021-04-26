using DSM_Graph_Layer.HPGraphModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    class EntityPort : Pole, ILabeledElement, IAttributedElement, IMetamodelingElement<EntityPort>
    {
        public EntityPort(string label = "", IEnumerable<Role> roles = null) : base()
        {
            Label = label;
            Attributes = new List<ElementAttribute>();
            if (roles != null)
                AcceptedRoles = new List<Role>(roles);
            else
                AcceptedRoles = new List<Role>();
        }

        public List<Role> AcceptedRoles { get; set; }
        public string Label { get; set; }
        public List<ElementAttribute> Attributes { get; set; }
        public EntityPort BaseElement { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<EntityPort> Instances { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void SetLabel(string label)
        {
            Label = label;
        }

        public override object Clone()
        {
            var port = new EntityPort(this.Label, this.AcceptedRoles);
            port.Type = Type;
            port.VertexOwner = this.VertexOwner;
            port.GraphOwner = this.GraphOwner;
            port.Attributes = new List<ElementAttribute>(this.Attributes);

            return port;
        }

        public void SetBaseElement(EntityPort baseElement)
        {
            throw new NotImplementedException();
        }

        public EntityPort Instantiate()
        {
            throw new NotImplementedException();
        }

        public void DeleteInstance(EntityPort instance)
        {
            throw new NotImplementedException();
        }
    }
}

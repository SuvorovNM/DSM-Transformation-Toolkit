using DSM_Graph_Layer.HPGraphModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    public class EntityPort : Pole, ILabeledElement, IAttributedElement, IMetamodelingElement<EntityPort>
    {
        public EntityPort(string label = "", IEnumerable<Role> roles = null) : base()
        {
            Label = label;
            Attributes = new List<ElementAttribute>();
            Instances = new List<EntityPort>();
            if (roles != null)
                AcceptedRoles = new List<Role>(roles);
            else
                AcceptedRoles = new List<Role>();
        }

        public List<Role> AcceptedRoles { get; set; }
        public string Label { get; set; }
        public List<ElementAttribute> Attributes { get; set; }
        public EntityPort BaseElement { get; set; }
        public List<EntityPort> Instances { get; set; }
        public List<HyperedgeRelation> Relations
        {
            get
            {
                // TODO: возможно получится сделать запрос проще
                return EdgeOwners.SelectMany(x => x.Links.Where(y => y.SourcePole == this).Select(y => y.TargetPole as HyperedgeRelation)).Union(EdgeOwners.SelectMany(x => x.Links.Where(y => y.TargetPole == this).Select(y => y.SourcePole as HyperedgeRelation))).ToList();
            }
        }
        public EntityVertex EntityOwner
        {
            get
            {
                return VertexOwner as EntityVertex;
            }
        }

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

        public void SetBaseElement(EntityPort baseElement) // Заменить на protected/private
        {
            // TODO: Продумать над проверками и необходимостью переопределения свойств
            BaseElement = baseElement;
            baseElement.Instances.Add(this);
        }

        public EntityPort Instantiate(string label)
        {
            var newPort = new EntityPort(label, this.AcceptedRoles);
            newPort.SetBaseElement(this);

            foreach(var attribute in Attributes)
            {
                newPort.Attributes.Add(new ElementAttribute(attribute.DataValue, ""));
            }
            newPort.Type = this.Type;

            return newPort;
        }

        public void DeleteInstance(EntityPort instance)
        {
            if (Instances.Contains(instance))
            {
                instance.BaseElement = null;
                Instances.Remove(instance);
            }
        }
    }
}

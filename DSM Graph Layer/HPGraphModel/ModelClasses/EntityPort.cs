using DSM_Graph_Layer.HPGraphModel.GraphClasses;
using DSM_Graph_Layer.HPGraphModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    [Serializable]
    /// <summary>
    /// Порт сущности
    /// </summary>
    public class EntityPort : Pole, IAttributedElement, IMetamodelingElement<EntityPort>
    {
        /// <summary>
        /// Создание порта сущности
        /// </summary>
        /// <param name="label">Наименование (метка)</param>
        /// <param name="roles">Принимаемые роли</param>
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

        public string Label { get; set; }
        public List<ElementAttribute> Attributes { get; set; }
        public EntityPort BaseElement { get; set; }
        public List<EntityPort> Instances { get; set; }

        /// <summary>
        /// Принимаемые роли
        /// </summary>
        public List<Role> AcceptedRoles { get; set; }
        /// <summary>
        /// Отношения, инцидентные порту
        /// </summary>
        public List<HyperedgeRelation> Relations
        {
            get
            {
                return EdgeOwners
                    .SelectMany(x => x.Links.Where(y => y.SourcePole == this).Select(y => y.TargetPole as HyperedgeRelation))
                    .Union(EdgeOwners.SelectMany(x => x.Links.Where(y => y.TargetPole == this).Select(y => y.SourcePole as HyperedgeRelation)))
                    .ToList();
            }
        }
        /// <summary>
        /// Сущность - владелец порта
        /// </summary>
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

        /// <summary>
        /// Клонировать объект
        /// </summary>
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
            if (BaseElement != null)
                BaseElement.DeleteInstance(this);

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

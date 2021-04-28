using DSM_Graph_Layer.HPGraphModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    public class HyperedgeRelation : Pole, ILabeledElement, IAttributedElement, IMetamodelingElement<HyperedgeRelation>
    {
        public HyperedgeRelation(Role role, string label = "") : base()
        {
            RelationRole = role;
            Label = label;
            Attributes = new List<ElementAttribute>();
            Instances = new List<HyperedgeRelation>();
        }

        public HyperedgeRelation OppositeRelation { get; set; }
        public Role RelationRole { get; set; }
        public string Label { get; set; }
        public List<ElementAttribute> Attributes { get; set; }
        public HyperedgeRelation BaseElement { get; set; }
        public List<HyperedgeRelation> Instances { get; set; }
        public HyperedgeVertex HyperEdge
        {
            get
            {
                return VertexOwner as HyperedgeVertex;
            }
        }

        public void SetLabel(string label)
        {
            Label = label;
        }

        public override object Clone()
        {
            var relation = new HyperedgeRelation(this.RelationRole, this.Label);
            relation.Type = Type;
            relation.VertexOwner = this.VertexOwner;
            relation.GraphOwner = this.GraphOwner;
            relation.Attributes = new List<ElementAttribute>(this.Attributes);

            return relation;
        }

        public void SetOppositeRelation(HyperedgeRelation relation)
        {
            OppositeRelation = relation;
            relation.OppositeRelation = this;
        }

        public void SetBaseElement(HyperedgeRelation baseElement)
        {
            BaseElement = baseElement;
            baseElement.Instances.Add(baseElement);
        }

        public HyperedgeRelation Instantiate(string label)
        {
            var newRelation = new HyperedgeRelation(RelationRole, label);
            newRelation.SetBaseElement(this);

            foreach (var attribute in Attributes)
            {
                newRelation.Attributes.Add(new ElementAttribute(attribute.DataValue, ""));
            }
            newRelation.Type = Type;
            return newRelation;
        }

        public void DeleteInstance(HyperedgeRelation instance)
        {
            if (Instances.Contains(instance))
            {
                instance.BaseElement = null;
                Instances.Remove(instance);
            }
        }
    }
}

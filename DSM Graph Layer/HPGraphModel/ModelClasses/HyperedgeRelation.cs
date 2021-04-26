using DSM_Graph_Layer.HPGraphModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    class HyperedgeRelation : Pole, ILabeledElement, IAttributedElement, IMetamodelingElement<HyperedgeRelation>
    {
        public HyperedgeRelation(Role role, string label = "") : base()
        {
            RelationRole = role;
            Label = label;
            Attributes = new List<ElementAttribute>();
        }

        public Role RelationRole { get; set; }
        public string Label { get; set; }
        public List<ElementAttribute> Attributes { get; set; }
        public HyperedgeRelation BaseElement { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<HyperedgeRelation> Instances { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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

        public void SetBaseElement(HyperedgeRelation baseElement)
        {
            throw new NotImplementedException();
        }

        public HyperedgeRelation Instantiate()
        {
            throw new NotImplementedException();
        }

        public void DeleteInstance(HyperedgeRelation instance)
        {
            throw new NotImplementedException();
        }
    }
}

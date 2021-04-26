using DSM_Graph_Layer.HPGraphModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    class HyperedgeVertex : Vertex, ILabeledElement, IAttributedElement, IMetamodelingElement<HyperedgeVertex>
    {
        public HyperedgeVertex(string label="") // base или нет?
        {
            Label = label;
            Attributes = new List<ElementAttribute>();
        }
        public List<HyperedgeRelation> Relations
        {
            get
            {
                return Poles.Select(x => x as HyperedgeRelation).ToList();
            }
        }
        public string Label { get; set; }
        public List<ElementAttribute> Attributes { get; set; }
        public HyperedgeVertex BaseElement { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<HyperedgeVertex> Instances { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void SetLabel(string label)
        {
            Label = label;
        }

        public void AddRelationToHyperedge(HyperedgeRelation relation)
        {
            throw new NotImplementedException();
        }

        public void RemoveRelationFromHyperedge(HyperedgeRelation relation)
        {
            throw new NotImplementedException();
        }

        public void SetBaseElement(HyperedgeVertex baseElement)
        {
            throw new NotImplementedException();
        }

        public HyperedgeVertex Instantiate()
        {
            throw new NotImplementedException();
        }

        public void DeleteInstance(HyperedgeVertex instance)
        {
            throw new NotImplementedException();
        }
    }
}

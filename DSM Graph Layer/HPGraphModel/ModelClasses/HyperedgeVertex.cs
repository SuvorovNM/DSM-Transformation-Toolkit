using DSM_Graph_Layer.HPGraphModel.GraphClasses;
using DSM_Graph_Layer.HPGraphModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    public class HyperedgeVertex : Vertex, ILabeledElement, IAttributedElement, IMetamodelingElement<HyperedgeVertex>
    {
        public HyperedgeVertex(string label = "") // base или нет?
        {
            Label = label;
            Attributes = new List<ElementAttribute>();
            Instances = new List<HyperedgeVertex>();
            Poles = new List<Pole>();
        }
        public List<HyperedgeRelation> Relations
        {
            get
            {
                return Poles.Select(x => x as HyperedgeRelation).ToList();
            }
        }
        public List<Model> ModelDecompositions
        {
            get
            {
                return Decompositions.Select(x => x as Model).ToList();
            }
        }
        public string Label { get; set; }
        public List<ElementAttribute> Attributes { get; set; }
        public HyperedgeVertex BaseElement { get; set; }
        public List<HyperedgeVertex> Instances { get; set; }
        public RelationsPortsHyperedge CorrespondingHyperedge
        {
            get
            {
                return Relations?.First()?.EdgeOwners?.FirstOrDefault() as RelationsPortsHyperedge;
            }
        }
        public List<EntityVertex> ConnectedVertices
        {
            get
            {
                return Relations.Select(x=>x.CorrespondingPort.EntityOwner).ToList();
            }
        }

        public void SetLabel(string label)
        {
            Label = label;
        }

        /// <summary>
        /// Производится добавление сразу обоих полюсов отношение, т.к. само отношение - составное
        /// </summary>
        /// <param name="relation">Добавляемое отношение</param>
        public void AddRelationPairToHyperedge(HyperedgeRelation relation1, HyperedgeRelation relation2)
        {
            relation1.SetOppositeRelation(relation2);
            AddPole(relation1);
            AddPole(relation2);
            relation1.VertexOwner = this;
            relation2.VertexOwner = this;

            foreach (var instance in Instances)
            {
                var rel1 = relation1.Instantiate(relation1.Label);
                var rel2 = relation2.Instantiate(relation2.Label);

                instance.AddRelationPairToHyperedge(rel1, rel2);
            }
        }

        /// <summary>
        /// Производится удаления сразу обоих полюсов отношения, т.к. само отношение - составное
        /// </summary>
        /// <param name="relation">Удаляемое отношение</param>
        public void RemoveRelationFromHyperedge(HyperedgeRelation relation)
        {
            relation.BaseElement?.DeleteInstance(relation);
            relation.OppositeRelation.BaseElement?.DeleteInstance(relation.OppositeRelation);

            RemovePole(relation);
            RemovePole(relation.OppositeRelation);

            relation.BaseElement = null;
            foreach (var instance in Instances)
            {
                var relations = relation.Instances.Where(x => x.VertexOwner == instance);
                foreach (var item in relations.ToList())
                {
                    instance.RemoveRelationFromHyperedge(item);
                    instance.RemoveRelationFromHyperedge(item.OppositeRelation);
                }
            }
        }

        public void SetBaseElement(HyperedgeVertex baseElement)
        {
            BaseElement = baseElement;
            baseElement.Instances.Add(this);
        }

        public HyperedgeVertex Instantiate(string label)
        {
            // Учесть связь между отношениями!
            var newHyperedgeVertex = new HyperedgeVertex(label);
            newHyperedgeVertex.SetBaseElement(this);

            foreach (var attribute in Attributes)
            {
                newHyperedgeVertex.Attributes.Add(new ElementAttribute(attribute.DataValue, ""));
            }

            newHyperedgeVertex.SemanticType = SemanticType; // Уточнить использование Semantic Type
            foreach (var decomposition in ModelDecompositions)
            {
                newHyperedgeVertex.AddDecomposition(decomposition.Instantiate(decomposition.Label));
            }

            // Временное решение для учета связей между отношениями
            var hyperedgeRelationInstances = new Dictionary<HyperedgeRelation, HyperedgeRelation>();

            var relationList = new List<HyperedgeRelation>();
            foreach(var rel in Relations)
            {
                if (!relationList.Contains(rel) && !relationList.Contains(rel.OppositeRelation))
                    relationList.Add(rel);
            }

            foreach (var rel in relationList)
            {
                var instance1 = rel.Instantiate(rel.Label);
                var instance2 = rel.OppositeRelation.Instantiate(rel.OppositeRelation.Label);                

                newHyperedgeVertex.AddRelationPairToHyperedge(instance1, instance2);
                hyperedgeRelationInstances.Add(rel, instance1);
            }
            /*foreach (var instRel in newHyperedgeVertex.Relations)
            {
                instRel.SetOppositeRelation(hyperedgeRelationInstances[instRel.BaseElement]);
            }*/

            return newHyperedgeVertex;
        }

        public void DeleteInstance(HyperedgeVertex instance)
        {
            if (Instances.Contains(instance))
            {
                Instances.Remove(instance);
                instance.BaseElement = null;

                foreach (var port in instance.Relations.ToList())
                {
                    port.BaseElement.DeleteInstance(port);
                }
            }
        }
    }
}

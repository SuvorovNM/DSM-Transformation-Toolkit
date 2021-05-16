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
    /// Гиперребро, представляемое вершиной с полюсами-отношениями
    /// </summary>
    public class HyperedgeVertex : Vertex, IAttributedElement, IMetamodelingElement<HyperedgeVertex>
    {
        /// <summary>
        /// Инициализация вершины
        /// </summary>
        /// <param name="label">Метка</param>
        public HyperedgeVertex(string label = "")
        {
            Label = label;
            Attributes = new List<ElementAttribute>();
            Instances = new List<HyperedgeVertex>();
            Poles = new List<Pole>();
        }
        public string Label { get; set; }
        public List<ElementAttribute> Attributes { get; set; }
        public HyperedgeVertex BaseElement { get; set; }
        public List<HyperedgeVertex> Instances { get; set; }
        /// <summary>
        /// Отношения, включаемые в гиперребро
        /// </summary>
        public List<HyperedgeRelation> Relations
        {
            get
            {
                return Poles.Select(x => x as HyperedgeRelation).ToList();
            }
        }
        /// <summary>
        /// Декомпозиции гиперребра
        /// </summary>
        public List<Model> ModelDecompositions
        {
            get
            {
                return Decompositions.Select(x => x as Model).ToList();
            }
        }
        /// <summary>
        /// Гиперребро-коннектор, связующее все отношения данного гиперребра
        /// </summary>
        public RelationsPortsHyperedge CorrespondingHyperedge
        {
            get
            {
                return Relations?.First()?.EdgeOwners?.FirstOrDefault() as RelationsPortsHyperedge;
            }
        }
        /// <summary>
        /// Связанные с гиперребром вершины
        /// </summary>
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


        /// <summary>
        /// Добавить связь между портом и отношением
        /// </summary>
        /// <param name="p">Порт</param>
        /// <param name="rel">Отношение</param>
        private void AddLinkBetweenPortAndRelation(EntityPort p, HyperedgeRelation rel)
        {
            var relationPortsLinks = CorrespondingHyperedge ?? new RelationsPortsHyperedge();
            relationPortsLinks.AddConnection(rel, p);
            // Добавление гиперребра происходит только в том случае, если оно еще не было добавлено
            OwnerGraph.AddStructure(relationPortsLinks);
        }

        /// <summary>
        /// Добавить к гиперребру отношение заданной роли между 2 портами
        /// </summary>
        /// <param name="source">Исходный порт</param>
        /// <param name="target">Целевой порт</param>
        /// <param name="r">Роль отношения</param>
        public void AddRelationToHyperedge(EntityPort source, EntityPort target, Role r)
        {
            var sourceRel = Relations.Where(x => x.RelationRole == r && x.CorrespondingPort == null);
            var targetRel = Relations.Where(x => x.RelationRole == r.OppositeRole && x.CorrespondingPort == null);
            if (sourceRel.Any() && targetRel.Any())
            {
                AddLinkBetweenPortAndRelation(source, sourceRel.First());
                AddLinkBetweenPortAndRelation(target, targetRel.First());
            }
        }

        /// <summary>
        /// Добавить к гиперребру отношение заданной роли между 2 вершинами
        /// </summary>
        /// <param name="source">Исходный порт</param>
        /// <param name="target">Целевой порт</param>
        /// <param name="r">Роль отношения</param>
        public void AddRelationToHyperedge(EntityVertex source, EntityVertex target, Role r)
        {
            var sourceRel = Relations.Where(x => x.RelationRole == r && x.CorrespondingPort == null).FirstOrDefault();
            var targetRel = sourceRel?.OppositeRelation;
            if (sourceRel == null || targetRel == null)
            {
                sourceRel = new HyperedgeRelation(r);
                targetRel = new HyperedgeRelation(r.OppositeRole);
                AddRelationPairToHyperedge(sourceRel, targetRel);

            }

            var sourcePort = source.Ports.Where(x => x.AcceptedRoles.Contains(r));
            var targetPort = target.Ports.Where(x => x.AcceptedRoles.Contains(r.OppositeRole));

            if (sourceRel != null && targetRel != null && sourcePort.Any() && targetPort.Any())
            {
                AddLinkBetweenPortAndRelation(sourcePort.First(), sourceRel);
                AddLinkBetweenPortAndRelation(targetPort.First(), targetRel);
            }
        }

        public void SetBaseElement(HyperedgeVertex baseElement)
        {
            if (BaseElement != null)
                BaseElement.DeleteInstance(this);
            BaseElement = baseElement;
            baseElement.Instances.Add(this);
        }

        public HyperedgeVertex Instantiate(string label)
        {
            var newHyperedgeVertex = new HyperedgeVertex(label);
            newHyperedgeVertex.SetBaseElement(this);

            foreach (var attribute in Attributes)
            {
                newHyperedgeVertex.Attributes.Add(new ElementAttribute(attribute.DataValue, ""));
            }

            newHyperedgeVertex.SemanticType = SemanticType;
            foreach (var decomposition in ModelDecompositions)
            {
                newHyperedgeVertex.AddDecomposition(decomposition.Instantiate(decomposition.Label));
            }

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
            }

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

using DSM_Graph_Layer.HPGraphModel.GraphClasses;
using DSM_Graph_Layer.HPGraphModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    [Serializable]
    /// <summary>
    /// Отношение - является полюсом вершины-гиперребра
    /// </summary>
    public class HyperedgeRelation : Pole, IAttributedElement, IMetamodelingElement<HyperedgeRelation>
    {
        /// <summary>
        /// Инициализировать отношений с заданной ролью
        /// </summary>
        /// <param name="role">Роль отношения</param>
        /// <param name="label">Метка</param>
        public HyperedgeRelation(Role role, string label = "") : base()
        {
            RelationRole = role;
            Label = label;
            Attributes = new List<ElementAttribute>();
            Instances = new List<HyperedgeRelation>();
        }
        public string Label { get; set; }
        public List<ElementAttribute> Attributes { get; set; }
        public HyperedgeRelation BaseElement { get; set; }
        public List<HyperedgeRelation> Instances { get; set; }
        /// <summary>
        /// Парное отношение - является противоположной частью текущего отношения (пример: это отношение "источник", парное - "приемник")
        /// </summary>
        public HyperedgeRelation OppositeRelation { get; set; }
        /// <summary>
        /// Роль отношения (задается наименованием)
        /// </summary>
        public Role RelationRole { get; set; }
        /// <summary>
        /// Порт, с которым связано данное отношение
        /// </summary>
        public EntityPort CorrespondingPort { get; set; }
        /// <summary>
        /// Гиперребро, владеющее отношением
        /// </summary>
        public HyperedgeVertex HyperedgeOwner
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

        /// <summary>
        /// Установить противоположное отношение для текущего отношения
        /// </summary>
        /// <param name="relation">Противоположное отношение</param>
        public void SetOppositeRelation(HyperedgeRelation relation)
        {
            OppositeRelation = relation;
            relation.OppositeRelation = this;
        }

        public void SetBaseElement(HyperedgeRelation baseElement)
        {
            if (BaseElement != null)
                BaseElement.DeleteInstance(this);
            BaseElement = baseElement;
            baseElement.Instances.Add(this);
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

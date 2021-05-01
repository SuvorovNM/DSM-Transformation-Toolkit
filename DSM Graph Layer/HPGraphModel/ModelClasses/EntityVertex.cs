using DSM_Graph_Layer.HPGraphModel.GraphClasses;
using DSM_Graph_Layer.HPGraphModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    public class EntityVertex : Vertex, ILabeledElement, IAttributedElement, IMetamodelingElement<EntityVertex>
    {
        public EntityVertex(string label = "") //: base()
        {
            GraphEnumerator.SetNextId(this);
            Attributes = new List<ElementAttribute>();
            Poles = new List<Pole>();
            Instances = new List<EntityVertex>();
            Decompositions = new List<HPGraph>();
            Label = label;
        }
        public List<EntityPort> Ports
        {
            get
            {
                return Poles.Select(x => x as EntityPort).ToList();
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
        public EntityVertex BaseElement { get; set; }
        public List<EntityVertex> Instances { get; set; }

        public void SetLabel(string label)
        {
            Label = label;
        }
        public void AddPortToEntity(EntityPort port)
        {
            // TODO: Предпроверка на то, является ли элемент без BaseElement - наверное, не нужно
            AddPole(port);
            foreach(var element in Instances)
            {
                var p = port.Instantiate(port.Label);
                element.AddPortToEntity(p);
            }
        }

        /// <summary>
        /// Удалить порт из сущности - на данный момент позволяется иметь несколько инициализаций порта в сущности
        /// </summary>
        /// <param name="port">Удаляемый порт</param>
        public void RemovePortFromEntity(EntityPort port)
        {
            var portRelations = port.Relations;
            foreach(var rel in portRelations)
            {
                rel.HyperedgeOwner.RemoveRelationFromHyperedge(rel);
            }

            port.BaseElement?.DeleteInstance(port);
            RemovePole(port);
            foreach(var element in Instances)
            {
                var ports = port.Instances.Where(x => x.VertexOwner == element);
                foreach(var item in ports.ToList())
                {
                    element.RemovePortFromEntity(item);
                }
            }
        }

        public void SetBaseElement(EntityVertex baseElement)
        {
            BaseElement = baseElement;
            baseElement.Instances.Add(this);
        }

        /// <summary>
        /// Создание экземпляра сущности. Создание экземпляров декомпозиций под вопросом.
        /// Имена портов/декомпозиций получаются от родительских элементов
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public EntityVertex Instantiate(string label)
        {
            var newEntity = new EntityVertex(label);
            newEntity.SetBaseElement(this);

            foreach (var attribute in Attributes)
            {
                newEntity.Attributes.Add(new ElementAttribute(attribute.DataValue,""));
            }

            newEntity.SemanticType = SemanticType; // Уточнить использование Semantic Type
            foreach (var decomposition in ModelDecompositions)
            {
                newEntity.AddDecomposition(decomposition.Instantiate(decomposition.Label));
            }

            foreach (var port in Ports)
            {
                newEntity.AddPortToEntity(port.Instantiate(port.Label));
            }

            return newEntity;
        }

        public void DeleteInstance(EntityVertex instance)
        {
            if (Instances.Contains(instance))
            {
                instance.BaseElement = null;
                Instances.Remove(instance);

                foreach (var port in instance.Ports.ToList())
                {
                    port.BaseElement.DeleteInstance(port);
                }
            }
        }
    }

    public class EntityVertexForTransformation: EntityVertex
    {
        public EntityVertexForTransformation(bool isIncomplete = false) :base()
        {
            IsIncomplete = isIncomplete;
        }
        public EntityVertexForTransformation(EntityVertex vertex, bool isIncomplete)
        {
            Attributes = vertex.Attributes;
            Poles = vertex.Poles;
            Instances = vertex.Instances;
            Decompositions = vertex.Decompositions;
            Label = vertex.Label;
            Id = vertex.Id;
            BaseElement = vertex.BaseElement;
            IsIncomplete = isIncomplete;
        }

        /// <summary>
        /// Является ли вершина-сущность неполной
        /// </summary>
        public bool IsIncomplete { get; set; }
    }
}

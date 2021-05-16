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
    /// Непосредственно сущность, представляемая вершиной
    /// </summary>
    public class EntityVertex : Vertex, IAttributedElement, IMetamodelingElement<EntityVertex>
    {
        /// <summary>
        /// Инициализировать сущность с заданной меткой
        /// </summary>
        /// <param name="label">Метка (наименование)</param>
        public EntityVertex(string label = "")
        {
            GraphEnumerator.SetNextId(this);
            Attributes = new List<ElementAttribute>();
            Poles = new List<Pole>();
            Instances = new List<EntityVertex>();
            Decompositions = new List<HPGraph>();
            Label = label;
        }
        public string Label { get; set; }
        public List<ElementAttribute> Attributes { get; set; }
        public EntityVertex BaseElement { get; set; }
        public List<EntityVertex> Instances { get; set; }

        /// <summary>
        /// Порты сущности
        /// </summary>
        public List<EntityPort> Ports
        {
            get
            {
                return Poles.Select(x => x as EntityPort).ToList();
            }
        }
        /// <summary>
        /// Возможные декомпозиции сущности
        /// </summary>
        public List<Model> ModelDecompositions
        {
            get
            {
                return Decompositions.Select(x => x as Model).ToList();
            }
        }
        /// <summary>
        /// Вершины, связанные с текущей
        /// </summary>
        public List<EntityVertex> ConnectedVertices
        {
            get
            {
                return Ports.SelectMany(x => x.Relations.Select(y => y.OppositeRelation.CorrespondingPort?.EntityOwner)).ToList();
            }
        }

        public void SetLabel(string label)
        {
            Label = label;
        }

        /// <summary>
        /// Добавить порт к сущности. На данный момент позволяется добавлять порт даже к экземплярам сущностей
        /// </summary>
        /// <param name="port">Порт сущности</param>
        public void AddPortToEntity(EntityPort port)
        {
            AddPole(port);
            foreach(var element in Instances)
            {
                var p = port.Instantiate(port.Label);
                element.AddPortToEntity(p);
            }
        }

        /// <summary>
        /// Удалить порт из сущности
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
            if (BaseElement != null)
                BaseElement.DeleteInstance(this);
            BaseElement = baseElement;
            baseElement.Instances.Add(this);
        }

        /// <summary>
        /// Создание экземпляра сущности. Декомпозиции также создаются и являются экземплярами исходных декомпозиций
        /// Имена портов/декомпозиций получаются от родительских элементов
        /// </summary>
        /// <param name="label">Метка модели</param>
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
}

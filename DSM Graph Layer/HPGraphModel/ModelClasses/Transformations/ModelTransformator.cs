using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses.Transformations
{
    /// <summary>
    /// Класс, выполняющий трансформации моделей
    /// </summary>
    class ModelTransformator
    {
        /// <summary>
        /// Инициализация трансформатора
        /// </summary>
        /// <param name="targetModel">Целевая модель</param>
        public ModelTransformator(Model targetModel)
        {
            TargetModel = targetModel;
            CorrespondingVerticies = new Dictionary<EntityVertex, List<EntityVertex>>();
        }

        /// <summary>
        /// Целевая модель
        /// </summary>
        public Model TargetModel { get; set; }
        /// <summary>
        /// Словарь соотнесения вершин исходной модели вершинам целевой
        /// </summary>
        private readonly Dictionary<EntityVertex, List<EntityVertex>> CorrespondingVerticies;

        /// <summary>
        /// Выполнить трансформацию
        /// </summary>
        /// <param name="searchResult">Найденная подмодель, изоморфная паттерну</param>
        /// <param name="rule">Правило, которое описывает преобразование</param>
        public void ExecuteTransformation(Model searchResult, TransformationRule rule)
        {
            var addedEntities = CreateEntityVertices(rule,  searchResult);
            var addedHyperedges = CreateHyperedgeVertices(rule);

            var currentConnected = searchResult.Entities.SelectMany(x => x.ConnectedVertices).Distinct().Except(searchResult.Entities).ToList();
            currentConnected = currentConnected.Union(searchResult.Hyperedges.SelectMany(x => x.ConnectedVertices).Distinct().Except(searchResult.Entities)).ToList();

            CreateHyperedgeConnections(rule, addedEntities, addedHyperedges, currentConnected);
        }
        private List<HyperedgeVertex> CreateHyperedgeVertices(TransformationRule rule)
        {
            var addedHyperedges = new List<HyperedgeVertex>();
            // Создание вершин-гиперребер
            foreach (var hyperedge in rule.RightPart.Hyperedges)
            {
                var hyperedgeInstance = hyperedge.Instantiate(hyperedge.Label);// Надо учитывать соотнесение информации (labels, attrs) с левой и правой частей
                TargetModel.AddNewHyperedgeVertex(hyperedgeInstance);
                addedHyperedges.Add(hyperedgeInstance);
            }
            return addedHyperedges;
        }
        private List<EntityVertex> CreateEntityVertices(TransformationRule rule, Model searchResult)
        {
            var addedEntities = new List<EntityVertex>();
            // Создание вершин-сущностей
            foreach (var entity in rule.RightPart.Entities)
            {
                var entityInstance = entity.Instantiate("[" + entity.Label + "]");
                TargetModel.AddNewEntityVertex(entityInstance);
                addedEntities.Add(entityInstance);
            }
            foreach (var ent in rule.LeftPart.Entities)
            {
                var entInst = searchResult.Entities.First(y => y.BaseElement == ent);
                if (!CorrespondingVerticies.TryGetValue(entInst, out var targetV))
                {
                    CorrespondingVerticies.Add(entInst, addedEntities);
                }
                else
                {
                    targetV.AddRange(addedEntities);
                }
                foreach (var addedEntity in addedEntities)
                {
                    addedEntity.Label += entInst.Label + " ";
                }
            }


            return addedEntities;
        }
        private void CreateHyperedgeConnections(TransformationRule rule, List<EntityVertex> addedEntities, List<HyperedgeVertex> addedHyperedges, List<EntityVertex> connectedVertices)
        {
            // Создание связующих гиперребер
            foreach (var hyperedgeConn in rule.RightPart.HyperedgeConnectors)
            {
                var correspondingHyperedgeInstance = addedHyperedges.First(x => x.BaseElement == hyperedgeConn.CorrespondingHyperedgeVertex);

                var connectedPortInstances = addedEntities.SelectMany(x => x.Ports).Where(y => hyperedgeConn.Ports.Contains(y.BaseElement));
                if (connectedPortInstances.Count() == hyperedgeConn.Ports.Count)
                {
                    // Если количество портов совпало, значит просто создаем на их основе гиперребро
                    CreateSingleHyperedgeConnector(hyperedgeConn, correspondingHyperedgeInstance, connectedPortInstances);
                }
                else
                {
                    // Если не совпало, необходимо учесть неполные вершины
                    var vertices = hyperedgeConn.Ports.Select(y => y.EntityOwner).Distinct();
                    foreach (var v in connectedVertices)
                    {
                        if (CorrespondingVerticies.TryGetValue(v, out var targetVerticies))
                        {
                            addedEntities.AddRange(targetVerticies.Where(x => vertices.Contains(x.BaseElement)));
                        }
                    }

                    connectedPortInstances = addedEntities.SelectMany(x => x.Ports).Where(y => hyperedgeConn.Ports.Contains(y.BaseElement));
                    CreateSingleHyperedgeConnector(hyperedgeConn, correspondingHyperedgeInstance, connectedPortInstances);
                }
            }
        }
        private void CreateSingleHyperedgeConnector(RelationsPortsHyperedge hyperedgeConn, HyperedgeVertex correspondingHyperedgeInstance, IEnumerable<EntityPort> connectedPortInstances)
        {
            var hyperedgeConnectorInstance = new RelationsPortsHyperedge();

            foreach (var port in connectedPortInstances)
            {
                hyperedgeConnectorInstance.AddPole(port);
            }

            foreach (var rel in correspondingHyperedgeInstance.Relations)
            {
                hyperedgeConnectorInstance.AddPole(rel);
                var port = hyperedgeConn.Relations.First(x => rel.BaseElement == x).CorrespondingPort;

                // Вынужденная недетерминированность - способ по возможности избегать генерации петель
                EntityPort portInst;
                if (connectedPortInstances.Any(x => x.BaseElement == port && !hyperedgeConnectorInstance.Links.Any(y => y.TargetPole == x)))
                {
                    portInst = connectedPortInstances.First(x => x.BaseElement == port && !hyperedgeConnectorInstance.Links.Any(y => y.TargetPole == x));
                }
                else
                {
                    portInst = connectedPortInstances.First(x => x.BaseElement == port);
                }

                hyperedgeConnectorInstance.AddConnection(rel, portInst);
            }

            TargetModel.AddStructure(hyperedgeConnectorInstance);
        }
    }
}

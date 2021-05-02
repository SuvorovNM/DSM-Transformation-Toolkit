using DSM_Graph_Layer.HPGraphModel.GraphClasses;
using System.Collections.Generic;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses.Transformations
{
    public class ModelForTransformation : Model
    {
        public List<EntityVertex> IncompleteVertices { get; set; }

        public ModelForTransformation(IEnumerable<EntityVertex> entities,
            IEnumerable<EntityVertex> incompleteEntities,
            IEnumerable<HyperedgeVertex> hyperedges,
            IEnumerable<RelationsPortsHyperedge> hyperedgeConnectors = null,
            IEnumerable<Pole> externalPoles = null) : base(entities, hyperedges, hyperedgeConnectors, externalPoles)
        {
            Vertices = entities == null ? new List<Vertex>() : new List<Vertex>(entities);
            IncompleteVertices = incompleteEntities == null ? new List<EntityVertex>() : new List<EntityVertex>(incompleteEntities);
            if (hyperedges != null)
                Vertices.AddRange(hyperedges);
            Edges = hyperedgeConnectors == null ? new List<Hyperedge>() : new List<Hyperedge>(hyperedgeConnectors);
            ExternalPoles = externalPoles == null ? new List<Pole>() : new List<Pole>(externalPoles);
        }
    }
}

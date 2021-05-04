using DSM_Graph_Layer.HPGraphModel.GraphClasses;
using System.Collections.Generic;
using System.Linq;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses.Transformations
{
    public class ModelForTransformation : Model
    {
        public ModelForTransformation(IEnumerable<EntityVertex> entities,
            IEnumerable<HyperedgeVertex> hyperedges,
            IEnumerable<Pole> externalPoles = null)
        {
            Vertices = entities == null ? new List<Vertex>() : new List<Vertex>(entities);
            if (hyperedges != null)
                Vertices.AddRange(hyperedges);
            Edges = hyperedges != null ? hyperedges.Select(x => x.CorrespondingHyperedge as Hyperedge).ToList() : new List<Hyperedge>();
            ExternalPoles = externalPoles == null ? new List<Pole>() : new List<Pole>(externalPoles);
        }
    }
}

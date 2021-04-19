using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.IsomorphicSubgraphMatching
{
    public class IsomorphicPoleFinder : IIsomorphicElementFinder<Pole>
    {
        public Dictionary<Pole, Pole> CoreSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Dictionary<Pole, Pole> CoreTarget { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Dictionary<Pole, long> ConnSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Dictionary<Pole, long> ConnTarget { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool CheckFisibiltyRules(Pole source, Pole target)
        {
            throw new NotImplementedException();
        }

        public List<(Pole, Pole)> GetAllCandidatePairs()
        {
            throw new NotImplementedException();
        }

        public void RestoreVectors()
        {
            throw new NotImplementedException();
        }

        public void UpdateVectors(Pole source, Pole target)
        {
            throw new NotImplementedException();
        }
    }
}

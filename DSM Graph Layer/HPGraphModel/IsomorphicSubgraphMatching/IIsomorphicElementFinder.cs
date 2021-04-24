using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.IsomorphicSubgraphMatching
{
    public interface IIsomorphicElementFinder<T>
    {
        public Dictionary<T, T> CoreSource { get; set; }
        public Dictionary<T, T> CoreTarget { get; set; }
        public Dictionary<T, long> ConnSource { get; set; }
        public Dictionary<T, long> ConnTarget { get; set; }

        //Dictionary<T, T> Recurse();
        void UpdateVectors(long step, T source, T target);
        void RestoreVectors(long step, T source, T target);
        List<(T, T)> GetAllCandidatePairs();
        bool CheckFisibiltyRules(T source, T target);
    }
}

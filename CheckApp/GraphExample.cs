using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;

namespace CheckApp
{
    public class GraphExample : BidirectionalGraph<DataVertex, DataEdge>
    {
        private static readonly Random Rand = new Random(Guid.NewGuid().GetHashCode());

        public void AddEdge(DataVertex source, DataVertex target, int? sourcePoint = null, int? targetPoint = null, int weight = 0)
        {
            var edge = new DataEdge(source, target, weight)
            {
                ID = Rand.Next(),
                SourceConnectionPointId = sourcePoint,
                TargetConnectionPointId = targetPoint,
            };

            AddEdge(edge);
        }
    }
}

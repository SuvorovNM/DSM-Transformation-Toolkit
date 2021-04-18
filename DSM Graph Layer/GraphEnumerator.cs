using System;
using System.Collections.Generic;
using System.Text;
using DSM_Graph_Layer.HPGraphModel;

namespace DSM_Graph_Layer
{
    public static class GraphEnumerator
    {
        public static long currentGraphId = 0;
        public static long currentStructureId = 0;
        public static long currentPoleId = 0;
        public static long currentLinkId = 0;
        public static void SetNextId(HPGraph hpGraph)
        {
            currentGraphId++;
            hpGraph.Id = currentGraphId;
        }

        public static void SetNextId(Structure structure)
        {
            currentStructureId++;
            structure.Id = currentStructureId;
        }

        public static void SetNextId(Pole pole)
        {
            currentPoleId++;
            pole.Id = currentPoleId;
        }

        public static void SetNextId(Link link)
        {
            currentLinkId++;
            link.Id = currentLinkId;
        }
    }
}

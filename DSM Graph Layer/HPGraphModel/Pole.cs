using DSM_Graph_Layer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel
{
    public class Pole : ElementWithId, ICloneable
    {
        private PoleType Type { get; set; }
        public Vertex VertexOwner { get; set; }
        public HPGraph GraphOwner { get; set; }
        public List<Hyperedge> EdgeOwners { get; set; }

        public Pole(PoleType type = PoleType.Both)
        {
            GraphEnumerator.SetNextId(this);
            Type = type;
            EdgeOwners = new List<Hyperedge>();
        }

        public bool CanBeInput()
        {
            return Type == PoleType.Both || Type == PoleType.Input;
        }

        public bool CanBeOutput()
        {
            return Type == PoleType.Both || Type == PoleType.Output;
        }

        public bool CanBeBoth()
        {
            return Type == PoleType.Both;
        }

        public void ChangeTypeTo(PoleType type)
        {
            Type = type;
        }

        public object Clone()
        {
            var pole = new Pole(this.Type);
            pole.VertexOwner = this.VertexOwner;
            pole.GraphOwner = this.GraphOwner;
            return pole;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel
{
    public class Role
    {
        public Role(string name)
        {
            Name = name;
            OppositeRole = this;
        }
        public Role(Role r, string name = "")
        {
            if (name == "")
                Name = r.Name;
            else
                Name = name;
            OppositeRole = r;
            r.OppositeRole = this;
        }

        public string Name { get; set; }
        public Role OppositeRole { get; set; }
    }
}

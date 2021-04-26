using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.Interfaces
{
    interface ILabeledElement
    {
        public string Label { get; set; }
        public void SetLabel(string label);
    }
}

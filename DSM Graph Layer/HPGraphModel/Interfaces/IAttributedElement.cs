using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.Interfaces
{
    interface IAttributedElement
    {
        List<ElementAttribute> Attributes { get; set; }
    }
}

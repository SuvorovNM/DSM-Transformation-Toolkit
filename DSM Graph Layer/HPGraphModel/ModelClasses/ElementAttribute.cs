using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    public class ElementAttribute // TODO: Определить, нужен ли здесь IMetamodelingElement<T>
    {
        public string DataType { get; set; }
        public string DataValue { get; set; }
    }
}

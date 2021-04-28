using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.Interfaces
{
    interface IMetamodelingElement<T>
    {
        public T BaseElement { get; set; }
        public List<T> Instances { get; set; }
        public void SetBaseElement(T baseElement);
        public T Instantiate(string label);
        public void DeleteInstance(T instance);
    }
}

using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.Interfaces
{
    /// <summary>
    /// Интерфейс атрибутированного элемента
    /// </summary>
    interface IAttributedElement
    {
        /// <summary>
        /// Атрибуты элемента
        /// </summary>
        List<ElementAttribute> Attributes { get; set; }
    }
}

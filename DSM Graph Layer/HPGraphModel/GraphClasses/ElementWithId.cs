using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.GraphClasses
{
    public abstract class ElementWithId
    {
        /// <summary>
        /// Уникальный идентификатор объекта
        /// </summary>
        public long Id { get; set; }
    }
}

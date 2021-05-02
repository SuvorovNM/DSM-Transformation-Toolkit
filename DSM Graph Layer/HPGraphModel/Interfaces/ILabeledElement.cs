using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.Interfaces
{
    /// <summary>
    /// Интерфейс помеченного элемента
    /// </summary>
    interface ILabeledElement
    {
        /// <summary>
        /// Метка элемента
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// Установить метку
        /// </summary>
        /// <param name="label">Новая метка</param>
        public void SetLabel(string label);
    }
}

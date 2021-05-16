using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    [Serializable]
    /// <summary>
    /// Атрибут объекта - пока просто в строковых значениях.
    /// При создании экземпляра объекта с атрибутом, значение исходного объекта становится типом экземпляра, а значение становится пустым
    /// </summary>
    public class ElementAttribute
    {
        /// <summary>
        /// Тип объекта
        /// </summary>
        public string DataType { get; set; }
        /// <summary>
        /// Значение объекта
        /// </summary>
        public string DataValue { get; set; }

        public ElementAttribute(string key, string val)
        {
            DataType = key;
            DataValue = val;
        }
    }
}

using DSM_Graph_Layer.HPGraphModel.GraphClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    /// <summary>
    /// Правило трансформации
    /// </summary>
    public class TransformationRule
    {
        /// <summary>
        /// Инициализация правила трансформации
        /// </summary>
        /// <param name="leftPart">Левая часть правила (паттерн)</param>
        /// <param name="rightPart">Правая часть правила (замена)</param>
        /// <param name="ruleName">Наименование правила</param>
        public TransformationRule(
             ModelForTransformation leftPart,
             ModelForTransformation rightPart,
            string ruleName)
        {
            LeftPart = leftPart;
            RightPart = rightPart;
            RuleName = ruleName;
        }
        /// <summary>
        /// Левая часть правила
        /// </summary>
        public ModelForTransformation LeftPart { get; set; }
        /// <summary>
        /// Правая часть правила
        /// </summary>
        public ModelForTransformation RightPart { get; set; }
        /// <summary>
        /// Наименование правила
        /// </summary>
        public string RuleName { get; set; }
    }
}

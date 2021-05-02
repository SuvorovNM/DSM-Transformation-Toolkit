using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.Interfaces
{
    /// <summary>
    /// Интерфейс объекта метамоделирования
    /// </summary>
    /// <typeparam name="T">Тип объекта</typeparam>
    interface IMetamodelingElement<T>
    {
        /// <summary>
        /// Базовый элемент, на котором основан текущий элемент
        /// </summary>
        public T BaseElement { get; set; }
        /// <summary>
        /// Экземпляры текущего элемента в построенных моделях
        /// </summary>
        public List<T> Instances { get; set; }
        /// <summary>
        /// Установить базовый элемент для объекта
        /// </summary>
        /// <param name="baseElement">Базовый элемент</param>
        public void SetBaseElement(T baseElement);
        /// <summary>
        /// Создать экземпляр текущего объекта
        /// </summary>
        /// <param name="label">Наименование (метка) получаемого объекта</param>
        /// <returns></returns>
        public T Instantiate(string label);
        /// <summary>
        /// Удаление ссылки на экземпляр объекта
        /// </summary>
        /// <param name="instance">Экземпляр объекта</param>
        public void DeleteInstance(T instance);
    }
}

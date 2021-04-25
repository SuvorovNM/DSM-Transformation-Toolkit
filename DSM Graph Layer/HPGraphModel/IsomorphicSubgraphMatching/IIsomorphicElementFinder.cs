using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.IsomorphicSubgraphMatching
{
    /// <summary>
    /// Интерфейс для поиска изоморфного элемента
    /// </summary>
    /// <typeparam name="T">Тип элементов</typeparam>
    public abstract class IIsomorphicElementFinder<T>
    {
        /// <summary>
        /// Основной словарь соответствий (Элемент исходного графа, Элемент графа-паттерна)
        /// </summary>
        public Dictionary<T, T> CoreSource { get; set; }
        /// <summary>
        /// Основной словарь соответствий (Элемент графа-паттерна, Элемент исходного графа)
        /// </summary>
        public Dictionary<T, T> CoreTarget { get; set; }
        /// <summary>
        /// Вспомогательный словарь соответствий (Элемент исходного графа, Шаг, на котором был добавлен в словарь)
        /// </summary>
        public Dictionary<T, long> ConnSource { get; set; }
        /// <summary>
        /// Вспомогательный словарь соответствий (Элемент графа-паттерна, Шаг, на котором был добавлен в словарь)
        /// </summary>
        public Dictionary<T, long> ConnTarget { get; set; }

        /// <summary>
        /// Обновить словари-векторы для перехода на следующий уровень
        /// </summary>
        /// <param name="step">Текущий шаг</param>
        /// <param name="source">Добавляемая вершина исходного графа</param>
        /// <param name="target">Добавляемая вершина графа-паттерна</param>
        protected abstract void UpdateVectors(long step, T source, T target);
        /// <summary>
        /// Восстановить векторы при возврате на предыдущий уровень
        /// </summary>
        /// <param name="step">Шаг</param>
        /// <param name="source">Вершина исходного графа</param>
        /// <param name="target">Вершина графа-паттерна</param>
        protected abstract void RestoreVectors(long step, T source, T target);
        /// <summary>
        /// Получить все возможные кандидаты пар элементов исходного и целевого графа
        /// </summary>
        /// <returns>Список пар</returns>
        protected abstract List<(T, T)> GetAllCandidatePairs();
        /// <summary>
        /// Осуществить проверку на корректность при добавлении элемента
        /// </summary>
        /// <param name="source">Вершина исходного графа</param>
        /// <param name="target">Вершина графа-паттерна</param>
        /// <returns>Результат проверки - True, если проверка успешно пройдена</returns>
        protected abstract bool CheckFisibiltyRules(T source, T target);
    }
}

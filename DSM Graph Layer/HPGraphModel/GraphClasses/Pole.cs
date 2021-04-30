using DSM_Graph_Layer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.GraphClasses
{
    /// <summary>
    /// Полюс HP-графа
    /// </summary>
    public class Pole : ElementWithId, ICloneable
    {
        /// <summary>
        /// Тип полюса - источник/приемник/оба
        /// </summary>
        public PoleType Type { get; set; }
        /// <summary>
        /// Вершина, которой принадлежит полюс (null, если внешний полюс)
        /// </summary>
        public Vertex VertexOwner { get; set; }
        /// <summary>
        /// HP-граф, которому принадлежит полюс (null, если внутренний полюс)
        /// </summary>
        public HPGraph GraphOwner { get; set; }
        /// <summary>
        /// Гиперребра, которые содержат полюс
        /// </summary>
        public List<Hyperedge> EdgeOwners { get; set; }

        /// <summary>
        /// Инициализировать полюс
        /// </summary>
        /// <param name="type">Тип полюса</param>
        public Pole(PoleType type = PoleType.Both)
        {
            GraphEnumerator.SetNextId(this);
            Type = type;
            EdgeOwners = new List<Hyperedge>();
        }

        /// <summary>
        /// Создать дубликат полюса (для неполных вершин при трансформации)
        /// </summary>
        /// <param name="p">Полюс</param>
        public Pole(Pole p)
        {
            Id = p.Id;
            Type = p.Type;
            EdgeOwners = new List<Hyperedge>();
        }

        /// <summary>
        /// Может ли полюс быть приемником
        /// </summary>
        /// <returns>True, если может</returns>
        public bool CanBeInput()
        {
            return Type == PoleType.Both || Type == PoleType.Input;
        }

        /// <summary>
        /// Может ли полюс быть источником
        /// </summary>
        /// <returns>True, если может</returns>
        public bool CanBeOutput()
        {
            return Type == PoleType.Both || Type == PoleType.Output;
        }

        /// <summary>
        /// Может ли полюс быть одновременно и источником, и приемником
        /// </summary>
        /// <returns>True, если может</returns>
        public bool CanBeBoth()
        {
            return Type == PoleType.Both;
        }

        /// <summary>
        /// Переопределить тип полюса
        /// </summary>
        /// <param name="type">Новый тип</param>
        public void ChangeTypeTo(PoleType type)
        {
            Type = type;
        }

        /// <summary>
        /// Клонировать полюс
        /// </summary>
        /// <returns>Копия полюса</returns>
        public virtual object Clone()
        {
            var pole = new Pole(Type);
            pole.VertexOwner = VertexOwner;
            pole.GraphOwner = GraphOwner;
            return pole;
        }
    }
}

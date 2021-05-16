using DSM_Graph_Layer.HPGraphModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    [Serializable]
    /// <summary>
    /// Роль - устанавливается для портов (список ролей, которые они могут принимать) и отношений (роль, соответствующая отношению)
    /// </summary>
    public class Role : ILabeledElement
    {
        /// <summary>
        /// Инициализация роли с заданным именем.
        /// В соответствие роли ставится противоположная роль, которая ссылается на текущую
        /// </summary>
        /// <param name="name">Наименование роли</param>
        public Role(string name)
        {
            Label = name;
            OppositeRole = this;
        }
        /// <summary>
        /// Создание роли, противоположной данной
        /// </summary>
        /// <param name="r">Исходная роль</param>
        /// <param name="name">Наименование противоположной роли</param>
        public Role(Role r, string name = "")
        {
            if (name == "")
                Label = r.Label;
            else
                Label = name;
            OppositeRole = r;
            r.OppositeRole = this;
        }

        public string Label { get; set; }
        /// <summary>
        /// Противоположная (парная) роль
        /// </summary>
        public Role OppositeRole { get; set; }

        public void SetLabel(string label)
        {
            Label = label;
        }
    }
}

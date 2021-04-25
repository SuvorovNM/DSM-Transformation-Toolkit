using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.Enums
{
    /// <summary>
    /// Тип полюса: 0 - приемник, 1 - источник, 2 - и приемник, и источник
    /// </summary>
    public enum PoleType
    {
        Input = 0,
        Output = 1,
        Both = 2
    }
}

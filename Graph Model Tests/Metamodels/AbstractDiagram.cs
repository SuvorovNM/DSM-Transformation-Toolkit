using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Graph_Model_Tests.Metamodels
{
    public abstract class AbstractDiagram
    {
        public AbstractDiagram()
        {
            Metamodel = CreateDiagram();
        }
        public Model Metamodel { get; protected set; }
        protected abstract Model CreateDiagram();
    }
}

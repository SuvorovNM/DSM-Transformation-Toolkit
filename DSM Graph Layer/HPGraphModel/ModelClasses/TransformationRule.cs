using DSM_Graph_Layer.HPGraphModel.GraphClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    public class TransformationRule
    {
        public TransformationRule(
            Model leftPart, 
            Model rightPart, 
            string ruleName,
            Dictionary<Pole, Pole> poleCorrespondence = null)
        {
            LeftPart = leftPart;
            RightPart = rightPart;
            RuleName = ruleName;
            CorrespondingPoles = poleCorrespondence ?? new Dictionary<Pole, Pole>();
        }
        public Model LeftPart { get; set; }
        public Model RightPart { get; set; }
        public string RuleName { get; set; }
        public Dictionary<Pole,Pole> CorrespondingPoles { get; set; }
    }
}

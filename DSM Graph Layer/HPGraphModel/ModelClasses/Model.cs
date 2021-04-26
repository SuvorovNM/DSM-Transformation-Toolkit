using DSM_Graph_Layer.HPGraphModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSM_Graph_Layer.HPGraphModel.ModelClasses
{
    class Model : HPGraph, ILabeledElement, IAttributedElement
    {
        public Model(string label = "", Model metamodel = null)
        {
            Label = label;
            Metamodel = metamodel;
            Roles = new List<Role>();            
            Attributes = new List<ElementAttribute>();
            ModelInstances = new List<Model>();
        }
        public Model(Model parentGraph, string label = "", List<Pole> externalPoles = null) : base(parentGraph, externalPoles)
        {
            Roles = parentGraph.Roles;
            Label = label;
            ModelInstances = new List<Model>();
        }

        public List<Role> Roles { get; }
        public string Label { get; set; }
        public List<ElementAttribute> Attributes { get; set; }
        public Model Metamodel { get; set; }
        public List<Model> ModelInstances { get; set; }
        public List<EntityVertex> Entities 
        { 
            get
            {
                return Vertices.Where(x => x as EntityVertex != null).Select(x => x as EntityVertex).ToList();
            } 
        }
        public List<HyperedgeVertex> Hyperedges
        {
            get
            {
                return Vertices.Where(x => x as HyperedgeVertex != null).Select(x => x as HyperedgeVertex).ToList();
            }
        }

        /// <summary>
        /// Добавить новую пару ролей к графу
        /// </summary>
        /// <param name="r">Добавляемая роль</param>
        public void AddNewRoleToGraph(Role r) // TODO: возможно не стоит сразу добавлять Opposite
        {
            if (!Roles.Contains(r))
                Roles.Add(r);
            if (r != r?.OppositeRole && !Roles.Contains(r?.OppositeRole))
                Roles.Add(r.OppositeRole);
        }

        /// <summary>
        /// Добавить новую роль к графу
        /// </summary>
        /// <param name="name">Наименование роли</param>
        /// <returns>Добавленная роль</returns>
        public Role AddNewRoleToGraph(string name)
        {
            var r = new Role(name);
            Roles.Add(r);

            return r;
        }

        public void SetLabel(string label)
        {
            Label = label;
        }

        public void AddNewEntityVertex(EntityVertex entity)
        {
            throw new NotImplementedException();
        }
        public void RemoveEntityVertex(EntityVertex entity)
        {
            throw new NotImplementedException();
        }

        public void AddNewHyperedgeVertex(HyperedgeVertex hyperedge)
        {
            throw new NotImplementedException();
        }
        public void RemoveHyperedgeVertex(HyperedgeVertex hyperedge)
        {
            throw new NotImplementedException();
        }
    }
}

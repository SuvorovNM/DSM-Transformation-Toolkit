using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using GraphX.Common.Enums;
using GraphX.Logic.Models;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CheckApp
{
    /// <summary>
    /// Логика взаимодействия для ModelGraph.xaml
    /// </summary>
    public partial class ModelGraph : UserControl
    {
        private Model Model { get; set; }
        public ModelGraph(Model model)
        {
            InitializeComponent();
            Model = model;
            DataContext = this;
            Loaded += ControlLoaded;
        }

        void ControlLoaded(object sender, RoutedEventArgs e)
        {
            graphArea.GenerateGraph(Model);
            zoomControl.ZoomToFill();
        }
    }
}

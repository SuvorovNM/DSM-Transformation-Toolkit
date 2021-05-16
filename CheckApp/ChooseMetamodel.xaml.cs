using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для ChooseMetamodel.xaml
    /// </summary>
    public partial class ChooseMetamodel : Window
    {
        public ChooseMetamodel(List<Model> targetMetamodels)
        {
            InitializeComponent();
            foreach(var metamodel in targetMetamodels)
            {
                MetamodelComboBox.Items.Add(new ComboBoxItem { Tag = metamodel, Content = metamodel.Label });
            }
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public Model ChosenModel
        {
            get
            {
                return (MetamodelComboBox.SelectedItem as ListBoxItem)?.Tag as Model;
            }
        }
    }
}

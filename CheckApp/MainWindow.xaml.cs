using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using Graph_Model_Tests.Metamodels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CheckApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Model> models;
        public MainWindow()
        {
            InitializeComponent();
            var erDiagram = new EntityRelationDiagram();
            var classDiagram = new ClassDiagram();

            models = new List<Model>() { erDiagram.Metamodel, classDiagram.Metamodel, erDiagram.GetSampleModel()};

            foreach(var model in models.Where(x => x.BaseElement == null))
            {
                MetamodelListBox.Items.Add(new ListBoxItem {Tag = model, Content = model.Label });
            }
        }

        private void MetamodelListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosenItem = (MetamodelListBox.SelectedItem as ListBoxItem).Tag as Model;
            FillVerticesInfo(chosenItem);

            ModelListBox.Items.Clear();
            foreach (var model in chosenItem.Instances)
            {
                ModelListBox.Items.Add(new ListBoxItem { Tag = model, Content = model.Label });
            }
        }

        private void ModelListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                var chosenItem = (ModelListBox.SelectedItem as ListBoxItem).Tag as Model;
                FillVerticesInfo(chosenItem);
            }
        }

        private void FillVerticesInfo(Model chosenItem)
        {
            EntityListBox.Items.Clear();
            foreach (var entity in chosenItem.Entities)
            {
                EntityListBox.Items.Add(new ListBoxItem { Tag = entity, Content = entity.Label });
            }
            HyperedgeListBox.Items.Clear();
            foreach (var hyperedge in chosenItem.Hyperedges)
            {
                HyperedgeListBox.Items.Add(new ListBoxItem { Tag = hyperedge, Content = hyperedge.Label });
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Window window = new Window
            {
                Title = "My User Control Dialog",
                Content = new ModelGraph((ModelListBox.SelectedItem as ListBoxItem)?.Tag as Model ?? (MetamodelListBox.SelectedItem as ListBoxItem)?.Tag as Model)
            };

            window.ShowDialog();
        }
    }
}

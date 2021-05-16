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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Microsoft.Win32;

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
            erDiagram.AddTransformationsToTargetMetamodel(classDiagram.Metamodel);

            models = new List<Model>() { erDiagram.Metamodel, classDiagram.Metamodel, erDiagram.GetSampleModel()};

            foreach(var model in models.Where(x => x.BaseElement == null))
            {
                MetamodelListBox.Items.Add(new ListBoxItem {Tag = model, Content = model.Label });
            }
        }

        private void MetamodelListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ModelListBox.Items.Clear();
            if (e.AddedItems.Count != 0)
            {
                var chosenItem = (MetamodelListBox.SelectedItem as ListBoxItem).Tag as Model;
                FillVerticesInfo(chosenItem);

                ModelListBox.Items.Clear();
                foreach (var model in chosenItem.Instances)
                {
                    ModelListBox.Items.Add(new ListBoxItem { Tag = model, Content = model.Label });
                }
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
            var chosenModel = (ModelListBox.SelectedItem as ListBoxItem)?.Tag as Model ?? (MetamodelListBox.SelectedItem as ListBoxItem)?.Tag as Model;

            if (chosenModel != null)
            {
                Window window = new Window
                {
                    Title = "Model View",
                    Content = new ModelGraph(chosenModel)
                };

                window.ShowDialog();
            }
        }

        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var chosenMetamodel = (MetamodelListBox.SelectedItem as ListBoxItem)?.Tag as Model;
            if (chosenMetamodel != null)
            {
                var sfd = new SaveFileDialog();
                sfd.Filter = "Model files (*.hpmodel) | *.hpmodel";
                if (sfd.ShowDialog() == true)
                {
                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = sfd.OpenFile();
                    formatter.Serialize(stream, chosenMetamodel);
                    stream.Close();
                }
            }
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Model files (*.hpmodel) | *.hpmodel";
            if (ofd.ShowDialog() == true)
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = ofd.OpenFile();
                var resultMetamodel = (Model)formatter.Deserialize(stream);
                stream.Close();

                MetamodelListBox.Items.Add(new ListBoxItem { Tag = resultMetamodel, Content = resultMetamodel.Label });
            }            
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Model chosenModel = null;

            if (ModelListBox.SelectedItem as ListBoxItem != null)
            {
                chosenModel = (ModelListBox.SelectedItem as ListBoxItem)?.Tag as Model;
                ModelListBox.Items.RemoveAt(MetamodelListBox.SelectedIndex);                
            }
            else if (MetamodelListBox.SelectedItem as ListBoxItem != null)
            {
                chosenModel = (MetamodelListBox.SelectedItem as ListBoxItem)?.Tag as Model;
                MetamodelListBox.Items.RemoveAt(MetamodelListBox.SelectedIndex);
            }
            if (chosenModel?.BaseElement != null)
            {
                chosenModel.BaseElement.Instances.Remove(chosenModel);
            }
        }

        private void MetamodelListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ModelListBox.SelectedIndex = -1;
        }

        private void ChangeNameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ModelListBox.SelectedItem as ListBoxItem != null || MetamodelListBox.SelectedItem as ListBoxItem != null)
            {
                Model chosenModel = null;
                ChangeName changeNameWindow = new ChangeName();
                if (changeNameWindow.ShowDialog() == true)
                {
                    if (ModelListBox.SelectedItem as ListBoxItem != null)
                    {
                        chosenModel = (ModelListBox.SelectedItem as ListBoxItem)?.Tag as Model;
                        chosenModel.Label = changeNameWindow.ModelName;
                        (ModelListBox.SelectedItem as ListBoxItem).Content = chosenModel.Label;
                    }
                    else if (MetamodelListBox.SelectedItem as ListBoxItem != null)
                    {
                        chosenModel = (MetamodelListBox.SelectedItem as ListBoxItem)?.Tag as Model;
                        chosenModel.Label = changeNameWindow.ModelName;
                        (MetamodelListBox.SelectedItem as ListBoxItem).Content = chosenModel.Label;
                    }
                }
            }
        }

        private void ViewRulesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MetamodelListBox.SelectedItem as ListBoxItem != null && ((MetamodelListBox.SelectedItem as ListBoxItem)?.Tag as Model).Transformations.Any())
            {
                var availableMetamodels = ((MetamodelListBox.SelectedItem as ListBoxItem)?.Tag as Model).Transformations.Keys.ToList();
                ChooseMetamodel chooseMetamodelWindow = new ChooseMetamodel(availableMetamodels);
                if (chooseMetamodelWindow.ShowDialog() == true)
                {
                    var chosenMetamodel = chooseMetamodelWindow.ChosenModel;
                    var wind = new TestTransform(((MetamodelListBox.SelectedItem as ListBoxItem)?.Tag as Model).Transformations[chosenMetamodel]);
                    wind.ShowDialog();
                }
            }
        }
    }
}

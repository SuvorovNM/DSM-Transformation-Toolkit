using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using Graph_Model_Tests.Metamodels;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Microsoft.Win32;

namespace CheckApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Открытые метамодели
        /// </summary>
        List<Model> Metamodels { get; set; }

        /// <summary>
        /// Инициализация демонстрационных моделей, метамоделей и правил трансформации
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            var erDiagram = new EntityRelationDiagram();
            var classDiagram = new ClassDiagram();
            var ebbDiagram = new EntityBasedBusDiagram();
            var hbbDiagram = new HyperedgeBasedBusDiagram();

            erDiagram.GetSampleModel();
            erDiagram.AddTransformationsToTargetMetamodel(classDiagram.Metamodel);
            ebbDiagram.GetSampleModel();
            ebbDiagram.AddTransformationsToTargetModel(hbbDiagram.Metamodel);
            hbbDiagram.GetSampleModel();
            hbbDiagram.AddTransformationsToTargetModel(ebbDiagram.Metamodel);

            Metamodels = new List<Model>() { erDiagram.Metamodel, classDiagram.Metamodel, ebbDiagram.Metamodel, hbbDiagram.Metamodel };

            foreach (var model in Metamodels.Where(x => x.BaseElement == null))
            {
                MetamodelListBox.Items.Add(new ListBoxItem { Tag = model, Content = model.Label });
            }
        }

        /// <summary>
        /// Изменение выбора элемента в списке метамоделей - также осуществляется обновление списка моделей и списков сущностей и гиперребер
        /// </summary>
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
            else
            {
                EntityListBox.Items.Clear();
                HyperedgeListBox.Items.Clear();
            }
        }

        /// <summary>
        /// Изменение элемента в списке моделей - также осуществляется обновление списка моделей и списков сущностей и гиперребер
        /// </summary>
        private void ModelListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                var chosenItem = (ModelListBox.SelectedItem as ListBoxItem).Tag as Model;
                FillVerticesInfo(chosenItem);
            }
            else
            {
                var chosenItem = (MetamodelListBox.SelectedItem as ListBoxItem).Tag as Model;
                FillVerticesInfo(chosenItem);
            }
        }

        /// <summary>
        /// Заполнить списки сущностей и гиперребер по выбранной модели
        /// </summary>
        /// <param name="chosenItem">Выбранная модель</param>
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

        /// <summary>
        /// Открыть окно просмотра модели/метамодели
        /// </summary>
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

                window.Owner = this;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.ShowDialog();
            }
        }

        /// <summary>
        /// Сохранить выбранную метамодель в качестве бинарного файла
        /// </summary>
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

        /// <summary>
        /// Открыть бинарный файл метамодели и добавить полученную метамодель в список метамоделей
        /// </summary>
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
                Metamodels.Add(resultMetamodel);

                foreach (var metamodel in Metamodels.Where(x => x.Transformations.Any(y => y.Key.Id == resultMetamodel.Id)))
                {
                    metamodel.RedefineTransformation(resultMetamodel);
                }
            }
        }

        /// <summary>
        /// Удалить модель/метамодель из списка
        /// </summary>
        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Model chosenModel = null;

            if (ModelListBox.SelectedItem as ListBoxItem != null)
            {
                chosenModel = (ModelListBox.SelectedItem as ListBoxItem)?.Tag as Model;
                ModelListBox.Items.RemoveAt(ModelListBox.SelectedIndex);
                ModelListBox.SelectedIndex = -1;
            }
            else if (MetamodelListBox.SelectedItem as ListBoxItem != null)
            {
                chosenModel = (MetamodelListBox.SelectedItem as ListBoxItem)?.Tag as Model;
                Metamodels.Remove(chosenModel);
                MetamodelListBox.Items.RemoveAt(MetamodelListBox.SelectedIndex);
                MetamodelListBox.SelectedIndex = -1;
            }
            if (chosenModel?.BaseElement != null)
            {
                chosenModel.BaseElement.Instances.Remove(chosenModel);
            }
        }

        /// <summary>
        /// Сброс выбора модели при щелчке по списку метамоделей
        /// </summary>
        private void MetamodelListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ModelListBox.SelectedIndex = -1;
        }

        /// <summary>
        /// Изменение наименования модели/метамодели - для выбора имени открывается диалоговое окно
        /// </summary>
        private void ChangeNameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ModelListBox.SelectedItem as ListBoxItem != null || MetamodelListBox.SelectedItem as ListBoxItem != null)
            {
                ChangeName changeNameWindow = new ChangeName();
                if (changeNameWindow.ShowDialog() == true)
                {
                    Model chosenModel;
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

        /// <summary>
        /// Посмотреть правила трансформации для выбранной метамодели:
        /// Для этого сначала открывается диалоговое окно для выбора целевой метамодели, а затем открывается окно с правилами преобразования
        /// </summary>
        private void ViewRulesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MetamodelListBox.SelectedItem as ListBoxItem != null && ((MetamodelListBox.SelectedItem as ListBoxItem)?.Tag as Model).Transformations.Keys.Intersect(Metamodels).Any())
            {
                var availableMetamodels = ((MetamodelListBox.SelectedItem as ListBoxItem)?.Tag as Model).Transformations.Keys.Intersect(Metamodels).ToList();
                ChooseMetamodel chooseMetamodelWindow = new ChooseMetamodel(availableMetamodels);
                chooseMetamodelWindow.Owner = this;
                if (chooseMetamodelWindow.ShowDialog() == true)
                {
                    var chosenMetamodel = chooseMetamodelWindow.ChosenModel;
                    if (chosenMetamodel != null)
                    {
                        var wind = new TestTransform(((MetamodelListBox.SelectedItem as ListBoxItem)?.Tag as Model).Transformations[chosenMetamodel]);
                        wind.Owner = this;
                        wind.ShowDialog();
                    }
                }
            }
        }

        /// <summary>
        /// Выполнить горизонтальную трансформацию выбранной модели в экземпляр целевой метамодели:
        /// Для этого сначала открывается диалоговое окно для выбора возможной целевой метамодели, а затем открывается окно с созданной моделью
        /// </summary>
        private void ExecuteTransformationTo_Click(object sender, RoutedEventArgs e)
        {
            if (ModelListBox.SelectedItem as ListBoxItem != null && ((ModelListBox.SelectedItem as ListBoxItem)?.Tag as Model).BaseElement.Transformations.Keys.Intersect(Metamodels).Any())
            {
                var availableMetamodels = ((ModelListBox.SelectedItem as ListBoxItem)?.Tag as Model).BaseElement.Transformations.Keys.Intersect(Metamodels).ToList();
                ChooseMetamodel chooseMetamodelWindow = new ChooseMetamodel(availableMetamodels);
                chooseMetamodelWindow.Owner = this;
                if (chooseMetamodelWindow.ShowDialog() == true)
                {
                    var chosenMetamodel = chooseMetamodelWindow.ChosenModel;
                    var result = ((ModelListBox.SelectedItem as ListBoxItem)?.Tag as Model).ExecuteTransformations(chosenMetamodel);

                    if (result != null)
                    {
                        Window window = new Window
                        {
                            Title = "Model View",
                            Content = new ModelGraph(result)
                        };
                        window.Owner = this;
                        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                        window.ShowDialog();
                    }
                }
            }
        }        
    }
}

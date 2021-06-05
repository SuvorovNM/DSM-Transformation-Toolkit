using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using System.Windows;
using System.Windows.Controls;

namespace CheckApp
{
    /// <summary>
    /// Логика взаимодействия для ModelGraph.xaml
    /// </summary>
    public partial class ModelGraph : UserControl
    {
        /// <summary>
        /// Демонстрируемая модель
        /// </summary>
        private Model Model { get; set; }

        /// <summary>
        /// Инициализация и загрузка визуального отображения графа модели
        /// </summary>
        /// <param name="model">Демонстрируемая модель</param>
        public ModelGraph(Model model)
        {
            InitializeComponent();
            Model = model;
            DataContext = this;
            Loaded += ControlLoaded;            
        }

        /// <summary>
        /// Загрузка визуального отображения графа модели и установка zoomControl
        /// </summary>
        void ControlLoaded(object sender, RoutedEventArgs e)
        {
            graphArea.GenerateGraph(Model);
            zoomControl.ZoomToFill();
        }
    }
}

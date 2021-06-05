using DSM_Graph_Layer.HPGraphModel.ModelClasses.Transformations;
using System.Collections.Generic;
using System.Windows;

namespace CheckApp
{
    /// <summary>
    /// Логика взаимодействия для TestTransform.xaml
    /// </summary>
    public partial class TestTransform : Window
    {
        /// <summary>
        /// Список правил трансформации
        /// </summary>
        private List<TransformationRule> Rules { get; set; }

        /// <summary>
        /// Текущий индекс
        /// </summary>
        private int CurrentIndex { get; set; }

        /// <summary>
        /// Отображаемый пользователю номер правила
        /// </summary>
        private int VisibleRuleNumber
        {
            get
            {
                return CurrentIndex + 1;
            }
        }

        /// <summary>
        /// Инициализация правил и индекса
        /// </summary>
        /// <param name="rules">Правила трансформации, установленные для метамодели</param>
        public TestTransform(List<TransformationRule> rules)
        {
            InitializeComponent();
            Rules = rules;
            CurrentIndex = 0;
            DataContext = this;
            NumberLabel.Content = VisibleRuleNumber;
        }

        /// <summary>
        /// Нажатие на кнопку "Вверх" и переключение на предыдущее правило
        /// </summary>
        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentIndex > 0)
            {
                CurrentIndex--;
                FillRules();
            }
            ValidateButtons();
            NumberLabel.Content = VisibleRuleNumber;
        }

        /// <summary>
        /// Нажатие на кнопку "Вниз" и переключение на следующее правило
        /// </summary>
        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentIndex < Rules.Count - 1)
            {
                CurrentIndex++;
                FillRules();
            }
            ValidateButtons();
            NumberLabel.Content = VisibleRuleNumber;
        }

        /// <summary>
        /// Загрузка окна - загрузка визуального отображения правил и валидация кнопок
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillRules();
            ValidateButtons();
        }

        /// <summary>
        /// Загрузка визуального представления левой и правой части текущего правила
        /// </summary>
        void FillRules()
        {
            graphArea.GenerateGraph(Rules[CurrentIndex].LeftPart, false);
            SetupZoom(zoomControl);

            graphArea1.GenerateGraph(Rules[CurrentIndex].RightPart, false);
            SetupZoom(zoomControl1);
        }

        /// <summary>
        /// Скрытие ViewFinder и установка ZoomControl
        /// </summary>
        /// <param name="zoomcontrol">Элемент, к которому применяются изменения</param>
        private void SetupZoom(GraphX.Controls.ZoomControl zoomcontrol)
        {
            zoomcontrol.ZoomToFill();
            if (zoomcontrol.ViewFinder != null)
                zoomcontrol.ViewFinder.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Валидация кнопок переключения между правилами
        /// </summary>
        private void ValidateButtons()
        {
            if (CurrentIndex == 0)
                UpButton.IsEnabled = false;
            else
                UpButton.IsEnabled = true;

            if (CurrentIndex == Rules.Count - 1)
                DownButton.IsEnabled = false;
            else
                DownButton.IsEnabled = true;
        }
    }
}

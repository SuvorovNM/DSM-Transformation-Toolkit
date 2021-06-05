using System.Windows;

namespace CheckApp
{
    /// <summary>
    /// Логика взаимодействия для ChangeName.xaml
    /// </summary>
    public partial class ChangeName : Window
    {
        /// <summary>
        /// Инициализация
        /// </summary>
        public ChangeName()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Нажатие кнопки "ОК" - диалоговое окно закрывается с результатом True
        /// </summary>
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        /// <summary>
        /// Получение наименования модели
        /// </summary>
        public string ModelName
        {
            get { return NameBox.Text; }
        }
    }
}

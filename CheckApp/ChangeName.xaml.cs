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

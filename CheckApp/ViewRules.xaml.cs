using DSM_Graph_Layer.HPGraphModel.ModelClasses;
using DSM_Graph_Layer.HPGraphModel.ModelClasses.Transformations;
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
    /// Логика взаимодействия для TestTransform.xaml
    /// </summary>
    public partial class TestTransform : Window
    {
        private List<TransformationRule> Rules { get; set; }
        private int currentIndex;
        private int VisibleRuleNumber
        {
            get
            {
                return currentIndex + 1;
            }
        }
        public TestTransform(List<TransformationRule> rules)
        {
            InitializeComponent();
            Rules = rules;
            currentIndex = 0;
            DataContext = this;
            NumberLabel.Content = VisibleRuleNumber;
        }

        void FillRules()
        {
            graphArea.GenerateGraph(Rules[currentIndex].LeftPart, false);
            SetupZoom(zoomControl);

            graphArea1.GenerateGraph(Rules[currentIndex].RightPart, false);
            SetupZoom(zoomControl1);
        }

        private void SetupZoom(GraphX.Controls.ZoomControl zoomcontrol)
        {
            zoomcontrol.ZoomToFill();
            if (zoomcontrol.ViewFinder != null)
                zoomcontrol.ViewFinder.Visibility = Visibility.Collapsed;
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                FillRules();
            }
            ValidateButtons();
            NumberLabel.Content = VisibleRuleNumber;
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex < Rules.Count - 1)
            {
                currentIndex++;
                FillRules();
            }
            ValidateButtons();
            NumberLabel.Content = VisibleRuleNumber;
        }

        private void ValidateButtons()
        {
            if (currentIndex == 0)
                UpButton.IsEnabled = false;
            else
                UpButton.IsEnabled = true;

            if (currentIndex == Rules.Count - 1)
                DownButton.IsEnabled = false;
            else
                DownButton.IsEnabled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillRules();
            ValidateButtons();
        }
    }
}

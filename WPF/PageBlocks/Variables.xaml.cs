using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;

namespace WPF
{
    public partial class Variables : Page
    {
        public static int CountVariable = 0;
        public static List<Border> ListVariables = new List<Border>();
        
        public Variables()
        {
            InitializeComponent();
            // ChangeAndSetValue.Visibility = Visibility.Hidden;
        }

        private void CreateVariable_OnMouseEnter(object sender, MouseEventArgs e)
        {
            BorderCreateVarialb.Background = Brushes.Gainsboro;
            Cursor = Cursors.Hand;
        }

        private void CreateVariable_OnMouseLeave(object sender, MouseEventArgs e)
        {
            BorderCreateVarialb.Background = Brushes.WhiteSmoke;
            Cursor = Cursors.Arrow;
        }
        private void CreateVariable_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CreateVariableDialogWindow dialogWindow = new CreateVariableDialogWindow();
            dialogWindow.CreateVariableOK.MouseLeftButtonDown += (o, args) =>
            {
                if (string.IsNullOrEmpty(dialogWindow.NameVariable.Text) ||
                    string.IsNullOrEmpty(dialogWindow.TypeVariable.Text))
                {
                    dialogWindow.Close();
                    MessageBox.Show("Variable not created", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    CreateVariableDialogWindow.Type = dialogWindow.TypeVariable.Text;
                    CreateVariableDialogWindow.Name = dialogWindow.NameVariable.Text;

                    MainWindow.Code += $"{dialogWindow.TypeVariable.Text} {dialogWindow.NameVariable.Text};";
                    dialogWindow.Close();
                    MessageBox.Show("Variable created", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogWindowOnClosed();
                }
            };
            dialogWindow.Visibility = Visibility.Visible;
        }
        private void DialogWindowOnClosed()
        {
            CountVariable++;
            var variable = CreateBorder();
            ListVariables.Add(variable);
            
            if (CountVariable != 0)
                ChangeAndSetValue.Visibility = Visibility.Visible;

            Panel.Children.Insert(CountVariable, variable);

            Panel.InvalidateVisual();
            Panel.UpdateLayout();
        }
        private void EllipseOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border ellipse = CreateBorder();
            ellipse.MouseLeftButtonDown -= EllipseOnMouseLeftButtonDown;
            Script.Move.Get(ref ellipse);
            MainWindow.WindowCode.Children.Add(ellipse);
        }
        private Border CreateBorder()
        {
            Border ellipse = new Border()
            {
                Name = CreateVariableDialogWindow.Name,
                Height = 25,
                Width = double.NaN,
                Background = Brushes.Fuchsia,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(10, 10, 0, 0),
                CornerRadius = new CornerRadius(15),
                AllowDrop = true,
            };
            ellipse.MouseLeftButtonDown += EllipseOnMouseLeftButtonDown;
            TextBlock textBlock = new TextBlock()
            {
                Text = CreateVariableDialogWindow.Name,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.White,
                Padding = new Thickness(10),
            };
            ellipse.Child = textBlock;
            return ellipse;
        }
        private void SetValue_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UIElement elementToClone = (UIElement)sender;
            UIElement clonedElement = CloneElement(elementToClone);
            
            MainWindow.WindowCode.InvalidateVisual();
            MainWindow.WindowCode.UpdateLayout();
            
            Script.Move.Get(ref clonedElement);
            MainWindow.WindowCode.Children.Add(clonedElement);
        }
        private UIElement CloneElement(UIElement element)
        {
            // Создаем объект для сериализации XAML
            var xaml = XamlWriter.Save(element);

            // Создаем поток для чтения XAML
            var reader = new StringReader(xaml);

            // Загружаем XAML в объект
            var xmlReader = XmlReader.Create(reader);
            var clonedElement = (UIElement)XamlReader.Load(xmlReader);
            
            return clonedElement;
        }
        private async void ChangeBorder_OnMouseMove(object sender, MouseEventArgs e)
        {
            await Task.Run(() =>
            {
                foreach (var border in ListVariables)
                {
                    Point borderPosition = border.TransformToAncestor(MainWindow.WindowCode).Transform(new Point(0, 0));
                    Point mousePosition = e.GetPosition(MainWindow.WindowCode);

                    if (mousePosition.X >= borderPosition.X &&
                        mousePosition.X <= borderPosition.X + border.ActualWidth &&
                        mousePosition.Y >= borderPosition.Y &&
                        mousePosition.Y <= borderPosition.Y + border.ActualHeight)
                    {
                        
                    }
                }
            });
        }

    }
}
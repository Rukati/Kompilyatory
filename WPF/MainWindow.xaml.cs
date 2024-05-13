using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WPF
{
    public partial class MainWindow
    {
        public static string Code;
        public static Canvas WindowCode;

        public MainWindow()
        {
            InitializeComponent();
            PageWithBlocks.Navigate(new Uri("BlocksPage.xaml", UriKind.Relative));
            PageWithBlocks.Width = 0;
            WindowCode = BlockCode;
        }

        Stopwatch stopwatch = new Stopwatch();

        private void Run_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            stopwatch.Start();

            string projectDirectory = Directory.GetCurrentDirectory();
            string fileName = "RukaroCode.txt";
            string filePath = Path.Combine(projectDirectory, fileName);

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Записываем текст в файл
                writer.WriteLine($"def Main begin {Code} end.");
            }

            Process process = new Process();
            process.StartInfo.FileName = Path.Combine(projectDirectory, "Kompilyatory.exe");
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = fileName;

            process.Start();

            StringBuilder result = new StringBuilder();
            stopwatch.Stop();
            result.Append($"Build completed in {stopwatch.Elapsed}\n");
            result.Append(process.StandardOutput.ReadToEnd());
            process.WaitForExit();
            TextResult.Text = result.ToString();
        }

        private void RUN_OnMouseEnter(object sender, MouseEventArgs e)
        {
            _RUN.BorderBrush = Brushes.CornflowerBlue;
            _RUNTextBlock.Foreground = Brushes.CornflowerBlue;
            Cursor = Cursors.Hand;
        }

        private void RUN_OnMouseLeave(object sender, MouseEventArgs e)
        {
            _RUN.BorderBrush = Brushes.Black;
            _RUNTextBlock.Foreground = Brushes.Black;
            Cursor = Cursors.Arrow;
        }

        private void CloseBlock_OnMouseLeave(object sender, MouseEventArgs e)
        {
            CloseBlock.BorderBrush = Brushes.Black;
            XBlock.Foreground = Brushes.Brown;
            Cursor = Cursors.Arrow;
        }

        private void CloseBlock_OnMouseEnter(object sender, MouseEventArgs e)
        {
            CloseBlock.BorderBrush = Brushes.CornflowerBlue;
            XBlock.Foreground = Brushes.CornflowerBlue;
            Cursor = Cursors.Hand;
        }

        private void CloseBlock_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void BLocks_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
            if (!openBlocks)
            {
                BorderCode.BorderBrush = Brushes.CornflowerBlue;
                Block.Foreground = Brushes.CornflowerBlue;
            }
        }

        private void BLocks_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
            if (!openBlocks)
            {
                BorderCode.BorderBrush = Brushes.Black;
                Block.Foreground = Brushes.Black;
            }
        }

        private bool openBlocks = false;

        private void BLocks_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DoubleAnimation animation;
            ThicknessAnimation marginAnimation;

            if (!openBlocks)
            {
                animation = new DoubleAnimation
                {
                    From = 0,
                    To = 200,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5))
                };
                marginAnimation = new ThicknessAnimation
                {
                    From = new Thickness(-200, 0, 0, 0),
                    To = new Thickness(0),
                    Duration = new Duration(TimeSpan.FromSeconds(0.5))
                };

                BorderCode.BorderBrush = Brushes.CornflowerBlue;
                Block.Foreground = Brushes.CornflowerBlue;
            }
            else
            {
                animation = new DoubleAnimation
                {
                    From = 200,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5))
                };
                marginAnimation = new ThicknessAnimation
                {
                    From = new Thickness(0),
                    To = new Thickness(-200, 0, 0, 0),
                    Duration = new Duration(TimeSpan.FromSeconds(0.5))
                };
            }

            PageWithBlocks.BeginAnimation(WidthProperty, animation);
            PageWithBlocks.BeginAnimation(MarginProperty, marginAnimation);

            openBlocks = !openBlocks;
        }


        private void Bar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DoubleAnimation ResultAnimation;
            DoubleAnimation CodeAnimation;
            DoubleAnimation GridCodeAnimation;

            if (GridCodeResult.Height == 100)
            {
                ResultAnimation = new DoubleAnimation
                {
                    From = 100,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5))
                };
                GridCodeAnimation = new DoubleAnimation
                {
                    From = 400,
                    To = 500,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5))
                };
                CodeAnimation = new DoubleAnimation
                {
                    From = 350,
                    To = 450,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5))
                };

                CodeAnimation.Completed += (o, args) => TextBlock_SizeCodeResult.Text = "\u25b2";
            }
            else
            {
                ResultAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 100,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5))
                };
                GridCodeAnimation = new DoubleAnimation
                {
                    From = 500,
                    To = 400,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5))
                };
                CodeAnimation = new DoubleAnimation
                {
                    From = 450,
                    To = 350,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5))
                };
                CodeAnimation.Completed += (o, args) => TextBlock_SizeCodeResult.Text = "\u25bc";
            }

            // Запуск анимации изменения размеров элементов
            GridCodeResult.BeginAnimation(HeightProperty, ResultAnimation);
            GridCodeUser.BeginAnimation(HeightProperty, GridCodeAnimation);
            BlockCode.BeginAnimation(HeightProperty, CodeAnimation);

            int targetY = 305;
            foreach (UIElement element in BlockCode.Children)
            {
                double top = Canvas.GetTop(element);
                if (top > targetY)
                {
                    Canvas.SetTop(element, 305);
                }
            }
        }

        private void TextBlock_SizeCodeResult_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
            ((TextBlock)sender).Foreground = Brushes.Black;
        }

        private void TextBlock_SizeCodeResult_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
            ((TextBlock)sender).Foreground = Brushes.DimGray;
        }
    }
}
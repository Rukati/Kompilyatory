using System.Windows.Input;
using System;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            TextCode.Text = "def Main\nbegin\n\twriteln(\"Hello world!\");\nend.";
            Line.X1 = Width;
        }
        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Снимаем фокус с клавиатуры
            Keyboard.ClearFocus();
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
                writer.Write(TextCode.Text);
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
        private void TextCode_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                // Отменяем обработку нажатия клавиши Tab по умолчанию
                e.Handled = true;

                // Добавляем символ табуляции в текстовое поле
                int caretIndex = TextCode.CaretIndex;
                TextCode.Text = TextCode.Text.Insert(caretIndex, "\t");

                // Перемещаем каретку вправо после вставки табуляции
                TextCode.CaretIndex = caretIndex + 1;
            }
            else if (e.Key == Key.OemQuotes)
            {
                e.Handled = true;

                // Добавляем символ кавычки в текстовое поле
                int caretIndex = TextCode.CaretIndex;
                TextCode.Text = TextCode.Text.Insert(caretIndex, "\"");
                TextCode.Text = TextCode.Text.Insert(caretIndex, "\"");

                // Перемещаем каретку вправо после вставки кавычки
                TextCode.CaretIndex = caretIndex + 1;
            }
            else if (e.Key == Key.D9)
            {
                bool shiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

                if (shiftPressed)
                {
                    e.Handled = true;

                    // Добавляем символ кавычки в текстовое поле
                    int caretIndex = TextCode.CaretIndex;
                    TextCode.Text = TextCode.Text.Insert(caretIndex, ")");
                    TextCode.Text = TextCode.Text.Insert(caretIndex, "(");

                    // Перемещаем каретку вправо после вставки кавычки
                    TextCode.CaretIndex = caretIndex + 1;
                }
            }
        }
    }
}
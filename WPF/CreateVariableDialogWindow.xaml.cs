using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPF
{
    public partial class CreateVariableDialogWindow : Window
    {
        public static string Type;
        public static string Name;
        
        public CreateVariableDialogWindow()
        {
            InitializeComponent();
        }

        private void EnterCreateVariable_OnMouseLeave(object sender, MouseEventArgs e)
        {
            CreateVariableOK.Background = Brushes.LightGreen;
            Cursor = Cursors.Arrow;
        }
        
        private void EnterCreateVariable_OnMouseEnter(object sender, MouseEventArgs e)
        {
            CreateVariableOK.Background = Brushes.Chartreuse;
            Cursor = Cursors.Hand;
        }
    }
}
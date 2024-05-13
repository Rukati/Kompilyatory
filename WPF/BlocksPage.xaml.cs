using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace WPF
{
    public partial class BlocksPage : Page
    {
        private static string _activeBlock;
        private readonly Dictionary<string, Page> _dictBlockPage;

        public BlocksPage()
        {
            InitializeComponent();
            _dictBlockPage = new Dictionary<string, Page>()
            {
                { "Control", new PageBlocks.Control() },
                { "Cycles", new PageBlocks.Cycles() },
                { "Function", new PageBlocks.Function() },
                { "Math", new PageBlocks.Math() },  
                { "Notation", new PageBlocks.Notation() },
                { "Variables", new Variables() } 
            };

        }

        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).Foreground = Brushes.CornflowerBlue;
            Cursor = Cursors.Hand;
        }
        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).Foreground = Brushes.DimGray;
            Cursor = Cursors.Arrow;
        }
        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement uiElement)
            {
                _activeBlock = ((TextBlock)sender).Name;
                PageWithBlocks.Content = _dictBlockPage[_activeBlock];
                
                var parentGrid = FindParentGrid(uiElement);

                if (parentGrid != null)
                {
                    var borderElement = parentGrid.FindName("SelectTypeBlocks") as Border;

                    if (borderElement != null)
                    {
                        var row = Grid.GetRow(uiElement);
                        var column = Grid.GetColumn(uiElement);
                
                        // Удаление Border из его текущего родительского элемента
                        if (borderElement.Parent is Panel panel)
                        {
                            panel.Children.Remove(borderElement);
                        }

                        // Добавление Border в новый родительский Grid и установка его Row и Column
                        Grid.SetRow(borderElement, row);
                        Grid.SetColumn(borderElement, column);
                        
                        parentGrid.Children.Insert(0, borderElement);
                    }
                }
            }
        }
        private Grid FindParentGrid(UIElement element)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(element);

            while (parent != null && !(parent is Grid))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as Grid;
        }
    }
}
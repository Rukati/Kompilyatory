using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPF.Script
{
    public class Move
    {
        // Перемещаемый элемент
        private static FrameworkElement moveObject;

        // Начальная позиция элемента
        private static double beginTop;
        private static double beginLeft;

        // Допустимый диапазон перемещения мыши
        private static double mouseMinX;
        private static double mouseMinY;
        private static double mouseMaxX;
        private static double mouseMaxY;

        // Родительский холст
        private static Canvas canvas;

        // Начальная позиция мыши в холсте
        private static double mouseBeginX;
        private static double mouseBeginY;

        private static void MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is FrameworkElement frm))
                return;

            moveObject = frm;
            if (!(moveObject.Parent is Canvas cnv))
                return;

            canvas = cnv;

            // Начальная позиция элемента
            beginTop = Canvas.GetTop(moveObject);
            beginLeft = Canvas.GetLeft(moveObject);

            // Допустимые границы перемещения мыши
            mouseMaxX = canvas.Width - moveObject.ActualWidth;
            mouseMaxY = canvas.Height - moveObject.ActualHeight;

            // Начальная позиция мыши в холсте
            Point point = e.GetPosition(moveObject);
            mouseMaxX += mouseMinX = point.X;
            mouseMaxY += mouseMinY = point.Y;

            point = e.GetPosition(canvas);
            mouseBeginX = point.X;
            mouseBeginY = point.Y;

            moveObject.CaptureMouse();
            moveObject.MouseMove += MouseMove;
            moveObject.MouseUp += MouseUp;
            moveObject.MouseLeave += MouseLeave;
        }

        private static void MouseMove(object sender, MouseEventArgs e)
        {
            if (!(sender is FrameworkElement moveObject))
                return;

            Point point = e.GetPosition(canvas);
            double x = point.X;
            double y = point.Y;
            if (x < mouseMinX)
                x = mouseMinX;
            else if (x > mouseMaxX)
                x = mouseMaxX;
            if (y < mouseMinY)
                y = mouseMinY;
            else if (y > mouseMaxY)
                y = mouseMaxY;

            double top = beginTop + y - mouseBeginY;
            double left = beginLeft + x - mouseBeginX;

            Canvas.SetLeft(moveObject, left);
            Canvas.SetTop(moveObject, top);
        }

        private static void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (moveObject != null)
            {
                moveObject.ReleaseMouseCapture();
                moveObject.MouseMove -= MouseMove;
                moveObject.MouseUp -= MouseUp;
                moveObject.MouseLeave -= MouseLeave;
            }
        }

        private static void MouseLeave(object sender, MouseEventArgs e)
        {
            if (moveObject != null)
            {
                moveObject.ReleaseMouseCapture();
                moveObject.MouseMove -= MouseMove;
                moveObject.MouseUp -= MouseUp;
                moveObject.MouseLeave -= MouseLeave;
            }
        }

        public static void Get<T>(ref T element) where T : UIElement
        { 
            element.MouseMove += MouseMove;
            element.MouseUp += MouseUp;
            element.MouseDown += MouseDown;
            element.MouseLeave += MouseLeave;
        }
    }
}

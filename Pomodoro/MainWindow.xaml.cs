using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pomodoro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _mouseDown;
        private bool _minimized;
        private readonly int _maxValue = 100;
        private readonly int _minValue = 0;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            Left = desktopWorkingArea.Right - Width - 10;
            Top = desktopWorkingArea.Bottom - Height - 10;
        }

        private void TitleGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleMinimize();
        }
        private void TaskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            ToggleMinimize();
        }
        private void ToggleMinimize()
        {
            if (_minimized)
            {
                Show();
                _minimized = false;
            }
            else
            {
                Hide();
                _minimized = true;
            }
        }

        private void TimeDial_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _mouseDown = true;
        }

        private void TimeDial_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _mouseDown = false;
        }

        private void TimeDial_MouseLeave(object sender, MouseEventArgs e)
        {
            _mouseDown = false;
        }

        private void TimeDial_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseDown)
            {
                var pos = e.GetPosition(sender as Grid);

                var centerX = this.MainPomoWindow.ActualWidth / 2;
                var centerY = this.MainPomoWindow.ActualHeight / 2;
                double angle = GetAngle(pos, centerX, centerY);

                var rotation = (360 - 0) * angle / (2 * Math.PI);

                DialRotation.Angle = rotation;

                var time = (_maxValue - _minValue) * angle / (2 * Math.PI);
                TimeTextBox.Text = ((int)time).ToString();
            }
        }

        private double GetAngle(Point pos, double x, double y)
        {
            //Calculate out the distance(r) between the center and the position
            Point center = new Point(x, y);
            double xDiff = center.X - pos.X;
            double yDiff = center.Y - pos.Y;
            double r = Math.Sqrt(xDiff * xDiff + yDiff * yDiff);

            //Calculate the angle
            double angle = Math.Acos((center.Y - pos.Y) / r);
            
            if (pos.X < x)
                angle = 2 * Math.PI - angle;
            if (Double.IsNaN(angle))
                return 0.0;
            else
                return angle;
        }

        private void TimeText_KeyUp(object sender, KeyEventArgs e)
        {
            var time = (sender as TextBox).Text;
            time = time.Length > 0 ? time : "0";
            double.TryParse(time, out double timeVal);

            if (e.Key == Key.Up)
            { 
                timeVal++;
                TimeTextBox.Text = (timeVal % 100).ToString();
            }

            if (e.Key == Key.Down)
            {
                timeVal--;
                if (timeVal == -1)
                    timeVal = 99;
                TimeTextBox.Text = (timeVal % 100).ToString();
            }


            var angle = (timeVal * 360) / (_maxValue - _minValue);

            DialRotation.Angle = angle;
        }
    }
}

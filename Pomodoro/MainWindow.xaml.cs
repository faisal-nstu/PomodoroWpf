using System;
using System.Diagnostics;
using System.Timers;
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
        private readonly int _maxValue = 30;
        private readonly int _minValue = 0;
        private static Timer _timer;
        private int _totalSeconds;
        private int _count;

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


        #region MOUSE EVENT HANDLERS
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
            if (!_mouseDown)
            {
                return;
            }

            var pos = e.GetPosition(sender as Grid);

            SetDialFromMousePosition(pos);
        }
        #endregion

        private void SetDialFromMousePosition(Point pos)
        {
            var centerX = this.MainPomoWindow.ActualWidth / 2;
            var centerY = this.MainPomoWindow.ActualHeight / 2;
            double angle = GetAngleFromMouse(pos, centerX, centerY);

            var limit = (_maxValue - _minValue) * angle / (2 * Math.PI);

            var timeSpan = GetTimeSpan(limit);

            RotateDial((int)timeSpan.TotalSeconds);

            SetTimer(timeSpan);
        }



        private void SetTimer(TimeSpan timeSpan)
        {
            _totalSeconds = (int)timeSpan.TotalSeconds;
            if (_timer != null && _timer.Enabled)
            {
                _count = 0;
                _timer.Stop();
            }
            _timer = new Timer();
            _timer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
            _timer.Interval = 1000;
            _timer.Start();
        }

        public void OnTimerElapsed(object source, ElapsedEventArgs e)
        {            
            RotateDial(_totalSeconds);
            _count++;

            if (_totalSeconds < _count)
            {
                _count = 0;
                _timer.Stop();
                this.Dispatcher.Invoke(() =>
                {
                    TimeTextBlock.Text = "DONE!";
                    System.Media.SystemSounds.Exclamation.Play();
                    MainPomoWindow.Show();
                    MainPomoWindow.Activate();
                });
            }
        }

        private void RotateDial(int totalSeconds)
        {
            double timeVal = (double)(totalSeconds - _count) / 60;

            var angle = (timeVal * 360) / (_maxValue - _minValue);

            this.Dispatcher.Invoke(() =>
            {
                SetDialText(totalSeconds);
                DialRotation.Angle = angle;
            });
        }

        private void SetDialText(int totalSeconds)
        {
            var remainingSeconds = totalSeconds - _count;
            var timeSpan = TimeSpan.FromSeconds(remainingSeconds);
            TimeTextBlock.Text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }

        private TimeSpan GetTimeSpan(double limit)
        {
            var minutes = (int)limit;
            var secondsInDecimal = limit - minutes;
            var seconds = (int)(secondsInDecimal * 60);
            var timeSpan = new TimeSpan(0, minutes, seconds);
            return timeSpan;
        }

        private double GetAngleFromMouse(Point pos, double x, double y)
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
    }
}

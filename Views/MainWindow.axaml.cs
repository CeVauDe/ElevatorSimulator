using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace ElevatorSimulator.Views;

public partial class MainWindow : Window
{
    private DispatcherTimer _timer;
    private double _startX, _startY, _endX, _endY, _t;
    private const double Duration = 1.0; // seconds
    private const double Interval = 0.016; // ~60 FPS
    
    public MainWindow()
    {
        _timer = new DispatcherTimer();
        InitializeComponent();
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        MoveCircle(200, 150);
    }

    private void MoveCircle(double xOffset, double yOffset)
    {
        var x = MyCircle.GetValue(Canvas.LeftProperty);
        var y = MyCircle.GetValue(Canvas.TopProperty);
        StartLerp(x, y, x + xOffset, y + yOffset);
    }
    
    public void StartLerp(double fromX, double fromY, double toX, double toY)
    {
        _startX = fromX;
        _startY = fromY;
        _endX = toX;
        _endY = toY;
        _t = 0;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(Interval)
        };
        _timer.Tick += OnLerpTick;
        _timer.Start();
    }

    private void OnLerpTick(object sender, EventArgs e)
    {
        _t += Interval / Duration;
        if (_t >= 1)
        {
            _t = 1;
            _timer.Stop();
        }

        double newX = Lerp(_startX, _endX, _t);
        double newY = Lerp(_startY, _endY, _t);
        
        MoveCircleTo(newX, newY);
        MoveImageTo(newX, newY);
    }

    private double Lerp(double start, double end, double t) => start + (end - start) * t;

    private void MoveCircleTo(double x, double y)
    {
        MyCircle.SetValue(Canvas.LeftProperty, x);
        MyCircle.SetValue(Canvas.TopProperty, y);
    }
    
    private void MoveImageTo(double x, double y)
    {
        MyImage.SetValue(Canvas.LeftProperty, x);
        MyImage.SetValue(Canvas.TopProperty, y);
    }
}
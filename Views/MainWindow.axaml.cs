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
        MoveSymbol(200, 150);
    }

    private void MoveSymbol(double xOffset, double yOffset)
    {
        var x = MySymbol.GetValue(Canvas.LeftProperty);
        var y = MySymbol.GetValue(Canvas.TopProperty);
        StartLerp(x, y, x + xOffset, y + yOffset);
    }

    private void StartLerp(double fromX, double fromY, double toX, double toY)
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

        var newX = Lerp(_startX, _endX, _t);
        var newY = Lerp(_startY, _endY, _t);
        
        MoveSymbolTo(newX, newY);
    }

    private static double Lerp(double start, double end, double t) => start + (end - start) * t;

    private void MoveSymbolTo(double x, double y)
    {
        MySymbol.SetValue(Canvas.LeftProperty, x);
        MySymbol.SetValue(Canvas.TopProperty, y);
    }

}
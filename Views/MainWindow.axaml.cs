using System;
using System.Diagnostics;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Media;

namespace ElevatorSimulator.Views;

public partial class MainWindow : Window
{
    private DispatcherTimer _timer;
    private double _startX, _startY, _endX, _endY, _t;
    private const double Duration = 1.0; // seconds
    private const double Interval = 0.016; // ~60 FPS
    
    private Point[] spawn = new Point[]
    {
        new Point(200, 100),
        new Point(200, 200),
        new Point(200, 300),
        new Point(200, 400)
    };
    
    private Point[] despawn = new Point[]
    {
        new Point(400, 100),
        new Point(400, 200),
        new Point(400, 300),
        new Point(400, 400)
    };
    
    List<Ellipse> circles = new List<Ellipse>();
    
    
    public MainWindow()
    {
        _timer = new DispatcherTimer();
        InitializeComponent();
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        Random rnd = new Random();
        int etage = rnd.Next(0, 4);

        var circle = new Ellipse() { Width = 50, Height = 50, Fill = Brushes.MintCream };
        
        Canvas.SetLeft(circle, spawn[etage].X);
        Canvas.SetTop(circle, spawn[etage].Y);
        MySymbol.Children.Add(circle);
        circles.Add(circle);
        MoveSymbol(circle, despawn[etage]);
    }

    private void MoveSymbol(Ellipse circle, Point goal)
    {
        Point start = new Point (Canvas.GetLeft(circle), Canvas.GetTop(circle));

        StartLerp(circle, start, goal);
    }

    private void StartLerp(Ellipse circle, Point start, Point goal)
    {
        
        double t = 0;
        double duration = 1000.0;       // Dauer in Sekunden?
        double interval = 0.016;        // ~60 FPS

        _t = 0;

        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(Interval)
        };
        
        _timer.Tick += (spawn, e) =>
        {
            t += interval / duration;
            if (t >= 1)
            {
                t = 1;
                timer.Stop();

                MySymbol.Children.Remove(circle);
                circles.Remove(circle);
            }
            
            double newX = Lerp(start.X, goal.X, t);
            double newY = Lerp(start.Y, goal.Y, t);

            Canvas.SetLeft(circle, newX);
            Canvas.SetTop(circle, newY);
        };
        
        _timer.Start();
    }

    private static double Lerp(double start, double end, double t) => start + (end - start) * t;
    
}
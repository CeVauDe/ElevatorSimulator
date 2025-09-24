using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace ElevatorSimulator.Views;

public partial class MainWindow : Window
{
    
    private readonly List<CircleAnimation> animations = new();
    private readonly Stopwatch stopwatch = new();
    
    public MainWindow()
    {
        InitializeComponent();
        stopwatch.Start();
        this.RequestAnimationFrame(OnFrame);
    }

    
    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        Random rnd = new Random();
        int etage = rnd.Next(0, 4);
        double duration = 1.0;

        var spawn = new[]
        {
            new Point(200, 100),
            new Point(200, 200),
            new Point(200, 300),
            new Point(200, 400)
        };
        
        var despawn = new[]
        {
            new Point(400, 100),
            new Point(400, 200),
            new Point(400, 300),
            new Point(400, 400)
        };
        
        var circle = new Ellipse
        {
            Width = 50, 
            Height = 50, 
            Fill = Brushes.MintCream
        };
        
        Canvas.SetLeft(circle, spawn[etage].X);
        Canvas.SetTop(circle, spawn[etage].Y);
        MySymbol.Children.Add(circle);
        
        animations.Add(new CircleAnimation(circle, spawn[etage], despawn[etage], stopwatch.Elapsed.TotalSeconds, duration));
    }

    
    private void OnFrame(TimeSpan frameTime)
    {
        double now = stopwatch.Elapsed.TotalSeconds;
        var finished = new List<CircleAnimation>();

        foreach (var circle in animations)
        {
            double t = (now - circle.StartTime) / circle.Duration;
            if (t >= 1.0)
            {
                t = 1.0;
                finished.Add(circle);
            }

            double newX = Lerp(circle.Start.X, circle.End.X, t);
            double newY = Lerp(circle.Start.Y, circle.End.Y, t);

            Canvas.SetLeft(circle.Circle, newX);
            Canvas.SetTop(circle.Circle, newY);
        }

        foreach (var circle in finished)
        {
            MySymbol.Children.Remove(circle.Circle);
            animations.Remove(circle);
        }

        this.RequestAnimationFrame(OnFrame);
    }

    private static double Lerp(double start, double end, double t)
        => start + (end - start) * t;
}


public class CircleAnimation
{
    public Ellipse Circle { get; }
    public Point Start { get; }
    public Point End { get; }
    public double StartTime { get; }
    public double Duration { get; }

    public CircleAnimation(Ellipse circle, Point start, Point end, double startTime, double duration)
    {
        Circle = circle;
        Start = start;
        End = end;
        StartTime = startTime;
        Duration = duration;
    }
}
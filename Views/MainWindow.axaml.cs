using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace ElevatorSimulator.Views;

public partial class MainWindow : Window
{
    
    private readonly List<CircleAnimation> _animations = new();
    private readonly Stopwatch _stopwatch = new();

    private const int num_of_floors = 4;
    private const int num_of_queue_spaces = 3;
    private int floor_distance = 100;
    
    public bool[,] queue = new bool[num_of_floors, num_of_queue_spaces];

    public int elevator_speed = 50;
    
    public Point[] wait_points = new Point[]
    {
        new Point(200, 100),
        new Point(600, 100),
        new Point(600, 100),
        new Point(700, 100),
        new Point(700, 100),
        new Point(800, 100),
        new Point(800, 100),
        new Point(1000, 100),
        new Point(1000, 100),
        new Point(1000, 100),
        new Point(1000, 100),
        new Point(1200, 100),
        new Point(1200, 100),
        new Point(1600, 100),
        new Point(1600, 100)
    };
    
   
    public MainWindow()
    {
        InitializeComponent();
        _stopwatch.Start();
        this.RequestAnimationFrame(OnFrame);
        

        
        foreach (var point in wait_points)
        {        
            for (int i = 0; i < num_of_floors; i++)
            {            
                var circle_red = new Ellipse
                {
                    Width = 50, 
                    Height = 50, 
                    Fill = Brushes.Red
                };
                var Point_Floor = new Point(point.X, point.Y + i * floor_distance);

                Canvas.SetLeft(circle_red, Point_Floor.X);
                Canvas.SetTop(circle_red, Point_Floor.Y);
                MySymbol.Children.Add(circle_red);
            }
        }
    }

    
    private void Button_OnClick_Spawn(object sender, RoutedEventArgs e)
    {
        bool check_rnd = true;
        int floor_start = new int();
        int floor_goal = new int();
        
        while (check_rnd)
        {
            Random rnd1 = new Random();
            Random rnd2 = new Random();
            floor_start = rnd1.Next(0, num_of_floors);
            floor_goal = rnd2.Next(0, num_of_floors);
            
            Console.WriteLine(floor_start + " " + floor_goal);

            if (floor_start != floor_goal)
            {
                check_rnd = false;
            }
        }
        
        double speed = 500;
        
        var circle = new Ellipse
        {
            Width = 50, 
            Height = 50, 
            Fill = Brushes.MintCream
        };
        
        var start_point = new Point(wait_points[0].X, wait_points[0].Y + floor_start * floor_distance);
        
        Canvas.SetLeft(circle, start_point.X);
        Canvas.SetTop(circle, start_point.Y);
        MySymbol.Children.Add(circle);
        
        _animations.Add(new CircleAnimation(circle, floor_start, floor_goal,0, speed));
    }



    private void Button_OnClick_Move(object sender, RoutedEventArgs e)
    {
        double start = _stopwatch.Elapsed.TotalSeconds;

        foreach (var circle in _animations)
        {
            circle.circle_state++;
            circle.StartTime = start;
        }
    }

    
    private void OnFrame(TimeSpan frameTime)
    {
        double now = _stopwatch.Elapsed.TotalSeconds;
        var finished = new List<CircleAnimation>();

        foreach (var circle in _animations)
        {
            double duration = 0;
            
            if (circle.circle_state % 2 == 1)
            {
                if (circle.circle_state == 9)
                {
                    duration = (Math.Abs(circle.StartFloor - circle.EndFloor)) * floor_distance / elevator_speed;
                }
                else
                {
                    duration = (wait_points[circle.circle_state].X - wait_points[circle.circle_state - 1].X) / circle.Speed;
                }
                
                double t = (now - circle.StartTime)/duration;
                if (t >= 1.0)
                {
                    t = 1.0;
                    circle.circle_state++;
                }
            
                int floor_offset_start = circle.StartFloor * floor_distance;
                int floor_offset_goal = circle.EndFloor * floor_distance;

                var old_point = new Point();
                var new_point = new Point();
            
                if (circle.circle_state < 9)
                {
                    old_point = new Point(wait_points[circle.circle_state-1].X,wait_points[circle.circle_state-1].Y + floor_offset_start);
                    new_point = new Point(wait_points[circle.circle_state].X,wait_points[circle.circle_state].Y + floor_offset_start);
                }
                else if (circle.circle_state == 9)
                {
                    old_point = new Point(wait_points[circle.circle_state].X,wait_points[circle.circle_state].Y + floor_offset_start);
                    new_point = new Point(wait_points[circle.circle_state].X,wait_points[circle.circle_state].Y + floor_offset_goal);                 
                }
                else
                {
                    old_point = new Point(wait_points[circle.circle_state-1].X,wait_points[circle.circle_state-1].Y + floor_offset_goal);
                    new_point = new Point(wait_points[circle.circle_state].X,wait_points[circle.circle_state].Y + floor_offset_goal);   
                }

                double newX = Lerp(old_point.X, new_point.X, t);
                double newY = Lerp(old_point.Y, new_point.Y, t);

                Canvas.SetLeft(circle.Circle, newX);
                Canvas.SetTop(circle.Circle, newY);           
            }
        }

        foreach (var circle in _animations)
        {
            if (circle.circle_state == 14)
            {
                finished.Add(circle);
            }
        }


        foreach (var circle in finished)
        {
            MySymbol.Children.Remove(circle.Circle);
            _animations.Remove(circle);
        }

        this.RequestAnimationFrame(OnFrame);
    }

    private static double Lerp(double start, double end, double t)
        => start + (end - start) * t;
    
}







public class CircleAnimation
{
    public Ellipse Circle { get; }
    public int StartFloor { get; }
    public int EndFloor { get; }
    public double Speed { get; }
    public int circle_state { get; set; }
    public double StartTime { get; set; }

    public CircleAnimation(Ellipse circle, int floor_start, int floor_goal, int state, double speed)
    {
        Circle = circle;
        StartFloor = floor_start;
        EndFloor = floor_goal;
        Speed = speed;
        circle_state = state;
    }
}
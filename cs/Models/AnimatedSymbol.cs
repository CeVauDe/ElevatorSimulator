using System;

namespace ElevatorSimulator.Models;

public class AnimatedSymbol
{
    private bool _isAnimating;
    
    public double X { get; set; }
    public double Y { get; set; }
    
    public bool IsAnimating 
    { 
        get => _isAnimating;
        set
        {
            if (_isAnimating == value) return;
            _isAnimating = value;
            AnimatingChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    
    public event EventHandler<PositionChangedEventArgs>? PositionChanged;
    public event EventHandler? AnimatingChanged;
    
    public void MoveTo(double newX, double newY)
    {
        X = newX;
        Y = newY;
        PositionChanged?.Invoke(this, new PositionChangedEventArgs(X, Y));
    }
    
    public void MoveBy(double deltaX, double deltaY)
    {
        MoveTo(X + deltaX, Y + deltaY);
    }
}

public class PositionChangedEventArgs : EventArgs
{
    public double X { get; }
    public double Y { get; }
    
    public PositionChangedEventArgs(double x, double y)
    {
        X = x;
        Y = y;
    }
}

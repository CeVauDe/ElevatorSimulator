using System;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace ElevatorSimulator.Models;

public class AnimationService
{
    private const double DefaultDuration = 1.0; // seconds
    private const double DefaultInterval = 0.016; // ~60 FPS
    
    public static async Task AnimateAsync(AnimatedSymbol symbol, double fromX, double fromY, double toX, double toY, double duration = DefaultDuration)
    {
        symbol.IsAnimating = true;
        
        try
        {
            var startTime = DateTime.Now;
            var endTime = startTime.AddSeconds(duration);
            
            while (DateTime.Now < endTime)
            {
                var elapsed = (DateTime.Now - startTime).TotalSeconds;
                var t = Math.Min(elapsed / duration, 1.0);
                
                var currentX = Lerp(fromX, toX, t);
                var currentY = Lerp(fromY, toY, t);
                
                // Update on UI thread
                await Dispatcher.UIThread.InvokeAsync(() => symbol.MoveTo(currentX, currentY));
                
                if (t >= 1.0) break;
                
                await Task.Delay(TimeSpan.FromSeconds(DefaultInterval));
            }
            
            // Ensure final position is exact
            await Dispatcher.UIThread.InvokeAsync(() => symbol.MoveTo(toX, toY));
        }
        finally
        {
            // Ensure the animation flag is reset even if an exception occurs
            symbol.IsAnimating = false;
        }
    }
    
    private static double Lerp(double start, double end, double t) => start + (end - start) * t;
}

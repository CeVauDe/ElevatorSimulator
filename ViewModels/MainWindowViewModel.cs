using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElevatorSimulator.Models;

namespace ElevatorSimulator.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly AnimatedSymbol _symbol;
    private readonly AnimationService _animationService;
    
    [ObservableProperty]
    private double _symbolX = 100;
    
    [ObservableProperty]
    private double _symbolY = 100;
    
    [ObservableProperty]
    private bool _isAnimating;
    
    public string Greeting { get; } = "Welcome to Elevator Simulator!";
    
    public MainWindowViewModel()
    {
        _symbol = new AnimatedSymbol { X = SymbolX, Y = SymbolY };
        _animationService = new AnimationService();
        
        _symbol.PositionChanged += OnSymbolPositionChanged;
        _symbol.AnimatingChanged += OnAnimatingChanged;
    }
    
    [RelayCommand]
    private async Task MoveSymbolAsync()
    {
        if (IsAnimating) return;
        
        IsAnimating = true; // Set this explicitly when starting animation
        await AnimationService.AnimateAsync(_symbol, _symbol.X, _symbol.Y, _symbol.X + 200, _symbol.Y + 150);
    }
    
    [RelayCommand]
    private async Task SpawnSymbolAsync()
    {
        if (IsAnimating) return;
        
        IsAnimating = true; // Set this explicitly when starting animation
        await AnimationService.AnimateAsync(_symbol, _symbol.X, _symbol.Y, _symbol.X + 200, _symbol.Y + 150);
    }
    
    private void OnSymbolPositionChanged(object? sender, PositionChangedEventArgs e)
    {
        SymbolX = e.X;
        SymbolY = e.Y;
    }
    
    private void OnAnimatingChanged(object? sender, System.EventArgs e)
    {
        if (sender is AnimatedSymbol symbol)
        {
            IsAnimating = symbol.IsAnimating;
        }
    }
}
# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is an Avalonia-based .NET 9.0 WPF application called "ElevatorSimulator" that currently demonstrates animated symbol movement (DVD logo bouncing). The project follows MVVM pattern using CommunityToolkit.Mvvm.

## Architecture

- **Framework**: Avalonia UI with .NET 9.0
- **Pattern**: MVVM using CommunityToolkit.Mvvm source generators
- **Structure**:
  - `Models/`: Core business logic and services
    - `AnimatedSymbol.cs`: Symbol entity with position tracking and events
    - `AnimationService.cs`: Static animation service with linear interpolation
  - `ViewModels/`: MVVM view models using ObservableProperty and RelayCommand attributes
    - `MainWindowViewModel.cs`: Main window logic with symbol animation controls
  - `Views/`: AXAML UI definitions and code-behind
    - `MainWindow.axaml`: Canvas-based UI with symbol rendering
  - `Assets/`: Application resources (icons, images)

## Development Commands

### Build and Run
```bash
# Build the solution
dotnet build

# Build in Release mode
dotnet build -c Release

# Run the application
dotnet run

# Clean build outputs
dotnet clean
```

### Testing
```bash
# Run all tests (if test project is configured)
dotnet test

# Build and run tests in one command
dotnet test --build
```

## Key Technologies

- **Avalonia 11.3.6**: Cross-platform .NET UI framework
- **CommunityToolkit.Mvvm 8.2.1**: MVVM helpers with source generators
- **Canvas-based rendering**: Using Avalonia Canvas for 2D positioning
- **Async animations**: Task-based animation system with Dispatcher.UIThread

## Code Patterns

- Use `[ObservableProperty]` for bindable properties
- Use `[RelayCommand]` for commands, with Async suffix for async operations
- Animation state management through `IsAnimating` property
- Event-driven position updates using custom EventArgs
- UI thread marshaling for animations using `Dispatcher.UIThread.InvokeAsync`
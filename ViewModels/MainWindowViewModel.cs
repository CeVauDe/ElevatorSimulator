using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElevatorSimulator.Models;

namespace ElevatorSimulator.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private const int TotalFloors = 4;
    private const int TickIntervalMs = 50;
    private const double FloorHeight = 100;
    private const double ElevatorShaftX = 200;
    private const double PersonWaitingX = 50;
    private const double PersonExitingX = 350;

    private readonly SimulationService _simulation;
    private readonly DispatcherTimer _timer;
    private readonly Random _random = new();
    private readonly Queue<double> _tickDurations = new();
    private readonly System.Diagnostics.Stopwatch _stopwatch = new();

    [ObservableProperty]
    private int _elevatorFloor;

    [ObservableProperty]
    private string _elevatorState = "Idle";

    [ObservableProperty]
    private int _passengerCount;

    [ObservableProperty]
    private ObservableCollection<PersonViewModel> _waitingPersons = new();

    [ObservableProperty]
    private ObservableCollection<PersonViewModel> _ridingPersons = new();

    [ObservableProperty]
    private ObservableCollection<PersonViewModel> _allVisiblePersons = new();

    [ObservableProperty]
    private double _elevatorY;

    [ObservableProperty]
    private bool _isDoorsOpen;

    [ObservableProperty]
    private string _statusLightColor = "Red";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PauseButtonText))]
    private bool _isPaused;

    [ObservableProperty]
    private string _averageTickDuration = "0.00 ms";

    public string PauseButtonText => IsPaused ? "Resume" : "Pause";

    public string Greeting { get; } = "Welcome to Elevator Simulator!";
    public int BuildingFloors => TotalFloors;

    public MainWindowViewModel()
    {
        _simulation = new SimulationService(TotalFloors);

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(TickIntervalMs)
        };
        _timer.Tick += OnTimerTick;
        _timer.Start();

        UpdateViewModel();
    }

    [RelayCommand]
    private void SpawnPerson()
    {
        int originFloor = _random.Next(0, TotalFloors);
        int targetFloor;

        do
        {
            targetFloor = _random.Next(0, TotalFloors);
        } while (targetFloor == originFloor);

        _simulation.SpawnPerson(originFloor, targetFloor);
        UpdateViewModel();
    }

    [RelayCommand]
    private void TogglePause()
    {
        IsPaused = !IsPaused;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (!IsPaused)
        {
            _stopwatch.Restart();
            _simulation.Tick();
            UpdateViewModel();
            _stopwatch.Stop();

            // Track last 20 tick durations
            _tickDurations.Enqueue(_stopwatch.Elapsed.TotalMilliseconds);
            if (_tickDurations.Count > 20)
            {
                _tickDurations.Dequeue();
            }

            // Calculate and display average
            double average = _tickDurations.Average();
            AverageTickDuration = $"{average:F2} ms";
        }
    }

    private void UpdateViewModel()
    {
        ElevatorFloor = _simulation.Elevator.CurrentFloor;
        ElevatorState = _simulation.Elevator.State.ToString();
        PassengerCount = _simulation.Elevator.Passengers.Count;
        IsDoorsOpen = _simulation.Elevator.State == Models.ElevatorState.DoorsOpen;

        // Status light: Green only when doors are open, Red otherwise
        StatusLightColor = IsDoorsOpen ? "Green" : "Red";

        // Calculate elevator Y position (floor 0 at bottom) using fractional Position
        ElevatorY = (TotalFloors - 1 - _simulation.Elevator.Position) * FloorHeight;

        // Update waiting persons
        WaitingPersons.Clear();
        foreach (var person in _simulation.AllPersons)
        {
            if (person.State == PersonState.WalkingToElevator || person.State == PersonState.WaitingAtElevator)
            {
                WaitingPersons.Add(new PersonViewModel
                {
                    Id = person.Id,
                    CurrentFloor = person.CurrentFloor,
                    TargetFloor = person.TargetFloor,
                    State = person.State.ToString()
                });
            }
        }

        // Update riding persons
        RidingPersons.Clear();
        foreach (var person in _simulation.Elevator.Passengers)
        {
            RidingPersons.Add(new PersonViewModel
            {
                Id = person.Id,
                CurrentFloor = person.CurrentFloor,
                TargetFloor = person.TargetFloor,
                State = person.State.ToString()
            });
        }

        // Update all visible persons with positions
        AllVisiblePersons.Clear();

        foreach (var person in _simulation.AllPersons)
        {
            double x = 0;
            double y;

            // Riding persons move with the elevator's fractional position
            if (person.State == PersonState.Riding)
            {
                y = ElevatorY + 50; // Elevator's Y position + offset to movement line
            }
            else
            {
                y = (TotalFloors - 1 - person.CurrentFloor) * FloorHeight + 50; // Movement line for floor
            }

            switch (person.State)
            {
                case PersonState.WalkingToElevator:
                case PersonState.WaitingAtElevator:
                case PersonState.Boarding:
                case PersonState.Riding:
                case PersonState.Exiting:
                case PersonState.WalkingAway:
                    x = person.XPosition;
                    break;
            }

            if (person.State != PersonState.Arrived)
            {
                var personVm = new PersonViewModel
                {
                    Id = person.Id,
                    CurrentFloor = person.CurrentFloor,
                    TargetFloor = person.TargetFloor,
                    State = person.State.ToString(),
                    X = x,
                    Y = y
                };
                AllVisiblePersons.Add(personVm);
            }
        }
    }
}

public class PersonViewModel
{
    public int Id { get; set; }
    public int CurrentFloor { get; set; }
    public int TargetFloor { get; set; }
    public string State { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
}

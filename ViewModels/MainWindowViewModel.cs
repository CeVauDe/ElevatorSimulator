using System;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElevatorSimulator.Models;

namespace ElevatorSimulator.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private const int TotalFloors = 10;
    private const int TickIntervalMs = 500;

    private readonly SimulationService _simulation;
    private readonly DispatcherTimer _timer;
    private readonly Random _random = new();

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
    [NotifyPropertyChangedFor(nameof(PauseButtonText))]
    private bool _isPaused;

    public string PauseButtonText => IsPaused ? "Resume" : "Pause";

    public string Greeting { get; } = "Welcome to Elevator Simulator!";

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
            _simulation.Tick();
            UpdateViewModel();
        }
    }

    private void UpdateViewModel()
    {
        ElevatorFloor = _simulation.Elevator.CurrentFloor;
        ElevatorState = _simulation.Elevator.State.ToString();
        PassengerCount = _simulation.Elevator.Passengers.Count;

        // Update waiting persons
        WaitingPersons.Clear();
        foreach (var person in _simulation.AllPersons)
        {
            if (person.State == PersonState.Waiting)
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
    }
}

public class PersonViewModel
{
    public int Id { get; set; }
    public int CurrentFloor { get; set; }
    public int TargetFloor { get; set; }
    public string State { get; set; } = string.Empty;
}

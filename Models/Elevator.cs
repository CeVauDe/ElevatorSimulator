using System;
using System.Collections.Generic;

namespace ElevatorSimulator.Models;

public enum ElevatorState
{
    Idle,
    MovingUp,
    MovingDown,
    DoorsOpen
}

public class Elevator
{
    private readonly int _totalFloors;
    private readonly int _doorOpenTicks;
    private int _doorTickCounter;

    public int CurrentFloor { get; set; }
    public ElevatorState State { get; private set; }
    public int? TargetFloor { get; private set; }
    public List<Person> Passengers { get; }

    public Elevator(int totalFloors, int doorOpenTicks = 3)
    {
        _totalFloors = totalFloors;
        _doorOpenTicks = doorOpenTicks;
        CurrentFloor = 0;
        State = ElevatorState.Idle;
        Passengers = new List<Person>();
    }

    public void MoveToFloor(int floor)
    {
        if (floor < 0 || floor >= _totalFloors)
            throw new ArgumentOutOfRangeException(nameof(floor));

        if (floor == CurrentFloor)
            return;

        TargetFloor = floor;
        State = floor > CurrentFloor ? ElevatorState.MovingUp : ElevatorState.MovingDown;
    }

    public void Tick()
    {
        switch (State)
        {
            case ElevatorState.MovingUp:
                CurrentFloor++;
                if (CurrentFloor == TargetFloor)
                {
                    State = ElevatorState.DoorsOpen;
                    _doorTickCounter = 0;
                }
                break;

            case ElevatorState.MovingDown:
                CurrentFloor--;
                if (CurrentFloor == TargetFloor)
                {
                    State = ElevatorState.DoorsOpen;
                    _doorTickCounter = 0;
                }
                break;

            case ElevatorState.DoorsOpen:
                _doorTickCounter++;
                if (_doorTickCounter >= _doorOpenTicks)
                {
                    State = ElevatorState.Idle;
                    TargetFloor = null;
                }
                break;

            case ElevatorState.Idle:
                // Do nothing
                break;
        }
    }

    public void AddPassenger(Person person)
    {
        Passengers.Add(person);
    }

    public void RemovePassenger(Person person)
    {
        Passengers.Remove(person);
    }

    public bool IsAtFloor(int floor)
    {
        return CurrentFloor == floor;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

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
    public double Position { get; set; } // Fractional floor position for smooth movement
    public ElevatorState State { get; private set; }
    public int? TargetFloor { get; private set; }
    public List<Person> Passengers { get; }

    public Elevator(int totalFloors, int doorOpenTicks = 3)
    {
        _totalFloors = totalFloors;
        _doorOpenTicks = doorOpenTicks;
        CurrentFloor = 0;
        Position = 0.0;
        State = ElevatorState.Idle;
        Passengers = new List<Person>();
    }

    public void MoveToFloor(int floor)
    {
        if (floor < 0 || floor >= _totalFloors)
            throw new ArgumentOutOfRangeException(nameof(floor));

        if (floor == CurrentFloor)
        {
            // Already at target floor, open doors immediately
            State = ElevatorState.DoorsOpen;
            _doorTickCounter = 0;
            return;
        }

        TargetFloor = floor;
        State = floor > CurrentFloor ? ElevatorState.MovingUp : ElevatorState.MovingDown;
    }

    public void Tick()
    {
        switch (State)
        {
            case ElevatorState.MovingUp:
                Position += 0.05;
                CurrentFloor = (int)Math.Round(Position);
                if (Position >= TargetFloor)
                {
                    Position = TargetFloor.Value; // Snap to exact floor
                    CurrentFloor = TargetFloor.Value;
                    State = ElevatorState.DoorsOpen;
                    _doorTickCounter = 0;
                }
                break;

            case ElevatorState.MovingDown:
                Position -= 0.05;
                CurrentFloor = (int)Math.Round(Position);
                if (Position <= TargetFloor)
                {
                    Position = TargetFloor.Value; // Snap to exact floor
                    CurrentFloor = TargetFloor.Value;
                    State = ElevatorState.DoorsOpen;
                    _doorTickCounter = 0;
                }
                break;

            case ElevatorState.DoorsOpen:
                _doorTickCounter++;
                // Keep doors open longer to allow people time to board/exit
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

    public bool CanCloseDoors(List<Person> allPersons)
    {
        // Check if anyone is still boarding or exiting on this floor
        return !allPersons.Any(p =>
            (p.State == PersonState.Boarding || p.State == PersonState.Exiting) &&
            p.CurrentFloor == CurrentFloor);
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

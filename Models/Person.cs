using System;

namespace ElevatorSimulator.Models;

public enum PersonState
{
    Waiting,
    Boarding,
    Riding,
    Exiting,
    Arrived
}

public class Person
{
    public int Id { get; }
    public int CurrentFloor { get; set; }
    public int TargetFloor { get; }
    public PersonState State { get; set; }

    public Person(int id, int currentFloor, int targetFloor)
    {
        if (currentFloor == targetFloor)
            throw new ArgumentException("Current floor and target floor cannot be the same.");

        Id = id;
        CurrentFloor = currentFloor;
        TargetFloor = targetFloor;
        State = PersonState.Waiting;
    }

    public bool NeedsToGoUp() => TargetFloor > CurrentFloor;
    public bool NeedsToGoDown() => TargetFloor < CurrentFloor;
}

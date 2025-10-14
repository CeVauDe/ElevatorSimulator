using System;

namespace ElevatorSimulator.Models;

public enum PersonState
{
    WalkingToElevator,
    WaitingAtElevator,
    Boarding,
    Riding,
    Exiting,
    WalkingAway,
    Arrived
}

public class Person
{
    public const double PersonWidth = 20.0;
    public const double PersonHitbox = 20.0;

    public int Id { get; }
    public int CurrentFloor { get; set; }
    public int TargetFloor { get; }
    public PersonState State { get; set; }
    public double XPosition { get; set; }

    public Person(int id, int currentFloor, int targetFloor)
    {
        if (currentFloor == targetFloor)
            throw new ArgumentException("Current floor and target floor cannot be the same.");

        Id = id;
        CurrentFloor = currentFloor;
        TargetFloor = targetFloor;
        State = PersonState.WalkingToElevator;
        XPosition = 0; // Start at left edge
    }

    public bool NeedsToGoUp() => TargetFloor > CurrentFloor;
    public bool NeedsToGoDown() => TargetFloor < CurrentFloor;

    public double GetHitboxLeft() => XPosition - (PersonHitbox - PersonWidth) / 2.0;
    public double GetHitboxRight() => XPosition + PersonWidth + (PersonHitbox - PersonWidth) / 2.0;
}

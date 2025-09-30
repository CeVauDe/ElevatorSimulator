using System;
using System.Collections.Generic;
using System.Linq;

namespace ElevatorSimulator.Models;

public class SimulationService
{
    private readonly int _totalFloors;
    private int _nextPersonId = 1;

    public Elevator Elevator { get; }
    public FloorQueue FloorQueue { get; }
    public List<Person> AllPersons { get; }

    public SimulationService(int totalFloors)
    {
        _totalFloors = totalFloors;
        Elevator = new Elevator(totalFloors);
        FloorQueue = new FloorQueue(totalFloors);
        AllPersons = new List<Person>();
    }

    public Person SpawnPerson(int originFloor, int targetFloor)
    {
        var person = new Person(_nextPersonId++, originFloor, targetFloor);
        AllPersons.Add(person);
        FloorQueue.AddCall(person);
        return person;
    }

    public int GetNextPersonId()
    {
        return _nextPersonId++;
    }

    public void Tick()
    {
        // Process person movement
        ProcessPersonMovement();

        // Process elevator boarding/disembarking when doors are open
        if (Elevator.State == ElevatorState.DoorsOpen)
        {
            ProcessElevatorAtFloor();
        }

        // Only allow elevator to move if no one is boarding or exiting
        // (once they're WalkingAway, they've cleared the shaft and elevator can move)
        bool canMove = !AllPersons.Any(p =>
            (p.State == PersonState.Boarding || p.State == PersonState.Exiting) &&
            p.CurrentFloor == Elevator.CurrentFloor);

        if (canMove)
        {
            // Tick the elevator (handles movement and door timing)
            Elevator.Tick();

            // Dispatch elevator if idle and there are pending calls
            if (Elevator.State == ElevatorState.Idle)
            {
                DispatchElevator();
            }
        }

        // Despawn arrived persons
        DespawnArrivedPersons();
    }

    private void ProcessElevatorAtFloor()
    {
        int currentFloor = Elevator.CurrentFloor;

        // Disembark passengers who reached their destination
        var passengersToDisembark = Elevator.Passengers
            .Where(p => p.TargetFloor == currentFloor)
            .ToList();

        foreach (var passenger in passengersToDisembark)
        {
            passenger.State = PersonState.Exiting;
            passenger.CurrentFloor = currentFloor; // Update to target floor
            // Keep current XPosition - person exits from where they are in the elevator
            Elevator.RemovePassenger(passenger);
        }

        // Persons will check capacity individually when they try to board
        // No automatic state change here anymore

        // Add persons who have finished boarding (reached Riding state) to elevator
        var boardingPersons = AllPersons
            .Where(p => p.State == PersonState.Riding && p.CurrentFloor == currentFloor && !Elevator.Passengers.Contains(p))
            .ToList();

        foreach (var person in boardingPersons)
        {
            Elevator.AddPassenger(person);
        }

        // If passengers boarded, set their destination
        if (Elevator.Passengers.Count > 0 && Elevator.State == ElevatorState.DoorsOpen)
        {
            // Find next destination from passengers
            var nextDestination = FindNextDestination();
            if (nextDestination.HasValue && nextDestination.Value != currentFloor)
            {
                // Will dispatch after doors close
            }
        }
    }

    private void DispatchElevator()
    {
        // First check if elevator has passengers
        if (Elevator.Passengers.Count > 0)
        {
            var nextDestination = FindNextDestination();
            if (nextDestination.HasValue)
            {
                Elevator.MoveToFloor(nextDestination.Value);
                return;
            }
        }

        // Otherwise, respond to persons waiting at elevator
        for (int floor = 0; floor < _totalFloors; floor++)
        {
            if (AllPersons.Any(p => p.State == PersonState.WaitingAtElevator && p.CurrentFloor == floor))
            {
                Elevator.MoveToFloor(floor);
                return;
            }
        }
    }

    private int? FindNextDestination()
    {
        if (Elevator.Passengers.Count == 0)
            return null;

        // Simple strategy: go to the first passenger's destination
        return Elevator.Passengers.First().TargetFloor;
    }

    private void ProcessPersonMovement()
    {
        const double MovementSpeed = 2.0; // px per tick
        const double ShaftLeft = 180.0; // Left edge of elevator shaft
        const double ShaftRight = 270.0; // Right edge of elevator shaft
        const double ElevatorInteriorLeft = 185.0; // Interior left edge
        const double ElevatorInteriorRight = 265.0; // Interior right edge
        const double RightEdge = 450.0; // Right edge of floor

        foreach (var person in AllPersons)
        {
            switch (person.State)
            {
                case PersonState.WalkingToElevator:
                    {
                        double desiredX = person.XPosition + MovementSpeed;

                        // Check if right edge of person would reach elevator shaft
                        if (desiredX + Person.PersonWidth >= ShaftLeft)
                        {
                            desiredX = ShaftLeft - Person.PersonWidth;
                        }

                        // Move as far as possible without collision
                        double nextX = CalculateMaxMovement(person, desiredX, person.CurrentFloor);
                        person.XPosition = nextX;

                        // Check if reached waiting position
                        if (person.XPosition + Person.PersonWidth >= ShaftLeft - 0.1)
                        {
                            person.State = PersonState.WaitingAtElevator;
                        }
                    }
                    break;

                case PersonState.WaitingAtElevator:
                    {
                        // Check if elevator is at this floor with doors open
                        if (Elevator.State == ElevatorState.DoorsOpen && Elevator.CurrentFloor == person.CurrentFloor)
                        {
                            // Check if there's room in the elevator
                            int currentOccupancy = AllPersons.Count(p => p.State == PersonState.Riding || p.State == PersonState.Boarding);
                            int capacity = GetElevatorCapacity();

                            if (currentOccupancy < capacity)
                            {
                                // There's room, start boarding
                                person.State = PersonState.Boarding;
                                FloorQueue.ClearFloor(person.CurrentFloor);
                            }
                        }
                    }
                    break;

                case PersonState.Boarding:
                    {
                        // Move right as far as possible
                        double desiredX = person.XPosition + MovementSpeed;

                        // Cap at elevator right interior
                        desiredX = Math.Min(desiredX, ElevatorInteriorRight - Person.PersonWidth);

                        // Move as far as possible without collision
                        double nextX = CalculateMaxMovement(person, desiredX, person.CurrentFloor);

                        // Don't move backward
                        if (nextX > person.XPosition)
                        {
                            person.XPosition = nextX;
                        }

                        // Change to Riding if inside elevator bounds and can't move further right
                        if (person.XPosition >= ElevatorInteriorLeft &&
                            (person.XPosition >= ElevatorInteriorRight - Person.PersonWidth - 0.1 ||
                             nextX <= person.XPosition + 0.1))
                        {
                            person.State = PersonState.Riding;
                        }
                    }
                    break;

                case PersonState.Exiting:
                    {
                        double desiredX = person.XPosition + MovementSpeed;

                        // Move as far as possible without collision
                        double nextX = CalculateMaxMovement(person, desiredX, person.CurrentFloor);
                        person.XPosition = nextX;

                        // Check if left edge cleared elevator shaft
                        if (person.XPosition >= ShaftRight)
                        {
                            person.State = PersonState.WalkingAway;
                        }
                    }
                    break;

                case PersonState.Riding:
                    {
                        // Continuously move right as far as possible
                        double desiredX = person.XPosition + MovementSpeed;

                        // Cap at elevator right interior
                        desiredX = Math.Min(desiredX, ElevatorInteriorRight - Person.PersonWidth);

                        // Move as far as possible without collision
                        double nextX = CalculateMaxMovement(person, desiredX, person.CurrentFloor);

                        // Only move if we can actually move further right
                        if (nextX > person.XPosition + 0.1) // Small epsilon to avoid jitter
                        {
                            // Final bounds check before setting position
                            nextX = Math.Max(nextX, ElevatorInteriorLeft);
                            nextX = Math.Min(nextX, ElevatorInteriorRight - Person.PersonWidth);
                            person.XPosition = nextX;
                        }
                    }
                    break;

                case PersonState.WalkingAway:
                    {
                        double desiredX = person.XPosition + MovementSpeed;

                        // Move as far as possible without collision
                        double nextX = CalculateMaxMovement(person, desiredX, person.CurrentFloor);
                        person.XPosition = nextX;

                        // Check if reached right edge
                        if (person.XPosition >= RightEdge)
                        {
                            person.State = PersonState.Arrived;
                        }
                    }
                    break;
            }
        }
    }

    private void DespawnArrivedPersons()
    {
        AllPersons.RemoveAll(p => p.State == PersonState.Arrived);
    }

    private bool CheckCollision(Person movingPerson, double targetX, int floor)
    {
        double hitboxLeft = targetX - (Person.PersonHitbox - Person.PersonWidth) / 2.0;
        double hitboxRight = targetX + Person.PersonWidth + (Person.PersonHitbox - Person.PersonWidth) / 2.0;

        foreach (var otherPerson in AllPersons)
        {
            if (otherPerson.Id == movingPerson.Id)
                continue;

            // Check persons on same floor
            if (otherPerson.CurrentFloor != floor)
                continue;

            // Skip persons who have arrived
            if (otherPerson.State == PersonState.Arrived)
                continue;

            // Check hitbox overlap
            double otherLeft = otherPerson.GetHitboxLeft();
            double otherRight = otherPerson.GetHitboxRight();

            if (hitboxLeft < otherRight && hitboxRight > otherLeft)
            {
                return true; // Collision detected
            }
        }

        return false; // No collision
    }

    private int GetElevatorCapacity()
    {
        const double ElevatorInteriorWidth = 80.0; // 265 - 185
        return (int)(ElevatorInteriorWidth / Person.PersonHitbox);
    }

    private double CalculateMaxMovement(Person movingPerson, double desiredX, int floor)
    {
        double maxX = desiredX;
        double hitboxLeft = desiredX - (Person.PersonHitbox - Person.PersonWidth) / 2.0;
        double hitboxRight = desiredX + Person.PersonWidth + (Person.PersonHitbox - Person.PersonWidth) / 2.0;

        foreach (var otherPerson in AllPersons)
        {
            if (otherPerson.Id == movingPerson.Id)
                continue;

            // For persons in Boarding/Riding states, check against all persons in elevator regardless of floor
            // Otherwise, only check persons on same floor
            bool shouldCheck = false;
            if (movingPerson.State == PersonState.Boarding || movingPerson.State == PersonState.Riding)
            {
                // Check all persons who are also in the elevator
                if (otherPerson.State == PersonState.Boarding ||
                    otherPerson.State == PersonState.Riding ||
                    otherPerson.State == PersonState.Exiting)
                {
                    shouldCheck = true;
                }
            }
            else
            {
                // For other states, check persons on same floor
                if (otherPerson.CurrentFloor == floor)
                {
                    shouldCheck = true;
                }
            }

            if (!shouldCheck)
                continue;

            // Skip persons who have arrived
            if (otherPerson.State == PersonState.Arrived)
                continue;

            // Check if we would collide with this person
            double otherLeft = otherPerson.GetHitboxLeft();
            double otherRight = otherPerson.GetHitboxRight();

            // If moving right and would collide, stop just before them
            if (desiredX > movingPerson.XPosition && hitboxRight > otherLeft && hitboxLeft < otherRight)
            {
                // Stop at position where our right edge touches their left edge
                double safeX = otherLeft - Person.PersonWidth;
                maxX = Math.Min(maxX, safeX);
            }
        }

        return maxX;
    }
}

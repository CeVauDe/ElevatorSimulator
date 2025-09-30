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
        // Process elevator boarding/disembarking when doors are open
        if (Elevator.State == ElevatorState.DoorsOpen)
        {
            ProcessElevatorAtFloor();
        }

        // Tick the elevator (handles movement and door timing)
        Elevator.Tick();

        // Dispatch elevator if idle and there are pending calls
        if (Elevator.State == ElevatorState.Idle)
        {
            DispatchElevator();
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
            Elevator.RemovePassenger(passenger);
            passenger.State = PersonState.Arrived;
        }

        // Board waiting passengers
        var waitingPersons = FloorQueue.GetWaitingPersons(currentFloor);
        foreach (var person in waitingPersons)
        {
            person.State = PersonState.Boarding;
            Elevator.AddPassenger(person);
            person.State = PersonState.Riding;
            person.CurrentFloor = currentFloor;
        }

        if (waitingPersons.Count > 0)
        {
            FloorQueue.ClearFloor(currentFloor);
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

        // Otherwise, respond to floor calls
        for (int floor = 0; floor < _totalFloors; floor++)
        {
            if (FloorQueue.HasCallAtFloor(floor))
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

    private void DespawnArrivedPersons()
    {
        AllPersons.RemoveAll(p => p.State == PersonState.Arrived);
    }
}

using ElevatorSimulator.Models;

namespace ElevatorSimulator.Tests.Models;

public class SimulationServiceTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        var simulation = new SimulationService(totalFloors: 10);

        Assert.NotNull(simulation.Elevator);
        Assert.NotNull(simulation.FloorQueue);
        Assert.Empty(simulation.AllPersons);
    }

    [Fact]
    public void SpawnPerson_ShouldCreatePersonAndAddToQueue()
    {
        var simulation = new SimulationService(totalFloors: 10);

        var person = simulation.SpawnPerson(originFloor: 2, targetFloor: 7);

        Assert.NotNull(person);
        Assert.Equal(2, person.CurrentFloor);
        Assert.Equal(7, person.TargetFloor);
        Assert.True(simulation.FloorQueue.HasCallAtFloor(2));
        Assert.Contains(person, simulation.AllPersons);
    }

    [Fact]
    public void Tick_WithIdleElevatorAndPendingCall_ShouldDispatchElevator()
    {
        var simulation = new SimulationService(totalFloors: 10);
        var person = simulation.SpawnPerson(originFloor: 5, targetFloor: 8);

        // Walk person to elevator position
        while (person.State == PersonState.WalkingToElevator)
        {
            simulation.Tick();
        }

        // Now person is waiting, dispatch elevator
        simulation.Tick();

        Assert.NotEqual(ElevatorState.Idle, simulation.Elevator.State);
    }

    [Fact]
    public void Tick_WhenElevatorArrivesAtPickupFloor_ShouldBoardPassengers()
    {
        var simulation = new SimulationService(totalFloors: 10);
        var person = simulation.SpawnPerson(originFloor: 1, targetFloor: 5);

        // Tick until elevator arrives at floor 1
        simulation.Tick(); // Elevator starts moving to floor 1
        simulation.Tick(); // Elevator arrives at floor 1, doors open

        Assert.Equal(ElevatorState.DoorsOpen, simulation.Elevator.State);
        Assert.Equal(1, simulation.Elevator.CurrentFloor);
    }

    [Fact]
    public void Tick_WhenDoorsOpen_ShouldBoardWaitingPassengers()
    {
        var simulation = new SimulationService(totalFloors: 10);
        var person = simulation.SpawnPerson(originFloor: 1, targetFloor: 5);

        // Move elevator to floor 1
        simulation.Tick(); // Dispatch
        simulation.Tick(); // Arrive and open doors
        simulation.Tick(); // Process boarding

        Assert.Contains(person, simulation.Elevator.Passengers);
        Assert.Equal(PersonState.Riding, person.State);
        Assert.False(simulation.FloorQueue.HasCallAtFloor(1));
    }

    [Fact]
    public void Tick_WhenElevatorArrivesAtDestination_ShouldDisembarkPassengers()
    {
        var simulation = new SimulationService(totalFloors: 10);
        var person = simulation.SpawnPerson(originFloor: 1, targetFloor: 3);

        // Move to floor 1 and board
        simulation.Tick(); // Dispatch to floor 1
        simulation.Tick(); // Arrive at floor 1
        simulation.Tick(); // Board passenger
        simulation.Tick(); // Doors close, go idle

        // Now elevator should move to floor 3
        simulation.Tick(); // Dispatch to floor 3
        simulation.Tick(); // Move from 1 to 2
        simulation.Tick(); // Move from 2 to 3, arrive

        Assert.Equal(3, simulation.Elevator.CurrentFloor);
        Assert.Equal(ElevatorState.DoorsOpen, simulation.Elevator.State);
    }

    [Fact]
    public void DespawnArrivedPersons_ShouldRemoveFromAllPersons()
    {
        var simulation = new SimulationService(totalFloors: 10);
        var person = simulation.SpawnPerson(originFloor: 1, targetFloor: 3);

        // Manually set person to arrived
        person.State = PersonState.Arrived;

        // Call tick to trigger despawn
        simulation.Tick();

        // Person should be removed from AllPersons
        Assert.DoesNotContain(person, simulation.AllPersons);
    }

    [Fact]
    public void GetNextPersonId_ShouldReturnIncrementingIds()
    {
        var simulation = new SimulationService(totalFloors: 10);

        var id1 = simulation.GetNextPersonId();
        var id2 = simulation.GetNextPersonId();
        var id3 = simulation.GetNextPersonId();

        Assert.Equal(1, id1);
        Assert.Equal(2, id2);
        Assert.Equal(3, id3);
    }
}

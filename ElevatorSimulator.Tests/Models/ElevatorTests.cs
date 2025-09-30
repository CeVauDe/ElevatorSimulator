using ElevatorSimulator.Models;

namespace ElevatorSimulator.Tests.Models;

public class ElevatorTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        var elevator = new Elevator(totalFloors: 10);

        Assert.Equal(0, elevator.CurrentFloor);
        Assert.Equal(ElevatorState.Idle, elevator.State);
        Assert.Empty(elevator.Passengers);
    }

    [Fact]
    public void MoveToFloor_WhenIdle_ShouldStartMovingUp()
    {
        var elevator = new Elevator(totalFloors: 10);

        elevator.MoveToFloor(5);

        Assert.Equal(ElevatorState.MovingUp, elevator.State);
        Assert.Equal(5, elevator.TargetFloor);
    }

    [Fact]
    public void MoveToFloor_WhenIdle_ShouldStartMovingDown()
    {
        var elevator = new Elevator(totalFloors: 10) { CurrentFloor = 8 };

        elevator.MoveToFloor(3);

        Assert.Equal(ElevatorState.MovingDown, elevator.State);
        Assert.Equal(3, elevator.TargetFloor);
    }

    [Fact]
    public void MoveToFloor_ToCurrentFloor_ShouldStayIdle()
    {
        var elevator = new Elevator(totalFloors: 10) { CurrentFloor = 5 };

        elevator.MoveToFloor(5);

        Assert.Equal(ElevatorState.Idle, elevator.State);
    }

    [Fact]
    public void Tick_WhenMovingUp_ShouldIncrementFloor()
    {
        var elevator = new Elevator(totalFloors: 10);
        elevator.MoveToFloor(3);

        elevator.Tick();

        Assert.Equal(1, elevator.CurrentFloor);
        Assert.Equal(ElevatorState.MovingUp, elevator.State);
    }

    [Fact]
    public void Tick_WhenReachingTarget_ShouldOpenDoors()
    {
        var elevator = new Elevator(totalFloors: 10) { CurrentFloor = 2 };
        elevator.MoveToFloor(3);

        elevator.Tick(); // Move to floor 3

        Assert.Equal(3, elevator.CurrentFloor);
        Assert.Equal(ElevatorState.DoorsOpen, elevator.State);
    }

    [Fact]
    public void Tick_WhenMovingDown_ShouldDecrementFloor()
    {
        var elevator = new Elevator(totalFloors: 10) { CurrentFloor = 5 };
        elevator.MoveToFloor(2);

        elevator.Tick();

        Assert.Equal(4, elevator.CurrentFloor);
        Assert.Equal(ElevatorState.MovingDown, elevator.State);
    }

    [Fact]
    public void Tick_WhenDoorsOpen_ShouldCountDownAndReturnToIdle()
    {
        var elevator = new Elevator(totalFloors: 10, doorOpenTicks: 2);
        elevator.MoveToFloor(1);
        elevator.Tick(); // Arrive at floor 1, doors open

        Assert.Equal(ElevatorState.DoorsOpen, elevator.State);

        elevator.Tick(); // Door open tick 1
        Assert.Equal(ElevatorState.DoorsOpen, elevator.State);

        elevator.Tick(); // Door open tick 2, should close and go idle
        Assert.Equal(ElevatorState.Idle, elevator.State);
    }

    [Fact]
    public void AddPassenger_ShouldAddToPassengersList()
    {
        var elevator = new Elevator(totalFloors: 10);
        var person = new Person(id: 1, currentFloor: 0, targetFloor: 5);

        elevator.AddPassenger(person);

        Assert.Single(elevator.Passengers);
        Assert.Contains(person, elevator.Passengers);
    }

    [Fact]
    public void RemovePassenger_ShouldRemoveFromPassengersList()
    {
        var elevator = new Elevator(totalFloors: 10);
        var person = new Person(id: 1, currentFloor: 0, targetFloor: 5);
        elevator.AddPassenger(person);

        elevator.RemovePassenger(person);

        Assert.Empty(elevator.Passengers);
    }

    [Fact]
    public void IsAtFloor_ShouldReturnTrueWhenAtSpecifiedFloor()
    {
        var elevator = new Elevator(totalFloors: 10) { CurrentFloor = 5 };

        Assert.True(elevator.IsAtFloor(5));
        Assert.False(elevator.IsAtFloor(3));
    }
}

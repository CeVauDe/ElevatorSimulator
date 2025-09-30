using ElevatorSimulator.Models;

namespace ElevatorSimulator.Tests.Models;

public class FloorQueueTests
{
    [Fact]
    public void Constructor_ShouldInitializeEmptyQueues()
    {
        var floorQueue = new FloorQueue(totalFloors: 10);

        Assert.False(floorQueue.HasCallAtFloor(0));
        Assert.False(floorQueue.HasCallAtFloor(5));
    }

    [Fact]
    public void AddCall_ShouldRegisterCallAtFloor()
    {
        var floorQueue = new FloorQueue(totalFloors: 10);
        var person = new Person(id: 1, currentFloor: 3, targetFloor: 7);

        floorQueue.AddCall(person);

        Assert.True(floorQueue.HasCallAtFloor(3));
    }

    [Fact]
    public void GetWaitingPersons_ShouldReturnPersonsAtFloor()
    {
        var floorQueue = new FloorQueue(totalFloors: 10);
        var person1 = new Person(id: 1, currentFloor: 3, targetFloor: 7);
        var person2 = new Person(id: 2, currentFloor: 3, targetFloor: 8);

        floorQueue.AddCall(person1);
        floorQueue.AddCall(person2);

        var waitingPersons = floorQueue.GetWaitingPersons(3);
        Assert.Equal(2, waitingPersons.Count);
        Assert.Contains(person1, waitingPersons);
        Assert.Contains(person2, waitingPersons);
    }

    [Fact]
    public void ClearFloor_ShouldRemoveAllPersonsAtFloor()
    {
        var floorQueue = new FloorQueue(totalFloors: 10);
        var person1 = new Person(id: 1, currentFloor: 3, targetFloor: 7);
        var person2 = new Person(id: 2, currentFloor: 3, targetFloor: 8);

        floorQueue.AddCall(person1);
        floorQueue.AddCall(person2);
        floorQueue.ClearFloor(3);

        Assert.False(floorQueue.HasCallAtFloor(3));
        Assert.Empty(floorQueue.GetWaitingPersons(3));
    }

    [Fact]
    public void HasCallAtFloor_WithNoPersons_ShouldReturnFalse()
    {
        var floorQueue = new FloorQueue(totalFloors: 10);

        Assert.False(floorQueue.HasCallAtFloor(5));
    }
}

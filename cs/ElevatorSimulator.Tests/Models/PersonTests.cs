using ElevatorSimulator.Models;

namespace ElevatorSimulator.Tests.Models;

public class PersonTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithCorrectValues()
    {
        var person = new Person(id: 1, currentFloor: 2, targetFloor: 5);

        Assert.Equal(1, person.Id);
        Assert.Equal(2, person.CurrentFloor);
        Assert.Equal(5, person.TargetFloor);
        Assert.Equal(PersonState.WalkingToElevator, person.State);
        Assert.Equal(0, person.XPosition);
    }

    [Fact]
    public void Constructor_WithSameOriginAndDestination_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => new Person(id: 1, currentFloor: 3, targetFloor: 3));
    }

    [Fact]
    public void NeedsToGoUp_WhenTargetIsAbove_ShouldReturnTrue()
    {
        var person = new Person(id: 1, currentFloor: 2, targetFloor: 5);

        Assert.True(person.NeedsToGoUp());
        Assert.False(person.NeedsToGoDown());
    }

    [Fact]
    public void NeedsToGoDown_WhenTargetIsBelow_ShouldReturnTrue()
    {
        var person = new Person(id: 1, currentFloor: 7, targetFloor: 3);

        Assert.True(person.NeedsToGoDown());
        Assert.False(person.NeedsToGoUp());
    }
}

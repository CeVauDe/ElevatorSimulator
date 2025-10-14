using System.Collections.Generic;
using System.Linq;

namespace ElevatorSimulator.Models;

public class FloorQueue
{
    private readonly Dictionary<int, List<Person>> _waitingPersons;
    private readonly int _totalFloors;

    public FloorQueue(int totalFloors)
    {
        _totalFloors = totalFloors;
        _waitingPersons = new Dictionary<int, List<Person>>();

        for (int i = 0; i < totalFloors; i++)
        {
            _waitingPersons[i] = new List<Person>();
        }
    }

    public void AddCall(Person person)
    {
        if (!_waitingPersons.ContainsKey(person.CurrentFloor))
            return;

        _waitingPersons[person.CurrentFloor].Add(person);
    }

    public bool HasCallAtFloor(int floor)
    {
        return _waitingPersons.ContainsKey(floor) && _waitingPersons[floor].Count > 0;
    }

    public List<Person> GetWaitingPersons(int floor)
    {
        return _waitingPersons.ContainsKey(floor)
            ? new List<Person>(_waitingPersons[floor])
            : new List<Person>();
    }

    public void ClearFloor(int floor)
    {
        if (_waitingPersons.ContainsKey(floor))
        {
            _waitingPersons[floor].Clear();
        }
    }
}

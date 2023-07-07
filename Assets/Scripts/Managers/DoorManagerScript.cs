using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManagerScript : MonoBehaviour
{
    [SerializeField] private List<CorridorDoorScript> doors = new List<CorridorDoorScript>();
    [SerializeField] private List<KeyScript> keys = new List<KeyScript>();

    public void RegisterDoor(CorridorDoorScript door)
    {
        doors.Add(door);
    }

    public void RegisterKey(KeyScript key)
    {
        keys.Add(key);
    }

    public void UnlockDoor(int doorID)
    {
        CorridorDoorScript door = doors.Find(d => d.doorID == doorID);
        if (door != null)
        {
            door.Unlock();
        }
    }

    public void CheckKeyCollected(int doorID)
    {
        KeyScript key = keys.Find(k => k.keyID == doorID);
        if (key != null && key.isPickedUp)
        {
            UnlockDoor(doorID);
        }
    }
}

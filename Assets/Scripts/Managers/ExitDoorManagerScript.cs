using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDoorManagerScript : MonoBehaviour
{
    PickupScript pickupScript;
    GameObject door;

    private void Start()
    {
        pickupScript = FindObjectOfType<PickupScript>();
        door = GameObject.FindGameObjectWithTag("Door");
    }

    private void Update()
    {
        if (!pickupScript.isPickedUp)
        {
            door.SetActive(false);
        }
        else
        {
            door.SetActive(true);
        }
    }
}

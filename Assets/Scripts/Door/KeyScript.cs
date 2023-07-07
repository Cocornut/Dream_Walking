using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyScript : MonoBehaviour
{
    public bool isPickedUp;         // Checks if key is collected
    public int keyID = 0;           // Identifies key to make sure it matches one door
    GameObject manager;

    private void Awake()
    {
        manager = GameObject.Find("DoorManager");
        manager.GetComponent<DoorManagerScript>().RegisterKey(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPickedUp)
        {
            isPickedUp = true;
            gameObject.SetActive(false);
        }
    }
}

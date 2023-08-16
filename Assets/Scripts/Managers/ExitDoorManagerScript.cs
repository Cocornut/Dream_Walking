using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDoorManagerScript : MonoBehaviour
{
    PickupScript pickupScript;
    GameObject[] doors;

    [SerializeField] private AudioSource pickupSound;

    private void Awake()
    {
        pickupScript = FindObjectOfType<PickupScript>();
        doors = GameObject.FindGameObjectsWithTag("ExitDoor");
    }

    private void Update()
    {
        if (!pickupScript.isPickedUp)
        {
            foreach (var door in doors)
            {
                door.SetActive(false);
            }
        }
        else
        {
            foreach (var door in doors)
            {
                door.SetActive(true);
            }
        }
    }

    internal void PlayPickupSound()
    {
        pickupSound.Play();
    }
}

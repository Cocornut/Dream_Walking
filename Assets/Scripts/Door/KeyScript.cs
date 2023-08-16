using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyScript : MonoBehaviour
{
    public bool isPickedUp;         // Checks if key is collected
    public int keyID = 0;           // Identifies key to make sure it matches one door
    public GameObject manager;

    bool highlighted = false;

    public Material outline;

    private void Awake()
    {
        manager = GameObject.FindGameObjectWithTag("DoorManager");
        manager.GetComponent<DoorManagerScript>().RegisterKey(this);
    }

    public void Pickup()
    {
        manager.GetComponent<DoorManagerScript>().PlayPickupSound();
        isPickedUp = true;
        gameObject.SetActive(false);
    }

    public void Highlight()
    {
        if (!highlighted)
        {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            Material[] materials = renderer.materials;
            Material[] newMaterials = new Material[materials.Length + 1];
            for (int i = 0; i < materials.Length; i++)
            {
                newMaterials[i] = materials[i];
            }
            newMaterials[materials.Length] = outline;
            renderer.materials = newMaterials;
            highlighted = true;
        }
    }

    public void StopHighlight()
    {
        if (highlighted)
        {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            Material[] materials = renderer.materials;

            // Create a new materials array with only the first element (original material)
            Material[] newMaterials = new Material[] { materials[0] };

            // Update the renderer's materials array with the new materials array
            renderer.materials = newMaterials;

            highlighted = false;
        }
    }
}

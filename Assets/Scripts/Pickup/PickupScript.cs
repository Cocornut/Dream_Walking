using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupScript : MonoBehaviour
{
    public bool isPickedUp = false;
    public bool isRendered = false;

    bool highlighted = false;

    public Material outline;

    private ExitDoorManagerScript exitManager;


    private void Start()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        exitManager = GameObject.FindGameObjectWithTag("ExitDoorManager").GetComponent<ExitDoorManagerScript>();
    }

    public void Pickup()
    {
        if (isRendered)
        {
            exitManager.PlayPickupSound();
            isPickedUp = true;
            gameObject.SetActive(false);
        }
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

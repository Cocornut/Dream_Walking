using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupScript : MonoBehaviour
{
    public bool isPickedUp = false;
    public bool isRendered = false;

    private void Start()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPickedUp)
        {
            if (isRendered)
            {
                isPickedUp = true;
                gameObject.SetActive(false);
            }
        }
    }
}

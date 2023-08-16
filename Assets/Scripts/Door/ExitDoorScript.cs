using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ExitDoorScript : MonoBehaviour
{
    private PickupScript pickupObject;

    [SerializeField] private AudioSource winSound;

    private void Start()
    {
        pickupObject = FindObjectOfType<PickupScript>();
    }
    private void Update()
    {
        if (pickupObject == null)
        {
            pickupObject = FindObjectOfType<PickupScript>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && pickupObject.isPickedUp)
        {
            winSound.Play();
            SceneManager.LoadScene(Scenes.victoryScene);
            Cursor.lockState = CursorLockMode.None;
        }
    }
}


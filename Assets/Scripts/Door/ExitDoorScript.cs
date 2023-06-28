using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class ExitDoorScript : MonoBehaviour
{
    private PickupScript pickupObject;

    // Win display
    private GameObject winTextObject;
    private TextMeshProUGUI winText;

    private void Start()
    {
        pickupObject = FindObjectOfType<PickupScript>();

        winTextObject = GameObject.Find("Win/Lose");
        winText = winTextObject.GetComponent<TextMeshProUGUI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && pickupObject.isPickedUp)
        {
            if (winText != null)
            {
                // Change the text
                winText.text = "You Won";
                // Set the text colour
                winText.color = new Color(1f, 0.84f, 0f);
                // Show the text
                winTextObject.SetActive(true);
                // Pause the game
                Time.timeScale = 0f;
            }
        }
    }
}

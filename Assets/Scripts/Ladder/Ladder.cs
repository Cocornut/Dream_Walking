using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    GameObject player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Collision");
            player.GetComponent<PlayerMovementScript>().onLadder = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovementScript player = other.GetComponent<PlayerMovementScript>();
            // Check if player is going up or down
            if (player.zInput < 0)
            {
                if (player.isGrounded)
                {
                    player.onLadder = false;
                }
            }
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Collision Exit");
            player.GetComponent<PlayerMovementScript>().onLadder = false;
        }
    }
}

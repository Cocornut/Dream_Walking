using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclePush : MonoBehaviour
{
    [SerializeField]
    private float forceMagnitude = 1f;

    PlayerMovementScript player;
    Transform barrelCheck;

    private void Awake()
    {
        player = gameObject.GetComponent<PlayerMovementScript>();
        barrelCheck = gameObject.transform.Find("BarrelCheck");
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!IsStandingOnBarrel())
        {
            Rigidbody rb = hit.collider.attachedRigidbody;
            
            if (rb != null)
            {
                Vector3 forceDirection = hit.gameObject.transform.position - transform.position;
                forceDirection.y = 0;
                forceDirection.Normalize();
                
                rb.AddForceAtPosition(forceDirection * forceMagnitude, transform.position, ForceMode.Impulse);
            }
        }
    }

    private bool IsStandingOnBarrel()
    {
        RaycastHit hit;
        if (Physics.Raycast(barrelCheck.position, Vector3.down, out hit, 0.1f))
        {
            if (hit.collider.CompareTag("Barrel"))
            {
                return true;
            }
        }
        return false;
    }
}

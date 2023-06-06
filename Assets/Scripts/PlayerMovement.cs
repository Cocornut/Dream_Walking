using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float movementSpeed = 5f;
    private Rigidbody rb;
    private float moveHorizontal;
    private float moveVertical;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertical = Input.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(moveHorizontal, 0f, moveVertical) * movementSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }
}

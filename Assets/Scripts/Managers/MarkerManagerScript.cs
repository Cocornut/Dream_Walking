using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerManagerScript : MonoBehaviour
{
    [SerializeField] Transform markerPrefab;
    [SerializeField] KeyCode sprayKey = KeyCode.Mouse0;
    [SerializeField] float maxSprayDistance;
    [SerializeField] float markerCooldown;

    private float markerLastTime;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        markerLastTime = -markerCooldown;
    }

    void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        if (Input.GetKeyDown(sprayKey))
        {            
            if (Time.time - markerLastTime >= markerCooldown)
            {
                SprayMarker();
                markerLastTime = Time.time;
            }
        }
    }

    private void SprayMarker()
    {
        // Create a ray from the mouse position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // Calculate distance between the player and target point
            float distance = Vector3.Distance(mainCamera.transform.position, hit.point);

            if (distance <= maxSprayDistance)
            {
                // Calculate an offset vector to avoid z-fighting
                Vector3 offset = hit.normal * 0.1f;

                // Instantiate a marker prefab
                Transform marker = Instantiate(markerPrefab, hit.point - offset, Quaternion.LookRotation(hit.normal));
                marker.SetParent(hit.transform);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorridorDoorScript : MonoBehaviour
{
    // Identifies door to match with the correct key
    [Header("Managing")]
    public int doorID;
    GameObject manager;

    [Header("Checks")]
    public bool isOpen = false;
    public bool hasKey = false;
    [SerializeField] bool isRotatingDoor = true;

    [Header("Rotation Configs")]
    [SerializeField] float speed = 1f;
    [SerializeField] float rotationAmount = 90f;
    [SerializeField] float forwardDirection = 0;

    private Vector3 startRotation;
    private Vector3 forward;

    private Coroutine animationCoroutine;

    private void Awake()
    {
        startRotation = transform.rotation.eulerAngles;
        forward = transform.right;
        manager = GameObject.Find("DoorManager");
        manager.GetComponent<DoorManagerScript>().RegisterDoor(this);
    }

    public void Unlock()
    {
        hasKey = true;
    }

    public bool CheckKeyCollected()
    {
        // Get all KeyScript components in the scene
        KeyScript[] keys = FindObjectsOfType<KeyScript>();

        foreach (KeyScript key in keys)
        {
            int i = 1;
            Debug.Log("Key: " + i);
            i++;
            if (key.keyID == doorID && key.isPickedUp)
            {
                Debug.Log("Key Collected");
                return true;                
            }
        }
        return false;
    }

    public void Budge()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        if (isRotatingDoor)
        {
            animationCoroutine = StartCoroutine(DoBudge());
        }
    }

    private IEnumerator DoBudge()
    {
        Quaternion _startRotation = transform.rotation;
        Quaternion _endRotation = Quaternion.Euler(new Vector3(0, startRotation.y + 10f, 0));
        float time = 0f;
        float budgeDuration = 0.1f;

        while (time < budgeDuration)
        {
            transform.rotation = Quaternion.Slerp(_startRotation, _endRotation, time);
            yield return null;
            time += Time.deltaTime;
        }

        _startRotation = transform.rotation;
        _endRotation = Quaternion.Euler(new Vector3(0, startRotation.y - 10f, 0));
        time = 0f;

        while (time < budgeDuration)
        {
            transform.rotation = Quaternion.Slerp(_startRotation, _endRotation, time);
            yield return null;
            time += Time.deltaTime;
        }

        _startRotation = transform.rotation;
        _endRotation = Quaternion.Euler(startRotation);
        time = 0f;

        while (time < budgeDuration)
        {
            transform.rotation = Quaternion.Slerp(_startRotation, _endRotation, time);
            yield return null;
            time += Time.deltaTime;
        }
    }

    public void Open(Vector3 userPosition)
    {
        if (!isOpen)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }

            if (isRotatingDoor)
            {
                float dot = Vector3.Dot(forward, (userPosition - transform.position).normalized);
                Debug.Log($"Dot: {dot.ToString("N3")}");
                animationCoroutine = StartCoroutine(DoRotationOpen(dot));
            }
        }
    }

    private IEnumerator DoRotationOpen(float forwardAmount)
    {
        Quaternion _startRotation = transform.rotation;
        Quaternion _endRotation;

        if (forwardAmount >= forwardDirection)
        {
            _endRotation = Quaternion.Euler(new Vector3(0, startRotation.y - rotationAmount, 0));
        }
        else
        {
            _endRotation = Quaternion.Euler(new Vector3(0, startRotation.y + rotationAmount, 0));
        }

        isOpen = true;

        float time = 0;
        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(_startRotation, _endRotation, time);
            yield return null;
            time += Time.deltaTime * speed;
        }
    }

    public void Close()
    {
        if (isOpen)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }

            if (isRotatingDoor)
            {
                animationCoroutine = StartCoroutine(DoRotationClose());
            }
        }
    }

    private IEnumerator DoRotationClose()
    {
        Quaternion _startRotation = transform.rotation;
        Quaternion _endRotation = Quaternion.Euler(startRotation);

        isOpen = false;

        float time = 0;
        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(_startRotation, _endRotation, time);
            yield return null;
            time += Time.deltaTime * speed;
        }
    }
}

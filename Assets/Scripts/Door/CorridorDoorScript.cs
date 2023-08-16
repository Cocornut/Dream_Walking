using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    public float forwardDirection = 0;

    private Vector3 startRotation;
    private Vector3 forward;

    private Coroutine animationCoroutine;

    private Transform door;

    [Header("Audio")]
    [SerializeField] private AudioSource budgeSound;
    [SerializeField] private AudioSource creakSound;

    private void Awake()
    {
        door = gameObject.transform;
        startRotation = door.rotation.eulerAngles;
        forward = door.forward;
        manager = GameObject.FindGameObjectWithTag("DoorManager");
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
            Debug.Log("Key: " + key.keyID);
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
        budgeSound.Play();

        Quaternion _startRotation = door.rotation;
        Quaternion _endRotation = Quaternion.Euler(new Vector3(0, startRotation.y + 10f, 0));
        float time = 0f;
        float budgeDuration = 0.1f;

        while (time < budgeDuration)
        {
            door.rotation = Quaternion.Slerp(_startRotation, _endRotation, time);
            yield return null;
            time += Time.deltaTime;
        }

        _startRotation = door.rotation;
        _endRotation = Quaternion.Euler(new Vector3(0, startRotation.y - 10f, 0));
        time = 0f;

        while (time < budgeDuration)
        {
            door.rotation = Quaternion.Slerp(_startRotation, _endRotation, time);
            yield return null;
            time += Time.deltaTime;
        }

        _startRotation = door.rotation;
        _endRotation = Quaternion.Euler(startRotation);
        time = 0f;

        while (time < budgeDuration)
        {
            door.rotation = Quaternion.Slerp(_startRotation, _endRotation, time);
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
                Vector3 doorForward = forward.normalized;
                Vector3 userDirection = (userPosition - door.position).normalized;
                float dot = Vector3.Dot(doorForward, userDirection);
                Debug.Log($"Dot: {dot.ToString("N3")}");
                animationCoroutine = StartCoroutine(DoRotationOpen(dot));
            }
        }
    }

    private IEnumerator DoRotationOpen(float forwardAmount)
    {
        creakSound.Play();


        Quaternion _startRotation = door.rotation;
        Quaternion _endRotation;

        if (forwardAmount <= forwardDirection)
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
            door.rotation = Quaternion.Slerp(_startRotation, _endRotation, time);
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
        creakSound.Play();

        Quaternion _startRotation = door.rotation;
        Quaternion _endRotation = Quaternion.Euler(startRotation);

        isOpen = false;

        float time = 0;
        while (time < 1)
        {
            door.rotation = Quaternion.Slerp(_startRotation, _endRotation, time);
            yield return null;
            time += Time.deltaTime * speed;
        }
    }
}

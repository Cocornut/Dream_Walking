using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundSoundScript : MonoBehaviour
{
    [SerializeField] private AudioSource backgroundMusic;
    private void Update()
    {
        if (!backgroundMusic.isPlaying)
        {
            backgroundMusic.Play();
        }
    }
}

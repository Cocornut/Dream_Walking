using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenOptions : MonoBehaviour
{
    [SerializeField] private AudioSource clickSound;

    public void startOptions()
    {
        clickSound.Play();

        StartCoroutine(LoadOptionsScene());
    }

    private IEnumerator LoadOptionsScene()
    {
        yield return new WaitForSeconds(clickSound.clip.length);

        SceneManager.LoadScene(Scenes.optionsScene);
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenMenu : MonoBehaviour
{
    [SerializeField] private AudioSource clickSound;

    public void startMenu()
    {
        clickSound.Play();
        StartCoroutine(LoadMenuScene());
    }

    private IEnumerator LoadMenuScene()
    {
        yield return new WaitForSeconds(clickSound.clip.length);

        SceneManager.LoadScene(Scenes.menuScene);
    }
}


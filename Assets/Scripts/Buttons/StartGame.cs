using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    [SerializeField] private AudioSource clickSound;

    public void startGame()
    {
        clickSound.Play();
        StartCoroutine(LoadGameScene());
    }

    private IEnumerator LoadGameScene()
    {
        yield return new WaitForSeconds(clickSound.clip.length);

        SceneManager.LoadScene(Scenes.gameScene);
    }
}

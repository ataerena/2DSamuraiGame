using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneController
{
    public static void LoadScene(string sceneName)
    {
        Debug.Log("Loading new Scene: " + sceneName);
        GameObject.FindGameObjectWithTag("UI Animation").GetComponent<Animator>().Play("CloseScene");
    }
    public static void ReloadScene()
    {
        GameObject.FindGameObjectWithTag("UI Animation").GetComponent<Animator>().Play("CloseScene");
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}

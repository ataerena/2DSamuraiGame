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

    public static IEnumerator LoadNextScene()
    {
        Animator animator = GameObject.FindGameObjectWithTag("UI Animation").GetComponent<Animator>();
        animator.Play("CloseScene");

        yield return new WaitUntil(() => IsAnimationComplete(animator, "CloseScene"));

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
    public static IEnumerator ReloadScene()
    {
        Animator animator = GameObject.FindGameObjectWithTag("UI Animation").GetComponent<Animator>();
        animator.Play("CloseScene");

        yield return new WaitUntil(() => IsAnimationComplete(animator, "CloseScene"));

        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    private static bool IsAnimationComplete(Animator animator, string animationName)
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        return info.IsName(animationName) && info.normalizedTime >= 1;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsController : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }
    public void GoToSoundCloud()
    {
        Application.OpenURL("https://on.soundcloud.com/MvNoJ");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

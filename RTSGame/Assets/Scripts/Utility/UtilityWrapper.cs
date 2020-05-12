using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UtilityWrapper : MonoBehaviour
{
    public void SetTmeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }

    public void ToggleActive(GameObject obj)
    {
        if (obj)
        {
            obj.SetActive(!obj.activeInHierarchy);
        }
    }

    public void LoadSceneName(string sceneName)
    {
        if (sceneName.Length == 0) return;

        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

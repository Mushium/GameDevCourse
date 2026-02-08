using EasyTransition;
using UnityEngine;

public class Pause : MonoBehaviour
{
    public GameObject pauseMenu;


    void Start()
    {
        pauseMenu.SetActive(false);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu.activeSelf == false)
            {
                AudioSingleton.Instance.PlayPause();
                pauseMenu.SetActive(true);
                Time.timeScale = 0;
            }
            else
            {
                AudioSingleton.Instance.PlayUnPause();
                pauseMenu.SetActive(false);
                Time.timeScale = 1;
            }
        }
    }



    public void ContinueButton()
    {
        AudioSingleton.Instance.PlayUnPause();
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    public void MainMenuButton()
    {
        Time.timeScale = 1;

        GameMangerSingleton.Instance.HomeScene();
    }
    
    public void RestartButton()
    {
        Time.timeScale = 1;

        GameMangerSingleton.Instance.RestartScene();
    }
}

using System;
using EasyTransition;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject levelsMenu;
    public TransitionSettings transition;
    public float startDelay;

    void Start()
    {
        Main();
    }
    

    public void LoadLevelOne()
    {
        TransitionManager.Instance().Transition(SceneManager.GetActiveScene().buildIndex+1,transition, startDelay);
    }

    public void Main()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        levelsMenu.SetActive(false);
    }

    public void Settings()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
        levelsMenu.SetActive(false);
    }

    public void Levels()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
        levelsMenu.SetActive(true);
    }

    private void Update()
    {
    
    }
    
    public void ChangeScreenMode(int index)
    {
        switch (index)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;

            case 1:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;

            case 2:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }
    }


    public void QuitGame()
    {
        Application.Quit();
    }
}

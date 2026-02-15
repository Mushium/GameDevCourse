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
    public GameObject htpMenu;

    public TransitionSettings transition;
    public float startDelay;

    public GameObject[] stagesUI;

    void Start()
    {
        Main();
        AudioSingleton.Instance.PlayMenu();
        Screen.SetResolution(1920, 1080, true);
    }
    

    public void LoadLevelOne()
    {
        AudioSingleton.Instance.PlayButton();
        TransitionManager.Instance().Transition(SceneManager.GetActiveScene().buildIndex+1,transition, startDelay);
    }
    public void LoadLevelTwo()
    {
        AudioSingleton.Instance.PlayButton();

        TransitionManager.Instance().Transition(SceneManager.GetActiveScene().buildIndex+2,transition, startDelay);
    }
    
    public void LoadLevelThree()
    {
        AudioSingleton.Instance.PlayButton();

        TransitionManager.Instance().Transition(SceneManager.GetActiveScene().buildIndex+3,transition, startDelay);
    }

    public void Main()
    {
        if (levelsMenu.activeSelf || settingsMenu.activeSelf || htpMenu.activeSelf)
        {
            AudioSingleton.Instance.PlayButton();
        }
        mainMenu.SetActive(true);
        htpMenu.SetActive(false);
        settingsMenu.SetActive(false);
        levelsMenu.SetActive(false);
    }

    public void Settings()
    {
        AudioSingleton.Instance.PlayButton();

        mainMenu.SetActive(false);
        htpMenu.SetActive(false);
        settingsMenu.SetActive(true);
        levelsMenu.SetActive(false);
    }

    public void Levels()
    {
        AudioSingleton.Instance.PlayButton();

        mainMenu.SetActive(false);
        htpMenu.SetActive(false);
        settingsMenu.SetActive(false);
        levelsMenu.SetActive(true);


        if (ValueSingleton.Instance.stage == 0)
        {
            stagesUI[0].GetComponent<Animator>().enabled = true;
            
            stagesUI[1].gameObject.SetActive(true);
            stagesUI[2].gameObject.SetActive(false);
            
            stagesUI[3].gameObject.SetActive(true);
            stagesUI[4].gameObject.SetActive(false);
        }
        else if (ValueSingleton.Instance.stage == 1)
        {
            stagesUI[0].GetComponent<Animator>().enabled = false;
            
            stagesUI[1].gameObject.SetActive(false);
            stagesUI[2].gameObject.SetActive(true);
            stagesUI[2].GetComponent<Animator>().enabled = true;
            
            stagesUI[3].gameObject.SetActive(true);
            stagesUI[4].gameObject.SetActive(false);
        }
        else
        {
            stagesUI[0].GetComponent<Animator>().enabled = false;
            stagesUI[2].GetComponent<Animator>().enabled = false;
            stagesUI[4].GetComponent<Animator>().enabled = true;
            
            stagesUI[1].gameObject.SetActive(false);
            stagesUI[2].gameObject.SetActive(true);
            
            stagesUI[3].gameObject.SetActive(false);
            stagesUI[4].gameObject.SetActive(true);
        }
    }

    public void HTP()
    {
        AudioSingleton.Instance.PlayButton();

        mainMenu.SetActive(false);
        htpMenu.SetActive(true);
        settingsMenu.SetActive(false);
        levelsMenu.SetActive(false);
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

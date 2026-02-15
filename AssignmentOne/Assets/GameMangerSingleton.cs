using System.Collections.Generic;
using EasyTransition;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMangerSingleton : MonoBehaviour
{
    public static GameMangerSingleton Instance { get; private set; } 
    public List<GameObject> players = new List<GameObject>();
    public Transform RestartPoint;
    public TransitionSettings transition;
    public float startDelay;

    [Header("UI Elements")]
    public Text CoinText;
    public List<GameObject> HealthUI = new List<GameObject>();
    public GameObject VictoryUI;
    public int Health;
    public Text level;
    public GameObject ChooseWeapon;
    public GameObject swordUI;
    public GameObject spearUI;
    public GameObject SwordPrefab;
    public GameObject SpearPrefab;
    public bool isDialog;
    
    private void Awake()
    {
        isDialog = false;
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject); // Destroy duplicate instances
            return;
        }

        Instance = this; // Set the instance to this object
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            AudioSingleton.Instance.menu.Pause();
            AudioSingleton.Instance.snow.Pause();
            AudioSingleton.Instance.cave.Pause();
            
            AudioSingleton.Instance.PlayWood();
        }
        else if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            
            AudioSingleton.Instance.wood.Pause();
            AudioSingleton.Instance.snow.Pause();
            AudioSingleton.Instance.menu.Pause();
            
            
            AudioSingleton.Instance.PlayCave();
        }
        else if (SceneManager.GetActiveScene().buildIndex == 3)
        {
            AudioSingleton.Instance.menu.Pause();
            AudioSingleton.Instance.wood.Pause();
            AudioSingleton.Instance.cave.Pause();
            
            AudioSingleton.Instance.PlaySnow();
        }
        VictoryUI.SetActive(false);
        GetComponent<PlayerInputManager>().DisableJoining();
        ChooseWeapon.SetActive(true);
        swordUI.SetActive(true);
        spearUI.SetActive(false);
        UpdateHealth();
    }

    public void ConfirmWeapon()
    {
        if (swordUI.activeSelf)
        {
            GameObject obj = Instantiate(SwordPrefab);
            Camera.main.GetComponent<CameraFollow2D>().target = obj.transform;
            GetComponent<PlayerInputManager>().playerPrefab = SwordPrefab;
        }
        else
        {
            GameObject obj = Instantiate(SpearPrefab);
            Camera.main.GetComponent<CameraFollow2D>().target = obj.transform;
            GetComponent<PlayerInputManager>().playerPrefab = SpearPrefab;
        }
        GetComponent<PlayerInputManager>().EnableJoining();
        ChooseWeapon.SetActive(false);
        AudioSingleton.Instance.PlayButton();
    }

    public void NextWeapon()
    {
        if (swordUI.activeSelf)
        {
            swordUI.SetActive(false);
            spearUI.SetActive(true);
        }
        else
        {
            swordUI.SetActive(true);
            spearUI.SetActive(false);
        }
        AudioSingleton.Instance.PlayButton();

    }

    public void UpdateHealth()
    {
        Health = ValueSingleton.Instance.health;
        for (int i = 0; i < 6; i++)
        {
            if (i < Health)
            {
                HealthUI[i].SetActive(true);
            }
            else
            {
                HealthUI[i].SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        CoinText.text =  ValueSingleton.Instance.coin.ToString();
        level.text = ValueSingleton.Instance.level.ToString();

        for (int i = 0; i < HealthUI.Count; i++)
        {
            if (i < Health)
            {
                HealthUI[i].transform.GetChild(0).gameObject.SetActive(true);
                HealthUI[i].transform.GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                HealthUI[i].transform.GetChild(0).gameObject.SetActive(false);
                HealthUI[i].transform.GetChild(1).gameObject.SetActive(true);
            }
        }

        if (Health <= 0)
        {
            RestartScene();
        }
    }

    public void RestartScene()
    {
        AudioSingleton.Instance.PlayButton();

        TransitionManager.Instance().Transition(SceneManager.GetActiveScene().name,transition, startDelay);
    }

    public void HomeScene()
    {
        AudioSingleton.Instance.PlayButton();
        AudioSingleton.Instance.PauseMusic();

        TransitionManager.Instance().Transition("Menu",transition, startDelay);
    }
    public void NextScene()
    {
        AudioSingleton.Instance.PlayButton();
        AudioSingleton.Instance.PauseMusic();


        if(SceneManager.GetActiveScene().buildIndex == 1)
            TransitionManager.Instance().Transition("LevelTwo",transition, startDelay);
        else if(SceneManager.GetActiveScene().buildIndex == 2)
            TransitionManager.Instance().Transition("LevelThree",transition, startDelay);
        else if(SceneManager.GetActiveScene().buildIndex == 3)
            TransitionManager.Instance().Transition("Menu",transition, startDelay);

            
        
    }

    public void Victory()
    {
        VictoryUI.SetActive(true);
        ValueSingleton.Instance.coin += 75;
        foreach (GameObject player in players)
        {
            player.SetActive(false);
        }
        GetComponent<PlayerInputManager>().enabled = false;
        ValueSingleton.Instance.LevelComplete();
        AudioSingleton.Instance.PlayWin();
    }
    
}

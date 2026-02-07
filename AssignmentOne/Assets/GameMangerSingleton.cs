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
    
    
    
    private void Awake()
    {
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
        VictoryUI.SetActive(false);
        UpdateHealth();
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
        TransitionManager.Instance().Transition(SceneManager.GetActiveScene().name,transition, startDelay);
    }

    public void HomeScene()
    {
        TransitionManager.Instance().Transition("Menu",transition, startDelay);
    }
    public void NextScene()
    {
        TransitionManager.Instance().Transition(SceneManager.GetSceneByBuildIndex(SceneManager.GetActiveScene().buildIndex + 1).name,transition, startDelay);
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
    }
    
}

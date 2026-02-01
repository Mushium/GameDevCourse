using System.Collections.Generic;
using EasyTransition;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMangerSingleton : MonoBehaviour
{
    public static GameMangerSingleton Instance { get; private set; } 
    public Transform player;
    public Transform RestartPoint;
    public TransitionSettings transition;
    public float startDelay;
    public int Coins;
    public int Health;
    
    
    [Header("UI Elements")]
    public Text CoinText;
    public List<GameObject> HealthUI = new List<GameObject>();
    
    
    
    
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
        player.position = RestartPoint.position;
        Coins = 0;
        Health = 4;
    }

    // Update is called once per frame
    void Update()
    {
        CoinText.text =  Coins.ToString();

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
}

using EasyTransition;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMangerSingleton : MonoBehaviour
{
    public static GameMangerSingleton Instance { get; private set; } 
    public Transform player;
    public Transform RestartPoint;
    public TransitionSettings transition;
    public float startDelay;

    
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject); // Destroy duplicate instances
            return;
        }

        Instance = this; // Set the instance to this object
        DontDestroyOnLoad(this.gameObject); // Persist across scene loads
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player.position = RestartPoint.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RestartScene()
    {
        TransitionManager.Instance().Transition(SceneManager.GetActiveScene().name,transition, startDelay);
    }
}

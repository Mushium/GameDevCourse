using UnityEngine;
using UnityEngine.SceneManagement;

public class ValueSingleton : MonoBehaviour
{
    public static ValueSingleton Instance { get; private set; }

    public int level;
    public int coin;
    public int health;
    public int stage;

    private void Awake()
    {
        stage = 0;
        level = 1;
        coin = 0;
        health = 4;
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject); // Destroy duplicate instances
            return;
        }
        DontDestroyOnLoad(gameObject);

        Instance = this; // Set the instance to this object
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;

    }


    public void AddHealth()
    {
        health++;
        GameMangerSingleton.Instance.UpdateHealth();
    }


    public void LevelComplete()
    {
        if (stage <= 0 && SceneManager.GetActiveScene().buildIndex == 1)
        {
            stage = 1;
        }
        else if (stage <= 1 && SceneManager.GetActiveScene().buildIndex == 2)
        {
            stage = 2;
        }
        else if (stage <= 2 && SceneManager.GetActiveScene().buildIndex == 3)
        {
            stage = 3;
        }
    }
}

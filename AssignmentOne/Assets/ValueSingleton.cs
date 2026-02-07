using UnityEngine;

public class ValueSingleton : MonoBehaviour
{
    public static ValueSingleton Instance { get; private set; }

    public int level;
    public int coin;
    public int health;

    private void Awake()
    {
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
    }


    public void AddHealth()
    {
        health++;
        GameMangerSingleton.Instance.UpdateHealth();
    }
}

using UnityEngine;
using UnityEngine.UI;

public class AudioSingleton : MonoBehaviour
{
    
    public static AudioSingleton Instance { get; private set; } 
    public float volume;

    public AudioSource jump;
    public AudioSource slash;
    public AudioSource death;
    public AudioSource pause;
    public AudioSource unpause;
    public AudioSource win;
    public AudioSource coin;



    public AudioSource button;

    public AudioSource menu;
    public AudioSource wood;
    public AudioSource cave;
    public AudioSource snow;
    private void Awake()
    {
        volume = 0.5f;
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject); // Destroy duplicate instances
            return;
        }
        DontDestroyOnLoad(gameObject);

        Instance = this; // Set the instance to this object
    }

    public void ChangeVolume(Slider slider)
    {
        volume = slider.value;
        slash.volume = volume;
        jump.volume = volume;
        death.volume = volume;
        coin.volume = volume;
        win.volume = volume;
        pause.volume = volume;
        unpause.volume = volume;
        button.volume = volume;
        menu.volume = volume;
        wood.volume = volume;
        cave.volume = volume;
        snow.volume = volume;
    }


    public void PlaySlash()
    {
        slash.volume = volume;
        slash.Play();
    }

    public void PlayJump()
    {
        jump.volume = volume;
        jump.Play();
    }

    public void PlayDeath()
    {
        death.volume = volume;
        death.Play();
    }
    public void PlayCoin()
    {
        coin.volume = volume;
        coin.Play();
    }

    public void PlayWin()
    {
        win.volume = volume;
        win.Play();
    }

    public void PlayPause()
    {
        pause.volume = volume;
        pause.Play();
    }
    public void PlayUnPause()
    {
        unpause.volume = volume;
        unpause.Play();
    }

    public void PlayButton()
    {
        button.volume = volume;
        button.Play();
    }

    public void PlayMenu()
    {
        menu.volume = volume;
        menu.Play();
    }

    public void PlayWood()
    {
        wood.volume = volume;
        wood.Play();
    }

    public void PlayCave()
    {
        cave.volume = volume;
        cave.Play();
    }

    public void PlaySnow()
    {
        snow.volume = volume;
        snow.Play();
    }

    public void PauseMusic()
    {
        snow.Stop();
        wood.Stop();
        cave.Stop();
        menu.Stop();
    }
}

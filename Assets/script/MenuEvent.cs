using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio ;
public class MenuEvents : MonoBehaviour
{
    
    public Slider volumeSlider ;
    public AudioMixer mixer;
    private void Start()
    {
        Time.timeScale=1;
    }
    public void SetVolume()
    {
        mixer.SetFloat("vol",volumeSlider.value);

    }
    public void LoadLevel(int index)
    {
        SceneManager.LoadScene(index);
    }
}
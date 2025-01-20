using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class PlayerManger : MonoBehaviour
{   
    public static bool isGameOver;
    public GameObject gameOverScreen ;
    public GameObject player;  // Référence au joueur
    public HealthBar healthBar; 
    public GameObject PauseMenu;
    public GameObject errorpanel;
    public static bool error;
  
    // Start is called before the first frame update
    private void Awake()
    {
       
        isGameOver = false ; 
    }
    void Start()
    {
       gameOverScreen.SetActive(false); 
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameOver)
        {
            gameOverScreen.SetActive(true);
        }
        if (error)
        {
            errorPanel();
        }

    }
     public void ReplayLevel()
    {
        // Réinitialise la santé du joueur à 100% (ou à la valeur initiale)
        Health.totalHealth = 1.0f;

        // Réactive le joueur
        player.SetActive(true);

        // Réinitialise la barre de santé
        healthBar.SetSize(1.0f);

        // Désactive l'écran Game Over
        gameOverScreen.SetActive(false);

        // Recharge la scène actuelle pour recommencer
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // Réinitialise l'état du jeu
        isGameOver = false;
    }
    public void PauseGame()
    {
        Time.timeScale=0;
        PauseMenu.SetActive(true);
    }
    public void ResumeGame()
    {
        Time.timeScale=1;
        PauseMenu.SetActive(false);
    }
    public void GoToMenu()
    {
        SceneManager.LoadScene("Mainmenu");
    }
       public void errorPanel()

    {   
        Time.timeScale=0;
        errorpanel.SetActive(true);
    }
     public void ResumeGame2()
    {
        Time.timeScale=1;
        error=false;
        errorpanel.SetActive(false);
    }
}

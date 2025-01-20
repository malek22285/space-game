using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCharacterManager : MonoBehaviour
{
    // Références aux deux personnages dans le niveau
    public GameObject player1;
    public GameObject player2;

    // Référence au CharacterManager qui gère la sélection du personnage
    

    void Start()
    {
        // Charger la sélection du personnage depuis le CharacterManager
        int selectedCharacter = PlayerPrefs.GetInt("selectedOption", 0);

        // Vérifier si la sélection est paire ou impaire
        if (selectedCharacter % 2 == 0)
        {
            // Si la sélection est paire (0, 2, 4, ...)
            ActivatePlayer1();
        }
        else
        {
            // Si la sélection est impaire (1, 3, 5, ...)
            ActivatePlayer2();
        }
    }

    // Méthode pour activer Player 1 et désactiver Player 2
    private void ActivatePlayer1()
    {
        player1.SetActive(true);
        player2.SetActive(false);
    }

    // Méthode pour activer Player 2 et désactiver Player 1
    private void ActivatePlayer2()
    {
        player1.SetActive(false);
        player2.SetActive(true);
    }
}

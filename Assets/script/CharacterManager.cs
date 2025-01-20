using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    // Référence à la base de données de personnages
    public CharacterDatabase characterDB;

    // SpriteRenderer pour afficher l'image du personnage
    public SpriteRenderer artworkSprite;

    // Variable privée pour suivre l'option sélectionnée
    private int selectedOption = 0;

    // Méthode appelée au début du jeu
    void Start()
    {
        // Charger la valeur de selectedOption depuis les PlayerPrefs
        Load();

        // Met à jour l'affichage du personnage en fonction de la valeur de selectedOption
        UpdateCharacter(selectedOption);
    }

    // Méthode pour passer à l'option suivante
    public void NextOption()
    {
        Debug.Log("NextOption method called");

        selectedOption++;

        // Si on dépasse le nombre de personnages, on revient au début
        if (selectedOption >= characterDB.CharacterCount)
        {
            selectedOption = 0;
        }

        // Met à jour l'affichage en fonction du personnage sélectionné
        UpdateCharacter(selectedOption);

        // Sauvegarder la nouvelle sélection dans les PlayerPrefs
        Save();
    }

    // Méthode pour revenir à l'option précédente
    public void BackOption()
    {
        Debug.Log("BackOption method called. Current selectedOption: " + selectedOption);

        // Décrémente l'index du personnage sélectionné
        selectedOption--;

        // Si on est en dessous de 0, on va au dernier personnage
        if (selectedOption < 0)
        {
            selectedOption = characterDB.CharacterCount - 1;
        }

        // Met à jour l'affichage avec le nouveau personnage sélectionné
        UpdateCharacter(selectedOption);

        // Sauvegarder la nouvelle sélection dans les PlayerPrefs
        Save();
    }

    // Méthode privée pour mettre à jour l'affichage avec les informations du personnage
    private void UpdateCharacter(int selectedOption)
    {
        // Récupère le personnage sélectionné à partir de la base de données
        Character character = characterDB.GetCharacter(selectedOption);

        // Met à jour l'image avec les informations du personnage
        artworkSprite.sprite = character.characterSprite;
    }

    // Méthode pour charger la sélection précédemment sauvegardée
    private void Load()
    {
        // Vérifie si la clé "selectedOption" existe dans les PlayerPrefs
        if (PlayerPrefs.HasKey("selectedOption"))
        {
            // Charge la valeur sauvegardée de selectedOption
            selectedOption = PlayerPrefs.GetInt("selectedOption");
        }
        else
        {
            // Si la valeur n'existe pas, initialise à 0
            selectedOption = 0;
        }
    }

    // Méthode pour sauvegarder la sélection actuelle de selectedOption
    private void Save()
    {
        // Enregistre la valeur actuelle de selectedOption dans les PlayerPrefs
        PlayerPrefs.SetInt("selectedOption", selectedOption);
        PlayerPrefs.Save();
    }
    public int GetSelectedCharacterIndex()
{
    return selectedOption; // Renvoie l'index du personnage sélectionné
}
}

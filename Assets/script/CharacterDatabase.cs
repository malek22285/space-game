using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterDatabase : ScriptableObject
{
    public Character[] characters; // Tableau public pour stocker les personnages

    public int CharacterCount
    {
        get 
        { 
            return characters.Length;
        } // Propriété pour obtenir le nombre de personnages
    }

    public Character GetCharacter(int index)
    {
        return characters[index]; // Méthode pour obtenir un personnage par son index
    }
}
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    // Référence à l'objet CharacterManager
    public GameObject characterManager;

    // Méthode pour afficher le CharacterManager
    public void ShowCharacterManager()
    {
        characterManager.SetActive(true); // Active l'objet
    }

    // Méthode pour cacher le CharacterManager
    public void HideCharacterManager()
    {
        characterManager.SetActive(false); // Désactive l'objet
    }

    private void Start()
    {
        // S'assurer que le CharacterManager est désactivé au démarrage
        if (characterManager != null)
        {
            characterManager.SetActive(false);
        }
    }
}

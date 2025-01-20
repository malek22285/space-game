using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Références aux deux joueurs
    public GameObject player1;
    public GameObject player2;

    // Décalage de la caméra
    public float offset;
    public float offsetSmoothing;
    
    // Position actuelle de la caméra
    private Vector3 playerPosition;
    
    // Référence au joueur actuellement suivi
    private GameObject activePlayer;

    // Start is called before the first frame update
    void Start()
    {
        // Charger la sélection du personnage depuis PlayerPrefs
        int selectedCharacter = PlayerPrefs.GetInt("selectedOption", 0);
        Debug.Log("Selected Character from PlayerPrefs: " + selectedCharacter);
        
        // Déterminer le joueur actif en fonction de la sélection
        if (selectedCharacter % 2 == 0)
        {
            // Si la sélection est paire, suivre player1
            SetActivePlayer(player1);
        }
        else
        {
            // Si la sélection est impaire, suivre player2
            SetActivePlayer(player2);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (activePlayer == null)
        {
            // Si aucun joueur n'est actif, ne rien faire
            return;
        }

        // Calculer la position de la caméra en fonction du joueur actif
        playerPosition = new Vector3(activePlayer.transform.position.x, activePlayer.transform.position.y, transform.position.z);

        // Déplacer la caméra en fonction de l'orientation du joueur
        if (activePlayer.transform.localScale.x > 0f)
        {
            playerPosition = new Vector3(playerPosition.x + offset, playerPosition.y, playerPosition.z);
        }
        else
        {
            playerPosition = new Vector3(playerPosition.x - offset, playerPosition.y, playerPosition.z);
        }

        // Lerp pour lisser le mouvement de la caméra
        transform.position = Vector3.Lerp(transform.position, playerPosition, offsetSmoothing * Time.deltaTime);
    }

    // Fonction pour définir le joueur actif
    private void SetActivePlayer(GameObject player)
    {
        activePlayer = player;
    }
}
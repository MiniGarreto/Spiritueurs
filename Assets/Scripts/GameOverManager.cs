using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("Scènes")]
    public string menuSceneName = "Menu";           // Nom de la scène du menu principal
    public string gameSceneName = "Game";           // Nom de la scène de jeu principale
    public string gameOverSceneName = "GameOver";   // Nom de la scène screamer

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Appelé quand le joueur est touché par le squelette
    public void TriggerGameOver()
    {
        SceneManager.LoadScene(gameOverSceneName);
    }

    // Appelé après le screamer pour retourner au menu
    public void ReturnToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    // Pour relancer directement le jeu (si besoin)
    public void RestartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public enum DeathType
{
    Skeleton,
    Shark
}

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("Scènes")]
    public string menuSceneName = "Menu";                     // Nom de la scène du menu principal
    public string gameSceneName = "Game";                     // Nom de la scène de jeu principale
    public string skeletonGameOverSceneName = "GameOverSkeleton";  // Scène screamer squelette
    public string sharkGameOverSceneName = "GameOverShark";        // Scène screamer requin
    public string smilerGameOverSceneName = "GameOverSmiler";      // Scène screamer smiler

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

    public void TriggerSmilerGameOver()
    {
        SceneManager.LoadScene(smilerGameOverSceneName);
    }

    // Appelé quand le joueur est touché par le squelette
    public void TriggerSkeletonGameOver()
    {
        SceneManager.LoadScene(skeletonGameOverSceneName);
    }

    // Appelé quand le joueur est touché par le requin
    public void TriggerSharkGameOver()
    {
        SceneManager.LoadScene(sharkGameOverSceneName);
    }

    // Ancienne méthode pour compatibilité (utilise squelette par défaut)
    public void TriggerGameOver()
    {
        TriggerSkeletonGameOver();
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

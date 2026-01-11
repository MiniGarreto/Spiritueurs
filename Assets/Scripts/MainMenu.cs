using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Configuration")]
    public string gameSceneName = "Game";  // Nom de la scène de jeu

    // Appelé quand on clique sur le bouton "Jouer"
    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // Appelé quand on clique sur le bouton "Quitter" (optionnel)
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}

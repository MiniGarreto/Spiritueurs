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

    public string menuSceneName = "Menu";                    
    public string gameSceneName = "Game";                    
    public string skeletonGameOverSceneName = "GameOverSkeleton"; 
    public string sharkGameOverSceneName = "GameOverShark";
    public string smilerGameOverSceneName = "GameOverSmiler";

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

    public void TriggerSkeletonGameOver()
    {
        SceneManager.LoadScene(skeletonGameOverSceneName);
    }

    public void TriggerSharkGameOver()
    {
        SceneManager.LoadScene(sharkGameOverSceneName);
    }

    public void TriggerGameOver()
    {
        TriggerSkeletonGameOver();
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}

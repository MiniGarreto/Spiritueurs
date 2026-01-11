using UnityEngine;

public class SharkSpawner : MonoBehaviour
{
    [Header("Références")]
    public ScaleOnMaxHeight waterRiseSystem;  // Le système qui scale l'eau de la pièce
    public Transform targetWater;              // L'eau qui monte (pour vérifier si elle est au max)
    public float maxWaterYScale = 2f;          // Le scale Y quand l'eau est au maximum

    [Header("Spawn du requin")]
    public GameObject sharkPrefab;
    public Transform sharkSpawnPoint;
    public Transform playerTransform;

    [Header("Waypoints")]
    public Transform[] waypoints;  // Points de passage avant d'aller vers le joueur

    [Header("Configuration")]
    public float spawnDelay = 1f;  // Délai avant de spawn le requin après que l'eau soit pleine

    private bool sharkSpawned = false;
    private bool waterIsFull = false;
    private float fullWaterTimer = 0f;

    void Update()
    {
        if (sharkSpawned) return;

        // Vérifier si l'eau a atteint son niveau maximum
        if (targetWater != null)
        {
            if (targetWater.localScale.y >= maxWaterYScale - 0.01f)
            {
                waterIsFull = true;
            }
        }

        // Si l'eau est pleine, attendre le délai puis spawner le requin
        if (waterIsFull)
        {
            fullWaterTimer += Time.deltaTime;

            if (fullWaterTimer >= spawnDelay)
            {
                SpawnShark();
            }
        }
    }

    private void SpawnShark()
    {
        if (sharkPrefab == null || sharkSpawnPoint == null)
        {
            Debug.LogWarning("SharkSpawner: Prefab ou spawn point manquant!");
            return;
        }

        // Créer le requin
        GameObject shark = Instantiate(sharkPrefab, sharkSpawnPoint.position, sharkSpawnPoint.rotation);

        // Activer l'IA du requin et lui passer les waypoints
        SharkAI sharkAI = shark.GetComponent<SharkAI>();
        if (sharkAI != null)
        {
            sharkAI.waypoints = waypoints;  // Passer les waypoints de la scène
            sharkAI.Activate(playerTransform);
        }

        sharkSpawned = true;
    }

    // Méthode pour reset si besoin (nouvelle partie)
    public void ResetSpawner()
    {
        sharkSpawned = false;
        waterIsFull = false;
        fullWaterTimer = 0f;
    }
}

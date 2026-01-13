using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ToiletPaperSpawner : MonoBehaviour
{
    public GameObject toiletPaperPrefab;
    public Transform spawnPoint;
    public float spawnCooldown = 0.3f;

    private float lastSpawnTime = 0f;

    void Awake()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        interactable.selectEntered.AddListener(OnInteract);
    }

    void OnInteract(SelectEnterEventArgs args)
    {
        if (Time.time - lastSpawnTime < spawnCooldown)
            return;

        Instantiate(toiletPaperPrefab, spawnPoint.position, spawnPoint.rotation);
        lastSpawnTime = Time.time;
    }
}

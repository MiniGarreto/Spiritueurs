using UnityEngine;
using System.Collections;

public class BathroomTap : MonoBehaviour
{
    [SerializeField] private ParticleSystem waterStream;
    [SerializeField] private AudioSource tapSound;
    [SerializeField] private float minWaitTime = 15f;
    [SerializeField] private float maxWaitTime = 30f;

    [Header("Contrôle par DifficultyManager")]
    public bool waitForActivation = false;  // Si true, attend l'activation du DifficultyManager
    

    private bool isTapOpen = false;
    private bool isActivated = false;
    private Coroutine tapRoutine;

    void Start()
    {
        if (waterStream != null)
            waterStream.Stop();
        
        // Ajoute un collider si nécessaire
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<SphereCollider>();
        }

        // Si pas de contrôle par difficulté, démarrer directement
        if (!waitForActivation)
        {
            isActivated = true;
            tapRoutine = StartCoroutine(TapRoutine());
        }
    }

    // Appelé par le DifficultyManager pour activer ce robinet
    public void ActivateByDifficulty()
    {
        if (isActivated) return;
        
        isActivated = true;
        tapRoutine = StartCoroutine(TapRoutine());
        Debug.Log($"[BathroomTap] Robinet {gameObject.name} activé!");
    }

    private IEnumerator TapRoutine()
    {
        while (true)
        {
            // Attendre avant d'ouvrir le robinet
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            // Ouvrir le robinet
            OpenTap();

            // Attendre que le robinet soit fermé (aucune limite de temps)
            yield return new WaitWhile(() => isTapOpen);
        }
    }

    private void OpenTap()
    {
        isTapOpen = true;

        if (waterStream != null)
            waterStream.Play();

        if (tapSound != null)
        {
            tapSound.loop = true;
            tapSound.Play();
        }
    }

    private void CloseTap()
    {
        isTapOpen = false;

        if (waterStream != null)
            waterStream.Stop();

        if (tapSound != null)
            tapSound.Stop();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Ferme immédiatement le robinet sur toute collision
        CloseImmediately();
    }

    private void OnTriggerEnter(Collider collision)
    {
        // Ferme immédiatement le robinet sur toute entrée de trigger
        CloseImmediately();
    }

    public void CloseImmediately()
    {
        // Arrête la coroutine et ferme immédiatement le robinet
        if (tapRoutine != null)
            StopCoroutine(tapRoutine);
        
        CloseTap();
        
        // Redémarre une nouvelle routine
        tapRoutine = StartCoroutine(TapRoutine());
    }

    public bool IsTapOpen => isTapOpen;
}

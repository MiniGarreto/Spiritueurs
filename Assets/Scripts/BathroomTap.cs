using UnityEngine;
using System.Collections;

public class BathroomTap : MonoBehaviour
{
    [SerializeField] private ParticleSystem waterStream;
    [SerializeField] private AudioSource tapSound;
    public float minWaitTime = 15f;
    public float maxWaitTime = 30f;

    public bool waitForActivation = false; 
    

    private bool isTapOpen = false;
    private bool isActivated = false;
    private Coroutine tapRoutine;

    void Start()
    {
        if (waterStream != null)
            waterStream.Stop();
        
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<SphereCollider>();
        }

        if (!waitForActivation)
        {
            isActivated = true;
            tapRoutine = StartCoroutine(TapRoutine());
        }
    }

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
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            OpenTap();

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
        CloseImmediately();
    }

    private void OnTriggerEnter(Collider collision)
    {
        CloseImmediately();
    }

    public void CloseImmediately()
    {
        if (tapRoutine != null)
            StopCoroutine(tapRoutine);
        
        CloseTap();
        
        tapRoutine = StartCoroutine(TapRoutine());
    }

    public bool IsTapOpen => isTapOpen;
}

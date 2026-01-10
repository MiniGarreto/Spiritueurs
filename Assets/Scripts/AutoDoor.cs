using UnityEngine;
using System.Collections;

public class AutoDoor : MonoBehaviour
{
    [Header("Référence")]
    public Transform pivotPorte;

    [Header("Ouverture")]
    public float baseOpenAngle = 90f;   // angle de base
    public float baseOpenSpeed = 10f;   // vitesse ouverture normale

    [Header("Fermeture")]
    public float fastCloseSpeed = 60f;  // vitesse fermeture rapide

    [Header("Temps d'attente pour réouverture")]
    public float minDelay = 2f;
    public float maxDelay = 5f;

    [Header("Jitter")]
    public float angleJitter = 10f;
    public float speedJitter = 3f;

    private float targetAngle; 
    private float currentSpeed;
    private float currentAngle = 0f;

    private bool opening = false;
    private bool closing = false;
    private bool waitingToOpen = false;

    void Start()
    {
        float initialDelay = Random.Range(minDelay, maxDelay);
        StartCoroutine(WaitAndOpen(initialDelay));
    }

    void Update()
    {
        if (opening)
        {
            currentAngle += currentSpeed * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, 0f, targetAngle);

            if (currentAngle >= targetAngle)
            {
                opening = false;
            }
        }
        else if (closing)
        {
            currentAngle -= currentSpeed * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, 0f, targetAngle);

            // Quand la porte est complètement fermée, on peut lancer le timer
            if (currentAngle <= 0f && !waitingToOpen)
            {
                waitingToOpen = true;
                float delay = Random.Range(minDelay, maxDelay);
                StartCoroutine(WaitAndOpen(delay));
            }
        }

        pivotPorte.localRotation = Quaternion.Euler(0f, -currentAngle, 0f);
    }

    // Appelée quand la porte est tapée par le papier toilette
    public void CloseDoorFast()
    {
        opening = false;
        closing = true;
        waitingToOpen = false;  // on ne relance pas le timer maintenant
        currentSpeed = fastCloseSpeed;
    }

    private IEnumerator WaitAndOpen(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Nouveau targetAngle avec jitter
        targetAngle = baseOpenAngle + Random.Range(-angleJitter, angleJitter);

        // Nouvelle vitesse avec jitter
        currentSpeed = baseOpenSpeed + Random.Range(-speedJitter, speedJitter);
        currentSpeed = Mathf.Max(1f, currentSpeed);

        closing = false;
        opening = true;
        waitingToOpen = false;
    }
}

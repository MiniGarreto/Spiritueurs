using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class ScreamerScene : MonoBehaviour
{
    [Header("Références")]
    public Animator monsterAnimator;        // Animator du monstre screamer
    public Transform monsterTransform;      // Position du monstre
    public Transform playerCameraPosition;  // Position où placer la caméra du joueur
    public AudioSource screamerAudio;       // Son du screamer

    [Header("Configuration")]
    public string screamerAnimationTrigger = "Screamer";  // Nom du trigger d'animation
    public float screamerDuration = 3f;     // Durée avant de retourner au jeu
    public float delayBeforeAnimation = 0.5f; // Petit délai avant le screamer

    private void Start()
    {
        // Désactiver les mouvements du joueur VR
        DisablePlayerMovement();

        // Positionner le joueur devant le monstre
        PositionPlayer();

        // Lancer le screamer
        StartCoroutine(PlayScreamer());
    }

    private void DisablePlayerMovement()
    {
        // Désactiver tous les locomotion providers
        var locomotionProviders = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider>();
        foreach (var provider in locomotionProviders)
        {
            provider.enabled = false;
        }

        // Désactiver les contrôleurs XR
        var xrControllers = FindObjectsOfType<XRBaseController>();
        foreach (var controller in xrControllers)
        {
            controller.enabled = false;
        }
    }

    private void PositionPlayer()
    {
        // Trouver le XR Origin/Rig
        var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin != null && playerCameraPosition != null)
        {
            xrOrigin.transform.position = playerCameraPosition.position;
            xrOrigin.transform.rotation = playerCameraPosition.rotation;
        }
    }

    private IEnumerator PlayScreamer()
    {
        yield return new WaitForSeconds(delayBeforeAnimation);

        // Jouer l'animation du monstre
        if (monsterAnimator != null)
        {
            monsterAnimator.SetTrigger(screamerAnimationTrigger);
        }

        // Jouer le son
        if (screamerAudio != null)
        {
            screamerAudio.Play();
        }

        // Attendre la fin du screamer
        yield return new WaitForSeconds(screamerDuration);

        // Retourner au menu
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ReturnToMenu();
        }
    }
}

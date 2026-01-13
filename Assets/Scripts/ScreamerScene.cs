using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class ScreamerScene : MonoBehaviour
{

    public Animator monsterAnimator;       
    public Transform monsterTransform;   
    public Transform playerCameraPosition;  
    public AudioSource screamerAudio;     
    public string screamerAnimationTrigger = "Screamer"; 
    public float screamerDuration = 3f;
    public float delayBeforeAnimation = 0.5f;

    private void Start()
    {
        DisablePlayerMovement();
        StartCoroutine(PlayScreamer());
    }

    private void DisablePlayerMovement()
    {
        var locomotionProviders = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider>();
        foreach (var provider in locomotionProviders)
        {
            provider.enabled = false;
        }

        var xrControllers = FindObjectsOfType<XRBaseController>();
        foreach (var controller in xrControllers)
        {
            controller.enabled = false;
        }
    }

    private void PositionPlayer()
    {
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

        if (monsterAnimator != null)
        {
            monsterAnimator.SetTrigger(screamerAnimationTrigger);
        }
        if (screamerAudio != null)
        {
            screamerAudio.Play();
        }

        yield return new WaitForSeconds(screamerDuration);

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ReturnToMenu();
        }
    }
}

using System.Collections;
using UnityEngine;

public class PhoneFlashCone : MonoBehaviour
{
    public Light flashLight;
    public bool startLightDisabled = true;

    public float flashDuration = 0.1f;
    public float range = 10f;
    [Range(0f, 180f)]
    public float coneAngle = 60f;

    public LayerMask enemyMask;

    public float cooldown = 1f;

    public AudioClip flashSfx;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    bool ready = true;

    public void TriggerFlash()
    {
        if (!ready) return;
        StartCoroutine(DoFlash());
    }

    IEnumerator DoFlash()
    {
        ready = false;
        if (flashLight != null) flashLight.enabled = true;

        if (flashSfx != null) AudioSource.PlayClipAtPoint(flashSfx, transform.position, sfxVolume);

        var hits = Physics.OverlapSphere(transform.position, range, enemyMask);
        foreach (var col in hits)
        {
            Vector3 dir = (col.transform.position - transform.position);
            Vector3 dirN = dir.normalized;
            if (Vector3.Angle(transform.forward, dirN) <= coneAngle * 0.5f)
            {
                if (Physics.Raycast(transform.position, dirN, out var hit, range))
                {
                    if (hit.transform == col.transform || hit.transform.IsChildOf(col.transform))
                    {
                        col.transform.GetComponentInParent<IFlashable>()?.OnFlashed();
                    }
                }
            }
        }

        yield return new WaitForSeconds(flashDuration);
        if (flashLight != null) flashLight.enabled = false;
        yield return new WaitForSeconds(cooldown);
        ready = true;
    }

    private void Awake()
    {
        if (startLightDisabled && flashLight != null)
            flashLight.enabled = false;
    }

    private void OnValidate()
    {
        if (flashLight == null) flashLight = GetComponentInChildren<Light>();
        if (flashLight != null && startLightDisabled && !Application.isPlaying)
            flashLight.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);

        var forward = transform.forward;
        float half = coneAngle * 0.5f;
        var left = Quaternion.AngleAxis(-half, transform.up) * forward;
        var right = Quaternion.AngleAxis(half, transform.up) * forward;
        Gizmos.DrawLine(transform.position, transform.position + left * range);
        Gizmos.DrawLine(transform.position, transform.position + right * range);
    }
}
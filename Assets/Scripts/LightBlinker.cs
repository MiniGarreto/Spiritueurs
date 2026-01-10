using UnityEngine;

public class LightBlinker : MonoBehaviour
{
    [Header("Références")]
    public Light myLight;               // La lumière à contrôler
    public Renderer targetRenderer;     // L’objet dont le matériau va changer
    public Material materialOn;         // Matériau quand la lumière est allumée
    public Material materialOff;        // Matériau quand la lumière est éteinte

    [Header("Paramètres")]
    public float interval = 10f;        // Intervalle entre les "extinctions"
    public float offDuration = 2f;      // Durée pendant laquelle la lumière est éteinte

    private float timer = 0f;
    private bool isLightOn = true;

    void Start()
    {
        if(myLight == null)
            myLight = GetComponent<Light>();

        if (targetRenderer == null)
            Debug.LogWarning($"{name}: 'targetRenderer' n'est pas assigné. Le matériau ne pourra pas être changé.");

        if (materialOn == null || materialOff == null)
            Debug.LogWarning($"{name}: 'materialOn' ou 'materialOff' n'est pas assigné.");

        // S'assurer que la lumière commence allumée
        SetLight(true);
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Si on atteint l'intervalle, éteindre la lumière
        if (isLightOn && timer >= interval)
        {
            SetLight(false);
            timer = 0f;
        }
        // Si la lumière est éteinte et qu'on a atteint la durée d'extinction
        else if (!isLightOn && timer >= offDuration)
        {
            SetLight(true);
            timer = 0f;
        }
    }

    void SetLight(bool state)
    {
        isLightOn = state;

        if (myLight != null)
            myLight.enabled = state;
        else
            Debug.LogWarning($"{name}: 'myLight' n'est pas assignée.");

        if (targetRenderer == null)
        {
            Debug.LogWarning($"{name}: 'targetRenderer' est null — impossible de changer le matériau.");
            return;
        }

        Material chosen = state ? materialOn : materialOff;
        if (chosen == null)
        {
            Debug.LogWarning($"{name}: matériau pour l'état {(state ? "On" : "Off")} est null.");
            return;
        }

        // Supporter les objets avec plusieurs matériaux : remplacer tous les slots
        var mats = targetRenderer.materials; // this returns a copy
        if (mats != null && mats.Length > 0)
        {
            for (int i = 0; i < mats.Length; i++)
                mats[i] = chosen;
            targetRenderer.materials = mats;
            Debug.Log($"{name}: matériau défini sur {(state ? "On" : "Off")} pour {targetRenderer.gameObject.name} (slots: {mats.Length}).");
        }
        else
        {
            // Fallback
            targetRenderer.material = chosen;
            Debug.Log($"{name}: matériau défini sur {(state ? "On" : "Off")} (single material). ");
        }
    }
}

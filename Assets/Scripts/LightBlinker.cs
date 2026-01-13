using UnityEngine;

public class LightBlinker : MonoBehaviour
{
    public Light myLight;              
    public Renderer targetRenderer;     
    public Material materialOn;         
    public Material materialOff;       

    public float interval = 10f;
    public float offDuration = 2f; 

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

        SetLight(true);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (isLightOn && timer >= interval)
        {
            SetLight(false);
            timer = 0f;
        }
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

        var mats = targetRenderer.materials; 
        if (mats != null && mats.Length > 0)
        {
            for (int i = 0; i < mats.Length; i++)
                mats[i] = chosen;
            targetRenderer.materials = mats;
            Debug.Log($"{name}: matériau défini sur {(state ? "On" : "Off")} pour {targetRenderer.gameObject.name} (slots: {mats.Length}).");
        }
        else
        {
            targetRenderer.material = chosen;
            Debug.Log($"{name}: matériau défini sur {(state ? "On" : "Off")} (single material). ");
        }
    }
}

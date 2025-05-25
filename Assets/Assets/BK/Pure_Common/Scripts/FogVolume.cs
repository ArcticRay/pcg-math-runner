using UnityEngine;
using BKPureNature;

namespace BKPureNature
{
public class FogVolumeTrigger : MonoBehaviour
{
    public Color fogColor = Color.white;
    public float fogDensity = 0.01f;

    private Color originalFogColor;
    private float originalFogDensity;
    private bool originalFogEnabled;
    private bool originalOverrideFogColor;

    public BK_EnvironmentManager envManager;

    private void Start()
    {
        // Attempt to find the Environment Manager in the scene
        envManager = FindObjectOfType<BK_EnvironmentManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            // Save original fog settings
            originalFogColor = RenderSettings.fogColor;
            originalFogDensity = RenderSettings.fogDensity;
            originalFogEnabled = RenderSettings.fog;

            if (envManager != null)
            {
                // Save the original override state and disable it if necessary
                originalOverrideFogColor = envManager.overrideFogColor;
                envManager.overrideFogColor = false;
            }

            // Override fog settings
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fog = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            // Restore original fog settings
            RenderSettings.fogColor = originalFogColor;
            RenderSettings.fogDensity = originalFogDensity;
            RenderSettings.fog = originalFogEnabled;

            if (envManager != null)
            {
                // Restore the original override state
                envManager.overrideFogColor = originalOverrideFogColor;
            }
        }
    }
}
}

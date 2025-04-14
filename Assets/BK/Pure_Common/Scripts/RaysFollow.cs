using UnityEngine;

namespace BKPureNature
{
[ExecuteInEditMode]
public class ParticleFollowLightManager : MonoBehaviour
{
    public Light directionalLight;
    public ParticleSystem[] particleSystems;

    private Quaternion lastLightRotation;

    void Start()
    {
        if (directionalLight != null)
        {
            lastLightRotation = directionalLight.transform.rotation;
        }
    }

    void Update()
    {
        if (directionalLight != null && directionalLight.transform.rotation != lastLightRotation)
        {
            UpdateParticles();
            lastLightRotation = directionalLight.transform.rotation;
        }
    }

    void UpdateParticles()
    {
        foreach (var ps in particleSystems)
        {
            if (ps != null)
            {
                UpdateRotation(ps);
                UpdateAlpha(ps);
            }
        }
    }

    void UpdateRotation(ParticleSystem ps)
    {
        // Offset particle system's rotation to match DirectionalLight projection axis
        Quaternion lightRotation = directionalLight.transform.rotation;
        Quaternion offsetRotation = Quaternion.Euler(-90, 0, 0);
        ps.transform.rotation = lightRotation * offsetRotation;
    }

    void UpdateAlpha(ParticleSystem ps)
    {
        float lightRotationX = directionalLight.transform.eulerAngles.x;

        // Normalize the rotation angle
        if (lightRotationX > 180)
        {
            lightRotationX -= 360;
        }

        // Determine the alpha value based on the light's rotation angle
        float alpha = 0f;
        if (lightRotationX >= 20 && lightRotationX <= 170)
        {
            // Map the angle to the alpha range
            alpha = Mathf.Clamp01((170 - Mathf.Abs(lightRotationX)) / 150);
        }

        // Update the particle system's color with the new alpha value
        var mainModule = ps.main;
        Color startColor = mainModule.startColor.color;
        startColor.a = alpha;
        mainModule.startColor = startColor;
    }
}
}

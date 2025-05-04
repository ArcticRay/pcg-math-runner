using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BKPureNature
{
    [ExecuteInEditMode]
public class BK_EnvironmentManager : MonoBehaviour
{
    public Light directionalLight;

    public Gradient sunColorGradient;
    public Gradient fogColorGradient;
    public Gradient cloudColorGradient;
    public Gradient scatteringColorGradient;

    [Header("Color Gradients Enable Flags")]
    public bool overrideSunColor = true;
    public bool overrideFogColor = true;
    public bool overrideCloudColor = true;

    [Header("Base Wind")]
    [Tooltip("Base wind animate the trunks")]
    [Range(0f, 5f)]
    public float baseWindPower = 3f;
    [Tooltip("Base wind animate the trunks")]
    public float baseWindSpeed = 1f;

    [Header("Wind Burst")]
    [Tooltip("Bursts are managed by a moving World-Space noise that multiply the base wind speed and power")]
    [Range(0f, 10f)]
    public float burstsPower = 0.5f;
    [Tooltip("Speed of the Bursts noise")]
    public float burstsSpeed = 5f;
    [Tooltip("Size of the Bursts noise in Word-Space")]
    public float burstsScale = 10f;

    [Header("Micro Wind")]
    [Tooltip("Micro wind animate the leaves")]
    [Range(0f, 1f)]
    public float microPower = 0.1f;
    [Tooltip("Micro wind animate the leaves")]
    public float microSpeed = 1f;
    [Tooltip("Micro wind animate the leaves")]
    public float microFrequency = 3f;

    [Space(10)]
    public float renderDistance = 30f;

    [Space(10)]
    public float Altitude = 1000f;
    public float volumeSize = 500f;
    public int volumeSamples = 25;

    private float volumeOffset;
    private Mesh quadMesh;
    private Matrix4x4[] matrices;

    [Space(10)]
    [Tooltip("Material for the clouds")]
    public Material cloudsMaterial;

    private bool hasIssuedMaterialWarning = false;

    void Awake()
    {
        quadMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
        matrices = new Matrix4x4[volumeSamples];
    }

    void Update()
    {
        UpdateEnvironment();
        UpdateCloudsVolume();
        UpdateLighting();
    }

    private void UpdateEnvironment()
    {
        Shader.SetGlobalFloat("WindPower", baseWindPower);
        Shader.SetGlobalFloat("WindSpeed", baseWindSpeed);
        Shader.SetGlobalFloat("WindBurstsPower", burstsPower);
        Shader.SetGlobalFloat("WindBurstsSpeed", burstsSpeed);
        Shader.SetGlobalFloat("WindBurstsScale", burstsScale);
        Shader.SetGlobalFloat("MicroPower", microPower);
        Shader.SetGlobalFloat("MicroSpeed", microSpeed);
        Shader.SetGlobalFloat("MicroFrequency", microFrequency);
        Shader.SetGlobalFloat("GrassRenderDist", renderDistance);
    }

    private void UpdateCloudsVolume()
    {
        volumeSamples = Mathf.Max(1, volumeSamples);
        volumeSize = Mathf.Max(0, volumeSize);

        if (cloudsMaterial == null)
        {
            return;
        }

        // Dynamically adjust the size of the matrices array to match volumeSamples
        if (matrices.Length != volumeSamples)
        {
            matrices = new Matrix4x4[volumeSamples];
        }

        if (!cloudsMaterial.HasProperty("_ScatteringColor"))
        {
            if (!hasIssuedMaterialWarning)
            {
                Debug.LogWarning("The assigned material in the Cloud material slot of the EnvironmentManager isn't supported.");
                hasIssuedMaterialWarning = true;
            }
            return;
        }
        else
        {
            hasIssuedMaterialWarning = false;
        }

        cloudsMaterial.SetFloat("_cloudsPosition", Altitude);
        cloudsMaterial.SetFloat("_cloudsHeight", volumeSize);

        volumeOffset = volumeSize / volumeSamples / 2f;
        Vector3 cloudsStartPosition = new Vector3(0, Altitude, 0) + (Vector3.up * (volumeOffset * volumeSamples / 2f));

        for (int i = 0; i < volumeSamples; i++)
        {
            matrices[i] = Matrix4x4.TRS(cloudsStartPosition - (Vector3.up * volumeOffset * i), Quaternion.Euler(-90, 0, 0), new Vector3(10000, 10000, 10000));
        }

        Graphics.DrawMeshInstanced(quadMesh, 0, cloudsMaterial, matrices, volumeSamples);
    }

    private void UpdateLighting()
    {
        if (directionalLight == null) return;

        float dot = Vector3.Dot(directionalLight.transform.forward, Vector3.up);
        float time = (dot + 1f) / 2f;

        if (overrideFogColor)
            RenderSettings.fogColor = fogColorGradient.Evaluate(time);
        if (overrideSunColor)
            directionalLight.color = sunColorGradient.Evaluate(time);

        if (cloudsMaterial != null && cloudsMaterial.HasProperty("_ScatteringColor") && overrideCloudColor)
        {
            cloudsMaterial.SetColor("_ScatteringColor", scatteringColorGradient.Evaluate(time));
        }
        else if (cloudsMaterial == null)
        {
            Debug.LogError("cloudsMaterial is null. Please assign a material.");
        }
    }
}
}

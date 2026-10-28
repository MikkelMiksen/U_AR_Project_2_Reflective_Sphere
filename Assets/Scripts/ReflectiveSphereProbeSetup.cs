using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Milestone 1 helper for the reflective-sphere demo.
/// Creates a reflective sphere and a realtime Reflection Probe so the scene can
/// be tested without grabbing or physics yet.
/// </summary>
public class ReflectiveSphereProbeSetup : MonoBehaviour
{
    [Header("Sphere")]
    [SerializeField] private Vector3 sphereLocalPosition = new Vector3(0f, 0.75f, 0f);

    [Header("Probe")]
    [SerializeField] private Vector3 probeOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private Vector3 probeSize = new Vector3(8f, 8f, 8f);
    [SerializeField] private int probeResolution = 256;

    private GameObject reflectiveSphere;
    private GameObject reflectionProbeObject;

    private void Reset()
    {
        Setup();
    }

    private void Awake()
    {
        Setup();
    }

    [ContextMenu("Setup Reflective Sphere")]
    public void Setup()
    {
        EnsureSphere();
        ConfigureSphereMaterial();
        EnsureReflectionProbe();
        ConfigureReflectionProbe();
    }

    private void EnsureSphere()
    {
        if (reflectiveSphere == null)
        {
            reflectiveSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            reflectiveSphere.name = "ReflectiveSphere";
            reflectiveSphere.transform.SetParent(transform, false);
            reflectiveSphere.transform.localPosition = sphereLocalPosition;
        }

        reflectiveSphere.transform.SetParent(transform, false);
        reflectiveSphere.transform.localPosition = sphereLocalPosition;
    }

    private void ConfigureSphereMaterial()
    {
        var renderer = reflectiveSphere.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            return;
        }

        var material = new Material(Shader.Find("Standard"));
        material.name = "M_ReflectiveSphere";
        material.SetFloat("_Metallic", 1f);
        material.SetFloat("_Glossiness", 1f);
        material.SetColor("_Color", new Color(0.9f, 0.9f, 1f, 1f));

        renderer.sharedMaterial = material;
    }

    private void EnsureReflectionProbe()
    {
        if (reflectionProbeObject == null)
        {
            reflectionProbeObject = new GameObject("RoomReflectionProbe");
            reflectionProbeObject.transform.SetParent(transform, false);
        }

        var probe = reflectionProbeObject.GetComponent<ReflectionProbe>();
        if (probe == null)
        {
            reflectionProbeObject.AddComponent<ReflectionProbe>();
        }
    }

    private void ConfigureReflectionProbe()
    {
        var probe = reflectionProbeObject.GetComponent<ReflectionProbe>();
        if (probe == null)
        {
            return;
        }

#pragma warning disable CS0618
        probe.type = ReflectionProbeType.Cube;
#pragma warning restore CS0618

        probe.mode = ReflectionProbeMode.Realtime;
        probe.refreshMode = ReflectionProbeRefreshMode.EveryFrame;
        probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.Off;
        probe.clearFlags = ReflectionProbeClearFlags.Skybox;
        probe.resolution = probeResolution;
        probe.size = probeSize;
        probe.nearClipPlane = 0.1f;
        probe.farClipPlane = 25f;
        probe.center = reflectiveSphere.transform.position + probeOffset;

        // Render once immediately so the reflection is visible at runtime.
        probe.RenderProbe();
    }
}

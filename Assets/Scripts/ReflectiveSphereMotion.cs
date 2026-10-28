using UnityEngine;

/// <summary>
/// Milestone 2: move the reflective sphere in real time and keep the nearby
/// reflection probe tracking its motion so the reflection updates immediately.
/// </summary>
public class ReflectiveSphereMotion : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] private Vector3 moveAxis = new Vector3(1f, 0.25f, 0.5f);
    [SerializeField] private float moveSpeed = 0.75f;
    [SerializeField] private float orbitRadius = 1.5f;
    [SerializeField] private bool useOrbit = true;

    [Header("Probe")]
    [SerializeField] private ReflectionProbe probe;
    [SerializeField] private Vector3 probeOffset = new Vector3(0f, 0.5f, 0f);

    private Vector3 startPosition;
    private float phase;

    private void Reset()
    {
        startPosition = transform.position;
        if (probe == null)
        {
            probe = FindObjectOfType<ReflectionProbe>();
        }
    }

    private void Awake()
    {
        startPosition = transform.position;
        if (probe == null)
        {
            probe = FindObjectOfType<ReflectionProbe>();
        }
    }

    private void Update()
    {
        if (useOrbit)
        {
            phase += Time.deltaTime * moveSpeed;
            Vector3 offset = new Vector3(
                Mathf.Cos(phase) * orbitRadius,
                0.2f + Mathf.Sin(phase * 1.7f) * 0.35f,
                Mathf.Sin(phase) * orbitRadius);

            transform.position = startPosition + offset;
        }
        else
        {
            transform.position += moveAxis * moveSpeed * Time.deltaTime;
        }

        if (probe != null)
        {
            probe.transform.position = transform.position + probeOffset;
            probe.RenderProbe();
        }
    }
}

using UnityEngine;

/// <summary>
/// Milestone 3: simple AR-style grab and move interaction for the reflective sphere.
/// The object is kinematic while grabbed and released back to physics when dropped.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ARGrabSphere : MonoBehaviour
{
    [Header("Grab")]
    [SerializeField] private float grabDistance = 0.2f;
    [SerializeField] private LayerMask grabLayerMask;
    [SerializeField] private KeyCode grabKey = KeyCode.G;

    private Rigidbody rb;
    private bool isGrabbed;
    private Transform grabAnchor;
    private Vector3 offset;
    private Camera trackedCamera;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        ResolveGrabCamera();
    }

    private void Update()
    {
        if (Input.GetKeyDown(grabKey))
        {
            TryGrab();
        }

        if (Input.GetKeyUp(grabKey))
        {
            Release();
        }

        if (isGrabbed && grabAnchor != null)
        {
            transform.position = grabAnchor.position + offset;
            transform.rotation = grabAnchor.rotation;
        }
    }

    private void ResolveGrabCamera()
    {
        trackedCamera = Camera.main;
        if (trackedCamera == null)
        {
            trackedCamera = FindAnyObjectByType<Camera>();
        }

        if (trackedCamera == null)
        {
            Debug.LogWarning($"[{nameof(ARGrabSphere)}] No camera found for grab interaction. Add a camera to the scene or assign one in the inspector.", this);
        }
    }

    private bool TryGetGrabCamera(out Camera camera)
    {
        if (trackedCamera == null)
        {
            ResolveGrabCamera();
        }

        camera = trackedCamera;
        if (camera == null)
        {
            Debug.LogWarning($"[{nameof(ARGrabSphere)}] Grab failed because no valid camera is available.", this);
            return false;
        }

        return true;
    }

    private void TryGrab()
    {
        if (isGrabbed)
        {
            return;
        }

        if (!TryGetGrabCamera(out Camera camera))
        {
            return;
        }

        Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance, grabLayerMask.value, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject != gameObject)
            {
                return;
            }
        }

        if (Vector3.Distance(transform.position, camera.transform.position) > grabDistance)
        {
            return;
        }

        isGrabbed = true;
        rb.isKinematic = true;
        rb.useGravity = false;

        grabAnchor = camera.transform;
        offset = transform.position - grabAnchor.position;
    }

    private void Release()
    {
        if (!isGrabbed)
        {
            return;
        }

        isGrabbed = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        grabAnchor = null;
    }
}

using UnityEngine;

/// <summary>
/// Milestone 4: simple floor/wall interaction for the reflective sphere.
/// This intentionally stays lightweight and is meant to prove the object can roll
/// and bounce in a room-like setup without overbuilding the project.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class SphereRoomPhysics : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private float bounciness = 0.7f;
    [SerializeField] private float friction = 0.35f;
    [SerializeField] private float throwForce = 3f;

    [Header("Respawn")]
    [SerializeField] private Vector3 startPosition = new Vector3(0f, 0.7f, 0f);
    [SerializeField] private float respawnFallThreshold = 2.5f;

    private Rigidbody rb;
    private SphereCollider sphereCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();

        rb.mass = 1f;
        rb.drag = 0.05f;
        rb.angularDrag = 0.1f;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        PhysicMaterial material = new PhysicMaterial("SphereRoomBounce");
        material.bounciness = bounciness;
        material.dynamicFriction = friction;
        material.staticFriction = friction;
        material.bounceCombine = PhysicMaterialCombine.Maximum;
        material.frictionCombine = PhysicMaterialCombine.Average;
        sphereCollider.material = material;

        transform.position = startPosition;
    }

    private void Update()
    {
        if (transform.position.y < startPosition.y - respawnFallThreshold)
        {
            Respawn();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(Vector3.forward * throwForce, ForceMode.VelocityChange);
        }
    }

    private void Respawn()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPosition;
    }
}

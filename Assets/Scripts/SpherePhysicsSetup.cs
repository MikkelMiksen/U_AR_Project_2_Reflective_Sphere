using UnityEngine;

/// <summary>
/// P2/P3 helper for ReflectiveSphere: configures the Rigidbody for fast throws,
/// applies a bouncy PhysicsMaterial created in code (no asset needed), and
/// respawns the sphere if it ever falls out of the room.
///
/// Status: written for Unity 6 APIs (PhysicMaterial, linearVelocity).
/// NOT compiled, NOT built, NOT tested on a headset.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class SpherePhysicsSetup : MonoBehaviour
{
    [Header("Bounce")]
    [Range(0f, 1f)] [SerializeField] private float bounciness = 0.6f;
    [Range(0f, 1f)] [SerializeField] private float friction = 0.4f;

    [Header("Respawn")]
    [Tooltip("If the sphere falls this far below its start height, it is reset.")]
    [SerializeField] private float respawnDropDistance = 3f;

    private Rigidbody body;
    private Vector3 startPosition;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        SphereCollider sphereCollider = GetComponent<SphereCollider>();

        startPosition = transform.position;

        // Step 1: Rigidbody settings for thrown objects.
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Step 2: bounce material. Created at runtime so no asset is required.
        PhysicsMaterial bounceMaterial = new PhysicsMaterial("SphereBounce");
        bounceMaterial.bounciness = bounciness;
        bounceMaterial.dynamicFriction = friction;
        bounceMaterial.staticFriction = friction;
        bounceMaterial.bounceCombine = PhysicsMaterialCombine.Maximum;
        bounceMaterial.frictionCombine = PhysicsMaterialCombine.Average;

        sphereCollider.material = bounceMaterial;
    }

    private void Update()
    {
        // Step 3: safety net if room colliders are missing or the sphere is lost.
        if (transform.position.y < startPosition.y - respawnDropDistance)
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        transform.position = startPosition;
        Debug.Log("[SpherePhysicsSetup] Sphere fell out of the room; respawned.");
    }
}

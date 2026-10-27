using System.Collections.Generic;
using System.Text;
using Meta.XR.MRUtilityKit;
using UnityEngine;

/// <summary>
/// P1 diagnostics: logs every MRUK room anchor once the scene model has loaded,
/// warns about missing core surfaces, and lets you toggle the generated
/// room visuals with a controller button (colliders are not affected).
///
/// Status: written against the MRUK 207.0.1 API as I know it from Meta's docs.
/// NOT compiled, NOT built, NOT tested on a headset.
/// </summary>
public class RoomAnchorDebugger : MonoBehaviour
{
    private const string Tag = "[RoomAnchorDebugger]";

    [Header("Visual toggle")]
    [Tooltip("Pressing this button toggles the room mesh renderers on/off.")]
    [SerializeField] private OVRInput.Button toggleButton = OVRInput.Button.Two; // B / Y

    private bool visualsVisible = true;

    private void Start()
    {
        // Start (not Awake) so MRUK.Instance has already been created by its own Awake.
        if (MRUK.Instance == null)
        {
            Debug.LogError($"{Tag} No MRUK component in the scene. Add the MR Utility Kit building block.");
            return;
        }

        // Calls OnSceneLoaded when the room data is ready
        // (immediately, if it has already loaded).
        MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
    }

    private void Update()
    {
        if (OVRInput.GetDown(toggleButton))
        {
            visualsVisible = !visualsVisible;
            SetRoomVisualsVisible(visualsVisible);
        }
    }

    private void OnSceneLoaded()
    {
        Debug.Log($"{Tag} Scene loaded. Rooms: {MRUK.Instance.Rooms.Count}");

        foreach (MRUKRoom room in MRUK.Instance.Rooms)
        {
            LogRoom(room);
        }
    }

    private void LogRoom(MRUKRoom room)
    {
        Debug.Log($"{Tag} Room '{room.name}' has {room.Anchors.Count} anchors.");

        // Step 1: log each anchor and count how many we have per label.
        Dictionary<string, int> countsByLabel = new Dictionary<string, int>();

        foreach (MRUKAnchor anchor in room.Anchors)
        {
            string label = anchor.Label.ToString();

            if (!countsByLabel.ContainsKey(label))
            {
                countsByLabel[label] = 0;
            }
            countsByLabel[label]++;

            Debug.Log(
                $"{Tag}   {label,-18} " +
                $"pos={anchor.transform.position.ToString("F2")} " +
                $"size={DescribeSize(anchor)}");
        }

        // Step 2: print a one-line summary of the counts.
        StringBuilder summary = new StringBuilder($"{Tag} Summary: ");
        foreach (KeyValuePair<string, int> pair in countsByLabel)
        {
            summary.Append($"{pair.Key}={pair.Value}  ");
        }
        Debug.Log(summary.ToString());

        // Step 3: warn if a surface we depend on for collisions is missing.
        WarnIfMissing(countsByLabel, "FLOOR");
        WarnIfMissing(countsByLabel, "CEILING");
        WarnIfMissing(countsByLabel, "WALL_FACE");
    }

    private void WarnIfMissing(Dictionary<string, int> counts, string label)
    {
        if (!counts.ContainsKey(label))
        {
            Debug.LogWarning($"{Tag} No '{label}' anchor found. Re-run Space Setup on the headset.");
        }
    }

    // Anchors are either volumes (furniture) or planes (walls, floor, ceiling).
    private string DescribeSize(MRUKAnchor anchor)
    {
        if (anchor.VolumeBounds.HasValue)
        {
            return "volume " + anchor.VolumeBounds.Value.size.ToString("F2");
        }

        if (anchor.PlaneRect.HasValue)
        {
            return "plane " + anchor.PlaneRect.Value.size.ToString("F2");
        }

        return "no bounds";
    }

    // Assumption: EffectMesh parents its generated objects under the anchors.
    // The log line below tells you if that was wrong (0 renderers toggled).
    private void SetRoomVisualsVisible(bool visible)
    {
        int toggled = 0;

        foreach (MRUKRoom room in MRUK.Instance.Rooms)
        {
            foreach (MRUKAnchor anchor in room.Anchors)
            {
                foreach (MeshRenderer meshRenderer in anchor.GetComponentsInChildren<MeshRenderer>(true))
                {
                    meshRenderer.enabled = visible;
                    toggled++;
                }
            }
        }

        Debug.Log($"{Tag} Room visuals visible={visible} ({toggled} renderers toggled).");
    }
}

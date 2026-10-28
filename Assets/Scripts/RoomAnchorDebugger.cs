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
        if (MRUK.Instance == null)
        {
            Debug.LogError($"{Tag} No MRUK component in the scene. Add the MR Utility Kit building block.");
            return;
        }

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
        if (MRUK.Instance == null || MRUK.Instance.Rooms == null)
        {
            Debug.LogWarning($"{Tag} Room data is not available yet. Scene may still be loading.");
            return;
        }

        Debug.Log($"{Tag} Scene loaded. Rooms: {MRUK.Instance.Rooms.Count}");

        if (MRUK.Instance.Rooms.Count == 0)
        {
            Debug.LogWarning($"{Tag} No MRUK rooms were found. Rerun Space Setup on the headset.");
            return;
        }

        foreach (MRUKRoom room in MRUK.Instance.Rooms)
        {
            LogRoom(room);
        }
    }

    private void LogRoom(MRUKRoom room)
    {
        Debug.Log($"{Tag} Room '{room.name}' has {room.Anchors.Count} anchors.");

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

        StringBuilder summary = new StringBuilder($"{Tag} Summary: ");
        foreach (KeyValuePair<string, int> pair in countsByLabel)
        {
            summary.Append($"{pair.Key}={pair.Value}  ");
        }
        Debug.Log(summary.ToString());

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

    private void SetRoomVisualsVisible(bool visible)
    {
        if (MRUK.Instance == null || MRUK.Instance.Rooms == null)
        {
            Debug.LogWarning($"{Tag} Cannot toggle room visuals because MRUK room data is not available.");
            return;
        }

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

        Debug.Log($"{Tag} Room visuals visible={visible} " +
                  $"(rooms={MRUK.Instance.Rooms.Count}, {toggled} renderers toggled).");
    }
}

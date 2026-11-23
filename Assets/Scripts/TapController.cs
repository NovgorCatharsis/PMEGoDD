using UnityEngine;
using UnityEngine.InputSystem;

public class ARTapController : MonoBehaviour
{
    [SerializeField] private EldritchGod godPrefab;

    private Overseer overseer;

    private void Start()
    {
        overseer = Overseer.Instance;
    }

    private void Update()
    {
        if (overseer.God != null) return;

        var pointer = Pointer.current;
        if (pointer == null || !pointer.press.wasPressedThisFrame) return;

        if (!overseer.RaycastPlane(pointer.position.ReadValue(), out Pose pose)) return;

        Vector3 toCamera = overseer.Cam.transform.position - pose.position;
        toCamera.y = 0f;
        Quaternion rotation = toCamera.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(toCamera)
            : pose.rotation;

        overseer.RegisterGod(Instantiate(godPrefab, pose.position, rotation));
    }
}
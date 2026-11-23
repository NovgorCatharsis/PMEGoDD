using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-10)]
public class PointerTracker : MonoBehaviour
{
    [SerializeField] private bool onlyWhilePressed = true;

    public bool Active { get; private set; }
    public Vector3 WorldPosition { get; private set; }

    private Overseer overseer;

    private void Start()
    {
        overseer = Overseer.Instance;
    }

    private void Update()
    {
        Active = false;

        var pointer = Pointer.current;
        if (pointer == null) return;
        if (onlyWhilePressed && !pointer.press.isPressed) return;

        if (overseer.RaycastPlane(pointer.position.ReadValue(), out Pose pose))
        {
            WorldPosition = pose.position;
            Active = true;
        }
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

public class HumanGrabber : MonoBehaviour
{
    [SerializeField] private float liftHeight = 0.04f;
    [SerializeField] private float grabDistance = 10f;

    private readonly RaycastHit[] buffer = new RaycastHit[16];
    private Overseer overseer;
    private Human held;

    private void Start()
    {
        overseer = Overseer.Instance;
    }

    private void Update()
    {
        var pointer = Pointer.current;
        if (pointer == null) return;

        Vector2 screenPos = pointer.position.ReadValue();

        if (pointer.press.wasPressedThisFrame)
            TryGrab(screenPos);

        if (held == null) return;

        if (pointer.press.wasReleasedThisFrame)
        {
            if (IsOverGod(screenPos))
                overseer.God.Feed(held);
            else
                held.SetHeld(false);

            held = null;
            return;
        }

        if (overseer.RaycastPlane(screenPos, out Pose pose))
            held.transform.position = pose.position + Vector3.up * liftHeight;
    }

    private void TryGrab(Vector2 screenPos)
    {
        Ray ray = overseer.Cam.ScreenPointToRay(screenPos);
        int count = Physics.RaycastNonAlloc(ray, buffer, grabDistance);

        Human closest = null;
        float closestDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (buffer[i].distance >= closestDist) continue;

            var human = buffer[i].collider.GetComponentInParent<Human>();
            if (human == null) continue;

            closest = human;
            closestDist = buffer[i].distance;
        }

        if (closest == null) return;

        held = closest;
        held.SetHeld(true);
    }

    // Checked on screen, since the plane point under the pointer can lie behind the god
    private bool IsOverGod(Vector2 screenPos)
    {
        if (overseer.God == null) return false;

        Ray ray = overseer.Cam.ScreenPointToRay(screenPos);
        int count = Physics.RaycastNonAlloc(ray, buffer, grabDistance);

        for (int i = 0; i < count; i++)
        {
            if (buffer[i].collider.GetComponentInParent<EldritchGod>() == overseer.God)
                return true;
        }
        return false;
    }
}
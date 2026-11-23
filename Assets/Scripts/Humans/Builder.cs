using UnityEngine;

[RequireComponent(typeof(Human))]
public class Builder : MonoBehaviour
{
    [SerializeField] private float buildInterval = 6f;
    [SerializeField] private Vector2 buildDistance = new Vector2(0.08f, 0.2f);
    [SerializeField] private float minDistanceFromGod = 0.2f;
    [SerializeField] private int attempts = 5;

    private Human human;
    private Overseer overseer;
    private Colony colony;
    private float timer;

    private void Awake()
    {
        human = GetComponent<Human>();
        overseer = Overseer.Instance;
        colony = overseer.Colony;
        timer = Random.Range(0f, buildInterval);
    }

    private void Update()
    {
        if (human.IsHeld) return;

        timer += Time.deltaTime;
        if (timer < buildInterval) return;
        timer = 0f;

        if (!colony.TryGetBuildPrefab(out Building prefab)) return;

        EldritchGod god = overseer.God;
        float clearance = minDistanceFromGod * god.SizeMultiplier;
        Vector3 origin = transform.position;

        for (int i = 0; i < attempts; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            float dist = Random.Range(buildDistance.x, buildDistance.y);
            Vector3 point = origin + new Vector3(dir.x, 0f, dir.y) * dist;

            if (!overseer.GroundAt(point, out Vector3 ground)) continue;
            if (!colony.IsSpotFree(ground)) continue;

            Vector3 fromGod = ground - god.transform.position;
            fromGod.y = 0f;
            if (fromGod.sqrMagnitude < clearance * clearance) continue;

            colony.Build(prefab, ground);
            return;
        }
    }
}
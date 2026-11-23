using Humans;
using UnityEngine;

public class Human : MonoBehaviour
{
    [SerializeField] private Profession profession;
    [SerializeField] private float speed = 0.04f;
    [SerializeField] private float wanderRadius = 0.25f;
    [SerializeField] private Vector2 idleTime = new Vector2(1f, 4f);
    [SerializeField] private float agePerTick = 0.1f;

    [SerializeField] private float radius = 0.015f;
    [SerializeField] private float godKeepDistance = 0.15f;
    [SerializeField] private float fleeRadius = 0.15f;
    [SerializeField] private float fleeSpeedMultiplier = 2.5f;

    private static readonly int AgeId = Shader.PropertyToID("_age");

    public Profession Profession => profession;
    public int Era { get; set; } = 1;
    public float Radius => radius;
    public bool IsHeld => held;

    private Overseer overseer;
    private Colony colony;
    private Renderer body;
    private Material material;
    private Vector3 target;
    private bool walking;
    private bool held;
    private float idleTimer;
    private float walkTimer;
    private float age;

    private void Awake()
    {
        overseer = Overseer.Instance;
        colony = overseer.Colony;
        colony.RegisterHuman(this);
        colony.Ticked += OnColonyTick;

        body = GetComponent<MeshRenderer>();
        material = body.material;
        material.SetFloat(AgeId, 0f);

        idleTimer = Random.Range(idleTime.x, idleTime.y);
    }

    private void OnDestroy()
    {
        if (colony != null)
        {
            colony.UnregisterHuman(this);
            colony.Ticked -= OnColonyTick;
        }

        if (material != null) Destroy(material);
    }

    private void Update()
    {
        if (held) return;

        if (TryFlee(out Vector3 away))
        {
            walking = false;
            idleTimer = 0.5f;
            Flee(away);
        }
        else if (walking)
        {
            Walk();
        }
        else
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f) PickTarget();
        }

        Separate();
    }

    private bool TryFlee(out Vector3 away)
    {
        away = default;

        PointerTracker pointer = overseer.Pointer;
        if (!pointer.Active) return false;

        away = transform.position - pointer.WorldPosition;
        away.y = 0f;

        float sqr = away.sqrMagnitude;
        if (sqr > fleeRadius * fleeRadius) return false;

        away = sqr > 1e-6f ? away / Mathf.Sqrt(sqr) : transform.forward;
        return true;
    }

    private void Flee(Vector3 dir)
    {
        Vector3 next = transform.position + dir * (speed * fleeSpeedMultiplier * Time.deltaTime);
        if (!overseer.GroundAt(next, out Vector3 ground)) return;

        Face(dir);
        transform.position = ground;
    }

    private void Walk()
    {
        Vector3 to = target - transform.position;
        to.y = 0f;
        float dist = to.magnitude;

        walkTimer -= Time.deltaTime;
        if (dist < 0.005f || walkTimer <= 0f)
        {
            StartIdle();
            return;
        }

        Vector3 dir = to / dist;
        Face(dir);

        Vector3 pos = transform.position + dir * Mathf.Min(speed * Time.deltaTime, dist);
        pos.y = Mathf.MoveTowards(pos.y, target.y, speed * Time.deltaTime);
        transform.position = pos;
    }

    private void Face(Vector3 dir)
    {
        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, 10f * Time.deltaTime);
    }

    private void Separate()
    {
        Vector3 pos = transform.position;
        PushOutOfStatics(ref pos);

        var humans = colony.Humans;
        for (int i = 0; i < humans.Count; i++)
        {
            Human other = humans[i];
            if (other == this || other.held) continue;

            PushOut(ref pos, other.transform.position, radius + other.radius);
        }

        transform.position = pos;
    }

    private void PushOutOfStatics(ref Vector3 pos)
    {
        var buildings = colony.Buildings;
        for (int i = 0; i < buildings.Count; i++)
            PushOut(ref pos, buildings[i].transform.position, buildings[i].Radius + radius);

        EldritchGod god = overseer.God;
        if (god != null)
            PushOut(ref pos, god.transform.position, godKeepDistance * god.SizeMultiplier + radius);
    }

    private static void PushOut(ref Vector3 pos, Vector3 center, float minDist)
    {
        Vector3 d = pos - center;
        d.y = 0f;

        float sqr = d.sqrMagnitude;
        if (sqr >= minDist * minDist) return;

        if (sqr < 1e-8f)
        {
            pos += Vector3.right * minDist;
            return;
        }

        float dist = Mathf.Sqrt(sqr);
        pos += d / dist * (minDist - dist);
    }

    private void OnColonyTick()
    {
        age += agePerTick;

        if (age >= 1f)
        {
            Die();
            return;
        }

        if (material != null) material.SetFloat(AgeId, age);
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    private void PickTarget()
    {
        for (int i = 0; i < 5; i++)
        {
            Vector2 offset = Random.insideUnitCircle * wanderRadius;
            Vector3 point = transform.position + new Vector3(offset.x, 0f, offset.y);

            if (!overseer.GroundAt(point, out Vector3 ground)) continue;

            // Skip points inside buildings or the god's zone
            Vector3 check = ground;
            PushOutOfStatics(ref check);
            if ((check - ground).sqrMagnitude > 1e-6f) continue;

            target = ground;
            walking = true;
            walkTimer = offset.magnitude / speed * 2f + 1f;
            return;
        }

        idleTimer = 1f;
    }

    public void SetHeld(bool value)
    {
        held = value;
        if (value) return;

        if (overseer.GroundAt(transform.position, out Vector3 ground))
            transform.position = ground;

        StartIdle();
    }

    private void StartIdle()
    {
        walking = false;
        idleTimer = Random.Range(idleTime.x, idleTime.y);
    }
}
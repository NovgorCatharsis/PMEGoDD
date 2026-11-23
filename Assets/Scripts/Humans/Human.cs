using UnityEngine;

public class Human : MonoBehaviour
{
    [SerializeField] private float speed = 0.04f;
    [SerializeField] private float wanderRadius = 0.25f;
    [SerializeField] private Vector2 idleTime = new Vector2(1f, 4f);
    [SerializeField] private float agePerTick = 0.02f;

    private static readonly int AgeId = Shader.PropertyToID("_age");

    public bool IsHeld => held;

    private Overseer overseer;
    private Colony colony;
    private Renderer body;
    private Material material;
    private Vector3 target;
    private bool walking;
    private bool held;
    private float idleTimer;
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

        if (!walking)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f) PickTarget();
            return;
        }

        Vector3 dir = target - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 10f * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.003f)
            StartIdle();
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

            if (overseer.GroundAt(point, out Vector3 ground))
            {
                target = ground;
                walking = true;
                return;
            }
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
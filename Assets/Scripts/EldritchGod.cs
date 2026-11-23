using System;
using Humans;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using Random = UnityEngine.Random;

public class EldritchGod : MonoBehaviour
{
    [SerializeField] private float scalePerGrowth = 0.1f;
    [SerializeField] private int growthPerHuman = 1;
    [SerializeField] private int maxGrowth = 0;
    [SerializeField] private float scaleSmoothing = 10f;

    [SerializeField] private int baseRequired = 2;
    [SerializeField] private float multiplierStep = 0.25f;
    [SerializeField] private float taskTimeMinutes = 3f;

    public int Growth => growth;
    public float SizeMultiplier => 1f + growth * scalePerGrowth;
    public GodTask Task { get; private set; }
    public float TimeLeft { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<GodTask> TaskChanged;
    public event Action Died;

    private Colony colony;
    private Vector3 baseScale;
    private Vector3 targetScale;
    private int growth;
    private float multiplier = 1f;

    private void Awake()
    {
        baseScale = transform.localScale;
        targetScale = baseScale;

        colony = Overseer.Instance.Colony;
        colony.EraChanged += OnEraChanged;
    }

    private void Start()
    {
        NewTask();
    }

    private void OnDestroy()
    {
        if (colony != null) colony.EraChanged -= OnEraChanged;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale, targetScale, 1f - Mathf.Exp(-scaleSmoothing * Time.deltaTime));

        if (IsDead || colony.GameOver) return;

        TimeLeft -= Time.deltaTime;
        //Debug.Log("Time left: " + TimeLeft);
        if (TimeLeft <= 0f) Die();
    }

    public void Feed(Human human)
    {
        Debug.Log('1');
        if (IsDead)
        {
            human.SetHeld(false);
            Debug.Log('2');
            return;
        }
        Debug.Log('3');
        bool counts = Task.Accepts(human);

        human.Die();
        Debug.Log('4');
        
        if (!counts)
        {
            Debug.Log(" GOD NOT SATISFIED. " + Task.Done+ "/" +Task.Required);
            return;
        }



        Grow(growthPerHuman);
        Task.Done++;
        Debug.Log("GOD SATISFIED. " + Task.Done+ "/" + Task.Required);

        if (Task.Done >= Task.Required)
        {
            multiplier += multiplierStep;
            NewTask();
        }
        else
        {
            TaskChanged?.Invoke(Task);
        }
    }

    private void Grow(int amount)
    {
        if (maxGrowth > 0 && growth >= maxGrowth) return;

        growth += amount;
        if (maxGrowth > 0) growth = Mathf.Min(growth, maxGrowth);

        targetScale = baseScale * SizeMultiplier;
    }

    private void NewTask()
    {
        Task = new GodTask
        {
            Professions = (Profession)Random.Range(1, (int)Profession.Any + 1),
            EraMask = Random.Range(1, 1 << colony.Era),
            Required = Mathf.CeilToInt(baseRequired * multiplier)
        };
        Debug.Log("GOD DEMANDS " + Task.Professions + ". " + Task.Done+"/"+Task.Required);
        
        TimeLeft = taskTimeMinutes * 60f;
        TaskChanged?.Invoke(Task);
    }

    private void OnEraChanged(int era)
    {
        multiplier = 1f;
    }

    private void Die()
    {
        IsDead = true;
        Died?.Invoke();
    }
}
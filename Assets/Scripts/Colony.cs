using System;
using System.Collections.Generic;
using UnityEngine;

public class Colony : MonoBehaviour
{
    // Index 0 = era 1 building, 1 = era 2, 2 = era 3
    [SerializeField] private Building[] buildingPrefabs;
    [SerializeField] private int[] minBuildingsForEra = { 0, 1, 2 };
    [SerializeField] private int[] progressToNextEra = { 50, 100, 100 };
    [SerializeField] private float tickInterval = 2f;
    [SerializeField, Range(0f, 1f)] private float minPopulationRatio = 0.6f;
    [SerializeField] private float buildingSpacing = 0.12f;

    [SerializeField] private Human starterBuilder;
    [SerializeField] private Human starterResearcher;
    [SerializeField] private float starterRadius = 0.25f;

    public int Era { get; private set; } = 1;
    public int Progress { get; private set; }
    public bool GameOver { get; private set; }
    public int HumanCount => humans.Count;
    public IReadOnlyList<Building> Buildings => buildings;
    public IReadOnlyList<Human> Humans => humans;
    public int BuildingCount => buildings.Count;
    public int NextEraThreshold => Era <= progressToNextEra.Length ? progressToNextEra[Era - 1] : 0;

    public event Action<int> EraChanged;
    public event Action Lost;
    public event Action Ticked;

    private readonly List<Building> buildings = new List<Building>();
    private Overseer overseer;
    private readonly List<Human> humans = new List<Human>();
    private int totalCapacity;
    private int researchRate;
    private float timer;
    private bool started;

    private void Start()
    {
        overseer = Overseer.Instance;
        overseer.GodSpawned += OnGodSpawned;
    }

    private void OnDestroy()
    {
        if (overseer != null) overseer.GodSpawned -= OnGodSpawned;
    }

    private void Update()
    {
        if (!started || GameOver) return;

        timer += Time.deltaTime;
        if (timer < tickInterval) return;
        timer -= tickInterval;

        Progress += researchRate;
        //Debug.Log("Progress: " + Progress + "/" + NextEraThreshold);
        Ticked?.Invoke();

        if (Progress >= NextEraThreshold)
            AdvanceEra();
    }

    private void AdvanceEra()
    {
        Era++;
        Progress = 0;
        EraChanged?.Invoke(Era);

        if (Era > progressToNextEra.Length)
            Lose();
    }

    public void Lose()
    {
        if (GameOver) return;

        GameOver = true;
        Lost?.Invoke();
    }

    public void RegisterBuilding(Building b)
    {
        buildings.Add(b);
        totalCapacity += b.MaxHumans;
        researchRate += b.ResearchPoints;
    }

    public void UnregisterBuilding(Building b)
    {
        if (!buildings.Remove(b)) return;
        totalCapacity -= b.MaxHumans;
        researchRate -= b.ResearchPoints;
    }

    public void RegisterHuman(Human h)
    {
        humans.Add(h);
    }

    public void UnregisterHuman(Human h)
    {
        humans.Remove(h);
    }

    public void AddResearch(int amount)
    {
        researchRate += amount;
    }

    // Picks the newest era building whose minimum building count is met, falling back era by era
    public bool TryGetBuildPrefab(out Building prefab)
    {
        prefab = null;

        if (GameOver || totalCapacity <= 0) return false;
        if (humans.Count < totalCapacity * minPopulationRatio) return false;

        int top = Mathf.Min(Era, buildingPrefabs.Length) - 1;

        for (int i = top; i > 0; i--)
        {
            if (buildings.Count >= minBuildingsForEra[i])
            {
                prefab = buildingPrefabs[i];
                return true;
            }
        }

        prefab = buildingPrefabs[0];
        return true;
    }

    public bool IsSpotFree(Vector3 pos)
    {
        float sqr = buildingSpacing * buildingSpacing;

        for (int i = 0; i < buildings.Count; i++)
        {
            if ((buildings[i].transform.position - pos).sqrMagnitude < sqr)
                return false;
        }
        return true;
    }

    public Building Build(Building prefab, Vector3 pos)
    {
        return Instantiate(prefab, pos, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));
    }

    private void OnGodSpawned(EldritchGod god)
    {
        god.Died += Lose;

        Vector3 godPos = god.transform.position;
        Vector3 spot = godPos + Vector3.right * starterRadius;

        for (int i = 0; i < 10; i++)
        {
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            Vector3 point = godPos + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * starterRadius;

            if (overseer.GroundAt(point, out Vector3 ground))
            {
                spot = ground;
                break;
            }
        }

        Building tent = Build(buildingPrefabs[0], spot);
        tent.SpawnHuman(starterBuilder);
        tent.SpawnHuman(starterResearcher);

        started = true;
    }
}
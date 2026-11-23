using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Building : MonoBehaviour
{
    [Serializable]
    public class SpawnEntry
    {
        public Human prefab;
        public float weight = 1f;
    }

    [SerializeField] private int researchPoints = 1;
    [SerializeField] private int maxHumans = 3;
    [SerializeField] private float spawnInterval = 8f;
    [SerializeField, Range(0f, 1f)] private float spawnChance = 0.5f;
    [SerializeField] private float spawnRadius = 0.05f;
    [SerializeField] private SpawnEntry[] spawnEntries;

    public int ResearchPoints => researchPoints;
    public int MaxHumans => maxHumans;

    private readonly List<Human> humans = new List<Human>();
    private Colony colony;
    private float timer;

    private void Awake()
    {
        colony = Overseer.Instance.Colony;
        colony.RegisterBuilding(this);
        timer = Random.Range(0f, spawnInterval);
    }

    private void OnDestroy()
    {
        if (colony != null) colony.UnregisterBuilding(this);
    }

    private void Update()
    {
        if (colony.GameOver) return;

        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;

        humans.RemoveAll(h => h == null);
        if (humans.Count >= maxHumans) return;
        if (Random.value > spawnChance) return;

        Human prefab = PickPrefab();
        if (prefab != null) SpawnHuman(prefab);
    }

    public Human SpawnHuman(Human prefab)
    {
        Vector2 offset = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 pos = transform.position + new Vector3(offset.x, 0f, offset.y);

        Human human = Instantiate(prefab, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        humans.Add(human);
        return human;
    }

    private Human PickPrefab()
    {
        float total = 0f;
        foreach (var e in spawnEntries) total += e.weight;
        if (total <= 0f) return null;

        float roll = Random.value * total;
        foreach (var e in spawnEntries)
        {
            roll -= e.weight;
            if (roll <= 0f) return e.prefab;
        }

        return spawnEntries[spawnEntries.Length - 1].prefab;
    }
}
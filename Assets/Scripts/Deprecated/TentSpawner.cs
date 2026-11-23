using System.Collections.Generic;
using UnityEngine;

public class TentSpawner : MonoBehaviour
{
    [SerializeField] private GameObject tentPrefab;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int attemptsPerTick = 10;
    [SerializeField] private float minRadiusFromGod = 0.15f;
    [SerializeField] private float maxRadiusFromGod = 0.6f;
    [SerializeField] private float minDistanceBetweenTents = 0.1f;
    //[SerializeField] private float heightTolerance = 0.1f;
    [SerializeField] private int maxTents = 20;

    private readonly List<Transform> tents = new List<Transform>();
    private Overseer overseer;
    private float timer;

    private void Start()
    {
        overseer = Overseer.Instance;
    }

    private void Update()
    {
        if (overseer.God == null) return;

        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;

        tents.RemoveAll(t => t == null);
        if (tents.Count >= maxTents) return;

        TrySpawnTent();
    }

    private void TrySpawnTent()
    {
        Vector3 godPos = overseer.God.transform.position;

        for (int i = 0; i < attemptsPerTick; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(minRadiusFromGod, maxRadiusFromGod);
            Vector3 point = godPos + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * dist;

            if (!overseer.GroundAt(point, out Vector3 ground)) continue;
            //if (Mathf.Abs(ground.y - godPos.y) > heightTolerance) continue;
            if (IsTooClose(ground)) continue;

            Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            tents.Add(Instantiate(tentPrefab, ground, rot).transform);
            return;
        }
    }

    private bool IsTooClose(Vector3 pos)
    {
        foreach (var t in tents)
        {
            if (Vector3.Distance(t.position, pos) < minDistanceBetweenTents)
                return true;
        }
        return false;
    }
}
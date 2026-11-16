using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


[RequireComponent(typeof(ARRaycastManager))]
public class TentSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject tentPrefab;

    [Header("Timing")]
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int attemptsPerTick = 10;

    [Header("Placement")]
    [SerializeField] private float minRadiusFromGod = 0.15f;
    [SerializeField] private float maxRadiusFromGod = 0.6f;
    [SerializeField] private float minDistanceBetweenTents = 0.1f;
    [SerializeField] private float heightTolerance = 0.1f;
    [SerializeField] private int maxTents = 20;

    [Header("Planes")]
    [SerializeField] private bool hidePlaneVisuals = true;

    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private EldritchGod god;
    private readonly List<Transform> tents = new List<Transform>();
    private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private float timer;

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
    }

    private void OnEnable()
    {
        if (planeManager != null)
            planeManager.trackablesChanged.AddListener(OnPlanesChanged);
    }

    private void OnDisable()
    {
        if (planeManager != null)
            planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);
    }

    private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        if (!hidePlaneVisuals || god == null) return;

        foreach (var plane in args.added)
            plane.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (god == null)
        {
            god = FindFirstObjectByType<EldritchGod>();
            if (god == null) return;

            if (hidePlaneVisuals && planeManager != null)
            {
                foreach (var plane in planeManager.trackables)
                    plane.gameObject.SetActive(false);
            }
            timer = 0f;
        }

        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;

        if (tents.Count >= maxTents) return;

        TrySpawnTent();
    }

    private void TrySpawnTent()
    {
        Vector3 godPos = god.transform.position;

        for (int i = 0; i < attemptsPerTick; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(minRadiusFromGod, maxRadiusFromGod);
            Vector3 point = godPos + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * dist;

            Ray ray = new Ray(point + Vector3.up * 0.5f, Vector3.down);
            if (!raycastManager.Raycast(ray, hits, TrackableType.PlaneWithinPolygon))
                continue;

            ARRaycastHit hit = hits[0];

            if (planeManager != null &&
                planeManager.GetPlane(hit.trackableId) is ARPlane plane &&
                plane.alignment != PlaneAlignment.HorizontalUp)
                continue;

            if (Mathf.Abs(hit.pose.position.y - godPos.y) > heightTolerance)
                continue;

            if (IsTooCloseToOtherTents(hit.pose.position))
                continue;

            Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            GameObject tent = Instantiate(tentPrefab, hit.pose.position, rot);
            tents.Add(tent.transform);
            return; 
        }
    }

    private bool IsTooCloseToOtherTents(Vector3 pos)
    {
        tents.RemoveAll(t => t == null);

        foreach (var t in tents)
        {
            if (Vector3.Distance(t.position, pos) < minDistanceBetweenTents)
                return true;
        }
        return false;
    }
}
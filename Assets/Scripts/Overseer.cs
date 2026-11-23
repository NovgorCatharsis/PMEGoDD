using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class Overseer : MonoBehaviour
{
    public static Overseer Instance { get; private set; }

    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private Camera camera;
    [SerializeField] private Colony colony;
    [SerializeField] private bool hidePlanesAfterGodSpawn = true;
    [SerializeField] private PointerTracker pointer;
    
    public PointerTracker Pointer => pointer;
    public ARRaycastManager RaycastManager => raycastManager;
    public ARPlaneManager PlaneManager => planeManager;
    public Camera Cam => camera;
    public Colony Colony => colony;
    public EldritchGod God { get; private set; }
    

    public event Action<EldritchGod> GodSpawned;

    private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        if (camera == null) camera = Camera.main;
    }

    private void OnEnable()
    {
        planeManager.trackablesChanged.AddListener(OnPlanesChanged);
    }

    private void OnDisable()
    {
        planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void RegisterGod(EldritchGod god)
    {
        God = god;

        if (hidePlanesAfterGodSpawn)
        {
            foreach (var plane in planeManager.trackables)
                plane.gameObject.SetActive(false);
        }

        GodSpawned?.Invoke(god);
    }

    private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        if (!hidePlanesAfterGodSpawn || God == null) return;

        foreach (var plane in args.added)
            plane.gameObject.SetActive(false);
    }

    public bool RaycastPlane(Vector2 screenPos, out Pose pose)
    {
        if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            pose = hits[0].pose;
            return true;
        }

        pose = default;
        return false;
    }

    // Finds a point on an upward-facing plane directly below the given world point
    public bool GroundAt(Vector3 point, out Vector3 ground)
    {
        ground = default;

        Ray ray = new Ray(point + Vector3.up * 0.3f, Vector3.down);
        if (!raycastManager.Raycast(ray, hits, TrackableType.PlaneWithinPolygon)) return false;

        ARRaycastHit hit = hits[0];
        ARPlane plane = planeManager.GetPlane(hit.trackableId);
        if (plane != null && plane.alignment != PlaneAlignment.HorizontalUp) return false;

        ground = hit.pose.position;
        return true;
    }
}
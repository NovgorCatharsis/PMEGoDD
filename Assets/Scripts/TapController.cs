using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;


[RequireComponent(typeof(ARRaycastManager))]
public class ARTapController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject godPrefab;

    [Header("Options")]
    [Tooltip("Скрыть и отключить детекцию плоскостей после спавна Бога")]
    [SerializeField] private float godRayDistance = 10f;

    private ARRaycastManager raycastManager;
    private Camera arCamera;
    private GameObject spawnedGod;

    private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        arCamera = Camera.main;
    }

    private void Update()
    {
        var pointer = Pointer.current;
        if (pointer == null || !pointer.press.wasPressedThisFrame)
            return;

        Vector2 screenPos = pointer.position.ReadValue();

        if (TryHitGod(screenPos))
            return;

        if (spawnedGod == null)
            TrySpawnGod(screenPos);
    }

    private bool TryHitGod(Vector2 screenPos)
    {
        if (spawnedGod == null)
        {
            Debug.Log("No god");
            return false;
        }

        Ray ray = arCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, godRayDistance))
        {
            var god = hit.collider.GetComponentInParent<EldritchGod>();
            if (god != null)
            {
                god.Grow(1);
                return true;
            }
        }
        return false;
    }

    private void TrySpawnGod(Vector2 screenPos)
    {
        if (!raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
            return;

        Pose pose = hits[0].pose;
        pose.position.y += 0.1f;
        
        Vector3 toCamera = arCamera.transform.position - pose.position;
        toCamera.y = 0f;
        Quaternion rotation = toCamera.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(toCamera)
            : pose.rotation;

        spawnedGod = Instantiate(godPrefab, pose.position, rotation);
        Debug.Log("God has been summoned");
        
    }
}
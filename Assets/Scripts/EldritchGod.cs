using UnityEngine;

public class EldritchGod : MonoBehaviour
{
    [Header("Growth")]
    [SerializeField] private float scalePerGrowth = 0.1f;
    [SerializeField] private int maxGrowth = 0;
    [SerializeField] private float scaleSmoothing = 10f;

    private Vector3 baseScale;
    private Vector3 targetScale;
    private int growth;

    public int Growth => growth;

    private void Awake()
    {
        baseScale = transform.localScale;
        targetScale = baseScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale, targetScale, 1f - Mathf.Exp(-scaleSmoothing * Time.deltaTime));
    }

    public void Grow(int amount = 1)
    {
        if (maxGrowth > 0 && growth >= maxGrowth) return;

        growth += amount;
        if (maxGrowth > 0) growth = Mathf.Min(growth, maxGrowth);

        targetScale = baseScale * (1f + growth * scalePerGrowth);
        Debug.Log($"God growth: {growth}, scale: {targetScale.x:F2}");
    }
}
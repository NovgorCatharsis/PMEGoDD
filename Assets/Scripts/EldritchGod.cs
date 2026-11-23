using UnityEngine;

public class EldritchGod : MonoBehaviour
{
    [SerializeField] private float scalePerGrowth = 0.1f;
    [SerializeField] private int growthPerHuman = 1;
    [SerializeField] private int maxGrowth = 0;
    [SerializeField] private float scaleSmoothing = 10f;

    public int Growth => growth;
    public float SizeMultiplier => 1f + growth * scalePerGrowth;

    private Vector3 baseScale;
    private Vector3 targetScale;
    private int growth;

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

    public void Feed(Human human)
    {
        human.Die();
        Grow(growthPerHuman);
    }

    public void Grow(int amount)
    {
        if (maxGrowth > 0 && growth >= maxGrowth) return;

        growth += amount;
        if (maxGrowth > 0) growth = Mathf.Min(growth, maxGrowth);

        targetScale = baseScale * SizeMultiplier;
    }
}
using UnityEngine;

public class Researcher : MonoBehaviour
{
    [SerializeField] private int researchPoints = 1;

    private Colony colony;

    private void Awake()
    {
        colony = Overseer.Instance.Colony;
        colony.AddResearch(researchPoints);
    }

    private void OnDestroy()
    {
        if (colony != null) colony.AddResearch(-researchPoints);
    }
}
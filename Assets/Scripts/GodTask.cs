using Humans;

public class GodTask
{
    public Profession Professions;
    public int EraMask;
    public int Required;
    public int Done;

    public bool Accepts(Human human)
    {
        return ((Professions & human.Profession) != 0) && ((EraMask & (1 << (human.Era - 1))) != 0);
    }
}
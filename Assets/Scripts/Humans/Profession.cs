using System;

namespace Humans
{
    [Flags]
    public enum Profession
    {
        None = 0,
        Citizen = 1,
        Builder = 2,
        Researcher = 4,
        Any = Citizen | Builder | Researcher
    }
}
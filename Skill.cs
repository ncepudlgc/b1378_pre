using System;

namespace TextRPG
{
    public class Skill
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int ManaCost { get; private set; }
        public int BaseDamage { get; private set; }
        public bool IsMagic { get; private set; }
        public bool IsRanged { get; private set; }
        public int NormalRange { get; private set; } // in feet
        public int LongRange { get; private set; } // in feet, 0 if none

        public Skill(string name, string description, int manaCost, int baseDamage, bool isMagic = false, bool isRanged = false, int normalRange = 0, int longRange = 0)
        {
            Name = name;
            Description = description;
            ManaCost = manaCost;
            BaseDamage = baseDamage;
            IsMagic = isMagic;
            IsRanged = isRanged;
            NormalRange = normalRange;
            LongRange = longRange;
        }
    }
} 
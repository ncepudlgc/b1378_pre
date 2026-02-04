// Weapon.cs
using System;

namespace TextRPG
{
    public class Weapon : Item
    {
        public int AttackBonus { get; private set; }
        public bool IsRanged { get; private set; }
        public int NormalRange { get; private set; } // in feet
        public int LongRange { get; private set; } // in feet, 0 if none

        public Weapon(string name, string description, int value, int attackBonus, bool isRanged = false, int normalRange = 0, int longRange = 0) 
            : base(name, description, value, false)
        {
            AttackBonus = attackBonus;
            IsRanged = isRanged;
            NormalRange = normalRange;
            LongRange = longRange;
        }
    }
}
// Armor.cs
using System;

namespace TextRPG
{
    public class Armor : Item
    {
        public int DefenseBonus { get; private set; }

        public Armor(string name, string description, int value, int defenseBonus) 
            : base(name, description, value, false)
        {
            DefenseBonus = defenseBonus;
        }
    }
}
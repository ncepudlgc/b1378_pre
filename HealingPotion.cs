// HealingPotion.cs
using System;

namespace TextRPG
{
    public class HealingPotion : Item
    {
        public int HealAmount { get; private set; }

        public HealingPotion(string name, string description, int value, int healAmount) 
            : base(name, description, value, true)
        {
            HealAmount = healAmount;
        }

        public void Use(Character character)
        {
            character.Heal(HealAmount);
        }
    }
}
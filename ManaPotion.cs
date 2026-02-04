using System;

namespace TextRPG
{
    public class ManaPotion : Item
    {
        public int ManaAmount { get; private set; }

        public ManaPotion(string name, string description, int value, int manaAmount) 
            : base(name, description, value, true)
        {
            ManaAmount = manaAmount;
        }

        public void Use(Character character)
        {
            character.RegenerateMana(ManaAmount);
        }
    }
} 
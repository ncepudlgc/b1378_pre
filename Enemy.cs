using System;
using System.Linq;

namespace TextRPG
{
    public class Enemy : Character
    {
        public int ExperienceReward { get; protected set; }
        public int GoldReward { get; protected set; }
        
        public Enemy(string name, int health, int attack, int defense, int expReward, int goldReward) 
            : base(name, health, 0, 0, attack, 0, defense, 0, 5, 0)
        {
            ExperienceReward = expReward;
            GoldReward = goldReward;
            
            // Add basic magic skills to certain enemy types
            if (name.Contains("Mage") || name.Contains("Wizard") || name.Contains("Elemental"))
            {
                Mana = 30;
                MaxMana = 30;
                MagicAttack = 8;
                MagicDefense = 5;
                
                // Add appropriate skills based on enemy type
                if (name.Contains("Fire"))
                {
                    AddSkill(new Skill("Fireball", "A basic fire spell", 10, 8, true));
                }
                else if (name.Contains("Ice"))
                {
                    AddSkill(new Skill("Ice Spike", "A freezing spell that reduces speed", 15, 6, true));
                }
                else if (name.Contains("Lightning"))
                {
                    AddSkill(new Skill("Lightning Bolt", "A powerful lightning spell", 20, 12, true));
                }
            }
        }

        public override int AttackMethod()
        {
            // 30% chance to use magic attack if the enemy has mana and skills
            if (Mana > 0 && Skills.Count > 0 && new Random().Next(100) < 30)
            {
                var availableSkills = Skills.Where(s => Mana >= s.ManaCost).ToList();
                if (availableSkills.Count > 0)
                {
                    var skill = availableSkills[new Random().Next(availableSkills.Count)];
                    Mana -= skill.ManaCost;
                    return CastMagicAttack() + skill.BaseDamage;
                }
            }
            return base.AttackMethod();
        }

        // Override to allow enemies to have skills
        public override bool AddSkill(Skill skill)
        {
            if (!Skills.Exists(s => s.Name == skill.Name))
            {
                Skills.Add(skill);
                return true;
            }
            return false;
        }

        public override bool UseSkill(string skillName, Character? target = null)
        {
            return base.UseSkill(skillName, target);
        }

        // Disable inventory-related methods (enemies have no inventory)
        public override bool AddItem(Item item)
        {
            return false;  // Enemies cannot add items
        }

        public override bool RemoveItem(string itemName)
        {
            return false;  // Enemies cannot remove items
        }

        public override bool UseItem(string itemName)
        {
            return false;  // Enemies cannot use items
        }

        // Disable quest-related methods (enemies have no quests)
        public bool AddQuest(Quest quest)
        {
            return false;  // Enemies cannot add quests
        }

        // Disable saving/loading (enemies are not persistent)
        public new void SaveCharacter(string filePath = "save.json")
        {
            // Do nothing or throw an exception, as enemies aren't saved like player characters
            throw new NotSupportedException("Enemies cannot be saved.");
        }

        // Static load method isn't applicable for enemies, so we can add a note or leave it out
        public new static Character LoadCharacter(string filePath = "save.json")
        {
            throw new NotSupportedException("Enemies cannot be loaded from save files.");
        }

        // Other methods (e.g., TakeDmg, IsAlive) are inherited and work as-is for basic combat
    }
}

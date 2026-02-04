using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace TextRPG
{
    public class Character
    {
        // Basic Stats
        public string Name { get; init; }
        public int Level { get; private set; } = 1;
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public int Experience { get; private set; } = 0;
        public int ExperienceToNextLevel { get; protected set; }

        // Combat Stats
        public int Attack { get; private set; }
        public int Defense { get; private set; }
        public int MagicAttack { get; protected set; }
        public int MagicDefense { get; protected set; }
        public int Speed { get; protected set; }
        public int Evasion { get; protected set; }

        // Resource Stats
        public int SP { get; protected set; }
        public int MaxSP { get; protected set; }
        public int Mana { get; set; }
        public int MaxMana { get; protected set; }

        // Economy Stats
        public int Gold { get; set; }
        public int XP { get; protected set; }
        public int XPNext { get; protected set; }

        // Collections
        public List<Item> Inventory { get; init; } = new List<Item>();
        public List<Skill> Skills { get; set; } = new List<Skill>();
        public List<SeasonalQuest> Quests { get; private set; } = new List<SeasonalQuest>();

        // Equipment slots
        public Weapon? EquippedWeapon { get; protected set; }
        public Armor? EquippedArmor { get; protected set; }

        // Constructor for basic character creation
        public Character(string name)
        {
            Name = name;
            Inventory = new List<Item>();
            Level = 1;
            MaxHealth = 100;
            Health = MaxHealth;
            Experience = 0;
            ExperienceToNextLevel = 100;
            Attack = 10;
            Defense = 5;
            MagicAttack = 5;
            MagicDefense = 5;
            Speed = 10;
            Evasion = 5;
            SP = 50;
            MaxSP = 50;
            Mana = 30;
            MaxMana = 30;
            XP = 0;
            XPNext = 100;
            
            // Add basic fireball skill to new characters
            Skills = new List<Skill>();
            AddSkill(new Skill("Fireball", "A basic fire spell that deals magic damage", 10, 8, true));
        }

        // Full constructor for more detailed character creation
        public Character(string name, int health, int sp, int mana, int attack, int magicAttack, 
            int defense, int magicDefense, int speed, int evasion)
        {
            Name = name;
            Level = 1;
            MaxHealth = health;
            Health = health;
            SP = sp;
            MaxSP = sp;
            Mana = mana;
            MaxMana = mana;
            Attack = attack;
            MagicAttack = magicAttack;
            Defense = defense;
            MagicDefense = magicDefense;
            Speed = speed;
            Evasion = evasion;
            Experience = 0;
            ExperienceToNextLevel = 100;
            XP = 0;
            XPNext = 100;
        }

        public virtual void TakeDamage(int damage)
        {
            int armorBonus = EquippedArmor?.DefenseBonus ?? 0;
            // Defense reduces damage by 10% per point, up to 50% reduction
            double defenseReduction = Math.Min(0.5, Defense * 0.1);
            // Armor bonus reduces damage by 5% per point, up to 25% reduction
            double armorReduction = Math.Min(0.25, armorBonus * 0.05);
            
            // Calculate total damage reduction
            double totalReduction = defenseReduction + armorReduction;
            // Ensure minimum damage of 1
            int actualDamage = Math.Max(1, (int)(damage * (1 - totalReduction)));
            Health = Math.Max(0, Health - actualDamage);
        }
        
        public virtual void Heal(int amount)
        {
            Health = Math.Min(MaxHealth, Health + amount);
        }

        public virtual void AddExperience(int exp)
        {
            Experience += exp;
            if (Experience >= ExperienceToNextLevel)
            {
                LevelUp();
            }
        }

        protected virtual void LevelUp()
        {
            Level++;
            MaxHealth += 20;
            Health = MaxHealth;
            Attack += 2;
            Defense += 1;
            MagicAttack += 1;
            MagicDefense += 1;
            Speed += 1;
            Evasion += 1;
            Experience -= ExperienceToNextLevel;
            ExperienceToNextLevel = (int)(ExperienceToNextLevel * 1.5);
        }

        public virtual int AttackMethod()
        {
            Random random = new Random();
            int weaponBonus = EquippedWeapon?.AttackBonus ?? 0;
            // Base damage is Attack + weapon bonus, with a random variation of -2 to +2
            int baseDamage = Attack + weaponBonus;
            int damage = baseDamage + random.Next(-2, 3);
            return Math.Max(1, damage); // Minimum damage of 1
        }
        
        public virtual int LightAttack()
        {
            Random random = new Random();
            int weaponBonus = EquippedWeapon?.AttackBonus ?? 0;
            // Light attack does 70-90% of total attack power (including weapon bonus)
            int baseDamage = Attack + weaponBonus;
            int damage = (int)(baseDamage * (0.7 + random.NextDouble() * 0.2));
            return Math.Max(1, damage); // Minimum damage of 1
        }
        
        public virtual int HeavyAttack()
        {
            Random random = new Random();
            int weaponBonus = EquippedWeapon?.AttackBonus ?? 0;
            // Heavy attack does 120-150% of total attack power (including weapon bonus)
            int baseDamage = Attack + weaponBonus;
            int damage = (int)(baseDamage * (1.2 + random.NextDouble() * 0.3));
            return Math.Max(1, damage); // Minimum damage of 1
        }

        public virtual bool AddSkill(Skill skill)
        {
            if (!Skills.Exists(s => s.Name == skill.Name))
            {
                Skills.Add(skill);
                return true;
            }
            return false;
        }

        public virtual bool UseSkill(string skillName, Character? target = null)
        {
            var skill = Skills.Find(s => s.Name == skillName);
            if (skill != null && Mana >= skill.ManaCost)
            {
                Mana -= skill.ManaCost;
                
                if (target != null)
                {
                    // Calculate base damage
                    int baseDamage = skill.BaseDamage + MagicAttack;
                    
                    // Apply random variation (-10% to +10%)
                    Random random = new Random();
                    double variation = 0.9 + (random.NextDouble() * 0.2);
                    int finalDamage = (int)(baseDamage * variation);
                    
                    // Apply the damage
                    target.TakeMagicDamage(finalDamage);
                    
                    // Apply any additional effects based on skill type
                    if (skill.Name == "Ice Spike")
                    {
                        // Reduce target's speed by 20% for 2 turns
                        target.Speed = (int)(target.Speed * 0.8);
                    }
                    else if (skill.Name == "Lightning Bolt")
                    {
                        // 20% chance to stun (skip next turn)
                        if (random.Next(100) < 20)
                        {
                            // TODO: Implement stun effect
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public virtual bool AddItem(Item item)
        {
            Inventory.Add(item);
            return true;
        }

        public virtual bool RemoveItem(string itemName)
        {
            var item = Inventory.Find(i => i.Name == itemName);
            if (item != null)
            {
                Inventory.Remove(item);
                return true;
            }
            return false;
        }

        public virtual bool UseItem(string itemName)
        {
            var item = Inventory.Find(i => i.Name == itemName);
            if (item != null)
            {
                if (item is HealingPotion potion)
                {
                    potion.Use(this);
                }
                
                if (item.IsConsumable)
                {
                    Inventory.Remove(item);
                }
                return true;
            }
            return false;
        }
        
        public bool EquipWeapon(string weaponName)
        {
            var weapon = Inventory.Find(i => i.Name == weaponName && i is Weapon) as Weapon;
            if (weapon != null)
            {
                EquippedWeapon = weapon;
                return true;
            }
            return false;
        }
        
        public bool EquipArmor(string armorName)
        {
            var armor = Inventory.Find(i => i.Name == armorName && i is Armor) as Armor;
            if (armor != null)
            {
                EquippedArmor = armor;
                return true;
            }
            return false;
        }
        public void AddGold(int amount)
        {
            Gold += amount;
        }
        
        public bool SpendGold(int amount)
        {
            if (Gold >= amount)
            {
                Gold -= amount;
                return true;
            }
            return false;
        }
    

        public virtual void SaveCharacter(string filePath = "save.json")
        {
            var saveData = new CharacterSaveData
            {
                Name = Name,
                Level = Level,
                Health = Health,
                MaxHealth = MaxHealth,
                Experience = Experience,
                ExperienceToNextLevel = ExperienceToNextLevel,
                Attack = Attack,
                Defense = Defense,
                MagicAttack = MagicAttack,
                MagicDefense = MagicDefense,
                Speed = Speed,
                Evasion = Evasion,
                SP = SP,
                MaxSP = MaxSP,
                Mana = Mana,
                MaxMana = MaxMana,
                Gold = Gold,
                XP = XP,
                XPNext = XPNext,
                Inventory = Inventory,
                EquippedWeapon = EquippedWeapon,
                EquippedArmor = EquippedArmor
            };

            string jsonString = JsonSerializer.Serialize(saveData, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                IncludeFields = true
            });
            File.WriteAllText(filePath, jsonString);
        }

        public static Character LoadCharacter(string filePath = "save.json")
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Save file not found.");
            }

            string jsonString = File.ReadAllText(filePath);
            var saveData = JsonSerializer.Deserialize<CharacterSaveData>(jsonString);

            if (saveData == null)
            {
                throw new InvalidOperationException("Failed to deserialize save data.");
            }

            // Create a new character with the saved data
            var character = new Character(saveData.Name)
            {
                Level = saveData.Level,
                Health = saveData.Health,
                MaxHealth = saveData.MaxHealth,
                Experience = saveData.Experience,
                ExperienceToNextLevel = saveData.ExperienceToNextLevel,
                Attack = saveData.Attack,
                Defense = saveData.Defense,
                MagicAttack = saveData.MagicAttack,
                MagicDefense = saveData.MagicDefense,
                Speed = saveData.Speed,
                Evasion = saveData.Evasion,
                SP = saveData.SP,
                MaxSP = saveData.MaxSP,
                Mana = saveData.Mana,
                MaxMana = saveData.MaxMana,
                Gold = saveData.Gold,
                XP = saveData.XP,
                XPNext = saveData.XPNext,
                Inventory = saveData.Inventory,
                EquippedWeapon = saveData.EquippedWeapon,
                EquippedArmor = saveData.EquippedArmor
            };

            return character;
        }

        public bool IsAlive()
        {
            return Health > 0;
        }

        public virtual void TakeMagicDamage(int damage)
        {
            // Magic defense reduces damage by 15% per point, up to 60% reduction
            double magicDefenseReduction = Math.Min(0.6, MagicDefense * 0.15);
            
            // Calculate total damage reduction
            int actualDamage = Math.Max(1, (int)(damage * (1 - magicDefenseReduction)));
            Health = Math.Max(0, Health - actualDamage);
        }

        public virtual int CastMagicAttack()
        {
            Random random = new Random();
            // Base damage is MagicAttack with a random variation of -1 to +1
            int baseDamage = MagicAttack;
            int damage = baseDamage + random.Next(-1, 2);
            return Math.Max(1, damage); // Minimum damage of 1
        }

        public void RegenerateMana(int amount)
        {
            Mana = Math.Min(MaxMana, Mana + amount);
        }
    }

    public class CharacterSaveData
    {
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Experience { get; set; }
        public int ExperienceToNextLevel { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int MagicAttack { get; set; }
        public int MagicDefense { get; set; }
        public int Speed { get; set; }
        public int Evasion { get; set; }
        public int SP { get; set; }
        public int MaxSP { get; set; }
        public int Mana { get; set; }
        public int MaxMana { get; set; }
        public int Gold { get; set; }
        public int XP { get; set; }
        public int XPNext { get; set; }
        public List<Item> Inventory { get; set; } = new List<Item>();
        public Weapon? EquippedWeapon { get; set; }
        public Armor? EquippedArmor { get; set; }
    }
} 
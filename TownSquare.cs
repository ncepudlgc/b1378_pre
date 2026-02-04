// TownSquare.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace TextRPG
{
    public class TownSquare
    {
        private Character Player { get; }
        private Display Display { get; }
        private List<Item> ShopInventory { get; } = new List<Item>();
        private List<Weapon> BlacksmithWeapons { get; } = new List<Weapon>();
        private List<Armor> BlacksmithArmor { get; } = new List<Armor>();
        private List<SeasonalQuest> AvailableQuests { get; set; } = new List<SeasonalQuest>();
        private GameCalendar Calendar { get; }
        private Random Random { get; } = new Random();
        private Dungeon Dungeon { get; }
    
        public TownSquare(Character player, Display display, GameCalendar calendar, Dungeon dungeon)
        {
            Player = player;
            Display = display;
            Calendar = calendar;
            Dungeon = dungeon;
            
            // Initialize shop inventory
            ShopInventory.Add(new HealingPotion("Small Potion", "Restores 20 HP", 10, 20));
            ShopInventory.Add(new HealingPotion("Medium Potion", "Restores 50 HP", 25, 50));
            ShopInventory.Add(new HealingPotion("Large Potion", "Restores 100 HP", 50, 100));
            
            // Initialize blacksmith inventory
            BlacksmithWeapons.Add(new Weapon("Iron Sword", "A basic iron sword", 50, 8));
            BlacksmithWeapons.Add(new Weapon("Steel Sword", "A sturdy steel sword", 120, 15));
            BlacksmithWeapons.Add(new Weapon("Silver Sword", "A finely crafted silver sword", 250, 25));
            BlacksmithWeapons.Add(new Weapon("Shortbow", "A simple shortbow. Light and easy to use.", 60, 6, true, 30, 0));
            BlacksmithWeapons.Add(new Weapon("Longbow", "A powerful longbow. Greater range.", 120, 10, true, 60, 80));
            BlacksmithWeapons.Add(new Weapon("Crossbow", "A heavy crossbow. High damage, long range.", 200, 14, true, 40, 60));
            
            BlacksmithArmor.Add(new Armor("Leather Armor", "Basic protection", 40, 3));
            BlacksmithArmor.Add(new Armor("Chain Mail", "Decent protection", 100, 8));
            BlacksmithArmor.Add(new Armor("Plate Armor", "Heavy protection", 200, 15));

            // Generate initial quests
            RefreshQuestBoard();
        }
            
        public void EnterTownSquare()
        {
            if (Display == null || Player == null) return;

            Display.displayString($"\nWelcome to the Town Square!");
            Display.displayString($"Current date: {Calendar.GetDateString()}");
            Display.displayString($"Actions until new day: {Calendar.GetActionsUntilNewDay()}");
            Calendar.RecordAction(false); // Record entering town as a non-combat action

            bool stayInTown = true;
            while (stayInTown)
            {
                Display.displayString("\n=== Town Square ===");
                Display.displayString($"Gold: {Player.Gold}");
                Display.displayString($"Date: {Calendar.GetDateString()}");
                Display.displayString("1. Visit Potion Shop");
                Display.displayString("2. Visit Blacksmith");
                Display.displayString("3. Visit Library");
                Display.displayString("4. View Inventory");
                Display.displayString("5. Manage Equipment");
                Display.displayString("6. Manage Skills");
                Display.displayString("7. Visit Quest Board");
                Display.displayString("8. View Active Quests");
                Display.displayString("9. Save Game");
                Display.displayString("10. Leave Town Square");
                
                string? input = Console.ReadLine();
                
                switch (input)
                {
                    case "1":
                        VisitPotionShop();
                        break;
                    case "2":
                        VisitBlacksmith();
                        break;
                    case "3":
                        VisitLibrary();
                        break;
                    case "4":
                        ViewInventory();
                        break;
                    case "5":
                        ManageEquipment();
                        break;
                    case "6":
                        ManageSkills();
                        break;
                    case "7":
                        VisitQuestBoard();
                        break;
                    case "8":
                        DisplayActiveQuests();
                        break;
                    case "9":
                        Player.SaveCharacter();
                        Display.displayString("Game saved successfully!");
                        break;
                    case "10":
                        Display.displayString("Leaving Town Square...");
                        RecordAction(); // Record leaving town as an action
                        return;
                    default:
                        Display.displayString("Invalid choice. Please try again.");
                        break;
                }
            }
        }
        
        private void RecordAction(bool isCombatAction = false)
        {
            Calendar.RecordAction(isCombatAction);
        }
        
        private void ViewActiveQuests()
        {
            if (Display == null || Player == null) return;

            Display.displayString("\n=== Quest Board ===");
            Display.displayString("Available Quests:");

            if (Player.Quests.Count == 0)
            {
                Display.displayString("No active quests.");
                return;
            }

            foreach (var quest in Player.Quests)
            {
                if (quest is SeasonalQuest seasonalQuest)
                {
                    string levelInfo = seasonalQuest.RequiredLevel > 1 ? $" [Level {seasonalQuest.RequiredLevel} Only]" : "";
                    Display.displayString($"\n{seasonalQuest.Name}{levelInfo}");
                    Display.displayString($"Description: {seasonalQuest.Description}");
                    Display.displayString($"Progress: {seasonalQuest.CurrentEnemyDefeats}/{seasonalQuest.RequiredEnemyDefeats} enemies defeated");
                    Display.displayString($"Rewards: {seasonalQuest.ExperienceReward} XP, {seasonalQuest.GoldReward} Gold");
                }
            }
        }
        
        private void VisitQuestBoard()
        {
            bool viewingBoard = true;

            while (viewingBoard)
            {
                Display.displayString("\n=== Quest Board ===");
                Display.displayString($"Current Season: {Calendar.GetCurrentSeason()}");
                Display.displayString("Available Quests:");
                
                if (AvailableQuests.Count == 0)
                {
                    Display.displayString("There are no quests available at the moment.");
                    Display.displayString("Check back tomorrow for new opportunities!");
                    viewingBoard = false;
                    continue;
                }
                
                for (int i = 0; i < AvailableQuests.Count; i++)
                {
                    Display.displayString($"{i+1}. {AvailableQuests[i]}");
                }
                
                Display.displayString($"{AvailableQuests.Count+1}. Refresh Quest Board (costs 10 gold)");
                Display.displayString($"{AvailableQuests.Count+2}. Back to Town Square");
                
                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    if (choice >= 1 && choice <= AvailableQuests.Count)
                    {
                        AcceptQuest(AvailableQuests[choice-1]);
                    }
                    else if (choice == AvailableQuests.Count+1)
                    {
                        if (Player.SpendGold(10))
                        {
                            RefreshQuestBoard();
                            Display.displayString("The quest board has been refreshed with new opportunities!");
                        }
                        else
                        {
                            Display.displayString("You don't have enough gold to refresh the quest board.");
                        }
                    }
                    else if (choice == AvailableQuests.Count+2)
                    {
                        viewingBoard = false;
                    }
                    else
                    {
                        Display.displayString("Invalid choice.");
                    }
                }
                else
                {
                    Display.displayString("Invalid choice.");
                }
            }
        }
        
        private void AcceptQuest(SeasonalQuest quest)
        {
            // Check if player already has this quest
            if (Player.Quests.Any(q => q.Name == quest.Name))
            {
                Display.displayString("You have already accepted this quest!");
                return;
            }

            // Check if player has reached the quest limit (e.g., 5 quests)
            if (Player.Quests.Count >= 5)
            {
                Display.displayString("You can only have 5 active quests at a time.");
                Display.displayString("Complete some quests before accepting new ones.");
                return;
            }

            RecordAction(false); // Record accepting a quest as a non-combat action
            Display.displayString($"\nYou've accepted the quest: {quest.Name}");
            Display.displayString(quest.Description);
            Display.displayString($"Rewards: {quest.ExperienceReward} XP, {quest.GoldReward} Gold");
            
            // Add the quest to player's quest log
            Player.Quests.Add(quest);
            
            // Add the quest to the dungeon level
            Dungeon.AddQuest(quest, quest.RequiredLevel - 1); // Convert to 0-based level
            
            // Remove the quest from available quests
            AvailableQuests.Remove(quest);
        }
        
        private void RefreshQuestBoard()
        {
            AvailableQuests.Clear();
            
            // Generate 2 level 1 quests
            for (int i = 0; i < 2; i++)
            {
                string season = Random.Next(100) < 70 ? Calendar.GetCurrentSeason() : 
                    Calendar.GetAllSeasons()[Random.Next(Calendar.GetAllSeasons().Length)];
                SeasonalQuest quest = GenerateSeasonalQuest(season, 1); // Level 1 quest
                AvailableQuests.Add(quest);
            }

            // Generate 2 level 2 quests
            for (int i = 0; i < 2; i++)
            {
                string season = Random.Next(100) < 70 ? Calendar.GetCurrentSeason() : 
                    Calendar.GetAllSeasons()[Random.Next(Calendar.GetAllSeasons().Length)];
                SeasonalQuest quest = GenerateSeasonalQuest(season, 2); // Level 2 quest
                AvailableQuests.Add(quest);
            }

            // Generate 2 level 3 quests
            for (int i = 0; i < 2; i++)
            {
                string season = Random.Next(100) < 70 ? Calendar.GetCurrentSeason() : 
                    Calendar.GetAllSeasons()[Random.Next(Calendar.GetAllSeasons().Length)];
                SeasonalQuest quest = GenerateSeasonalQuest(season, 3); // Level 3 quest
                AvailableQuests.Add(quest);
            }
        }
        
        private SeasonalQuest GenerateSeasonalQuest(string season, int requiredLevel)
        {
            string[] questTypes = {
                "Defeat the {0} Infestation",
                "Clear the {0} Nest",
                "Hunt the {0} Pack",
                "Investigate the {0} Disturbance",
                "Protect against {0} Invasion"
            };

            // Add special quest types for level 3
            if (requiredLevel == 3)
            {
                questTypes = new string[] {
                    "Confront the {0} Lord",
                    "Challenge the {0} Champion",
                    "Defeat the {0} Guardian",
                    "Face the {0} Master",
                    "Battle the {0} Tyrant"
                };
            }

            string[] enemyTypes = Calendar.GetSeasonalEnemyTypes();
            string enemyType = enemyTypes[Random.Next(enemyTypes.Length)];
            
            string questName = string.Format(questTypes[Random.Next(questTypes.Length)], enemyType);
            string description = requiredLevel == 3 
                ? $"A powerful {enemyType} awaits in the deepest level of the dungeon. Defeat it to complete this quest."
                : $"Defeat {enemyType} enemies to complete this quest.";
            
            // Base rewards that will be modified by difficulty and level
            int baseExp = 100;
            int baseGold = 50;
            
            // Random difficulty (1-3)
            int difficulty = Random.Next(1, 4);
            
            // Scale rewards based on difficulty and level
            // Level 3 quests give significantly more rewards
            double levelMultiplier = requiredLevel == 3 ? 3.0 : 1.0;
            int expReward = (int)(baseExp * difficulty * levelMultiplier);
            int goldReward = (int)(baseGold * difficulty * levelMultiplier);
            
            return new SeasonalQuest(questName, description, expReward, goldReward, season, difficulty, Calendar, requiredLevel, enemyType);
        }
        
        private void VisitBlacksmith()
        {
            bool inBlacksmith = true;
            RecordAction(); // Record entering blacksmith as an action
            
            while (inBlacksmith)
            {
                Display.displayString("\n=== Blacksmith ===");
                Display.displayString($"Your Gold: {Player.Gold}");
                Display.displayString("\nAvailable Weapons:");
                
                for (int i = 0; i < BlacksmithWeapons.Count; i++)
                {
                    Display.displayString($"{i+1}. {BlacksmithWeapons[i].Name} - {BlacksmithWeapons[i].Description} - {BlacksmithWeapons[i].Value} Gold");
                }
                
                Display.displayString("\nAvailable Armor:");
                
                for (int i = 0; i < BlacksmithArmor.Count; i++)
                {
                    Display.displayString($"{i+1 + BlacksmithWeapons.Count}. {BlacksmithArmor[i].Name} - {BlacksmithArmor[i].Description} - {BlacksmithArmor[i].Value} Gold");
                }
                
                Display.displayString($"\n{BlacksmithWeapons.Count + BlacksmithArmor.Count + 1}. Leave Blacksmith");
                
                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    if (choice >= 1 && choice <= BlacksmithWeapons.Count)
                    {
                        Weapon selectedWeapon = BlacksmithWeapons[choice-1];
                        if (Player.SpendGold(selectedWeapon.Value))
                        {
                            Player.AddItem(selectedWeapon);
                            Display.displayString($"You purchased {selectedWeapon.Name}!");
                            RecordAction(); // Record buying weapon as an action
                        }
                        else
                        {
                            Display.displayString("You don't have enough gold!");
                        }
                    }
                    else if (choice > BlacksmithWeapons.Count && choice <= BlacksmithWeapons.Count + BlacksmithArmor.Count)
                    {
                        Armor selectedArmor = BlacksmithArmor[choice - BlacksmithWeapons.Count - 1];
                        if (Player.SpendGold(selectedArmor.Value))
                        {
                            Player.AddItem(selectedArmor);
                            Display.displayString($"You purchased {selectedArmor.Name}!");
                            RecordAction(); // Record buying armor as an action
                        }
                        else
                        {
                            Display.displayString("You don't have enough gold!");
                        }
                    }
                    else if (choice == BlacksmithWeapons.Count + BlacksmithArmor.Count + 1)
                    {
                        inBlacksmith = false;
                    }
                    else
                    {
                        Display.displayString("Invalid choice.");
                    }
                }
                else
                {
                    Display.displayString("Invalid choice.");
                }
            }
        }
        
        private void VisitPotionShop()
        {
            Display.displayString("\n=== Potion Shop ===");
            Display.displayString($"Your Gold: {Player.Gold}");

            // Create potion shop inventory
            List<Item> potionShop = new List<Item>
            {
                new HealingPotion("Small Health Potion", "Restores 20 HP", 50, 20),
                new HealingPotion("Medium Health Potion", "Restores 50 HP", 100, 50),
                new HealingPotion("Large Health Potion", "Restores 100 HP", 200, 100),
                new ManaPotion("Small Mana Potion", "Restores 10 MP", 75, 10),
                new ManaPotion("Medium Mana Potion", "Restores 25 MP", 150, 25),
                new ManaPotion("Large Mana Potion", "Restores 50 MP", 300, 50)
            };

            while (true)
            {
                Display.displayString("\nAvailable Potions:");
                for (int i = 0; i < potionShop.Count; i++)
                {
                    var potion = potionShop[i];
                    Display.displayString($"{i + 1}. {potion.Name} - {potion.Description} - {potion.Value} gold");
                }
                Display.displayString($"{potionShop.Count + 1}. Leave Potion Shop");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    if (choice == potionShop.Count + 1)
                    {
                        Display.displayString("Leaving Potion Shop...");
                        RecordAction(); // Record leaving shop as an action
                        break;
                    }
                    else if (choice >= 1 && choice <= potionShop.Count)
                    {
                        var selectedPotion = potionShop[choice - 1];
                        if (Player.SpendGold(selectedPotion.Value))
                        {
                            Player.AddItem(selectedPotion);
                            Display.displayString($"You purchased {selectedPotion.Name}!");
                            RecordAction(); // Record purchase as an action
                        }
                        else
                        {
                            Display.displayString("Not enough gold!");
                        }
                    }
                    else
                    {
                        Display.displayString("Invalid choice.");
                    }
                }
                else
                {
                    Display.displayString("Invalid input.");
                }
            }
        }
        
        private void VisitLibrary()
        {
            bool inLibrary = true;
            while (inLibrary)
            {
                Display.displayString("\n=== Arcane Library ===");
                Display.displayString($"Your Gold: {Player.Gold}");
                Display.displayString($"Your Mana: {Player.Mana}/{Player.MaxMana}");
                Display.displayString("\nAvailable Spells:");
                Display.displayString("1. Fireball (25 gold, 2 actions)");
                Display.displayString("   A basic fire spell that deals moderate damage.");
                Display.displayString("   Mana Cost: 10, Base Damage: 15");
                Display.displayString("\n2. Ice Spike (35 gold, 2 actions)");
                Display.displayString("   A freezing spell that reduces enemy speed.");
                Display.displayString("   Mana Cost: 15, Base Damage: 12");
                Display.displayString("\n3. Lightning Bolt (50 gold, 3 actions)");
                Display.displayString("   A powerful lightning spell with high damage.");
                Display.displayString("   Mana Cost: 20, Base Damage: 25");
                Display.displayString("\n4. Magic Missile (40 gold, 2 actions)");
                Display.displayString("   A basic ranged arcane spell.");
                Display.displayString("   Mana Cost: 8, Base Damage: 10, Range: 40 ft (Long: 75 ft)");
                Display.displayString("\n5. Arcane Shot (60 gold, 3 actions)");
                Display.displayString("   A precise magical projectile.");
                Display.displayString("   Mana Cost: 12, Base Damage: 16, Range: 60 ft (No long range)");
                Display.displayString("\n6. Return to Town Square");

                string? input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        LearnSpell("Fireball", "A basic fire spell that deals moderate damage.", 25, 2, 10, 15, true);
                        break;
                    case "2":
                        LearnSpell("Ice Spike", "A freezing spell that reduces enemy speed.", 35, 2, 15, 12, true);
                        break;
                    case "3":
                        if (Player.SpendGold(40))
                        {
                            if (Player.AddSkill(new Skill("Magic Missile", "A basic ranged arcane spell.", 8, 10, true, true, 40, 75)))
                                Display.displayString("You learned Magic Missile!");
                            else
                                Display.displayString("You already know Magic Missile.");
                        }
                        else
                        {
                            Display.displayString("Not enough gold!");
                        }
                        break;
                    case "4":
                        if (Player.SpendGold(60))
                        {
                            if (Player.AddSkill(new Skill("Arcane Shot", "A precise magical projectile.", 12, 16, true, true, 60, 0)))
                                Display.displayString("You learned Arcane Shot!");
                            else
                                Display.displayString("You already know Arcane Shot.");
                        }
                        else
                        {
                            Display.displayString("Not enough gold!");
                        }
                        break;
                    case "6":
                        inLibrary = false;
                        break;
                    default:
                        Display.displayString("Invalid choice. Please try again.");
                        break;
                }
            }
        }
        
        private void LearnSpell(string name, string description, int goldCost, int actionCost, int manaCost, int baseDamage, bool isMagic)
        {
            // Check if player already knows this spell
            if (Player.Skills.Exists(s => s.Name == name))
            {
                Display.displayString($"You already know {name}!");
                return;
            }

            // Check if player has enough gold
            if (Player.Gold < goldCost)
            {
                Display.displayString($"You need {goldCost} gold to learn {name}.");
                return;
            }

            // Check if player has enough actions left in the day
            if (Calendar.GetActionsUntilNewDay() < actionCost)
            {
                Display.displayString($"You don't have enough time left today to learn {name}.");
                Display.displayString($"You need {actionCost} actions, but only have {Calendar.GetActionsUntilNewDay()} left.");
                return;
            }

            // Deduct gold and record actions
            Player.SpendGold(goldCost);
            for (int i = 0; i < actionCost; i++)
            {
                RecordAction(false); // Record studying as a non-combat action
            }

            // Create and add the new skill
            Skill newSkill = new Skill(name, description, manaCost, baseDamage, isMagic);
            Player.AddSkill(newSkill);

            Display.displayString($"\nYou have learned {name}!");
            Display.displayString($"Actions remaining today: {Calendar.GetActionsUntilNewDay()}");
        }
        
        private void DisplayActiveQuests()
        {
            ViewActiveQuests();
        }

        private void ViewInventory()
        {
            Display.displayString("\n=== Your Inventory ===");
            
            if (Player.Inventory.Count == 0)
            {
                Display.displayString("Your inventory is empty.");
                return;
            }
            
            for (int i = 0; i < Player.Inventory.Count; i++)
            {
                var item = Player.Inventory[i];
                Display.displayString($"{i+1}. {item.Name} - {item.Description}");
            }
            
            Display.displayString($"{Player.Inventory.Count+1}. Use an item");
            Display.displayString($"{Player.Inventory.Count+2}. Back");
            
            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                if (choice == Player.Inventory.Count+1)
                {
                    UseInventoryItem();
                }
                // If they chose "Back", just return to the previous menu
            }
            else
            {
                Display.displayString("Invalid choice.");
            }
        }
        
        private void UseInventoryItem()
        {
            Display.displayString("Which item would you like to use? (Enter number)");
            
            for (int i = 0; i < Player.Inventory.Count; i++)
            {
                var item = Player.Inventory[i];
                Display.displayString($"{i+1}. {item.Name}");
            }
            
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= Player.Inventory.Count)
            {
                var item = Player.Inventory[choice-1];
                Player.UseItem(item.Name);
            }
            else
            {
                Display.displayString("Invalid choice.");
            }
        }

        private void ManageSkills()
        {
            Display.displayString("\n=== Your Skills ===");
            
            if (Player.Skills.Count == 0)
            {
                Display.displayString("You don't know any skills yet!");
                return;
            }
            else
            {
                Display.displayString("\nKnown Skills:");
                for (int i = 0; i < Player.Skills.Count; i++)
                {
                    var skill = Player.Skills[i];
                    Display.displayString($"{i + 1}. {skill.Name}");
                    Display.displayString($"   Description: {skill.Description}");
                    Display.displayString($"   Mana Cost: {skill.ManaCost}");
                    Display.displayString($"   Base Damage: {skill.BaseDamage}");
                    Display.displayString($"   Type: {(skill.IsMagic ? "Magic" : "Physical")}");
                    Display.displayString(""); // Empty line for readability
                }
            }
            return;
        }

        private void ManageEquipment()
        {
            Display.displayString("\n=== Equipment ===");
            Display.displayString($"Current Weapon: {Player.EquippedWeapon?.Name ?? "None"}");
            Display.displayString($"Current Armor: {Player.EquippedArmor?.Name ?? "None"}");
            Display.displayString("1. Equip Weapon");
            Display.displayString("2. Equip Armor");
            Display.displayString("3. Back");
            
            string? input = Console.ReadLine();
            
            switch (input)
            {
                case "1":
                    EquipWeapon();
                    break;
                case "2":
                    EquipArmor();
                    break;
                case "3":
                    // Return to previous menu
                    break;
                default:
                    Display.displayString("Invalid choice.");
                    break;
            }
        }
        
        private void EquipWeapon()
        {
            var weapons = Player.Inventory.FindAll(i => i is Weapon).Cast<Weapon>().ToList();
            
            if (weapons.Count == 0)
            {
                Display.displayString("You don't have any weapons in your inventory.");
                return;
            }
            
            Display.displayString("Choose a weapon to equip:");
            for (int i = 0; i < weapons.Count; i++)
            {
                Display.displayString($"{i+1}. {weapons[i].Name} - Attack Bonus: +{weapons[i].AttackBonus}");
            }
            
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= weapons.Count)
            {
                if (Player.EquipWeapon(weapons[choice-1].Name))
                {
                    Display.displayString($"You equipped {weapons[choice-1].Name}.");
                }
                else
                {
                    Display.displayString("Failed to equip the weapon.");
                }
            }
            else
            {
                Display.displayString("Invalid choice.");
            }
        }
        
        private void EquipArmor()
        {
            var armors = Player.Inventory.FindAll(i => i is Armor).Cast<Armor>().ToList();
            
            if (armors.Count == 0)
            {
                Display.displayString("You don't have any armor in your inventory.");
                return;
            }
            
            Display.displayString("Choose armor to equip:");
            for (int i = 0; i < armors.Count; i++)
            {
                Display.displayString($"{i+1}. {armors[i].Name} - Defense Bonus: +{armors[i].DefenseBonus}");
            }
            
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= armors.Count)
            {
                if (Player.EquipArmor(armors[choice-1].Name))
                {
                    Display.displayString($"You equipped {armors[choice-1].Name}.");
                }
                else
                {
                    Display.displayString("Failed to equip the armor.");
                }
            }
            else
            {
                Display.displayString("Invalid choice.");
            }
        }
    }
}
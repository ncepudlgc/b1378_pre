using System;
using System.Linq;

namespace TextRPG
{
    public class BattleSystem
    {
        private Character Player { get; }
        private Enemy Enemy { get; }
        private Display Display { get; }
        private bool isRangedBattle = false;
        private int distanceInSquares = 0; // Distance between player and enemy in squares
        private (int x, int y) playerPos;
        private (int x, int y) enemyPos;
        private int initialDistanceSnapshot;

        public BattleSystem(Character player, Enemy enemy, Display display)
        {
            Player = player;
            Enemy = enemy;
            Display = display;
        }

        public BattleSystem(Character player, Enemy enemy, Display display, bool rangedBattle = false, int initialDistance = 0, (int x, int y)? playerStart = null, (int x, int y)? enemyStart = null)
            : this(player, enemy, display)
        {
            isRangedBattle = rangedBattle;
            distanceInSquares = initialDistance;
            if (playerStart.HasValue) playerPos = playerStart.Value;
            if (enemyStart.HasValue) enemyPos = enemyStart.Value;
        }

        public void StartBattle()
        {
            Display.displayString($"\nA {Enemy.Name} appears!");
    
            while (Player.IsAlive() && Enemy.IsAlive())
            {
                // If ranged battle, update available attacks based on distance
                if (isRangedBattle)
                {
                    int distanceFeet = distanceInSquares * 5;
                    var rangedWeapons = Player.Inventory.OfType<Weapon>().Where(w => w.IsRanged && (distanceFeet <= w.LongRange && w.LongRange > 0 || distanceFeet <= w.NormalRange)).ToList();
                    var rangedSkills = Player.Skills.Where(s => s.IsRanged && (distanceFeet <= s.LongRange && s.LongRange > 0 || distanceFeet <= s.NormalRange)).ToList();
                    var meleeWeapons = Player.Inventory.OfType<Weapon>().Where(w => !w.IsRanged).ToList();
                    bool inMelee = distanceInSquares <= 1;
                    Display.displayString("\nYour turn! Choose an action:");
                    int option = 1;
                    if (inMelee)
                    {
                        Display.displayString($"{option}. Melee Attack");
                        option++;
                    }
                    if (rangedWeapons.Count > 0)
                    {
                        Display.displayString($"{option}. Ranged Weapon Attack");
                        option++;
                    }
                    if (rangedSkills.Count > 0)
                    {
                        Display.displayString($"{option}. Ranged Spell Attack");
                        option++;
                    }
                    Display.displayString($"{option}. Use Item");
                    option++;
                    Display.displayString($"{option}. Run");
                    string? input = Console.ReadLine();
                    int chosen = 0;
                    int.TryParse(input, out chosen);
                    bool playerActed = false;
                    string? attackType = null;
                    int menuIndex = 1;
                    if (inMelee && chosen == menuIndex++)
                    {
                        // Melee attack
                        Display.displayString("Choose your attack type:");
                        Display.displayString("1. Light Attack (Less damage, take less damage in return)");
                        Display.displayString("2. Normal Attack");
                        Display.displayString("3. Heavy Attack (More damage, take more damage in return)");
                        attackType = Console.ReadLine();
                        switch (attackType)
                        {
                            case "1":
                                int lightDamage = Player.LightAttack();
                                int enemyHealthBefore = Enemy.Health;
                                Enemy.TakeDamage(lightDamage);
                                int actualLightDamage = enemyHealthBefore - Enemy.Health;
                                Display.displayString($"You perform a light attack on the {Enemy.Name} for {actualLightDamage} damage!");
                                playerActed = true;
                                break;
                            case "2":
                                int normalDamage = Player.AttackMethod();
                                int enemyHealthBeforeNormal = Enemy.Health;
                                Enemy.TakeDamage(normalDamage);
                                int actualNormalDamage = enemyHealthBeforeNormal - Enemy.Health;
                                Display.displayString($"You attack the {Enemy.Name} for {actualNormalDamage} damage!");
                                playerActed = true;
                                break;
                            case "3":
                                int heavyDamage = Player.HeavyAttack();
                                int enemyHealthBeforeHeavy = Enemy.Health;
                                Enemy.TakeDamage(heavyDamage);
                                int actualHeavyDamage = enemyHealthBeforeHeavy - Enemy.Health;
                                Display.displayString($"You perform a heavy attack on the {Enemy.Name} for {actualHeavyDamage} damage!");
                                playerActed = true;
                                break;
                            default:
                                Display.displayString("Invalid attack type. Choose 1-3.");
                                continue;
                        }
                    }
                    else if (rangedWeapons.Count > 0 && chosen == menuIndex++)
                    {
                        // Ranged weapon attack
                        Display.displayString("Choose a ranged weapon:");
                        for (int i = 0; i < rangedWeapons.Count; i++)
                        {
                            Display.displayString($"{i + 1}. {rangedWeapons[i].Name} (Normal: {rangedWeapons[i].NormalRange} ft, Long: {rangedWeapons[i].LongRange} ft)");
                        }
                        int weaponChoice = int.Parse(Console.ReadLine() ?? "1") - 1;
                        var weapon = rangedWeapons[weaponChoice];
                        int disadvantage = (distanceFeet > weapon.NormalRange || distanceFeet <= 5) ? 1 : 0;
                        int damage = Player.AttackMethod();
                        if (disadvantage == 1)
                        {
                            // Roll twice, take lower (simulate by halving damage for now)
                            damage = Math.Max(1, damage / 2);
                            Display.displayString("Attack at disadvantage!");
                        }
                        int enemyHealthBefore = Enemy.Health;
                        Enemy.TakeDamage(damage);
                        int actualDamage = enemyHealthBefore - Enemy.Health;
                        Display.displayString($"You shoot {weapon.Name} at the {Enemy.Name} for {actualDamage} damage!");
                        playerActed = true;
                    }
                    else if (rangedSkills.Count > 0 && chosen == menuIndex++)
                    {
                        // Ranged spell attack
                        Display.displayString("Choose a ranged spell:");
                        for (int i = 0; i < rangedSkills.Count; i++)
                        {
                            Display.displayString($"{i + 1}. {rangedSkills[i].Name} (Mana: {rangedSkills[i].ManaCost}, Normal: {rangedSkills[i].NormalRange} ft, Long: {rangedSkills[i].LongRange} ft)");
                        }
                        int skillChoice = int.Parse(Console.ReadLine() ?? "1") - 1;
                        var skill = rangedSkills[skillChoice];
                        if (Player.Mana < skill.ManaCost)
                        {
                            Display.displayString($"Not enough mana! You need {skill.ManaCost} mana to cast {skill.Name}.");
                            continue;
                        }
                        int disadvantage = (distanceFeet > skill.NormalRange || distanceFeet <= 5) ? 1 : 0;
                        int enemyHealthBefore = Enemy.Health;
                        if (Player.UseSkill(skill.Name, Enemy))
                        {
                            int actualSpellDamage = enemyHealthBefore - Enemy.Health;
                            if (disadvantage == 1)
                            {
                                actualSpellDamage = Math.Max(1, actualSpellDamage / 2);
                                Enemy.Heal(actualSpellDamage); // Undo half, then apply half
                                Enemy.TakeDamage(actualSpellDamage);
                                Display.displayString("Spell at disadvantage!");
                            }
                            Display.displayString($"You cast {skill.Name} on the {Enemy.Name} for {actualSpellDamage} magic damage!");
                            playerActed = true;
                        }
                        else
                        {
                            Display.displayString("Failed to cast spell.");
                            continue;
                        }
                    }
                    else if (chosen == menuIndex++)
                    {
                        // Use Item
                        if (Player.Inventory.Count == 0)
                        {
                            Display.displayString("You have no items in your inventory!");
                            continue;
                        }
                        Display.displayString("\nYour inventory:");
                        for (int i = 0; i < Player.Inventory.Count; i++)
                        {
                            var item = Player.Inventory[i];
                            Display.displayString($"{i + 1}. {item.Name} - {item.Description}");
                        }
                        Display.displayString($"{Player.Inventory.Count + 1}. Back");
                        if (int.TryParse(Console.ReadLine(), out int itemChoice) && itemChoice >= 1 && itemChoice <= Player.Inventory.Count)
                        {
                            var selectedItem = Player.Inventory[itemChoice - 1];
                            if (selectedItem is HealingPotion potion)
                            {
                                Player.UseItem(selectedItem.Name);
                                Display.displayString($"You used {selectedItem.Name} and restored {potion.HealAmount} HP!");
                                playerActed = true;
                            }
                            else
                            {
                                Display.displayString("You can't use that item in battle!");
                            }
                        }
                        else
                        {
                            Display.displayString("Invalid choice.");
                        }
                    }
                    else if (chosen == menuIndex++)
                    {
                        // Run
                        if (TryToRun())
                        {
                            Display.displayString("You successfully escaped!");
                            return;
                        }
                        Display.displayString("You failed to escape!");
                        playerActed = true;
                    }
                    else
                    {
                        Display.displayString("Invalid input. Choose a valid action.");
                        continue;
                    }

                    if (playerActed && Enemy.IsAlive())
                    {
                        // Enemy's turn: only ranged attack or move closer
                        int enemyDistanceFeet = distanceInSquares * 5;
                        var enemyRangedSkills = Enemy.Skills.Where(s => s.IsRanged && (enemyDistanceFeet <= s.LongRange && s.LongRange > 0 || enemyDistanceFeet <= s.NormalRange)).ToList();
                        bool enemyInMelee = distanceInSquares <= 1;
                        if (enemyInMelee)
                        {
                            // Enemy can attack normally
                            int enemyDamage = Enemy.AttackMethod();
                            int playerHealthBefore = Player.Health;
                            Player.TakeDamage(enemyDamage);
                            int actualEnemyDamage = playerHealthBefore - Player.Health;
                            Display.displayString($"The {Enemy.Name} attacks you for {actualEnemyDamage} damage!");
                        }
                        else if (enemyRangedSkills.Count > 0)
                        {
                            // Enemy uses a random ranged skill
                            var skill = enemyRangedSkills[new Random().Next(enemyRangedSkills.Count)];
                            if (Enemy.Mana >= skill.ManaCost)
                            {
                                int playerHealthBefore = Player.Health;
                                Enemy.UseSkill(skill.Name, Player);
                                int actualDamage = playerHealthBefore - Player.Health;
                                Display.displayString($"The {Enemy.Name} uses {skill.Name} and hits you for {actualDamage} magic damage!");
                            }
                            else
                            {
                                // Move closer if can't use skill
                                distanceInSquares = Math.Max(1, distanceInSquares - 1);
                                Display.displayString($"The {Enemy.Name} moves closer!");
                            }
                        }
                        else
                        {
                            // Move closer
                            distanceInSquares = Math.Max(1, distanceInSquares - 1);
                            Display.displayString($"The {Enemy.Name} moves closer!");
                        }
                    }

                    // Display battle status
                    Display.displayString($"\nYour HP: {Player.Health}/{Player.MaxHealth}");
                    Display.displayString($"Your Mana: {Player.Mana}/{Player.MaxMana}");
                    Display.displayString($"Enemy HP: {Enemy.Health}/{Enemy.MaxHealth}");
                }
                else
                {
                    Display.displayString("\nYour turn! Choose an action:");
                    Display.displayString("1. Attack");
                    Display.displayString("2. Magic Attack");
                    Display.displayString("3. Use Item");
                    Display.displayString("4. Run");

                    string? input = Console.ReadLine();
                    bool playerActed = false;
                    string? attackType = null;

                    switch (input)
                    {
                        case "1": // Physical Attack
                            Display.displayString("\nChoose your attack type:");
                            Display.displayString("1. Light Attack (Less damage, take less damage in return)");
                            Display.displayString("2. Normal Attack");
                            Display.displayString("3. Heavy Attack (More damage, take more damage in return)");
                            
                            attackType = Console.ReadLine();
                            switch (attackType)
                            {
                                case "1": // Light Attack
                                    int lightDamage = Player.LightAttack();
                                    int enemyHealthBefore = Enemy.Health;
                                    Enemy.TakeDamage(lightDamage);
                                    int actualLightDamage = enemyHealthBefore - Enemy.Health;
                                    Display.displayString($"You perform a light attack on the {Enemy.Name} for {actualLightDamage} damage!");
                                    playerActed = true;
                                    break;

                                case "2": // Normal Attack
                                    int normalDamage = Player.AttackMethod();
                                    int enemyHealthBeforeNormal = Enemy.Health;
                                    Enemy.TakeDamage(normalDamage);
                                    int actualNormalDamage = enemyHealthBeforeNormal - Enemy.Health;
                                    Display.displayString($"You attack the {Enemy.Name} for {actualNormalDamage} damage!");
                                    playerActed = true;
                                    break;

                                case "3": // Heavy Attack
                                    int heavyDamage = Player.HeavyAttack();
                                    int enemyHealthBeforeHeavy = Enemy.Health;
                                    Enemy.TakeDamage(heavyDamage);
                                    int actualHeavyDamage = enemyHealthBeforeHeavy - Enemy.Health;
                                    Display.displayString($"You perform a heavy attack on the {Enemy.Name} for {actualHeavyDamage} damage!");
                                    playerActed = true;
                                    break;

                                default:
                                    Display.displayString("Invalid attack type. Choose 1-3.");
                                    continue;
                            }
                            break;

                        case "2": // Magic Attack
                            if (Player.Skills.Count == 0)
                            {
                                Display.displayString("You don't know any spells!");
                                continue;
                            }

                            Display.displayString("\nChoose your spell:");
                            for (int i = 0; i < Player.Skills.Count; i++)
                            {
                                var skill = Player.Skills[i];
                                Display.displayString($"{i + 1}. {skill.Name} ({skill.ManaCost} mana) - {skill.Description}");
                            }
                            Display.displayString($"{Player.Skills.Count + 1}. Back");

                            if (int.TryParse(Console.ReadLine(), out int spellChoice) && spellChoice >= 1 && spellChoice <= Player.Skills.Count)
                            {
                                var selectedSkill = Player.Skills[spellChoice - 1];
                                if (Player.Mana >= selectedSkill.ManaCost)
                                {
                                    int enemyHealthBeforeSpell = Enemy.Health;
                                    if (Player.UseSkill(selectedSkill.Name, Enemy))
                                    {
                                        int actualSpellDamage = enemyHealthBeforeSpell - Enemy.Health;
                                        Display.displayString($"You cast {selectedSkill.Name} on the {Enemy.Name} for {actualSpellDamage} magic damage!");
                                        playerActed = true;
                                    }
                                }
                                else
                                {
                                    Display.displayString($"Not enough mana! You need {selectedSkill.ManaCost} mana to cast {selectedSkill.Name}.");
                                    continue;
                                }
                            }
                            else if (spellChoice != Player.Skills.Count + 1)
                            {
                                Display.displayString("Invalid spell choice.");
                                continue;
                            }
                            break;

                        case "3": // Use Item
                            if (Player.Inventory.Count == 0)
                            {
                                Display.displayString("You have no items in your inventory!");
                                continue;
                            }

                            Display.displayString("\nYour inventory:");
                            for (int i = 0; i < Player.Inventory.Count; i++)
                            {
                                var item = Player.Inventory[i];
                                Display.displayString($"{i + 1}. {item.Name} - {item.Description}");
                            }
                            Display.displayString($"{Player.Inventory.Count + 1}. Back");

                            if (int.TryParse(Console.ReadLine(), out int itemChoice) && itemChoice >= 1 && itemChoice <= Player.Inventory.Count)
                            {
                                var selectedItem = Player.Inventory[itemChoice - 1];
                                if (selectedItem is HealingPotion potion)
                                {
                                    Player.UseItem(selectedItem.Name);
                                    Display.displayString($"You used {selectedItem.Name} and restored {potion.HealAmount} HP!");
                                    playerActed = true;
                                }
                                else
                                {
                                    Display.displayString("You can't use that item in battle!");
                                }
                            }
                            else if (itemChoice != Player.Inventory.Count + 1)
                            {
                                Display.displayString("Invalid choice.");
                            }
                            break;

                        case "4": // Run
                            if (TryToRun())
                            {
                                Display.displayString("You successfully escaped!");
                                return;
                            }
                            Display.displayString("You failed to escape!");
                            playerActed = true;
                            break;

                        default:
                            Display.displayString("Invalid input. Choose 1-4.");
                            continue;
                    }

                    if (playerActed && Enemy.IsAlive())
                    {
                        // Enemy's turn with damage modification based on attack type
                        int enemyDamage = Enemy.AttackMethod();
                        int playerHealthBefore = Player.Health;
                        
                        // Modify incoming damage based on attack type
                        if (input == "1" && attackType == "1") // Light attack - take 80% damage
                        {
                            enemyDamage = (int)(enemyDamage * 0.8);
                            Display.displayString("Your light stance reduces incoming damage!");
                        }
                        else if (input == "1" && attackType == "3") // Heavy attack - take 120% damage
                        {
                            enemyDamage = (int)(enemyDamage * 1.2);
                            Display.displayString("Your heavy stance leaves you vulnerable!");
                        }
                        
                        Player.TakeDamage(enemyDamage);
                        int actualEnemyDamage = playerHealthBefore - Player.Health;
                        Display.displayString($"The {Enemy.Name} attacks you for {actualEnemyDamage} damage!");
                    }

                    // Display battle status
                    Display.displayString($"\nYour HP: {Player.Health}/{Player.MaxHealth}");
                    Display.displayString($"Your Mana: {Player.Mana}/{Player.MaxMana}");
                    Display.displayString($"Enemy HP: {Enemy.Health}/{Enemy.MaxHealth}");
                }
            }

            if (Player.IsAlive())
            {
                int goldReward = Enemy.GoldReward;
                int expReward = Enemy.ExperienceReward;
                
                Display.displayString($"\nYou defeated the {Enemy.Name}!");
                Display.displayString($"You gained {expReward} experience points!");
                Display.displayString($"You found {goldReward} gold!");
                
                Player.AddExperience(expReward);
                Player.AddGold(goldReward);
            }
            else
            {
                Display.displayString("\nYou were defeated!");
                // TODO: Handle player death
            }
        }

        private bool TryToRun()
        {
            Random random = new Random();
            return random.Next(100) < 50; // 50% chance to escape
        }

        public (int x, int y) EnemyFinalPosition => enemyPos;
        public bool EnemyMoved => isRangedBattle && distanceInSquares < initialDistanceSnapshot;

        public void StartBattleWithFirstRangedHit(string attackName, bool isWeapon, int normalRange, int longRange, int damage, int initialDistance)
        {
            isRangedBattle = true;
            distanceInSquares = initialDistance;
            initialDistanceSnapshot = initialDistance;
            // Deliver the first hit
            int disadvantage = (initialDistance * 5 > normalRange || initialDistance * 5 <= 5) ? 1 : 0;
            int actualDamage = damage;
            if (disadvantage == 1)
            {
                actualDamage = Math.Max(1, damage / 2);
                Display.displayString("Attack at disadvantage!");
            }
            int enemyHealthBefore = Enemy.Health;
            Enemy.TakeDamage(actualDamage);
            int dealt = enemyHealthBefore - Enemy.Health;
            Display.displayString($"You hit {Enemy.Name} with {attackName} for {dealt} damage!");
            if (!Enemy.IsAlive())
            {
                Display.displayString($"You defeated the {Enemy.Name} before it could react!");
                return;
            }
            // Enemy's first response
            Display.displayString($"The {Enemy.Name} is surprised! It turns and moves toward you.");
            distanceInSquares = Math.Max(1, distanceInSquares - 1);
            // Now enter the normal ranged battle loop
            StartBattleLoop();
        }

        private void StartBattleLoop()
        {
            while (Player.IsAlive() && Enemy.IsAlive())
            {
                int distanceFeet = distanceInSquares * 5;
                var rangedWeapons = Player.Inventory.OfType<Weapon>().Where(w => w.IsRanged && (distanceFeet <= w.LongRange && w.LongRange > 0 || distanceFeet <= w.NormalRange)).ToList();
                var rangedSkills = Player.Skills.Where(s => s.IsRanged && (distanceFeet <= s.LongRange && s.LongRange > 0 || distanceFeet <= s.NormalRange)).ToList();
                var meleeWeapons = Player.Inventory.OfType<Weapon>().Where(w => !w.IsRanged).ToList();
                bool inMelee = distanceInSquares <= 1;
                Display.displayString("\nYour turn! Choose an action:");
                int option = 1;
                if (inMelee)
                {
                    Display.displayString($"{option}. Melee Attack");
                    option++;
                }
                if (rangedWeapons.Count > 0)
                {
                    Display.displayString($"{option}. Ranged Weapon Attack");
                    option++;
                }
                if (rangedSkills.Count > 0)
                {
                    Display.displayString($"{option}. Ranged Spell Attack");
                    option++;
                }
                Display.displayString($"{option}. Use Item");
                option++;
                Display.displayString($"{option}. Run");
                string? input = Console.ReadLine();
                int chosen = 0;
                int.TryParse(input, out chosen);
                bool playerActed = false;
                string? attackType = null;
                int menuIndex = 1;
                if (inMelee && chosen == menuIndex++)
                {
                    // Melee attack
                    Display.displayString("Choose your attack type:");
                    Display.displayString("1. Light Attack (Less damage, take less damage in return)");
                    Display.displayString("2. Normal Attack");
                    Display.displayString("3. Heavy Attack (More damage, take more damage in return)");
                    attackType = Console.ReadLine();
                    switch (attackType)
                    {
                        case "1":
                            int lightDamage = Player.LightAttack();
                            int enemyHealthBefore = Enemy.Health;
                            Enemy.TakeDamage(lightDamage);
                            int actualLightDamage = enemyHealthBefore - Enemy.Health;
                            Display.displayString($"You perform a light attack on the {Enemy.Name} for {actualLightDamage} damage!");
                            playerActed = true;
                            break;
                        case "2":
                            int normalDamage = Player.AttackMethod();
                            int enemyHealthBeforeNormal = Enemy.Health;
                            Enemy.TakeDamage(normalDamage);
                            int actualNormalDamage = enemyHealthBeforeNormal - Enemy.Health;
                            Display.displayString($"You attack the {Enemy.Name} for {actualNormalDamage} damage!");
                            playerActed = true;
                            break;
                        case "3":
                            int heavyDamage = Player.HeavyAttack();
                            int enemyHealthBeforeHeavy = Enemy.Health;
                            Enemy.TakeDamage(heavyDamage);
                            int actualHeavyDamage = enemyHealthBeforeHeavy - Enemy.Health;
                            Display.displayString($"You perform a heavy attack on the {Enemy.Name} for {actualHeavyDamage} damage!");
                            playerActed = true;
                            break;
                        default:
                            Display.displayString("Invalid attack type. Choose 1-3.");
                            continue;
                    }
                }
                else if (rangedWeapons.Count > 0 && chosen == menuIndex++)
                {
                    // Ranged weapon attack
                    Display.displayString("Choose a ranged weapon:");
                    for (int i = 0; i < rangedWeapons.Count; i++)
                    {
                        Display.displayString($"{i + 1}. {rangedWeapons[i].Name} (Normal: {rangedWeapons[i].NormalRange} ft, Long: {rangedWeapons[i].LongRange} ft)");
                    }
                    int weaponChoice = int.Parse(Console.ReadLine() ?? "1") - 1;
                    var weapon = rangedWeapons[weaponChoice];
                    int disadvantage = (distanceFeet > weapon.NormalRange || distanceFeet <= 5) ? 1 : 0;
                    int damage = Player.AttackMethod();
                    if (disadvantage == 1)
                    {
                        damage = Math.Max(1, damage / 2);
                        Display.displayString("Attack at disadvantage!");
                    }
                    int enemyHealthBefore = Enemy.Health;
                    Enemy.TakeDamage(damage);
                    int actualDamage = enemyHealthBefore - Enemy.Health;
                    Display.displayString($"You shoot {weapon.Name} at the {Enemy.Name} for {actualDamage} damage!");
                    playerActed = true;
                }
                else if (rangedSkills.Count > 0 && chosen == menuIndex++)
                {
                    // Ranged spell attack
                    Display.displayString("Choose a ranged spell:");
                    for (int i = 0; i < rangedSkills.Count; i++)
                    {
                        Display.displayString($"{i + 1}. {rangedSkills[i].Name} (Mana: {rangedSkills[i].ManaCost}, Normal: {rangedSkills[i].NormalRange} ft, Long: {rangedSkills[i].LongRange} ft)");
                    }
                    int skillChoice = int.Parse(Console.ReadLine() ?? "1") - 1;
                    var skill = rangedSkills[skillChoice];
                    if (Player.Mana < skill.ManaCost)
                    {
                        Display.displayString($"Not enough mana! You need {skill.ManaCost} mana to cast {skill.Name}.");
                        continue;
                    }
                    int disadvantage = (distanceFeet > skill.NormalRange || distanceFeet <= 5) ? 1 : 0;
                    int enemyHealthBefore = Enemy.Health;
                    if (Player.UseSkill(skill.Name, Enemy))
                    {
                        int actualSpellDamage = enemyHealthBefore - Enemy.Health;
                        if (disadvantage == 1)
                        {
                            actualSpellDamage = Math.Max(1, actualSpellDamage / 2);
                            Enemy.Heal(actualSpellDamage); // Undo half, then apply half
                            Enemy.TakeDamage(actualSpellDamage);
                            Display.displayString("Spell at disadvantage!");
                        }
                        Display.displayString($"You cast {skill.Name} on the {Enemy.Name} for {actualSpellDamage} magic damage!");
                        playerActed = true;
                    }
                    else
                    {
                        Display.displayString("Failed to cast spell.");
                        continue;
                    }
                }
                else if (chosen == menuIndex++)
                {
                    // Use Item
                    if (Player.Inventory.Count == 0)
                    {
                        Display.displayString("You have no items in your inventory!");
                        continue;
                    }
                    Display.displayString("\nYour inventory:");
                    for (int i = 0; i < Player.Inventory.Count; i++)
                    {
                        var item = Player.Inventory[i];
                        Display.displayString($"{i + 1}. {item.Name} - {item.Description}");
                    }
                    Display.displayString($"{Player.Inventory.Count + 1}. Back");
                    if (int.TryParse(Console.ReadLine(), out int itemChoice) && itemChoice >= 1 && itemChoice <= Player.Inventory.Count)
                    {
                        var selectedItem = Player.Inventory[itemChoice - 1];
                        if (selectedItem is HealingPotion potion)
                        {
                            Player.UseItem(selectedItem.Name);
                            Display.displayString($"You used {selectedItem.Name} and restored {potion.HealAmount} HP!");
                            playerActed = true;
                        }
                        else
                        {
                            Display.displayString("You can't use that item in battle!");
                        }
                    }
                    else
                    {
                        Display.displayString("Invalid choice.");
                    }
                }
                else if (chosen == menuIndex++)
                {
                    // Run
                    if (TryToRun())
                    {
                        Display.displayString("You successfully escaped!");
                        break;
                    }
                    Display.displayString("You failed to escape!");
                    playerActed = true;
                }
                else
                {
                    Display.displayString("Invalid input. Choose a valid action.");
                    continue;
                }

                if (playerActed && Enemy.IsAlive())
                {
                    // Enemy's turn: only ranged attack or move closer
                    int enemyDistanceFeet = distanceInSquares * 5;
                    var enemyRangedSkills = Enemy.Skills.Where(s => s.IsRanged && (enemyDistanceFeet <= s.LongRange && s.LongRange > 0 || enemyDistanceFeet <= s.NormalRange)).ToList();
                    bool enemyInMelee = distanceInSquares <= 1;
                    if (enemyInMelee)
                    {
                        // Enemy can attack normally
                        int enemyDamage = Enemy.AttackMethod();
                        int playerHealthBefore = Player.Health;
                        Player.TakeDamage(enemyDamage);
                        int actualEnemyDamage = playerHealthBefore - Player.Health;
                        Display.displayString($"The {Enemy.Name} attacks you for {actualEnemyDamage} damage!");
                    }
                    else if (enemyRangedSkills.Count > 0)
                    {
                        // Enemy uses a random ranged skill
                        var skill = enemyRangedSkills[new Random().Next(enemyRangedSkills.Count)];
                        if (Enemy.Mana >= skill.ManaCost)
                        {
                            int playerHealthBefore = Player.Health;
                            Enemy.UseSkill(skill.Name, Player);
                            int actualDamage = playerHealthBefore - Player.Health;
                            Display.displayString($"The {Enemy.Name} uses {skill.Name} and hits you for {actualDamage} magic damage!");
                        }
                        else
                        {
                            // Move closer if can't use skill
                            distanceInSquares = Math.Max(1, distanceInSquares - 1);
                            Display.displayString($"The {Enemy.Name} moves closer!");
                        }
                    }
                    else
                    {
                        // Move closer
                        distanceInSquares = Math.Max(1, distanceInSquares - 1);
                        Display.displayString($"The {Enemy.Name} moves closer!");
                    }
                }

                // Display battle status
                Display.displayString($"\nYour HP: {Player.Health}/{Player.MaxHealth}");
                Display.displayString($"Your Mana: {Player.Mana}/{Player.MaxMana}");
                Display.displayString($"Enemy HP: {Enemy.Health}/{Enemy.MaxHealth}");
            }
        }
    }
} 
// QuestManager.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace TextRPG
{
    /// <summary>
    /// Centralizes quest generation logic to maintain separation of concerns.
    /// Handles generation of regular quests and bonus seasonal quests.
    /// </summary>
    public class QuestManager
    {
        private GameCalendar Calendar { get; }
        private Dungeon Dungeon { get; }
        private Random Random { get; } = new Random();

        public QuestManager(GameCalendar calendar, Dungeon dungeon)
        {
            Calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
            Dungeon = dungeon ?? throw new ArgumentNullException(nameof(dungeon));
        }

        /// <summary>
        /// Generates regular quests for the quest board: 2 quests per dungeon level for the current season,
        /// plus non-dungeon quests for the current season.
        /// </summary>
        /// <returns>List of generated seasonal quests for the quest board.</returns>
        public List<SeasonalQuest> GenerateSeasonQuests()
        {
            var quests = new List<SeasonalQuest>();
            string currentSeason = Calendar.GetCurrentSeason();

            // Generate 2 regular quests per dungeon level (levels 1, 2, 3)
            for (int level = 1; level <= 3; level++)
            {
                for (int i = 0; i < 2; i++)
                {
                    var quest = GenerateSeasonalQuest(currentSeason, level);
                    quests.Add(quest);
                }
            }

            // Generate non-dungeon quests for the current season
            // These are quests that don't require entering the dungeon
            var nonDungeonQuests = GenerateNonDungeonQuests(currentSeason);
            quests.AddRange(nonDungeonQuests);

            return quests;
        }

        /// <summary>
        /// Generates and places one seasonal bonus quest with bonus rewards in a random location
        /// on one dungeon level.
        /// </summary>
        public void GenerateAndPlaceBonusQuest()
        {
            string currentSeason = Calendar.GetCurrentSeason();
            
            // Generate a bonus quest (level 1-3, randomly selected)
            int bonusLevel = Random.Next(1, 4);
            var bonusQuest = GenerateBonusQuest(currentSeason, bonusLevel);

            // Place the quest in a random location on the selected dungeon level
            Dungeon.AddQuest(bonusQuest, bonusLevel - 1); // Convert to 0-based level
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

        private SeasonalQuest GenerateBonusQuest(string season, int requiredLevel)
        {
            string[] bonusQuestTypes = {
                "Seasonal Bonus: {0} Challenge",
                "Special {0} Mission",
                "Exclusive {0} Hunt",
                "Limited {0} Quest"
            };

            string[] enemyTypes = Calendar.GetSeasonalEnemyTypes();
            string enemyType = enemyTypes[Random.Next(enemyTypes.Length)];

            string questName = string.Format(bonusQuestTypes[Random.Next(bonusQuestTypes.Length)], enemyType);
            string description = $"A special seasonal quest with bonus rewards! Find this quest in the dungeon at level {requiredLevel}.";

            // Bonus quests have significantly higher rewards (2x multiplier)
            int baseExp = 100;
            int baseGold = 50;
            int difficulty = Random.Next(1, 4);
            double levelMultiplier = requiredLevel == 3 ? 3.0 : 1.0;
            double bonusMultiplier = 2.0; // Bonus multiplier for seasonal quests

            int expReward = (int)(baseExp * difficulty * levelMultiplier * bonusMultiplier);
            int goldReward = (int)(baseGold * difficulty * levelMultiplier * bonusMultiplier);

            return new SeasonalQuest(questName, description, expReward, goldReward, season, difficulty, Calendar, requiredLevel, enemyType);
        }

        private List<SeasonalQuest> GenerateNonDungeonQuests(string season)
        {
            var quests = new List<SeasonalQuest>();

            // Generate 1-2 non-dungeon quests (e.g., gathering, delivery, etc.)
            int count = Random.Next(1, 3);
            string[] nonDungeonTypes = {
                "Gather {0} Materials",
                "Deliver {0} Supplies",
                "Collect {0} Resources",
                "Trade {0} Goods"
            };

            for (int i = 0; i < count; i++)
            {
                string[] enemyTypes = Calendar.GetSeasonalEnemyTypes();
                string enemyType = enemyTypes[Random.Next(enemyTypes.Length)];
                string questName = string.Format(nonDungeonTypes[Random.Next(nonDungeonTypes.Length)], enemyType);
                string description = $"Complete this quest without entering the dungeon. {enemyType} related task.";

                int expReward = 50;
                int goldReward = 25;
                int difficulty = Random.Next(1, 3);

                var quest = new SeasonalQuest(questName, description, expReward, goldReward, season, difficulty, Calendar, 0, enemyType);
                quests.Add(quest);
            }

            return quests;
        }
    }
}

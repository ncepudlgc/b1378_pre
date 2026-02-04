using System;
using System.Collections.Generic;
using System.Linq;

namespace TextRPG
{
    /// <summary>
    /// Centralized quest management system
    /// Handles quest generation, distribution, and tracking
    /// </summary>
    public class QuestManager
    {
        private GameCalendar Calendar { get; }
        private Dungeon Dungeon { get; }
        private Random Random { get; }
        private const int MaxDungeonLevels = 3;

        public QuestManager(GameCalendar calendar, Dungeon dungeon)
        {
            Calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
            Dungeon = dungeon ?? throw new ArgumentNullException(nameof(dungeon));
            Random = new Random();
        }

        /// <summary>
        /// Generates quests for the beginning of a season
        /// - Two regular quests per dungeon level for the current season
        /// - Non-dungeon quests for the season
        /// - One seasonal bonus quest placed randomly in the dungeon
        /// </summary>
        public List<SeasonalQuest> GenerateSeasonQuests()
        {
            string currentSeason = Calendar.GetCurrentSeason();
            List<SeasonalQuest> quests = new List<SeasonalQuest>();

            // Generate two regular quests per dungeon level for the current season
            for (int level = 1; level <= MaxDungeonLevels; level++)
            {
                for (int i = 0; i < 2; i++)
                {
                    SeasonalQuest quest = GenerateRegularQuest(currentSeason, level);
                    quests.Add(quest);
                }
            }

            // Generate non-dungeon quests for the season (these are handled separately)
            // They can be added to the quest board but don't require dungeon levels

            return quests;
        }

        /// <summary>
        /// Generates a seasonal bonus quest and places it randomly in the dungeon
        /// </summary>
        public SeasonalQuest GenerateAndPlaceBonusQuest()
        {
            string currentSeason = Calendar.GetCurrentSeason();
            
            // Generate bonus quest with higher rewards
            int randomLevel = Random.Next(1, MaxDungeonLevels + 1);
            SeasonalQuest bonusQuest = GenerateBonusQuest(currentSeason, randomLevel);
            
            // Place the bonus quest in the dungeon at the random level
            Dungeon.AddQuest(bonusQuest, randomLevel - 1); // Convert to 0-based level
            
            return bonusQuest;
        }

        /// <summary>
        /// Generates a regular quest for a specific season and level
        /// </summary>
        private SeasonalQuest GenerateRegularQuest(string season, int requiredLevel)
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
            
            // Base rewards
            int baseExp = 100;
            int baseGold = 50;
            
            // Random difficulty (1-3)
            int difficulty = Random.Next(1, 4);
            
            // Scale rewards based on difficulty and level
            double levelMultiplier = requiredLevel == 3 ? 3.0 : requiredLevel == 2 ? 1.5 : 1.0;
            int expReward = (int)(baseExp * difficulty * levelMultiplier);
            int goldReward = (int)(baseGold * difficulty * levelMultiplier);
            
            return new SeasonalQuest(questName, description, expReward, goldReward, season, difficulty, Calendar, requiredLevel, enemyType);
        }

        /// <summary>
        /// Generates a bonus quest with enhanced rewards
        /// </summary>
        private SeasonalQuest GenerateBonusQuest(string season, int requiredLevel)
        {
            string[] bonusQuestTypes = {
                "Seasonal Challenge: Master {0} Hunter",
                "Seasonal Challenge: {0} Exterminator",
                "Seasonal Challenge: {0} Slayer",
                "Seasonal Challenge: {0} Bane"
            };

            string[] enemyTypes = Calendar.GetSeasonalEnemyTypes();
            string enemyType = enemyTypes[Random.Next(enemyTypes.Length)];
            
            string questName = string.Format(bonusQuestTypes[Random.Next(bonusQuestTypes.Length)], enemyType);
            string description = $"A special seasonal challenge! Defeat {enemyType} enemies to earn bonus rewards.";
            
            // Bonus quests have significantly higher rewards
            int baseExp = 200;
            int baseGold = 100;
            
            // Higher difficulty for bonus quests
            int difficulty = Random.Next(2, 4); // 2-3 difficulty
            
            // Enhanced rewards for bonus quests
            double levelMultiplier = requiredLevel == 3 ? 3.0 : requiredLevel == 2 ? 1.5 : 1.0;
            double bonusMultiplier = 1.5; // 50% bonus for seasonal quests
            int expReward = (int)(baseExp * difficulty * levelMultiplier * bonusMultiplier);
            int goldReward = (int)(baseGold * difficulty * levelMultiplier * bonusMultiplier);
            
            SeasonalQuest bonusQuest = new SeasonalQuest(questName, description, expReward, goldReward, season, difficulty, Calendar, requiredLevel, enemyType);
            
            // Mark as seasonal bonus (this will be handled by SeasonalQuest constructor if season matches)
            return bonusQuest;
        }
    }
}

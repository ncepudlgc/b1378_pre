// SeasonalQuest.cs
using System;

namespace TextRPG
{
    public class SeasonalQuest : Quest
    {
        public string Season { get; private set; }
        public int Difficulty { get; private set; }
        public bool IsSeasonalBonus { get; private set; }
        public int RequiredEnemyDefeats { get; private set; }
        public int CurrentEnemyDefeats { get; private set; }
        public int RequiredLevel { get; private set; }  // Level required to complete this quest
        private GameCalendar Calendar { get; }
        public string EnemyType { get; private set; }

        public SeasonalQuest(string name, string description, int experienceReward, int goldReward, 
                            string season, int difficulty, GameCalendar calendar, int requiredLevel = 1, string enemyType = "") 
            : base(name, description, experienceReward, goldReward)
        {
            Season = season;
            Difficulty = difficulty;
            Calendar = calendar;
            RequiredLevel = requiredLevel;
            IsSeasonalBonus = false;
            CurrentEnemyDefeats = 0;
            EnemyType = enemyType;
            
            // Set required enemy defeats based on difficulty and level
            RequiredEnemyDefeats = (difficulty + requiredLevel) switch
            {
                1 => 2,  // Easy quests on level 1
                2 => 4,  // Medium quests on level 1 or Easy quests on level 2
                3 => 6,  // Hard quests on level 1 or Medium quests on level 2
                4 => 8,  // Hard quests on level 2
                _ => 2   // Default to 2
            };
            
            // Apply seasonal bonus if quest matches current season
            if (season == Calendar.GetCurrentSeason())
            {
                // Seasonal quests give 25% more rewards
                ExperienceReward = (int)(experienceReward * 1.25);
                GoldReward = (int)(goldReward * 1.25);
                IsSeasonalBonus = true;
            }

            // Level 2 quests give 50% more rewards
            if (requiredLevel == 2)
            {
                ExperienceReward = (int)(ExperienceReward * 1.5);
                GoldReward = (int)(GoldReward * 1.5);
            }
        }

        public void IncrementEnemyDefeats()
        {
            CurrentEnemyDefeats++;
        }

        public bool IsComplete()
        {
            return CurrentEnemyDefeats >= RequiredEnemyDefeats;
        }

        public override string ToString()
        {
            string seasonalTag = IsSeasonalBonus ? $"[{Season} - BONUS REWARDS!]" : $"[{Season}]";
            string difficultyStars = new string('★', Difficulty);
            string progress = $"({CurrentEnemyDefeats}/{RequiredEnemyDefeats} enemies)";
            string levelTag = RequiredLevel > 1 ? $" [Level {RequiredLevel} Only]" : "";
            
            return $"{seasonalTag} {Name} {difficultyStars}{levelTag}\n" +
                   $"  {Description}\n" +
                   $"  Progress: {progress}\n" +
                   $"  Rewards: {ExperienceReward} XP, {GoldReward} Gold";
        }
    }
}

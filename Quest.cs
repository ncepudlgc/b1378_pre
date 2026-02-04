using System;

namespace TextRPG
{
    public class Quest
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int ExperienceReward { get; protected set; }
        public int GoldReward { get; protected set; }
        public bool IsCompleted { get; private set; }

        public Quest(string name, string description, int experienceReward, int goldReward)
        {
            Name = name;
            Description = description;
            ExperienceReward = experienceReward;
            GoldReward = goldReward;
            IsCompleted = false;
        }

        public void Complete()
        {
            IsCompleted = true;
        }
    }
} 
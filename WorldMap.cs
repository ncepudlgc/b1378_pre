using System;

namespace TextRPG
{
    public class WorldMap
    {
        public enum MapOption
        {
            Town,
            TownSquare,
            Dungeon1,
            Dungeon2,
            Exit,
            Invalid
        }

        public string GetIntroText()
        {
            return @"
            World Map:
            1. Return to Town
            2. Visit Town Square
            3. Enter Dungeon 1
            4. Enter Dungeon 2
            5. Exit to Main Menu
            Please enter your choice (1-5):";
        }

        public MapOption OptionValidation(string input)
        {
            if (int.TryParse(input, out int choice))
            {
                return choice switch
                {
                    1 => MapOption.Town,
                    2 => MapOption.TownSquare,
                    3 => MapOption.Dungeon1,
                    4 => MapOption.Dungeon2,
                    5 => MapOption.Exit,
                    _ => MapOption.Invalid
                };
            }
            return MapOption.Invalid;
        }
    }
} 
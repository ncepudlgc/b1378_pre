using System;

namespace TextRPG
{
    public class Display
    {
        public void displayString(string message)
        {
            Console.WriteLine(message);
        }

        // New: Method to display a dungeon map (renders a 2D char array as a grid)
        public void ShowDungeonMap(string mapDisplay)
        {
            Console.WriteLine("Dungeon Map:");
            Console.WriteLine(mapDisplay);
            Console.WriteLine("Legend: @ = You, . = Floor, # = Wall, $ = Treasure, E = Enemy");
        }

        // ... (Rest of the class remains unchanged)
    }
}

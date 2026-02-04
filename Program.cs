using System;

namespace TextRPG
{
    class Program
    {
        static void Main(string[] args)
        {
            Display display = new Display();
            MainMenu mainMenu = new MainMenu(display);
            
            // Get character from main menu
            Character player = mainMenu.StartMenu();
            
            // Initialize game world
            WorldMap worldMap = new WorldMap();
            GameCalendar calendar = new GameCalendar(display, player);
            Dungeon dungeon = new Dungeon("The Ancient Crypt", 20, 10, calendar);
            TownSquare townSquare = new TownSquare(player, display, calendar, dungeon);
            dungeon.SetDisplayAndPlayer(display, player);

            bool gameRunning = true;
            while (gameRunning)
            {
                display.displayString("\n=== World Map ===");
                display.displayString("1. Enter Town");
                display.displayString("2. Enter Dungeon");
                display.displayString("3. Save and Quit");

                string? input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        townSquare.EnterTownSquare();
                        break;
                    case "2":
                        dungeon.EnterDungeon();
                        break;
                    case "3":
                        try
                        {
                            player.SaveCharacter();
                            display.displayString("Game saved successfully!");
                        }
                        catch (Exception ex)
                        {
                            display.displayString($"Error saving game: {ex.Message}");
                        }
                        gameRunning = false;
                        break;
                    default:
                        display.displayString("Invalid choice. Please try again.");
                        break;
                }
            }
        }
    }
}

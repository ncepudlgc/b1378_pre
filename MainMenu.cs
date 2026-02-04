using System;
using System.IO;
using System.Collections.Generic;

namespace TextRPG
{
    public class MainMenu
    {
        private Display Display { get; }
        private Character? Player { get; set; }

        public MainMenu(Display display)
        {
            Display = display;
        }

        public Character StartMenu()
        {
            while (true)
            {
                Display.displayString("\n=== Main Menu ===");
                Display.displayString("1. New Game");
                Display.displayString("2. Load Game");
                Display.displayString("3. Exit");

                string? input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        CreateNewCharacter();
                        return Player!;

                    case "2":
                        LoadCharacter();
                        if (Player != null)
                        {
                            return Player;  // Only return if Player was successfully loaded
                        }
                        // If loading failed (e.g., no save file), break to loop back to the menu
                        break;

                    case "3":
                        Environment.Exit(0);
                        break;

                    default:
                        Display.displayString("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        private void CreateNewCharacter()
        {
            Display.displayString("\nEnter your character's name:");
            string? name = Console.ReadLine();
            if (string.IsNullOrEmpty(name))
            {
                Display.displayString("Invalid name. Please try again.");
                return;
            }

            Player = new Character(name)
            {
                Inventory = new List<Item>()
            };
            Display.displayString($"\nWelcome, {Player.Name}!");
        }

        private void LoadCharacter()
        {
            try
            {
                Player = Character.LoadCharacter();
                Display.displayString($"\nWelcome back, {Player.Name}!");
            }
            catch (FileNotFoundException)
            {
                Display.displayString("\nNo save file found. Please create a new character.");
                Display.displayString("Returning to the main menu...");  // Added for better UX to inform the user
            }
            catch (Exception ex)
            {
                Display.displayString($"\nError loading character: {ex.Message}");
                Display.displayString("Returning to the main menu...");  // Added for better UX to inform the user
            }
            // Note: Player remains null on failure, which is now handled in StartMenu()
        }
    }
} 
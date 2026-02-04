// Dungeon.cs
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace TextRPG
{
    public class Dungeon
    {
        public string Name { get; private set; }
        private int Width { get; }
        private int Height { get; }
        private char[][,] Maps { get; set; }  // Array of 2D maps for different levels
        private int PlayerX { get; set; }
        private int PlayerY { get; set; }
        private int CurrentLevel { get; set; } = 0;  // Current dungeon level (0-based)
        private Random Random { get; } = new Random();
        private Display? Display { get; set; }
        private Character? Player { get; set; }
        private GameCalendar Calendar { get; set; }
        private List<SeasonalQuest>[] LevelQuests { get; set; }  // Quests for each level
        private Dictionary<char, SeasonalQuest>[] QuestMarkers { get; set; }  // Quest markers for each level
        private Dictionary<(int x, int y), string>[] EnemyTypeMarkers = new Dictionary<(int x, int y), string>[MaxLevels]; // Track enemy type per cell
        private Dictionary<(int x, int y), Enemy>[] EnemyInstances = new Dictionary<(int x, int y), Enemy>[MaxLevels]; // Track Enemy objects per cell
        private Dictionary<string, char>[] EnemyTypeToMarker = new Dictionary<string, char>[MaxLevels]; // Per-level enemy type to marker
        private const int MaxLevels = 3;  // Number of dungeon levels
        private char tileUnderPlayer = '.'; // Track the tile under the player

        public Dungeon(string name, int width, int height, GameCalendar calendar)
        {
            Name = name;
            Width = width;
            Height = height;
            Calendar = calendar;
            
            // Subscribe to season change event
            Calendar.OnSeasonChanged += HandleSeasonChange;
            
            // Initialize maps for all levels
            Maps = new char[MaxLevels][,];
            for (int i = 0; i < MaxLevels; i++)
            {
                Maps[i] = new char[width, height];
            }
            
            // Initialize quest collections for each level
            LevelQuests = new List<SeasonalQuest>[MaxLevels];
            QuestMarkers = new Dictionary<char, SeasonalQuest>[MaxLevels];
            EnemyTypeMarkers = new Dictionary<(int x, int y), string>[MaxLevels];
            EnemyInstances = new Dictionary<(int x, int y), Enemy>[MaxLevels];
            EnemyTypeToMarker = new Dictionary<string, char>[MaxLevels];
            
            for (int i = 0; i < MaxLevels; i++)
            {
                LevelQuests[i] = new List<SeasonalQuest>();
                QuestMarkers[i] = new Dictionary<char, SeasonalQuest>();
                EnemyTypeMarkers[i] = new Dictionary<(int x, int y), string>();
                EnemyInstances[i] = new Dictionary<(int x, int y), Enemy>();
                EnemyTypeToMarker[i] = new Dictionary<string, char>();
            }
        }

        private void HandleSeasonChange()
        {
            Display?.displayString("The dungeon has changed with the new season!");
            GenerateRandomMap();
        }

        public void SetDisplayAndPlayer(Display display, Character player)
        {
            Display = display;
            Player = player;
        }

        public void AddQuest(SeasonalQuest quest, int level = 0)
        {
            if (level < 0 || level >= MaxLevels)
            {
                level = 0; // Default to first level if invalid
            }
            
            LevelQuests[level].Add(quest);
            // Generate a unique marker for this quest
            char marker = (char)('Q' + LevelQuests[level].Count);
            QuestMarkers[level][marker] = quest;
            Display?.displayString($"New quest added to level {level + 1}: {quest.Name}");
        }

        // Generate a random map with rooms and corridors for all levels
        public void GenerateRandomMap()
        {
            for (int level = 0; level < MaxLevels; level++)
            {
                // Initialize map with walls
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        Maps[level][x, y] = '#';  // Wall by default
                    }
                }

                // Create random rooms (e.g., 3-5 rooms)
                int roomCount = Random.Next(3, 6);
                var roomCenters = new List<(int x, int y)>();
                var roomBounds = new List<(int x, int y, int w, int h)>();
                for (int i = 0; i < roomCount; i++)
                {
                    int roomWidth = Random.Next(3, 7);
                    int roomHeight = Random.Next(3, 5);
                    int roomX = Random.Next(1, Width - roomWidth - 1);
                    int roomY = Random.Next(1, Height - roomHeight - 1);
                    roomCenters.Add((roomX + roomWidth / 2, roomY + roomHeight / 2));
                    roomBounds.Add((roomX, roomY, roomWidth, roomHeight));
                    // Carve out the room
                    for (int x = roomX; x < roomX + roomWidth; x++)
                    {
                        for (int y = roomY; y < roomY + roomHeight; y++)
                        {
                            Maps[level][x, y] = '.';  // Floor
                        }
                    }
                }

                // --- Corridor Placement: Delaunay Triangulation + MST ---
                // Helper function for squared distance
                int Dist2((int x, int y) a, (int x, int y) b) => (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);

                // Delaunay triangulation
                var delaunayEdges = new HashSet<(int, int)>();
                int n = roomCenters.Count;
                for (int i = 0; i < n; i++)
                {
                    for (int j = i + 1; j < n; j++)
                    {
                        // Check if edge (i, j) is Delaunay: no other point is inside the circle defined by i and j and any third point
                        bool isDelaunay = true;
                        for (int k = 0; k < n; k++)
                        {
                            if (k == i || k == j) continue;
                            // For 3 points, check if k is inside the circumcircle of (i, j, k)
                            for (int l = 0; l < n; l++)
                            {
                                if (l == i || l == j || l == k) continue;
                                // Only check triangles
                                var a = roomCenters[i];
                                var b = roomCenters[j];
                                var c = roomCenters[k];
                                var p = roomCenters[l];
                                // Calculate circumcircle
                                double A = b.x - a.x, B = b.y - a.y;
                                double C = c.x - a.x, D = c.y - a.y;
                                double E = A * (a.x + b.x) + B * (a.y + b.y);
                                double F = C * (a.x + c.x) + D * (a.y + c.y);
                                double G = 2.0 * (A * (c.y - b.y) - B * (c.x - b.x));
                                if (Math.Abs(G) < 1e-6) continue; // Collinear
                                double cx = (D * E - B * F) / G;
                                double cy = (A * F - C * E) / G;
                                double r2 = (a.x - cx) * (a.x - cx) + (a.y - cy) * (a.y - cy);
                                double d2 = (p.x - cx) * (p.x - cx) + (p.y - cy) * (p.y - cy);
                                if (d2 < r2 - 1e-6) { isDelaunay = false; break; }
                            }
                            if (!isDelaunay) break;
                        }
                        if (isDelaunay)
                        {
                            delaunayEdges.Add((i, j));
                            delaunayEdges.Add((j, i));
                        }
                    }
                }

                // Build MST (Prim's algorithm) on Delaunay edges
                var mstEdges = new List<(int, int)>();
                var connected = new HashSet<int> { 0 };
                while (connected.Count < n)
                {
                    int bestFrom = -1, bestTo = -1, bestDist = int.MaxValue;
                    foreach (var from in connected)
                    {
                        for (int to = 0; to < n; to++)
                        {
                            if (connected.Contains(to)) continue;
                            if (!delaunayEdges.Contains((from, to))) continue;
                            int dist = Dist2(roomCenters[from], roomCenters[to]);
                            if (dist < bestDist)
                            {
                                bestDist = dist;
                                bestFrom = from;
                                bestTo = to;
                            }
                        }
                    }
                    if (bestTo != -1)
                    {
                        mstEdges.Add((bestFrom, bestTo));
                        connected.Add(bestTo);
                    }
                    else
                    {
                        // Fallback: connect to any unconnected node
                        for (int to = 0; to < n; to++)
                        {
                            if (connected.Contains(to)) continue;
                            int dist = Dist2(roomCenters[0], roomCenters[to]);
                            if (dist < bestDist)
                            {
                                bestDist = dist;
                                bestFrom = 0;
                                bestTo = to;
                            }
                        }
                        if (bestTo != -1)
                        {
                            mstEdges.Add((bestFrom, bestTo));
                            connected.Add(bestTo);
                        }
                    }
                }

                // Add some extra Delaunay edges for a natural look with loops
                var extraEdges = new List<(int, int)>();
                var rand = new Random();
                foreach (var edge in delaunayEdges)
                {
                    if (!mstEdges.Contains(edge) && rand.NextDouble() < 0.2) // 20% chance
                        extraEdges.Add(edge);
                }

                // Carve corridors for each MST edge (and extra edges)
                foreach (var (from, to) in mstEdges.Concat(extraEdges))
                {
                    var a = roomCenters[from];
                    var b = roomCenters[to];
                    // L-shaped corridor: horizontal then vertical (randomize order for variety)
                    if (rand.Next(2) == 0)
                    {
                        for (int x = Math.Min(a.x, b.x); x <= Math.Max(a.x, b.x); x++)
                            Maps[level][x, a.y] = '.';
                        for (int y = Math.Min(a.y, b.y); y <= Math.Max(a.y, b.y); y++)
                            Maps[level][b.x, y] = '.';
                    }
                    else
                    {
                        for (int y = Math.Min(a.y, b.y); y <= Math.Max(a.y, b.y); y++)
                            Maps[level][a.x, y] = '.';
                        for (int x = Math.Min(a.x, b.x); x <= Math.Max(a.x, b.x); x++)
                            Maps[level][x, b.y] = '.';
                    }
                }

                // Helper to check for at least two open neighbors (orthogonal)
                bool HasAtLeastTwoOpenNeighbors(int x, int y, char[,] map)
                {
                    int count = 0;
                    if (x > 0 && map[x - 1, y] == '.') count++;
                    if (x < map.GetLength(0) - 1 && map[x + 1, y] == '.') count++;
                    if (y > 0 && map[x, y - 1] == '.') count++;
                    if (y < map.GetLength(1) - 1 && map[x, y + 1] == '.') count++;
                    return count >= 2;
                }

                // Helper to get all valid floor tiles in a room for stairs
                List<(int x, int y)> GetRoomTiles(int roomX, int roomY, int roomWidth, int roomHeight)
                {
                    var tiles = new List<(int, int)>();
                    for (int x = roomX; x < roomX + roomWidth; x++)
                        for (int y = roomY; y < roomY + roomHeight; y++)
                            if (Maps[level][x, y] == '.' && HasAtLeastTwoOpenNeighbors(x, y, Maps[level]))
                                tiles.Add((x, y));
                    return tiles;
                }

                // Place stairs in random rooms (never on top of each other)
                var usedStairTiles = new HashSet<(int, int)>();
                if (level < MaxLevels - 1)
                {
                    // Down stairs
                    int stairsRoom = rand.Next(roomBounds.Count);
                    var (rx, ry, rw, rh) = roomBounds[stairsRoom];
                    var tiles = GetRoomTiles(rx, ry, rw, rh);
                    if (tiles.Count > 0)
                    {
                        var (sx, sy) = tiles[rand.Next(tiles.Count)];
                        Maps[level][sx, sy] = '>';
                        usedStairTiles.Add((sx, sy));
                    }
                }
                if (level > 0)
                {
                    // Up stairs
                    int stairsRoom;
                    (int sx, int sy) = (-1, -1);
                    do {
                        stairsRoom = rand.Next(roomBounds.Count);
                        var (rx, ry, rw, rh) = roomBounds[stairsRoom];
                        var tiles = GetRoomTiles(rx, ry, rw, rh);
                        if (tiles.Count > 0)
                        {
                            (sx, sy) = tiles[rand.Next(tiles.Count)];
                        }
                    } while (usedStairTiles.Contains((sx, sy)));
                    if (sx != -1 && sy != -1)
                    {
                        Maps[level][sx, sy] = '<';
                        usedStairTiles.Add((sx, sy));
                    }
                }
                // Special case for level 3: Add additional up stairs to level 1
                if (level == MaxLevels - 1)
                {
                    int stairsRoom;
                    (int sx, int sy) = (-1, -1);
                    do {
                        stairsRoom = rand.Next(roomBounds.Count);
                        var (rx, ry, rw, rh) = roomBounds[stairsRoom];
                        var tiles = GetRoomTiles(rx, ry, rw, rh);
                        if (tiles.Count > 0)
                        {
                            (sx, sy) = tiles[rand.Next(tiles.Count)];
                        }
                    } while (usedStairTiles.Contains((sx, sy)));
                    if (sx != -1 && sy != -1)
                    {
                        Maps[level][sx, sy] = '^'; // Special stairs to top level
                        usedStairTiles.Add((sx, sy));
                    }
                }

                // Place quest objectives and enemies for this level
                PlaceQuestObjectives(level);
                PlaceRandomEnemies(level);
            }
            
            // Place player in a random floor cell on the first level
            CurrentLevel = 0;
            do
            {
                PlayerX = Random.Next(1, Width - 1);
                PlayerY = Random.Next(1, Height - 1);
            } while (Maps[CurrentLevel][PlayerX, PlayerY] != '.');
            tileUnderPlayer = Maps[CurrentLevel][PlayerX, PlayerY];
            Maps[CurrentLevel][PlayerX, PlayerY] = '@';  // Player symbol
        }

        private void PlaceQuestObjectives(int level)
        {
            foreach (var quest in LevelQuests[level])
            {
                // Place quest objective marker
                int x, y;
                do
                {
                    x = Random.Next(1, Width - 1);
                    y = Random.Next(1, Height - 1);
                } while (Maps[level][x, y] != '.');
                
                char marker = QuestMarkers[level].FirstOrDefault(kvp => kvp.Value == quest).Key;
                Maps[level][x, y] = marker;
            }
        }

        // Helper to generate unique map markers for enemy types
        private char GenerateUniqueEnemyMarker(string enemyType, List<string> existingEnemyTypes)
        {
            if (string.IsNullOrEmpty(enemyType) || enemyType.Length == 0)
                return 'E'; // Fallback for empty strings
            
            char firstLetter = char.ToUpper(enemyType[0]);
            
            // Check if any existing enemy type starts with the same letter
            bool hasConflict = existingEnemyTypes.Any(existing => 
                !string.IsNullOrEmpty(existing) && 
                existing.Length > 0 && 
                char.ToUpper(existing[0]) == firstLetter);
            
            if (!hasConflict)
                return firstLetter;
            
            // If there's a conflict, try using the second letter
            if (enemyType.Length > 1)
            {
                char secondLetter = char.ToUpper(enemyType[1]);
                // Check if second letter also conflicts
                bool secondLetterConflict = existingEnemyTypes.Any(existing => 
                    !string.IsNullOrEmpty(existing) && 
                    existing.Length > 1 && 
                    char.ToUpper(existing[1]) == secondLetter);
                
                if (!secondLetterConflict)
                    return secondLetter;
            }
            
            // If both first and second letters conflict, use a fallback
            return 'E';
        }

        private void PlaceRandomEnemies(int level)
        {
            EnemyTypeMarkers[level].Clear();
            EnemyInstances[level].Clear();
            EnemyTypeToMarker[level].Clear();
            string[] enemyTypes = Calendar.GetSeasonalEnemyTypes();
            int baseEnemyCount = LevelQuests[level].Count + Random.Next(1, 3);
            int enemyCount = baseEnemyCount + (level * 2); // More enemies on deeper levels
            // Assign markers per enemy type for this level
            List<char> usedMarkers = new List<char>();
            foreach (var type in enemyTypes)
            {
                char marker = GenerateUniqueEnemyMarkerForType(type, usedMarkers);
                EnemyTypeToMarker[level][type] = marker;
                usedMarkers.Add(marker);
            }
            for (int i = 0; i < enemyCount; i++)
            {
                int x, y;
                do
                {
                    x = Random.Next(1, Width - 1);
                    y = Random.Next(1, Height - 1);
                } while (Maps[level][x, y] != '.');
                string enemyType = enemyTypes[Random.Next(enemyTypes.Length)];
                char marker = EnemyTypeToMarker[level][enemyType];
                Maps[level][x, y] = marker;
                EnemyTypeMarkers[level][(x, y)] = enemyType;
                // Create the enemy instance and store it
                Enemy enemy = CreateEnemyOfType(enemyType, level);
                EnemyInstances[level][(x, y)] = enemy;
            }
        }

        // Helper to generate a unique marker for each enemy type
        private char GenerateUniqueEnemyMarkerForType(string enemyType, List<char> usedMarkers)
        {
            if (string.IsNullOrEmpty(enemyType) || enemyType.Length == 0)
                return 'E';
            char firstLetter = char.ToUpper(enemyType[0]);
            if (!usedMarkers.Contains(firstLetter))
                return firstLetter;
            if (enemyType.Length > 1)
            {
                char secondLetter = char.ToUpper(enemyType[1]);
                if (!usedMarkers.Contains(secondLetter))
                    return secondLetter;
            }
            // Try numbers as a last resort
            for (char c = '0'; c <= '9'; c++)
            {
                if (!usedMarkers.Contains(c))
                    return c;
            }
            return 'E';
        }

        // Helper to create an enemy of a specific type for a given level
        private Enemy CreateEnemyOfType(string enemyType, int level)
        {
            var multipliers = Calendar.GetSeasonalEnemyMultipliers();
            int baseHealth = 50;
            int baseAttack = 8;
            int baseDefense = 3;
            int expReward = 20;
            int goldReward = 15;
            double levelMultiplier = 1.0 + (level * 0.5); // 50% stronger per level
            int health = (int)(baseHealth * multipliers.health * levelMultiplier);
            int attack = (int)(baseAttack * multipliers.attack * levelMultiplier);
            int defense = (int)(baseDefense * multipliers.defense * levelMultiplier);
            int exp = (int)(expReward * levelMultiplier);
            int gold = (int)(goldReward * levelMultiplier);
            string name = (level > 0 ? "Elite " : "") + enemyType;
            return new Enemy(name, health, attack, defense, exp, gold);
        }

        // Get a string representation of the map for display
        public string GetMapDisplay()
        {
            StringBuilder sb = new StringBuilder();
            
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    char cell = Maps[CurrentLevel][x, y];
                    // If this cell is a quest marker, always show 'Q'
                    if (QuestMarkers[CurrentLevel].ContainsKey(cell))
                    {
                        sb.Append('Q');
                    }
                    // If this cell is an enemy marker, show the marker assigned to this enemy type
                    else if (EnemyTypeMarkers[CurrentLevel].ContainsKey((x, y)))
                    {
                        string enemyType = EnemyTypeMarkers[CurrentLevel][(x, y)];
                        char marker = EnemyTypeToMarker[CurrentLevel].ContainsKey(enemyType) ? EnemyTypeToMarker[CurrentLevel][enemyType] : 'E';
                        sb.Append(marker);
                    }
                    else
                    {
                        sb.Append(cell);
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        // Move the player if the new position is valid
        public bool MovePlayer(int deltaX, int deltaY)
        {
            int newX = PlayerX + deltaX;
            int newY = PlayerY + deltaY;
            if (newX >= 0 && newX < Width && newY >= 0 && newY < Height && Maps[CurrentLevel][newX, newY] != '#')
            {
                char cellType = Maps[CurrentLevel][newX, newY];
                if (cellType == '>')
                {
                    ChangeLevel(CurrentLevel + 1);
                    return true;
                }
                else if (cellType == '<')
                {
                    ChangeLevel(CurrentLevel - 1);
                    return true;
                }
                else if (cellType == '^')
                {
                    ChangeLevel(0); // Go to top level
                    return true;
                }
                // Restore the tile under the player at the old position
                Maps[CurrentLevel][PlayerX, PlayerY] = tileUnderPlayer;
                // Save the tile under the player at the new position
                tileUnderPlayer = Maps[CurrentLevel][newX, newY];
                PlayerX = newX;
                PlayerY = newY;
                Maps[CurrentLevel][PlayerX, PlayerY] = '@';
                // Handle enemy encounter if present
                if (EnemyInstances[CurrentLevel].ContainsKey((newX, newY)))
                {
                    Enemy enemy = EnemyInstances[CurrentLevel][(newX, newY)];
                    HandleEnemyEncounter(enemy, (newX, newY));
                }
                else if (QuestMarkers[CurrentLevel].ContainsKey(cellType))
                {
                    HandleQuestObjective(QuestMarkers[CurrentLevel][cellType]);
                    QuestMarkers[CurrentLevel].Remove(cellType);
                    tileUnderPlayer = '.';
                }
                return true;
            }
            return false;
        }

        private void ChangeLevel(int newLevel)
        {
            if (newLevel < 0 || newLevel >= MaxLevels || Display == null)
                return;
            // Restore the tile under the player at the old position
            Maps[CurrentLevel][PlayerX, PlayerY] = tileUnderPlayer;
            int oldLevel = CurrentLevel;
            CurrentLevel = newLevel;
            // Find a suitable starting position on the new level
            char targetStair;
            if (oldLevel == MaxLevels - 1 && newLevel == 0) // Special case: from bottom to top via '^'
            {
                targetStair = '^';
            }
            else if (newLevel > oldLevel)
            {
                targetStair = '<';
            }
            else
            {
                targetStair = '>';
            }
            bool foundStairs = false;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (Maps[CurrentLevel][x, y] == targetStair)
                    {
                        PlayerX = x;
                        PlayerY = y;
                        foundStairs = true;
                        break;
                    }
                }
                if (foundStairs) break;
            }
            // If no stairs found, find any open floor
            if (!foundStairs)
            {
                do
                {
                    PlayerX = Random.Next(1, Width - 1);
                    PlayerY = Random.Next(1, Height - 1);
                } while (Maps[CurrentLevel][PlayerX, PlayerY] != '.');
            }
            // Save the tile under the player at the new position
            tileUnderPlayer = Maps[CurrentLevel][PlayerX, PlayerY];
            // Place player at the new position
            Maps[CurrentLevel][PlayerX, PlayerY] = '@';
            Display.displayString($"You are now on dungeon level {CurrentLevel + 1}.");
            if (CurrentLevel > oldLevel)
            {
                Display.displayString("The enemies here seem stronger than before...");
            }
            Display.displayString("\nUse arrow keys to move, 'R' for ranged attack, or 'E' to escape.");
            Display.displayString("Legend: @ = player, > = down stairs, < = up stairs, ^ = stairs to top level,");
            Display.displayString("Q = Quest, other letters = 1st/2nd letter of enemy type");

        }

        // New: Handle enemy encounter with a specific enemy instance
        private void HandleEnemyEncounter(Enemy enemy, (int x, int y) pos)
        {
            if (Player == null || Display == null) return;
            Calendar.RecordAction(true);
            BattleSystem battle = new BattleSystem(Player, enemy, Display);
            battle.StartBattle();
            if (enemy.Health <= 0)
            {
                Display.displayString("\n=== Quest Progress Update ===");
                bool anyQuestsUpdated = false;
                foreach (var quest in Player.Quests.ToList())
                {
                    if (quest is SeasonalQuest seasonalQuest &&
                        seasonalQuest.RequiredLevel == CurrentLevel + 1 &&
                        !string.IsNullOrEmpty(seasonalQuest.EnemyType) &&
                        enemy.Name.Contains(seasonalQuest.EnemyType, StringComparison.OrdinalIgnoreCase))
                    {
                        anyQuestsUpdated = true;
                        seasonalQuest.IncrementEnemyDefeats();
                        Display.displayString($"\nQuest: {seasonalQuest.Name}");
                        Display.displayString($"Progress: {seasonalQuest.CurrentEnemyDefeats}/{seasonalQuest.RequiredEnemyDefeats} enemies defeated");
                        if (seasonalQuest.IsComplete())
                        {
                            Display.displayString($"\nQuest Complete: {seasonalQuest.Name}!");
                            Display.displayString($"You receive:");
                            Display.displayString($"- {seasonalQuest.ExperienceReward} Experience Points");
                            Display.displayString($"- {seasonalQuest.GoldReward} Gold");
                            Player.AddExperience(seasonalQuest.ExperienceReward);
                            Player.AddGold(seasonalQuest.GoldReward);
                            Player.Quests.Remove(seasonalQuest);
                            Display.displayString($"Quest removed from your quest log.");
                        }
                    }
                }
                if (!anyQuestsUpdated)
                {
                    Display.displayString("\nNo active quests for this enemy type and level.");
                }
                // Remove enemy from map and dictionary
                EnemyInstances[CurrentLevel].Remove(pos);
                EnemyTypeMarkers[CurrentLevel].Remove(pos);
                Maps[CurrentLevel][pos.x, pos.y] = '.';
                tileUnderPlayer = '.';
                // Restore player symbol at current position
                Maps[CurrentLevel][PlayerX, PlayerY] = '@';
            }
        }

        private void HandleQuestObjective(SeasonalQuest quest)
        {
            if (Player == null || Display == null || quest == null) return;

            Display.displayString($"\n=== Quest Objective Found ===");
            Display.displayString($"Quest: {quest.Name}");
            Display.displayString($"Description: {quest.Description}");
            Display.displayString($"Progress: {quest.CurrentEnemyDefeats}/{quest.RequiredEnemyDefeats} enemies defeated");

            // Check if the quest can be completed
            if (quest.IsComplete())
            {
                Display.displayString("\nQuest completed! You receive:");
                Player.AddExperience(quest.ExperienceReward);
                Display.displayString($"- {quest.ExperienceReward} Experience Points");
                Player.AddGold(quest.GoldReward);
                Display.displayString($"- {quest.GoldReward} Gold");
                Player.Quests.Remove(quest);
            }
        }

        // Get the current calendar date string
        public string GetCalendarDate()
        {
            return Calendar.GetDateString();
        }

        public void RecordAction()
        {
            Calendar.RecordAction();
        }

        // Get the cell type at the player's current position
        public char GetCellAtPlayerPosition()
        {
            return Maps[CurrentLevel][PlayerX, PlayerY];
        }

        // Get list of active quests for the current level
        public List<SeasonalQuest> GetActiveQuests()
        {
            return new List<SeasonalQuest>(LevelQuests[CurrentLevel]);
        }

        // Get all active quests across all levels
        public List<SeasonalQuest> GetAllActiveQuests()
        {
            List<SeasonalQuest> allQuests = new List<SeasonalQuest>();
            for (int i = 0; i < MaxLevels; i++)
            {
                allQuests.AddRange(LevelQuests[i]);
            }
            return allQuests;
        }

        public void EnterDungeon()
        {
            if (Display == null || Player == null) return;

            Display.displayString($"Entering {Name}...");
            Display.displayString($"Current date: {GetCalendarDate()}");
            Display.displayString($"Actions until new day: {Calendar.GetActionsUntilNewDay()}");
            Calendar.RecordAction(false); // Record entering dungeon as a non-combat action
            GenerateRandomMap();
            Display.displayString("Use arrow keys to move, 'R' for ranged attack, or 'E' to escape.");
            Display.displayString("Legend: @ = player, > = down stairs, < = up stairs, ^ = stairs to top level,");
            Display.displayString("Q = Quest, other letters = 1st/2nd letter of enemy type");

            bool inDungeon = true;
            while (inDungeon)
            {
                // Display the current map
                Display.displayString(GetMapDisplay());

                // Get user key input
                var keyInfo = Console.ReadKey(true);
                var key = keyInfo.Key;
                bool moved = false;

                if (key == ConsoleKey.E)
                {
                    Display.displayString($"Leaving {Name} and returning to the World Map.");
                    Calendar.RecordAction(); // Record leaving dungeon as an action
                    inDungeon = false;
                    continue;
                }

                // Handle ranged attack
                if (key == ConsoleKey.R)
                {
                    // Find all ranged weapons and ranged skills
                    var rangedWeapons = Player.Inventory.OfType<Weapon>().Where(w => w.IsRanged).ToList();
                    var rangedSkills = Player.Skills.Where(s => s.IsRanged).ToList();
                    if (rangedWeapons.Count == 0 && rangedSkills.Count == 0)
                    {
                        Display.displayString("You have no ranged weapons or spells.");
                        continue;
                    }
                    // Find all enemies within range of any ranged attack
                    int maxLongRange = 0;
                    int maxNormalRange = 0;
                    if (rangedWeapons.Count > 0)
                    {
                        maxLongRange = rangedWeapons.Max(w => w.LongRange);
                        maxNormalRange = rangedWeapons.Max(w => w.NormalRange);
                    }
                    if (rangedSkills.Count > 0)
                    {
                        maxLongRange = Math.Max(maxLongRange, rangedSkills.Max(s => s.LongRange));
                        maxNormalRange = Math.Max(maxNormalRange, rangedSkills.Max(s => s.NormalRange));
                    }
                    // If no long range, use normal range for check
                    int effectiveRange = maxLongRange > 0 ? maxLongRange : maxNormalRange;
                    // Find enemies within effective range
                    var enemiesInRange = EnemyInstances[CurrentLevel].Where(e => 
                        GetDistance(PlayerX, PlayerY, e.Key.x, e.Key.y) * 5 <= effectiveRange).ToList();
                    if (enemiesInRange.Count == 0)
                    {
                        Display.displayString("There are no enemies within range of your ranged attacks.");
                        continue;
                    }
                    // List available ranged attacks
                    Display.displayString("Choose a ranged weapon or spell:");
                    int option = 1;
                    foreach (var weapon in rangedWeapons)
                    {
                        Display.displayString($"{option}. {weapon.Name} (Normal: {weapon.NormalRange} ft, Long: {weapon.LongRange} ft)");
                        option++;
                    }
                    foreach (var skill in rangedSkills)
                    {
                        Display.displayString($"{option}. {skill.Name} (Normal: {skill.NormalRange} ft, Long: {skill.LongRange} ft, Mana: {skill.ManaCost})");
                        option++;
                    }
                    Display.displayString($"{option}. Cancel");
                    int attackChoice = 0;
                    if (!int.TryParse(Console.ReadLine(), out attackChoice) || attackChoice < 1 || attackChoice > option)
                    {
                        Display.displayString("Invalid choice.");
                        continue;
                    }
                    if (attackChoice == option) // Cancel
                        continue;
                    bool isWeapon = attackChoice <= rangedWeapons.Count;
                    // List enemies in range
                    Display.displayString("Choose a target:");
                    for (int i = 0; i < enemiesInRange.Count; i++)
                    {
                        var enemy = enemiesInRange[i];
                        Display.displayString($"{i + 1}. {enemy.Value.Name} at ({enemy.Key.x + 1},{enemy.Key.y + 1})");
                    }
                    Display.displayString($"{enemiesInRange.Count + 1}. Cancel");
                    int targetChoice = 0;
                    if (!int.TryParse(Console.ReadLine(), out targetChoice) || targetChoice < 1 || targetChoice > enemiesInRange.Count + 1)
                    {
                        Display.displayString("Invalid choice.");
                        continue;
                    }
                    if (targetChoice == enemiesInRange.Count + 1) // Cancel
                        continue;
                    var target = enemiesInRange[targetChoice - 1];
                    int distance = GetDistance(PlayerX, PlayerY, target.Key.x, target.Key.y);
                    // Prepare for ranged battle: deliver first hit, then start loop
                    int damage = 0;
                    string attackName = "";
                    int normalRange = 0, longRange = 0;
                    if (isWeapon)
                    {
                        var weapon = rangedWeapons[attackChoice - 1];
                        attackName = weapon.Name;
                        normalRange = weapon.NormalRange;
                        longRange = weapon.LongRange;
                        damage = Player.AttackMethod();
                    }
                    else
                    {
                        var skill = rangedSkills[attackChoice - rangedWeapons.Count - 1];
                        attackName = skill.Name;
                        normalRange = skill.NormalRange;
                        longRange = skill.LongRange;
                        if (Player.Mana < skill.ManaCost)
                        {
                            Display.displayString($"Not enough mana! You need {skill.ManaCost} mana to cast {skill.Name}.");
                            continue;
                        }
                        Player.Mana -= skill.ManaCost;
                        damage = skill.BaseDamage + Player.MagicAttack;
                    }
                    var battle = new BattleSystem(Player, target.Value, Display);
                    battle.StartBattleWithFirstRangedHit(attackName, isWeapon, normalRange, longRange, damage, distance);
                    // If enemy is defeated, remove from map
                    if (!target.Value.IsAlive())
                    {
                        EnemyInstances[CurrentLevel].Remove(target.Key);
                        EnemyTypeMarkers[CurrentLevel].Remove(target.Key);
                        Maps[CurrentLevel][target.Key.x, target.Key.y] = '.';
                    }
                    else if (battle.EnemyMoved)
                    {
                        // Move enemy closer on the map
                        int dx = Math.Sign(PlayerX - target.Key.x);
                        int dy = Math.Sign(PlayerY - target.Key.y);
                        int newX = target.Key.x + dx;
                        int newY = target.Key.y + dy;
                        // Only move if the new cell is open
                        if (Maps[CurrentLevel][newX, newY] == '.')
                        {
                            EnemyInstances[CurrentLevel].Remove(target.Key);
                            EnemyTypeMarkers[CurrentLevel].Remove(target.Key);
                            Maps[CurrentLevel][target.Key.x, target.Key.y] = '.';
                            EnemyInstances[CurrentLevel][(newX, newY)] = target.Value;
                            EnemyTypeMarkers[CurrentLevel][(newX, newY)] = target.Value.Name;
                            Maps[CurrentLevel][newX, newY] = Maps[CurrentLevel][newX, newY] == '@' ? '@' : Maps[CurrentLevel][newX, newY];
                        }
                    }
                    // Restore player symbol at current position
                    Maps[CurrentLevel][PlayerX, PlayerY] = '@';
                    continue;
                }

                // Handle movement (arrow keys)
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        moved = MovePlayer(0, -1);
                        break;
                    case ConsoleKey.DownArrow:
                        moved = MovePlayer(0, 1);
                        break;
                    case ConsoleKey.LeftArrow:
                        moved = MovePlayer(-1, 0);
                        break;
                    case ConsoleKey.RightArrow:
                        moved = MovePlayer(1, 0);
                        break;
                    default:
                        Display.displayString("Invalid input. Use arrow keys to move, 'R' for ranged attack, or 'E' to escape.");
                        break;
                }

                if (!moved && (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow || key == ConsoleKey.LeftArrow || key == ConsoleKey.RightArrow))
                {
                    Display.displayString("You can't move there! (Wall or boundary.)");
                }
            }
        }

        // Helper method for distance
        private int GetDistance(int x1, int y1, int x2, int y2)
        {
            // Chebyshev distance (diagonal allowed)
            return Math.Max(Math.Abs(x1 - x2), Math.Abs(y1 - y2));
        }
    }
}
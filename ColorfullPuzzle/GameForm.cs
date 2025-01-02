//Bacoro dit Elhadji Lansar
//B211200567
//Bilisim Sistemleri Muhendisligi
//24-25-NDP-Ödev 


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;


namespace ColorfullPuzzle
{
    public class HighScore
    {
        public string PlayerName { get; set; }
        public int Score { get; set; }
    }
    public partial class GameForm : Form
    {
        private const int GridSize = 8; // 8x8 grid
        private const int TileSize = 50; // 50 Tile size in pixels
        private const int TimerDuration = 50; // Game duration in seconds

        private Button[,] tiles;
        private Timer gameTimer;
        private int score;
        private int timeLeft;
        private Random random;
        private Button selectedTile = null; // Selected tile
        private int selectedRow = 0;
        private int selectedCol = 0;

        private static readonly Color[] MatchColors = { Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Purple, Color.Aqua };
        private static readonly Color JokerColor = Color.Black; // Joker tile color

        //joker images
        private static readonly string[] JokerImages =
          {
            "C:\\Users\\lansa\\source\\repos\\ColorfullPuzzle\\ColorfullPuzzle\\Images/discoball.png",
             "C:\\Users\\lansa\\source\\repos\\ColorfullPuzzle\\ColorfullPuzzle\\Images/bomb.png",
             "C:\\Users\\lansa\\source\\repos\\ColorfullPuzzle\\ColorfullPuzzle\\Images/helicopter.png",
             "C:\\Users\\lansa\\source\\repos\\ColorfullPuzzle\\ColorfullPuzzle\\Images/roket.png"
         };
        private bool isPaused = false; // Variable to track the pause state

        // high  scores
        
        List<HighScore> highScores = new List<HighScore>();
        private string currentPlayerName;
        // Le nom du fichier texte où les scores sont enregistrés
        private const string highScoresFile = "D:\\highScores.txt";



       
        public GameForm(string playerName)
        {
            InitializeComponent();
            currentPlayerName = playerName;
            InitializeGame(playerName);
            LoadHighScoresFromFile();
        }

        private void InitializeGame(string playerName)
        {
            random = new Random(); // Initialize the random number generator for the game
            score = 0;              // Reset the player's score to 0
            timeLeft = TimerDuration; // Set the time left to the initial timer duration

            // Display the player's name on a label or textbox called player_name
            player_name.Text = playerName;

            //random positions for joker
            List<Point> jokerPositions = GetRandomJokerPositions(4);     

            // Initialize the grid, tiles is a 2D array of Buttons
            tiles = new Button[GridSize, GridSize];

            // Calculate the starting position for the grid so it is centered on the panel
            int startX = (game_panel.Width - (GridSize * TileSize)) / 2;
            int startY = (game_panel.Height - (GridSize * TileSize)) / 2;

            // Create the grid of tiles
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    bool isJoker = jokerPositions.Contains(new Point(i, j));
                    // Create each tile with the specified parameters (i, j for grid positions and startX, startY for placement)
                    tiles[i, j] = CreateTile(i, j, startX, startY, isJoker);

                    // Add the tile button to the game_panel controls so it will be displayed
                    game_panel.Controls.Add(tiles[i, j]);
                }
            }

            // Initialize the timer that controls the game’s countdown
            gameTimer = new Timer { Interval = 1000 }; // Timer interval is 1000 milliseconds (1 second)
            gameTimer.Tick += GameTimer_Tick;           // Subscribe to the timer's Tick event
            gameTimer.Start();                          // Start the timer
        }
        private List<Point> GetRandomJokerPositions(int jokerCount)
        {
            List<Point> jokerPositions = new List<Point>();

            while (jokerPositions.Count < jokerCount)
            {
                Point newPoint = new Point(random.Next(GridSize), random.Next(GridSize));

                if (!jokerPositions.Contains(newPoint))
                {
                    jokerPositions.Add(newPoint);
                }
            }

            return jokerPositions;
        }


        private Button CreateTile(int row, int col, int startX, int startY, bool isJoker)
        {
            Button tile = new Button
            {
                Width = TileSize,
                Height = TileSize,
                Left = startX + (col * TileSize),
                Top = startY + (row * TileSize),
                Tag = new Point(row, col) // Tile position in grid
            };

            if (isJoker)
            {
                string imagePath = JokerImages[random.Next(JokerImages.Length)];
                tile.BackgroundImage = Image.FromFile(imagePath);
                (tile.BackgroundImage as Bitmap).Tag = imagePath; // Store path for identification
                tile.BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                tile.BackColor = GetRandomTileColor(); // Couleur aléatoire pour les tuiles normales
            }

            tile.Click += Tile_Click;
            return tile;
        }


        private Color GetRandomTileColor()
        {
            // Randomly select a regular tile
            return MatchColors[random.Next(MatchColors.Length)];
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            timeLeft--;
            time_label.Text = timeLeft.ToString();

            if (timeLeft <= 0)
            {
                gameTimer.Stop();
                AddScore(score); // Add current score to high scores
                ShowHighScores(); // Display high scores
                MessageBox.Show($"Time's up! Your score: {score}", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
        }
        private async Task HandleJokerTile(Button tile)
        {
            string imagePath = tile.BackgroundImage != null ?
                ((tile.BackgroundImage as Bitmap)?.Tag as string) ?? "" : "";

            // Clear the joker tile first
            if (tile.BackgroundImage != null)
            {
                tile.BackgroundImage.Dispose();
                tile.BackgroundImage = null;
                tile.BackColor = Color.Transparent;
            }

            // Execute effect based on saved image path
            if (imagePath.Contains("roket"))
                await HandleRocketEffect(tile);
            else if (imagePath.Contains("bomb"))
                await HandleBombEffect(tile);
            else if (imagePath.Contains("helicopter"))
                await HandleHelicopterEffect(tile);
            else if (imagePath.Contains("discoball"))
                await HandleDiscoballEffect(tile);

            await Task.Delay(500);
            ShiftTilesDown();
        }

        private async void Tile_Click(object sender, EventArgs e)
        {
            Button clickedTile = sender as Button;
            if (clickedTile.BackgroundImage != null)
            {
                 HandleJokerTile(clickedTile);
                return;
            }

            if (selectedTile == null)
            {
                // Select first tile
                selectedTile = clickedTile;
                HighlightTile(selectedTile, true);
            }
            else
            {
                // Check if the second tile is adjacent
                if (IsAdjacent(selectedTile, clickedTile))
                {
                    SwapTiles(selectedTile, clickedTile);

                    // Check for matches
                    if (!await CheckForMatchesAsync())
                    {
                        // If no matches, swap tiles back
                        SwapTiles(selectedTile, clickedTile);
                    }
                    else
                    {
                        // If matches, check entire grid
                        CheckAndHandleMatches();
                    }
                }

                // Reset selection
                HighlightTile(selectedTile, false);
                selectedTile = null;
            }
        }



        private void HighlightTile(Button tile, bool highlight)
        {
            if (highlight)
            {
                tile.FlatAppearance.BorderColor = Color.Blue;
                tile.FlatAppearance.BorderSize = 2;
            }
            else
            {
                tile.FlatAppearance.BorderSize = 0;
            }
        }

        private bool IsAdjacent(Button tile1, Button tile2)
        {
            Point pos1 = (Point)tile1.Tag;
            Point pos2 = (Point)tile2.Tag;

            return (Math.Abs(pos1.X - pos2.X) == 1 && pos1.Y == pos2.Y) ||
                   (Math.Abs(pos1.Y - pos2.Y) == 1 && pos1.X == pos2.X);
        }

        private void SwapTiles(Button tile1, Button tile2)
        {
            Color tempColor = tile1.BackColor;
            tile1.BackColor = tile2.BackColor;
            tile2.BackColor = tempColor;
        }
        private void ShiftTilesDown()
        {
            for (int col = 0; col < GridSize; col++)
            {
                for (int row = GridSize - 1; row >= 0; row--)
                {
                    if (tiles[row, col].BackColor == Color.Transparent)
                    {
                        // Find the closest tile above and move it down
                        for (int k = row - 1; k >= 0; k--)
                        {
                            if (tiles[k, col].BackColor != Color.Transparent)
                            {
                                tiles[row, col].BackColor = tiles[k, col].BackColor;
                                tiles[k, col].BackColor = Color.Transparent;
                                break;
                            }
                        }

                        // If no tiles above, generate a new tile at the top
                        if (tiles[row, col].BackColor == Color.Transparent)
                        {
                            tiles[row, col].BackColor = GetRandomTileColor();
                        }
                    }
                }
            }
        }

        private async Task<bool> CheckForMatchesAsync()
        {
            bool matchesFound = false;
            List<Button> tilesToRemove = new List<Button>();

            // Check for horizontal matches
            for (int row = 0; row < GridSize; row++)
            {
                for (int col = 0; col < GridSize - 2; col++)
                {
                    if (tiles[row, col].BackColor == tiles[row, col + 1].BackColor &&
                        tiles[row, col].BackColor == tiles[row, col + 2].BackColor &&
                        tiles[row, col].BackColor != JokerColor)
                    {
                        tilesToRemove.Add(tiles[row, col]);
                        tilesToRemove.Add(tiles[row, col + 1]);
                        tilesToRemove.Add(tiles[row, col + 2]);
                        matchesFound = true;
                    }
                }
            }

            // Check for vertical matches
            for (int col = 0; col < GridSize; col++)
            {
                for (int row = 0; row < GridSize - 2; row++)
                {
                    if (tiles[row, col].BackColor == tiles[row + 1, col].BackColor &&
                        tiles[row, col].BackColor == tiles[row + 2, col].BackColor &&
                        tiles[row, col].BackColor != JokerColor)
                    {
                        tilesToRemove.Add(tiles[row, col]);
                        tilesToRemove.Add(tiles[row + 1, col]);
                        tilesToRemove.Add(tiles[row + 2, col]);
                        matchesFound = true;
                    }
                }
            }

            // Remove matched tiles
            if (matchesFound)
            {
                foreach (var tile in tilesToRemove)
                {
                    tile.BackColor = Color.Transparent;
                }

                // Wait for 2 seconds before shifting tiles down and adding new ones
                await Task.Delay(1000);

                // Shift tiles down and add new tiles
                ShiftTilesDown();

                // Update score
                score += tilesToRemove.Count * 10;
                score_label.Text = score.ToString();
            }

            return matchesFound;
        }


        private async void CheckAndHandleMatches()
        {
            bool hasNewMatches;

            do
            {
                hasNewMatches = await CheckForMatchesAsync();
            }
            while (hasNewMatches);
        }

        // Form'da klavye girişlerini işlemek için KeyDown olayını kullan
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.P)
            {
                TogglePause();
                return true; // Indique que l'événement a été géré
            }

            if (selectedTile != null)
            {
                switch (keyData)
                {
                    case Keys.Up:
                        MoveSelection(-1, 0); // Déplacer vers le haut
                        break;
                    case Keys.Down:
                        MoveSelection(1, 0); // Déplacer vers le bas
                        break;
                    case Keys.Left:
                        MoveSelection(0, -1); // Déplacer vers la gauche
                        break;
                    case Keys.Right:
                        MoveSelection(0, 1); // Déplacer vers la droite
                        break;
                    case Keys.Space:
                        PerformTileSwap(); // Effectuer un échange de tuiles
                        break;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void TogglePause()
        {
            if (isPaused)
            {
                // Reprendre le jeu
                gameTimer.Start();
                isPaused = false;
                SetControlsEnabled(true);  // Réactiver les contrôles
            }
            else
            {
                // Mettre en pause le jeu
                gameTimer.Stop();
                isPaused = true;
                SetControlsEnabled(false);  // desactivate controlls
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            foreach (Control control in this.Controls)
            {
                control.Enabled = enabled;
            }
        }




        // Seçimi hareket ettir
        private void MoveSelection(int rowOffset, int colOffset)
        {
            int newRow = selectedRow + rowOffset;
            int newCol = selectedCol + colOffset;

            if (newRow >= 0 && newRow < GridSize && newCol >= 0 && newCol < GridSize)
            {
                // Eski seçimi kaldır
                HighlightTile(selectedTile, false);

                // Yeni konumu seç
                selectedRow = newRow;
                selectedCol = newCol;
                selectedTile = tiles[selectedRow, selectedCol];

                // Yeni seçimi vurgula
                HighlightTile(selectedTile, true);
            }
        }

        // Taş değişimi gerçekleştir
        // Taş değişimi gerçekleştir
        private async void PerformTileSwap()
        {
            if (selectedTile != null)
            {
                // Swap the selected tile with the neighboring tile
                Button neighborTile = GetNeighborTile();

                if (neighborTile != null && IsAdjacent(selectedTile, neighborTile))
                {
                    // Swap the tiles
                    SwapTiles(selectedTile, neighborTile);

                    // Check for matches
                    if (!await CheckForMatchesAsync())
                    {
                        // If no matches, revert the swap
                        SwapTiles(selectedTile, neighborTile);
                    }
                    else
                    {
                        // If matches occur, handle updates
                        CheckAndHandleMatches();
                    }
                }
            }
        }



        // Komşu taşı bul (klavye girişine göre)
        private Button GetNeighborTile()
        {
            int neighborRow = selectedRow;
            int neighborCol = selectedCol;

            // Kullanıcı yön tuşlarıyla bir komşu taşı seçmiş olabilir
            if (neighborRow >= 0 && neighborRow < GridSize && neighborCol >= 0 && neighborCol < GridSize)
            {
                return tiles[neighborRow, neighborCol];
            }

            return null;
        }

        //add score to txt file
        public void AddScore(int score)
        {
            highScores.Add(new HighScore { PlayerName = currentPlayerName, Score = score });
            highScores = highScores.OrderByDescending(x => x.Score).Take(5).ToList();
            SaveHighScoresToFile();
        }

        // Sauvegarder les scores dans un fichier texte
        private void SaveHighScoresToFile()
        {
            using (StreamWriter writer = new StreamWriter(highScoresFile))
            {
                foreach (var score in highScores)
                {
                    writer.WriteLine($"{score.PlayerName},{score.Score}");
                }
            }
        }

        // Charger les scores depuis le fichier texte
        private void LoadHighScoresFromFile()
        {
            if (File.Exists(highScoresFile))
            {
                using (StreamReader reader = new StreamReader(highScoresFile))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split(',');
                        if (parts.Length == 2)
                        {
                            highScores.Add(new HighScore
                            {
                                PlayerName = parts[0],
                                Score = int.Parse(parts[1])
                            });
                        }
                    }
                }
            }
        }

        // show 5 highscores
        public void ShowHighScores()
        {
            string highScoresText = "Top 5 Scores:\n";
            for (int i = 0; i < highScores.Count; i++)
            {
                highScoresText += $"{i + 1}. {highScores[i].PlayerName}: {highScores[i].Score}\n";
            }
           // MessageBox.Show(highScoresText, "High Scores", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    
        private async Task HandleRocketEffect(Button tile)
        {
            Point pos = (Point)tile.Tag;
            List<Button> tilesToRemove = new List<Button>();

            // Random direction (horizontal or vertical)
            bool horizontal = random.Next(2) == 0;

            if (horizontal)
            {
                for (int col = 0; col < GridSize; col++)
                    tilesToRemove.Add(tiles[pos.X, col]);
            }
            else
            {
                for (int row = 0; row < GridSize; row++)
                    tilesToRemove.Add(tiles[row, pos.Y]);
            }

            await RemoveTilesAndUpdateScore(tilesToRemove);
        }
        // Bomb: Destroys a random object
        private async Task HandleBombEffect(Button jokerTile)
        {
            int randomRow = random.Next(GridSize);
            int randomCol = random.Next(GridSize);

            List<Button> tilesToRemove = new List<Button> { tiles[randomRow, randomCol] };
            await RemoveTilesAndUpdateScore(tilesToRemove);
        }

        // Helicopter: Destroys 8 surrounding neighbors
        private async Task HandleHelicopterEffect(Button tile)
        {
            Point pos = (Point)tile.Tag;
            List<Button> tilesToRemove = new List<Button>();

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int newRow = pos.X + i;
                    int newCol = pos.Y + j;

                    if (newRow >= 0 && newRow < GridSize &&
                        newCol >= 0 && newCol < GridSize &&
                        !(i == 0 && j == 0))
                    {
                        tilesToRemove.Add(tiles[newRow, newCol]);
                    }
                }
            }

            await RemoveTilesAndUpdateScore(tilesToRemove);
        }
        // Disco Ball: Destroys all objects of a random color
        private async Task HandleDiscoballEffect(Button tile)
        {
            Color randomColor = MatchColors[random.Next(MatchColors.Length)];
            List<Button> tilesToRemove = new List<Button>();

            for (int row = 0; row < GridSize; row++)
            {
                for (int col = 0; col < GridSize; col++)
                {
                    if (tiles[row, col].BackColor == randomColor)
                        tilesToRemove.Add(tiles[row, col]);
                }
            }

            await RemoveTilesAndUpdateScore(tilesToRemove);
        }
        private async Task RemoveTilesAndUpdateScore(List<Button> tilesToRemove)
        {
            foreach (var tile in tilesToRemove)
            {
                if (tile.BackgroundImage != null)
                {
                    tile.BackgroundImage.Dispose();
                    tile.BackgroundImage = null;
                }
                tile.BackColor = Color.Transparent;
            }

            await Task.Delay(500);
            score += tilesToRemove.Count * 10;
            score_label.Text = score.ToString();
        }
    }
}

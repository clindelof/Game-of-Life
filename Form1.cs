using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Game_of_Life
{
    ///<summary>
    /// This class contains all the members and functions that define the main form for the Game of Life application.
    ///<summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// The universe array that the paint function utilizes to display the game of life
        /// </summary>
        bool[,] universe = new bool[Properties.Settings.Default.universe_width, Properties.Settings.Default.universe_height];

        /// <summary>
        /// The timer that controlls the pace of Game of Life
        /// </summary>
        Timer timer = new Timer();

        /// <summary>
        /// How many generations have been processed by the Game of Life
        /// </summary>
        int generations = 0;

        /// <summary>
        /// Dimensions of the universe
        /// </summary>
        int universe_width;
        int universe_height;

        /// <summary>
        /// The seed used to seed the random number generator used to populate the universe
        /// </summary>
        int seed;

        /// <summary>
        /// Count of "alive" cells in the universe
        /// </summary>
        int aliveCellCount = 0;

        /// <summary>
        /// Colors used for background, "living" cells, and the background of the universe
        /// </summary>
        Color aliveCellColor;
        Color backgroundColor;
        Color gridColor;

        /// <summary>
        /// What mode the universe boundary is set to
        /// </summary>
        /// <remarks>
        /// This can be set to "Toroidal" or "Finite"
        /// </remarks>
        string boundaryMode;

        /// <summary>
        /// Boolean values for whether to display the grid lines, the count of alive neighbors for each cell, and the heads up display - HUD
        /// </summary>
        bool gridLines;
        bool neighborCount;
        bool hudVisible;


        /// <summary>
        /// Set all members and status labels to the values stored in settings.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            Properties.Settings.Default.Reload();

            // Setup the timer
            timer.Interval = Properties.Settings.Default.interval; // milliseconds
            timer.Tick += Timer_Tick;

            toolStripStatusLabelInterval.Text = "Interval: " + Properties.Settings.Default.interval; 
            toolStripStatusLabelSeed.Text = "Seed: " + Properties.Settings.Default.seed;

            gridToolStripMenuItem.Checked = Properties.Settings.Default.gridLines;
            this.gridLines = Properties.Settings.Default.gridLines;

            neighborCountToolStripMenuItem.Checked = Properties.Settings.Default.count;
            this.neighborCount = Properties.Settings.Default.count;

            hudToolStripMenuItem.Checked = Properties.Settings.Default.hud;
            this.hudVisible = Properties.Settings.Default.hud;

            this.universe_width = Properties.Settings.Default.universe_width;
            this.universe_height = Properties.Settings.Default.universe_height;

            this.boundaryMode = Properties.Settings.Default.mode;

            this.seed = Properties.Settings.Default.seed;

            this.aliveCellColor = Properties.Settings.Default.cellColor;
            this.backgroundColor = Properties.Settings.Default.backColor;
            this.gridColor = Properties.Settings.Default.gridColor;

        }

        /// <summary>
        /// Function called by clicking on the neighbor count menu item, alternates the visibility of the neighbor count for each cell.
        /// Sets the values of the preferences for persistence, the member variable for application use, and sets whether the menu item is checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toggleNeighborCount(object sender, EventArgs e)
        {
            Properties.Settings.Default.count = !Properties.Settings.Default.count;
            
            neighborCountToolStripMenuItem.Checked = Properties.Settings.Default.count;

            this.neighborCount = Properties.Settings.Default.count;

            Properties.Settings.Default.Save();

            graphicsPanel1.Invalidate();
        }

        /// <summary>
        /// Function called by clicking the grid line menu button, toggles whether the grid lines are displayed or not.
        /// Sets preference value for persistence, and sets whether the menu item is checked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toggleGridLines(object sender, EventArgs e)
        {
            Properties.Settings.Default.gridLines = !Properties.Settings.Default.gridLines;
            gridToolStripMenuItem.Checked = Properties.Settings.Default.gridLines;

            Properties.Settings.Default.Save();

            
            
            this.gridLines = Properties.Settings.Default.gridLines;

            graphicsPanel1.Invalidate();
        }

        /// <summary>
        /// Function called by clicking the HUD menu button, toggles whether the HUD is displayed.
        /// Sets preference value for persistence, and sets whether the menu item is checked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toggleHUD(object sender, EventArgs e)
        {
            Properties.Settings.Default.hud = !Properties.Settings.Default.hud;

            Properties.Settings.Default.Save();

            hudToolStripMenuItem.Checked = Properties.Settings.Default.hud;

            this.hudVisible = Properties.Settings.Default.hud;

            graphicsPanel1.Invalidate();
        }


        /// <summary>
        /// set the boundary mode menu options checked values then cause the universe to be redrawn to account for the change in boundary behavior
        /// </summary>
        private void setMode(object sender, EventArgs e)
        {
            ToolStripMenuItem clicked = (ToolStripMenuItem)sender;

            if (clicked.Equals(toroidalToolStripMenuItem))
            {
                Properties.Settings.Default.mode = "Toroidal";
                this.boundaryMode = "Toroidal";
                toroidalToolStripMenuItem.Checked = true;
                finiteToolStripMenuItem.Checked = false;
            }
            else if (clicked.Equals(finiteToolStripMenuItem))
            {
                Properties.Settings.Default.mode = "Finite";
                this.boundaryMode = "Finite";
                finiteToolStripMenuItem.Checked = true;
                toroidalToolStripMenuItem.Checked = false;
            }

            Properties.Settings.Default.Save();
            graphicsPanel1.Invalidate();
        }



        /// <summary>
        /// function called when menu color option items are clicked, updates the values of the specific color option and causes the redraw of the universe
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cellColorMenuItem(object sender, EventArgs e)
        {
            ColorDialog cp = new ColorDialog();

            ToolStripMenuItem clicked = (ToolStripMenuItem)sender;

            if (DialogResult.OK == cp.ShowDialog())
            {
                if (clicked.Equals(backColorToolStripMenuItem))
                {
                    Properties.Settings.Default.backColor = cp.Color;
                    this.backgroundColor = cp.Color;
                }
                else if (clicked.Equals(gridColorToolStripMenuItem))
                {
                    Properties.Settings.Default.gridColor = cp.Color;
                    this.gridColor = cp.Color;
                }
                else if (clicked.Equals(cellColorToolStripMenuItem))
                {
                    Properties.Settings.Default.cellColor = cp.Color;
                    this.aliveCellColor = cp.Color;
                }

                Properties.Settings.Default.Save();

                graphicsPanel1.Invalidate();
            }
        }

        /// <summary>
        /// Update seed from user imput in an instance of the Seed Modal, then generates the universe from the new seed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateSeed(object sender, EventArgs e)
        {
            Seed updateSeed = new Seed();
            updateSeed.seedValue.Value = this.seed;

            if (DialogResult.OK == updateSeed.ShowDialog())
            {
                Properties.Settings.Default.seed = (int)updateSeed.seedValue.Value;
                this.seed = (int)updateSeed.seedValue.Value;

                Properties.Settings.Default.Save();

                toolStripStatusLabelSeed.Text = "Seed: " + Properties.Settings.Default.seed;

                generateUniverse();
            }
        }

        /// <summary>
        /// Set seed based on current time, then generates the universe from the new time based seed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timeSeed(object sender, EventArgs e)
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;

            Random rnd = new Random(secondsSinceEpoch);

            Properties.Settings.Default.seed = rnd.Next(0, int.MaxValue);

            this.seed = Properties.Settings.Default.seed;

            Properties.Settings.Default.Save();

            toolStripStatusLabelSeed.Text = "Seed: " + this.seed;

            generateUniverse();
        }

        /// <summary>
        /// Regenerates the universe based on the current seed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void currentSeed(object sender, EventArgs e)
        {
            generateUniverse();
        }


        


        /// <summary>
        /// randomly generate universe using the seed member variable to seed the random number generator.
        /// if the random number generated % 2 is 0, the cell is alive otherwise it is dead.
        /// </summary>
        private void generateUniverse()
        {
            universe = new bool[this.universe_width, this.universe_height];
            this.aliveCellCount = 0;
            this.generations = 0;
            
            Random rnd = new Random(this.seed);

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    int test = rnd.Next(0, 3);

                    universe[x, y] = (test == 0);

                    if (universe[x, y]) this.aliveCellCount++;
                }
            }


            this.setAliveCount();
            graphicsPanel1.Invalidate();
        }

        /// <summary>
        /// Clear universe, reset generations, and set alive count
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearUniverse(object sender, EventArgs e)
        {
            universe = new bool[this.universe_width, this.universe_height];
            setAliveCount();
            generations = 0;

            graphicsPanel1.Invalidate();

            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();
            toolStripStatusLabelAlive.Text = "Alive: " + aliveCellCount;
        }

        /// <summary>
        /// Reset application to last saved settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resetApplication(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();

            // Setup the timer
            timer.Interval = Properties.Settings.Default.interval; // milliseconds
            timer.Tick += Timer_Tick;

            toolStripStatusLabelInterval.Text = "Interval: " + Properties.Settings.Default.interval;
            toolStripStatusLabelSeed.Text = "Seed: " + Properties.Settings.Default.seed;

            gridToolStripMenuItem.Checked = Properties.Settings.Default.gridLines;
            this.gridLines = Properties.Settings.Default.gridLines;

            neighborCountToolStripMenuItem.Checked = Properties.Settings.Default.count;
            this.neighborCount = Properties.Settings.Default.count;

            hudToolStripMenuItem.Checked = Properties.Settings.Default.hud;
            this.hudVisible = Properties.Settings.Default.hud;

            this.universe_width = Properties.Settings.Default.universe_width;
            this.universe_height = Properties.Settings.Default.universe_height;

            this.boundaryMode = Properties.Settings.Default.mode;
            toroidalToolStripMenuItem.Checked = (this.boundaryMode == "Toroidal");
            finiteToolStripMenuItem.Checked = (this.boundaryMode == "Finite");

            this.seed = Properties.Settings.Default.seed;

            this.aliveCellColor = Properties.Settings.Default.cellColor;
            this.backgroundColor = Properties.Settings.Default.backColor;
            this.gridColor = Properties.Settings.Default.gridColor;

            graphicsPanel1.Invalidate();
        }

        /// <summary>
        /// Open the options modal, save user input and then regenerate and redraw the universe.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optionsMenuItem_click(object sender, EventArgs e)
        {
            OptionsModal optionsModal = new OptionsModal();

            optionsModal.interval.Value = Properties.Settings.Default.interval;
            optionsModal.universeWidth.Value = this.universe_width;
            optionsModal.universeHeight.Value = this.universe_height;

            if (DialogResult.OK == optionsModal.ShowDialog())
            {
                Properties.Settings.Default.interval = (int)optionsModal.interval.Value;
                Properties.Settings.Default.universe_width = (int)optionsModal.universeWidth.Value;
                Properties.Settings.Default.universe_height = (int)optionsModal.universeHeight.Value;

                this.universe_width = (int)optionsModal.universeWidth.Value;
                this.universe_height = (int)optionsModal.universeHeight.Value;

                Properties.Settings.Default.Save();

                universe = new bool[this.universe_width, this.universe_height];
                setAliveCount();

                timer.Interval = Properties.Settings.Default.interval;
                toolStripStatusLabelInterval.Text = "Interval: " + Properties.Settings.Default.interval;

                this.universe_width = Properties.Settings.Default.universe_width;
                this.universe_height = Properties.Settings.Default.universe_height;

                graphicsPanel1.Invalidate();
            }


        }

        /// <summary>
        /// The event called by the timer every Interval milliseconds.
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            NextGeneration();
        }

        /// <summary>
        /// generate the next generation and update controls accordingly, cause universe to be redrawn.
        /// </summary>
        private void NextGeneration()
        {

            bool[,] scratchPad = new bool[this.universe_width, this.universe_height];
            int tempAlive = 0;

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    int neighbors = 0;

                    if (this.boundaryMode == "Finite")
                    {
                        neighbors = finiteNeighbors(x, y);
                    }
                    else if (this.boundaryMode == "Toroidal")
                    {
                        neighbors = toroidalNeighbors(x, y);
                    }

                    if (neighbors < 2 && universe[x, y])
                    {
                        scratchPad[x, y] = false;
                    }
                    else if (neighbors > 3 && universe[x, y])
                    {
                        scratchPad[x, y] = false;
                    }
                    else if ((neighbors == 2 || neighbors == 3) && universe[x, y])
                    {
                        scratchPad[x, y] = true;
                    }
                    else if (neighbors == 3 && !universe[x, y])
                    {
                        scratchPad[x, y] = true;
                    }

                    if (scratchPad[x, y]) tempAlive++;

                }
            }

            universe = scratchPad;
            setAliveCount();

            // Increment generation count
            generations++;

            // Update status strip generations
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();
            toolStripStatusLabelAlive.Text = "Alive: " + this.aliveCellCount.ToString();

            //redraw universe
            graphicsPanel1.Invalidate();
            
        }

        /// <summary>
        /// Determine the number of living cells neighboring a cell based of the x and y coordinate in the universe.
        /// The edge of the universe is considered dead with the function.
        /// </summary>
        /// <param name="x">The x value of the cell</param>
        /// <param name="y">The y value of the cell</param>
        /// <returns>the count of living cells neighboring the cell in question as an integer between 0 and 8</returns>
        private int finiteNeighbors(int x, int y)
        {
            int count = 0;

            if (y == 0)
            {
                if (x == 0)
                {

                    if (universe[x + 1, y]) count++; //right
                    if (universe[x + 1, y + 1]) count++; //bottom right
                    if (universe[x, y + 1]) count++; //bottom
                }
                else if (x > 0 && x < universe.GetLength(0))
                {

                    if (universe[x - 1, y]) count++; //left
                    if (universe[x - 1, y + 1]) count++; //bottom right
                    if (universe[x, y + 1]) count++; //bottom
                }
                else
                {
                    if (universe[x - 1, y]) count++; //left
                    if (universe[x + 1, y]) count++; //right
                    if (universe[x - 1, y + 1]) count++; //bottom left
                    if (universe[x, y + 1]) count++; //bottom
                    if (universe[x + 1, y + 1]) count++; //bottom right;
                }
            }
            else if (y == universe.GetLength(1) - 1)
            {

                if (x == 0)
                {

                    if (universe[x + 1, y]) count++; //right
                    if (universe[x + 1, y - 1]) count++; //upper right
                    if (universe[x, y - 1]) count++; //top
                }
                else if (x > 0 && x < universe.GetLength(0))
                {

                    if (universe[x - 1, y]) count++; //left
                    if (universe[x - 1, y - 1]) count++; //upper left
                    if (universe[x, y - 1]) count++; //top

                }
                else
                {
                    if (universe[x - 1, y]) count++; //left
                    if (universe[x + 1, y]) count++; //right
                    if (universe[x - 1, y - 1]) count++; //top left
                    if (universe[x, y - 1]) count++; //top
                    if (universe[x + 1, y - 1]) count++; //top right;
                }
            }
            else
            {
                if (x == 0)
                {
                    if (universe[x, y - 1]) count++; //upper
                    if (universe[x + 1, y - 1]) count++; //upper right
                    if (universe[x + 1, y]) count++; //right
                    if (universe[x + 1, y + 1]) count++; //bottom right
                    if (universe[x, y + 1]) count++; //bottom
                }
                else if (x > 0 && x < universe.GetLength(0))
                {

                    if (universe[x, y - 1]) count++; //upper
                    if (universe[x - 1, y - 1]) count++; //upper left
                    if (universe[x - 1, y]) count++; //left
                    if (universe[x - 1, y + 1]) count++; //bottom left
                    if (universe[x, y + 1]) count++; //bottom

                }
                else
                {
                    if (universe[x, y - 1]) count++; //upper
                    if (universe[x + 1, y - 1]) count++; //upper right
                    if (universe[x + 1, y]) count++; //right
                    if (universe[x + 1, y + 1]) count++; //bottom right
                    if (universe[x, y + 1]) count++; //bottom
                    if (universe[x - 1, y + 1]) count++; //bottom left
                    if (universe[x - 1, y]) count++; //left
                    if (universe[x - 1, y - 1]) count++; //upper left
                }
            }


            return count;
        }

        /// <summary>
        /// Determine the number of living cells neighboring a cell based of the x and y coordinate in the universe.
        /// The edge of the universe wraps around to the other side of the universe, edges go to the opposite edge, corners go to the opposite corner across the universe.
        /// </summary>
        /// <param name="x">The x value of the cell</param>
        /// <param name="y">The y value of the cell</param>
        /// <returns>the count of living cells neighboring the cell in question as an integer between 0 and 8</returns>
        private int toroidalNeighbors(int x, int y)
        {
            int count = 0;

            if (y == 0)
            {
                if (universe[x, universe.GetLength(1) - 1]) count++; //opposite end of column

                if (x == 0)
                {
                    if (universe[universe.GetLength(0) - 1, y]) count++; //opposite end of row
                    if (universe[universe.GetLength(0) - 1, y + 1]) count++; //opposite end of next row down

                    if (universe[x + 1, universe.GetLength(1) - 1]) count++; //opposite end of column to the right

                    if (universe[universe.GetLength(0) - 1, universe.GetLength(1) - 1]) count++; //opposite corner of universe

                    if (universe[x + 1, y]) count++; //right
                    if (universe[x + 1, y + 1]) count++; //bottom right
                    if (universe[x, y + 1]) count++; //bottom
                }
                else if (x == universe.GetLength(0) - 1)
                {
                    if (universe[0, y]) count++; //opposite end of row
                    if (universe[0, y + 1]) count++; //opposite end of next row down

                    if (universe[x - 1, universe.GetLength(1) - 1]) count++; //opposite end of column to the left

                    if (universe[0, universe.GetLength(1) - 1]) count++; //opposite corner of universe

                    if (universe[x - 1, y]) count++; //left
                    if (universe[x - 1, y + 1]) count++; //bottom left
                    if (universe[x, y + 1]) count++; //bottom
                }
                else
                {
                    if (universe[x - 1, universe.GetLength(1) - 1]) count++; //opposite end of column to the left
                    if (universe[x + 1, universe.GetLength(1) - 1]) count++; //opposite end of column to the right

                    if (universe[x - 1, y]) count++; //left
                    if (universe[x + 1, y]) count++; //right
                    if (universe[x - 1, y + 1]) count++; //bottom left
                    if (universe[x, y + 1]) count++; //bottom
                    if (universe[x + 1, y + 1]) count++; //bottom right;
                }
            }
            else if (y == universe.GetLength(1) - 1)
            {

                if (universe[x, 0]) count++; //opposite end of column

                if (x == 0)
                {
                    if (universe[universe.GetLength(0) - 1, y]) count++; //opposite end of row
                    if (universe[universe.GetLength(0) - 1, y - 1]) count++; //opposite end of next row up

                    if (universe[universe.GetLength(0) - 1, 0]) count++; //opposite corner of universe
                    if (universe[0, x + 1]) count++; //opposite end of column to the right

                    if (universe[x + 1, y]) count++; //right
                    if (universe[x + 1, y - 1]) count++; //upper right
                    if (universe[x, y - 1]) count++; //top
                }
                else if (x == universe.GetLength(0) - 1)
                {
                    if (universe[0, y]) count++; //opposite end of row
                    if (universe[0, y - 1]) count++; //opposite end of next row up

                    if (universe[x - 1, 0]) count++; //opposite end of column to the left

                    if (universe[0, 0]) count++; //opposite corner of universe

                    if (universe[x - 1, y]) count++; //left
                    if (universe[x - 1, y - 1]) count++; //upper left
                    if (universe[x, y - 1]) count++; //top

                }
                else
                {
                    if (universe[x - 1, 0]) count++; //opposite end of column to the left
                    if (universe[x + 1, 0]) count++; //opposite end of column to the right

                    if (universe[x - 1, y]) count++; //left
                    if (universe[x + 1, y]) count++; //right
                    if (universe[x - 1, y - 1]) count++; //top left
                    if (universe[x, y - 1]) count++; //top
                    if (universe[x + 1, y - 1]) count++; //top right;
                }
            }
            else
            {
                if (x == 0)
                {

                    if (universe[universe.GetLength(0) - 1, y]) count++; //opposite end of row
                    if (universe[universe.GetLength(0) - 1, y + 1]) count++; //opposite end of row down
                    if (universe[universe.GetLength(0) - 1, y - 1]) count++; //opposite end of row up

                    if (universe[x, y - 1]) count++; //upper
                    if (universe[x + 1, y - 1]) count++; //upper right
                    if (universe[x + 1, y]) count++; //right
                    if (universe[x + 1, y + 1]) count++; //bottom right
                    if (universe[x, y + 1]) count++; //bottom
                }
                else if (x == universe.GetLength(0) - 1)
                {
                    if (universe[0, y]) count++; //opposite end of row
                    if (universe[0, y + 1]) count++; //opposite end of row down
                    if (universe[0, y - 1]) count++; //opposite end of row up

                    if (universe[x, y - 1]) count++; //upper
                    if (universe[x - 1, y - 1]) count++; //upper left
                    if (universe[x - 1, y]) count++; //left
                    if (universe[x - 1, y + 1]) count++; //bottom left
                    if (universe[x, y + 1]) count++; //bottom

                }
                else
                {
                    if (universe[x, y - 1]) count++; //upper
                    if (universe[x + 1, y - 1]) count++; //upper right
                    if (universe[x + 1, y]) count++; //right
                    if (universe[x + 1, y + 1]) count++; //bottom right
                    if (universe[x, y + 1]) count++; //bottom
                    if (universe[x - 1, y + 1]) count++; //bottom left
                    if (universe[x - 1, y]) count++; //left
                    if (universe[x - 1, y - 1]) count++; //upper left
                }
            }


            return count;
        }

        /// <summary>
        /// update the alive count member variable, and the status bar alive count
        /// </summary>
        private void setAliveCount()
        {
            int alive = 0;

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    if (universe[x, y]) alive++;
                }
            }

            this.aliveCellCount = alive;
            toolStripStatusLabelAlive.Text = "Alive: " + this.aliveCellCount;
        }

        /// <summary>
        /// The function to draw the panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void graphicsPanel1_Paint(object sender, PaintEventArgs e)
        {

            // Calculate the width and height of each cell in pixels
            // CELL WIDTH = WINDOW WIDTH / NUMBER OF CELLS IN X
            int cellWidth = graphicsPanel1.ClientSize.Width / universe.GetLength(0);
            // CELL HEIGHT = WINDOW HEIGHT / NUMBER OF CELLS IN Y
            int cellHeight = graphicsPanel1.ClientSize.Height / universe.GetLength(1);

            graphicsPanel1.BackColor = this.backgroundColor;
            graphicsPanel1.AutoSize = true;

            // A Pen for drawing the grid lines (color, width)
            Pen gridPen = new Pen(this.gridColor, 1);

            // A Brush for filling living cells interiors (color)
            Brush cellBrush = new SolidBrush(this.aliveCellColor);

            SolidBrush aliveCell = new SolidBrush(Color.FromArgb(255 - aliveCellColor.R, 255 - aliveCellColor.G, 255 - aliveCellColor.B));
            SolidBrush deadCell = new SolidBrush(Color.FromArgb(255 - backgroundColor.R, 255 - backgroundColor.G, 255 - backgroundColor.B));

            // Iterate through the universe in the y, top to bottom
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    // A rectangle to represent each cell in pixels
                    Rectangle cellRect = Rectangle.Empty;
                    cellRect.X = x * cellWidth;
                    cellRect.Y = y * cellHeight;
                    cellRect.Width = cellWidth;
                    cellRect.Height = cellHeight;



                    // Fill the cell with a brush if alive
                    if (universe[x, y] == true)
                    {
                        e.Graphics.FillRectangle(cellBrush, cellRect);
                    }


                    if (this.gridLines)
                    {
                        // Outline the cell with a pen
                        e.Graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                    }

                    if (this.neighborCount)
                    {
                        StringFormat stringFormat = new StringFormat();
                        stringFormat.Alignment = StringAlignment.Center;
                        stringFormat.LineAlignment = StringAlignment.Center;

                        if (this.boundaryMode == "Finite")
                        {
                            if (universe[x, y] == true)
                            {
                                e.Graphics.DrawString(finiteNeighbors(x, y).ToString(), graphicsPanel1.Font, aliveCell, cellRect, stringFormat);
                            }
                            else
                            {
                                e.Graphics.DrawString(finiteNeighbors(x, y).ToString(), graphicsPanel1.Font, deadCell, cellRect, stringFormat);
                            }

                        }
                        else if (this.boundaryMode == "Toroidal")
                        {
                            if (universe[x, y] == true)
                            {
                                e.Graphics.DrawString(toroidalNeighbors(x, y).ToString(), graphicsPanel1.Font, aliveCell, cellRect, stringFormat);
                            }
                            else
                            {
                                e.Graphics.DrawString(toroidalNeighbors(x, y).ToString(), graphicsPanel1.Font, deadCell, cellRect, stringFormat);
                            }
                        }
                    }

                }
            }

            if (hudVisible)
            {
                Rectangle hud = Rectangle.Empty;
                hud.X = 0;
                hud.Y = 0;
                hud.Width = graphicsPanel1.Width;
                hud.Height = graphicsPanel1.Height;

                string hudString = "Timer Interval: " + timer.Interval + "\nGenerations: " + this.generations + "\nCells Alive: " + this.aliveCellCount + "\nBoundary Mode: " + this.boundaryMode + "\nUniverse:\n" + "  Width: " + this.universe_width + "\n  Height: " + this.universe_height;

                Font hudFont = new Font("Sans Serif", 12f);

                e.Graphics.DrawString(hudString, hudFont, deadCell, hud);
            }

            // Cleaning up pens and brushes
            gridPen.Dispose();
            cellBrush.Dispose();
            deadCell.Dispose();
            aliveCell.Dispose();
            
        }

        /// <summary>
        /// Determine where the mouse was clicked, toggle that cell's alive status. Cause a repaint.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void graphicsPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            // If the left mouse button was clicked
            if (e.Button == MouseButtons.Left)
            {
                // Calculate the width and height of each cell in pixels
                int cellWidth = graphicsPanel1.ClientSize.Width / universe.GetLength(0);
                int cellHeight = graphicsPanel1.ClientSize.Height / universe.GetLength(1);

                // Calculate the cell that was clicked in
                // CELL X = MOUSE X / CELL WIDTH
                int x = e.X / cellWidth;
                // CELL Y = MOUSE Y / CELL HEIGHT
                int y = e.Y / cellHeight;

                // Toggle the cell's state
                universe[x, y] = !universe[x, y];
                this.setAliveCount();

                // Tell Windows you need to repaint
                graphicsPanel1.Invalidate();
            }
        }

        /// <summary>
        /// Handle the click of the run button, start the timer, activate the pause button, disable the next button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void runButtonClick(object sender, EventArgs e)
        {
            timer.Enabled = true;
            Run.Enabled = false;
            Pause.Enabled = true;
            Next.Enabled = false;
        }

        /// <summary>
        /// Handle the click of the pause button, pause the timer, activivate the run button, activate the next button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pauseButtonClick(object sender, EventArgs e)
        {
            timer.Stop();

            Run.Enabled = true;
            Pause.Enabled = false;
            Next.Enabled = true;
        }

        /// <summary>
        /// handle the click of the next button, generate the next generation and repaint.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nextButtonClick(object sender, EventArgs e)
        {
            NextGeneration();
        }

        /// <summary>
        /// Save universe to text file, alive cells = "0", dead cells = "."
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveUniverse(object sender, EventArgs e)
        {

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2; dlg.DefaultExt = "cells";

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamWriter writer = new StreamWriter(dlg.FileName);

                for (int y = 0; y < universe.GetLength(1); y++)
                {
                    String buffer = String.Empty;

                    for (int x = 0;  x <universe.GetLength(0); x++)
                    {
                        buffer += (universe[x, y]) ? "O" : ".";
                    }

                    writer.WriteLine(buffer);
                }

                writer.Close();
            }
        }

        /// <summary>
        /// open saved universe, set universe size to the size of the opened universe and set all cells to the values represented in the text file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openSavedUniverse(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2; dlg.DefaultExt = "cells";

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamReader reader = new StreamReader(dlg.FileName);

                int maxWidth = 0;
                int maxHeight = 0;

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (!line.StartsWith("!"))
                    {
                        maxHeight++;
                        maxWidth = (line.Length > maxWidth) ? line.Length : maxWidth;
                    }
                }

                universe = new bool[maxWidth, maxHeight];

                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                int y = 0;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (!line.StartsWith("!"))
                    {
                        int x = 0;
                        foreach (char chr in line)
                        {
                            universe[x, y] = (chr == 'O');
                            x++;
                        }
                        y++;
                    }
                }

                reader.Close();
                graphicsPanel1.Invalidate();
            }
        }


        /// <summary>
        /// import a saved universe, keep size of universe the same as set by settings, center the imported universe and set all cells contained in the imported universe
        /// to that of the text file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importUniverse(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2; dlg.DefaultExt = "cells";

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamReader reader = new StreamReader(dlg.FileName);

                int maxWidth = 0;
                int maxHeight = 0;

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (!line.StartsWith("!"))
                    {
                        maxHeight++;
                        maxWidth = (line.Length > maxWidth) ? line.Length : maxWidth;
                    }
                }

                clearUniverse(sender, e);

                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                int y = ((universe.GetLength(1) - maxHeight) / 2);
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (!line.StartsWith("!"))
                    {
                        int x = ((universe.GetLength(0) - maxWidth) / 2);
                        foreach (char chr in line)
                        {
                            universe[x,y ] = (chr == 'O');
                            x++;
                        }
                        y++;
                    }
                }

                reader.Close();
                graphicsPanel1.Invalidate();
            }
        }

        /// <summary>
        /// Make sure that all settings are saved on exit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}

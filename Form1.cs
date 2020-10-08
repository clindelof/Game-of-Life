using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Game_of_Life
{
    public partial class Form1 : Form
    {
        // The universe array
        bool[,] universe = new bool[Properties.Settings.Default.universe_width, Properties.Settings.Default.universe_height];

        // The Timer class
        Timer timer = new Timer();

        // Generation count
        int generations = 0;

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

            neighborCountToolStripMenuItem.Checked = Properties.Settings.Default.count;

            hudToolStripMenuItem.Checked = Properties.Settings.Default.hud;

            setMenuMode();
        }

        private void toggleNeighborCount(object sender, EventArgs e)
        {
            Properties.Settings.Default.count = !Properties.Settings.Default.count;

            Properties.Settings.Default.Save();

            neighborCountToolStripMenuItem.Checked = Properties.Settings.Default.count;

            graphicsPanel1.Invalidate();
        }

        private void toggleGridLines(object sender, EventArgs e)
        {
            Properties.Settings.Default.gridLines = !Properties.Settings.Default.gridLines;

            Properties.Settings.Default.Save();

            gridToolStripMenuItem.Checked = Properties.Settings.Default.gridLines;

            graphicsPanel1.Invalidate();
        }

        private void toggleHUD(object sender, EventArgs e)
        {
            Properties.Settings.Default.hud = !Properties.Settings.Default.hud;

            Properties.Settings.Default.Save();

            hudToolStripMenuItem.Checked = Properties.Settings.Default.hud;

            graphicsPanel1.Invalidate();
        }

        private void generateNextGeneration()
        {
            bool[,] scratchPad = new bool[Properties.Settings.Default.universe_width, Properties.Settings.Default.universe_height];

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    int neighbors = 0;

                    if (Properties.Settings.Default.mode == "Finite")
                    {
                        neighbors = finiteNeighbors(x, y);
                    } else if (Properties.Settings.Default.mode == "Toroidal")
                    {
                        neighbors = toroidalNeighbors(x, y);
                    }

                    if (neighbors < 2 && universe[x,y])
                    {
                        scratchPad[x, y] = false;
                    } else if (neighbors > 3 && universe[x,y])
                    {
                        scratchPad[x, y] = false;
                    } else if ((neighbors == 2 || neighbors == 3) && universe[x,y]) {
                        scratchPad[x, y] = true;
                    } else if (neighbors == 3 && !universe[x,y])
                    {
                        scratchPad[x, y] = true;
                    }
                }
            }

            universe = scratchPad;
            graphicsPanel1.Invalidate();
        }

        private int finiteNeighbors(int x, int y)
        {
            int count = 0;

            if (y == 0)
            {
                if (x == 0)
                {

                    if (universe[y, x + 1]) count++; //right
                    if (universe[y + 1, x + 1]) count++; //bottom right
                    if (universe[y + 1, x]) count++; //bottom
                }
                else if (x == universe.GetLength(0) - 1)
                {

                    if (universe[y, x - 1]) count++; //left
                    if (universe[y + 1, x - 1]) count++; //bottom right
                    if (universe[y + 1, x]) count++; //bottom
                }
                else
                {
                    if (universe[y, x - 1]) count++; //left
                    if (universe[y, x + 1]) count++; //right
                    if (universe[y + 1, x - 1]) count++; //bottom left
                    if (universe[y + 1, x]) count++; //bottom
                    if (universe[y + 1, x + 1]) count++; //bottom right;
                }
            } 
            else if (y == universe.GetLength(1) - 1)
            {

                if (x == 0)
                {

                    if (universe[y, x + 1]) count++; //right
                    if (universe[y - 1, x + 1]) count++; //upper right
                    if (universe[y - 1, x]) count++; //top
                } else if (x == universe.GetLength(0) - 1)
                {

                    if (universe[y, x - 1]) count++; //left
                    if (universe[y - 1, x - 1]) count++; //upper left
                    if (universe[y - 1, x]) count++; //top

                } else
                {
                    if (universe[y, x - 1]) count++; //left
                    if (universe[y, x + 1]) count++; //right
                    if (universe[y - 1, x - 1]) count++; //top left
                    if (universe[y - 1, x]) count++; //top
                    if (universe[y - 1, x + 1]) count++; //top right;
                }
            } 
            else
            {
                if (x == 0)
                {
                    if (universe[y -1, x]) count++; //upper
                    if (universe[y - 1, x + 1]) count++; //upper right
                    if (universe[y, x + 1]) count++; //right
                    if (universe[y + 1, x + 1]) count++; //bottom right
                    if (universe[y + 1, x]) count++; //bottom
                }
                else if (x == universe.GetLength(0) - 1)
                {

                    if (universe[y - 1, x]) count++; //upper
                    if (universe[y - 1, x - 1]) count++; //upper left
                    if (universe[y, x - 1]) count++; //left
                    if (universe[y + 1, x - 1]) count++; //bottom left
                    if (universe[y + 1, x]) count++; //bottom

                } 
                else
                {
                    if (universe[y - 1, x]) count++; //upper
                    if (universe[y - 1, x + 1]) count++; //upper right
                    if (universe[y, x + 1]) count++; //right
                    if (universe[y + 1, x + 1]) count++; //bottom right
                    if (universe[y + 1, x]) count++; //bottom
                    if (universe[y + 1, x - 1]) count++; //bottom left
                    if (universe[y, x - 1]) count++; //left
                    if (universe[y - 1, x - 1]) count++; //upper left
                }
            }


            return count;
        }

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

                    if (universe[x +1, universe.GetLength(1) - 1]) count++; //opposite end of column to the right

                    if (universe[universe.GetLength(0) - 1, universe.GetLength(1) - 1]) count++; //opposite corner of universe

                    if (universe[x + 1, y]) count++; //right
                    if (universe[x + 1, y + 1]) count++; //bottom right
                    if (universe[x, y + 1]) count++; //bottom
                }
                else if (x == universe.GetLength(0) - 1)
                {
                    if (universe[0, y]) count++; //opposite end of row
                    if (universe[ 0, y + 1]) count++; //opposite end of next row down

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

                    if (universe[x - 1,y]) count++; //left
                    if (universe[x + 1,y]) count++; //right
                    if (universe[x - 1, y + 1]) count++; //bottom left
                    if (universe[ x, y + 1]) count++; //bottom
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
                    if (universe[ x, y - 1]) count++; //top
                }
                else if (x == universe.GetLength(0) - 1)
                {
                    if (universe[0, y]) count++; //opposite end of row
                    if (universe[0, y - 1]) count++; //opposite end of next row up

                    if (universe[x -1, 0]) count++; //opposite end of column to the left

                    if (universe[0, 0]) count++; //opposite corner of universe

                    if (universe[x - 1 , y ]) count++; //left
                    if (universe[ x - 1, y - 1]) count++; //upper left
                    if (universe[x, y - 1]) count++; //top

                }
                else
                {
                    if (universe[x - 1, 0]) count++; //opposite end of column to the left
                    if (universe[ x + 1, 0]) count++; //opposite end of column to the right

                    if (universe[x - 1, y]) count++; //left
                    if (universe[ x + 1, y]) count++; //right
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
                    if (universe[universe.GetLength(0) - 1, y + 1 ]) count++; //opposite end of row down
                    if (universe[universe.GetLength(0) - 1, y - 1 ]) count++; //opposite end of row up

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

        private void generateUniverse()
        {
            universe = new bool[Properties.Settings.Default.universe_width, Properties.Settings.Default.universe_height];
            
            Random rnd = new Random(Properties.Settings.Default.seed);

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    int test = rnd.Next(0, 3);

                    universe[x, y] = (test == 0);
                }
            }

            graphicsPanel1.Invalidate();
        }

        private void setMenuMode()
        {
            if (Properties.Settings.Default.mode == "Toroidal")
            {
                toroidalToolStripMenuItem.Checked = true;
                finiteToolStripMenuItem.Checked = false;
            }
            else if (Properties.Settings.Default.mode == "Finite")
            {
                finiteToolStripMenuItem.Checked = true;
                toroidalToolStripMenuItem.Checked = false;
            }

            graphicsPanel1.Invalidate();
        }

        private void setMode(object sender, EventArgs e)
        {
            ToolStripMenuItem clicked = (ToolStripMenuItem)sender;

            if (clicked.Equals(toroidalToolStripMenuItem))
            {
                Properties.Settings.Default.mode = "Toroidal";
            }
            else if (clicked.Equals(finiteToolStripMenuItem))
            {
                Properties.Settings.Default.mode = "Finite";
            }

            Properties.Settings.Default.Save();
            setMenuMode();
        }

        private void cellColorMenuItem(object sender, EventArgs e)
        {
            ColorDialog cp = new ColorDialog();

            ToolStripMenuItem clicked = (ToolStripMenuItem)sender;

            if (DialogResult.OK == cp.ShowDialog())
            {
                if (clicked.Equals(backColorToolStripMenuItem))
                {
                    Properties.Settings.Default.backColor = cp.Color;
                }
                else if (clicked.Equals(gridColorToolStripMenuItem))
                {
                    Properties.Settings.Default.gridColor = cp.Color;
                }
                else if (clicked.Equals(cellColorToolStripMenuItem))
                {
                    Properties.Settings.Default.cellColor = cp.Color;
                }

                Properties.Settings.Default.Save();

                graphicsPanel1.Invalidate();
            }
        }

        private void resetApplication(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();
            graphicsPanel1.Invalidate();
        }

        private void clearUniverse(object sender, EventArgs e)
        {
            universe = new bool[Properties.Settings.Default.universe_width, Properties.Settings.Default.universe_height];
            
            graphicsPanel1.Invalidate();

            generations = 0;

            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();
        }

        private void updateSeed(object sender, EventArgs e)
        {
            Seed updateSeed = new Seed();
            updateSeed.seedValue.Value = Properties.Settings.Default.seed;

            if (DialogResult.OK == updateSeed.ShowDialog())
            {
                Properties.Settings.Default.seed = (int)updateSeed.seedValue.Value;
                Properties.Settings.Default.Save();

                toolStripStatusLabelSeed.Text = "Seed: " + Properties.Settings.Default.seed;

                generateUniverse();
            }
        }

        private void timeSeed(object sender, EventArgs e)
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;

            Random rnd = new Random(secondsSinceEpoch);

            Properties.Settings.Default.seed = rnd.Next(0, int.MaxValue);
            Properties.Settings.Default.Save();

            toolStripStatusLabelSeed.Text = "Seed: " + Properties.Settings.Default.seed;

            generateUniverse();
        }

        private void currentSeed(object sender, EventArgs e)
        {
            generateUniverse();
        }

        private void optionsMenuItem_click(object sender, EventArgs e)
        {
            OptionsModal optionsModal = new OptionsModal();

            optionsModal.interval.Value = Properties.Settings.Default.interval;
            optionsModal.universeWidth.Value = Properties.Settings.Default.universe_width;
            optionsModal.universeHeight.Value = Properties.Settings.Default.universe_height;

            if (DialogResult.OK == optionsModal.ShowDialog())
            {
                Properties.Settings.Default.interval = (int)optionsModal.interval.Value;
                Properties.Settings.Default.universe_width = (int)optionsModal.universeWidth.Value;
                Properties.Settings.Default.universe_height = (int)optionsModal.universeHeight.Value;

                Properties.Settings.Default.Save();
            }

            updateOptionValues();
        }

        private void updateOptionValues()
        {
            universe = new bool[Properties.Settings.Default.universe_width, Properties.Settings.Default.universe_height];

            timer.Interval = Properties.Settings.Default.interval;
            toolStripStatusLabelInterval.Text = "Interval: " + Properties.Settings.Default.interval;

            graphicsPanel1.Invalidate();
        }

        // Calculate the next generation of cells
        private void NextGeneration()
        {

            generateNextGeneration();
            // Increment generation count
            generations++;

            // Update status strip generations
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();
        }

        // The event called by the timer every Interval milliseconds.
        private void Timer_Tick(object sender, EventArgs e)
        {
            NextGeneration();
        }

        private void graphicsPanel1_Paint(object sender, PaintEventArgs e)
        {

            int alive = 0;
            HUD.Visible = Properties.Settings.Default.hud;

            // Calculate the width and height of each cell in pixels
            // CELL WIDTH = WINDOW WIDTH / NUMBER OF CELLS IN X
            int cellWidth = graphicsPanel1.ClientSize.Width / universe.GetLength(0);
            // CELL HEIGHT = WINDOW HEIGHT / NUMBER OF CELLS IN Y
            int cellHeight = graphicsPanel1.ClientSize.Height / universe.GetLength(1);

            graphicsPanel1.BackColor = Properties.Settings.Default.backColor;
            graphicsPanel1.AutoSize = true;

            // A Pen for drawing the grid lines (color, width)
            Pen gridPen = new Pen(Properties.Settings.Default.gridColor, 1);

            // A Brush for filling living cells interiors (color)
            Brush cellBrush = new SolidBrush(Properties.Settings.Default.cellColor);

            SolidBrush aliveCell = new SolidBrush(Color.FromArgb(255 - Properties.Settings.Default.cellColor.R, 255 - Properties.Settings.Default.cellColor.G, 255 - Properties.Settings.Default.cellColor.B));
            SolidBrush deadCell = new SolidBrush(Color.FromArgb(255 - Properties.Settings.Default.backColor.R, 255 - Properties.Settings.Default.backColor.G, 255 - Properties.Settings.Default.backColor.B));

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
                        alive++;
                    }

                    
                    if (Properties.Settings.Default.gridLines)
                    {
                        // Outline the cell with a pen
                        e.Graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                    }
                    
                    
                    if (Properties.Settings.Default.mode == "Finite" && Properties.Settings.Default.count)
                    {
                        if (universe[x,y] == true)
                        {
                            e.Graphics.DrawString(finiteNeighbors(x, y).ToString(), graphicsPanel1.Font, aliveCell, cellRect.Location);
                        } else
                        {
                            e.Graphics.DrawString(finiteNeighbors(x, y).ToString(), graphicsPanel1.Font, deadCell, cellRect.Location);
                        }
                        
                    } else if (Properties.Settings.Default.mode == "Toroidal" && Properties.Settings.Default.count)
                    {
                        if (universe[x, y] == true)
                        {
                            e.Graphics.DrawString(toroidalNeighbors(x, y).ToString(), graphicsPanel1.Font, aliveCell, cellRect.Location);
                        }
                        else
                        {
                            e.Graphics.DrawString(toroidalNeighbors(x, y).ToString(), graphicsPanel1.Font, deadCell, cellRect.Location);
                        }
                    }
                }
            }

            toolStripStatusLabelAlive.Text = "Alive: " + alive;

            // Cleaning up pens and brushes
            gridPen.Dispose();
            cellBrush.Dispose();

            HUD.ForeColor = Color.FromArgb(255 - Properties.Settings.Default.backColor.R, 255 - Properties.Settings.Default.backColor.G, 255 - Properties.Settings.Default.backColor.B);

            HUDgeneration.Text = "Generation: " + generations;
            HUDalive.Text = "Alive: " + alive;
            HUDboundary.Text = "Boundary Mode: " + Properties.Settings.Default.mode;
            HUDuniverseWidth.Text = "Width: " + Properties.Settings.Default.universe_width.ToString();
            HUDuniverseHeight.Text = "Height: " + Properties.Settings.Default.universe_height.ToString();


            
        }

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

                // Tell Windows you need to repaint
                graphicsPanel1.Invalidate();
            }
        }

        private void runButtonClick(object sender, EventArgs e)
        {
            timer.Enabled = true;
            Run.Enabled = false;
            Pause.Enabled = true;
            Next.Enabled = false;
        }

        private void pauseButtonClick(object sender, EventArgs e)
        {
            timer.Stop();

            Run.Enabled = true;
            Pause.Enabled = false;
            Next.Enabled = true;
        }

        private void nextButtonClick(object sender, EventArgs e)
        {
            NextGeneration();
        }

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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}

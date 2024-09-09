using BallSort.Properties;

namespace BallSort
{
    public partial class Form1 : Form
    {
        readonly Image[] ballpics = [Resources.ball_yellow, Resources.ball_orange, Resources.ball_red, Resources.ball_purple, Resources.ball_blue, Resources.ball_green,
            Resources.ball_black, Resources.ball_gray, Resources.ball_white];
        readonly Image highlight = Resources.ball_highlight;
        readonly Image[] flaskparts = [Resources.flask_mid, Resources.flask_top, Resources.flask_bottom];
        readonly Image stopper = Resources.flask_stopper;
        readonly Image flasknew = Resources.flask_new;
        static readonly Bitmap canvas = new(1859, 856, System.Drawing.Imaging.PixelFormat.Format32bppArgb); //set up canvas with support for transparency
        readonly Bitmap animcanvas = new(canvas.Width, canvas.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb); //same with animation canvas
        readonly Random rand = new();
        readonly int[,] flasks = new int[13, 8];
        readonly int[] balls = new int[10];
        readonly int[] flaskxpos = [25, 156, 287, 418, 549, 680, 811, 942, 1073, 1204, 1335, 1466, 1597];
        readonly int[] flaskypos = [25, 78, 178, 278, 378, 478, 578, 678, 778];
        readonly int[] ballypos = [28, 128, 228, 328, 428, 528, 628, 728];
        readonly System.Timers.Timer balltimer = new();
        bool animating = false, inthisstep = false;
        int chosenspot = -1;
        int nflasks = 10;
        int level = 0;
        int nmoves = 0, nsteps = 0;
        readonly List<string> strings =
        [
            "Sorry, you have the maximum number of flasks.  Would you like to continue?",
            "You win!  Going to the next level.",
            "Would you like to restart the level?",
            "Restart the entire game?",
            "Do you wish to exit the game?"
        ];
        readonly LevelData[] levels =
        [
            new LevelData(-1, 2, 3, 4),
            new LevelData(1, 3, 5, 6),
            new LevelData(3, 4, 6, 7),
            new LevelData(5, 5, 7, 8),
            new LevelData(6, 6, 9, 9),
            new LevelData(7, 6, 8, 9),
            new LevelData(8, 7, 10, 10),
            new LevelData(9, 7, 9, 10),
            new LevelData(10, 8, 10, 11),
            new LevelData(11, 8, 9, 11),
            new LevelData(12, 9, 11, 13),
            new LevelData(14, 9, 10, 13),
            new LevelData(15, 9, 10, 12),
            new LevelData(int.MaxValue, 9, 10, 10) //since level will never be greater than int.MaxValue, this entry prevents attempts to search past it
        ];
        LevelData currentlevel;
        readonly StreamWriter sw = new(@"D:\Documents\ballsort.log"); //for logging (will later change to @".\ballsort.log" but this is more convenient now)
        readonly string helptext = Resources.helptext;

        public Form1()
        {
            InitializeComponent(); //display window, we'll begin game in BS_OnLoad
        }
        public void LogIt(string str, bool timestamp = true)
        {
            if (timestamp) //if a timestamp is requested (it is by default)
                sw.Write($"{DateTime.Now}: "); //write a timestamp
            sw.WriteLine(str); //now write the string given and carriage return
            sw.Flush();
        }
        public void InitGame()
        {
            currentlevel = GetLevelData(level); //get level data for current level
            nflasks = currentlevel.NumberFlasks; //get number flasks
            for (int i = 0; i < nflasks; i++) //for each flask
            {
                for (int j = 0; j < 8; j++) //and position
                {
                    flasks[i, j] = -1; //it contains nothing (yet)
                }
            }
            for (int i = 0; i < currentlevel.NumberBalls; i++) //for each color ball
            {
                balls[i] = 0; //set counter to 0
            }
            for (int f = 0; f < currentlevel.NumberBalls; f++) //for each of the first flasks
            {
                for (int p = 0; p < 8; p++) //for each position within flask
                {
                    int b; //variable to hold our ball color
                    do
                    {
                        b = rand.Next(currentlevel.NumberBalls); //choose a new ball color
                    }
                    while (balls[b] > 7); //while counter is high
                    flasks[f, p] = b; //add that color ball to the flask
                    balls[b]++; //increment counter
                }
            }
            chosenspot = -1; //we don't have any chosen spot
            nsteps = nmoves = 0; //since we haven't yet moved any balls
            DrawGame(); //draw the game
        }
        public void DrawGame(bool transparent = false)
        {
            using Graphics g = Graphics.FromImage(canvas); //using graphics object from canvas
            {
                if (!transparent)
                    g.Clear(Color.Black); //clear background to black
                else
                    g.Clear(Color.FromArgb(0, 0, 0, 0)); //clear to transparent
                for (int f = 0; f < nflasks; f++) //for each flask
                {
                    for (int p = 0; p < 8; p++) //for each position within flask
                    {
                        if (flasks[f, p] != -1) //if there is a ball there
                        {
                            DrawPic(g, ballpics[flasks[f, p]], flaskxpos[f] + 3, ballypos[p]); //draw it
                        }
                    }
                    if (IsFlaskFull(f) && !IsFlaskEmpty(f)) //if flask is full (of one color) and not empty
                    {
                        DrawPic(g, stopper, flaskxpos[f], flaskypos[0] - 4); //draw a stopper
                    }
                    DrawPic(g, flaskparts[1], flaskxpos[f], flaskypos[0]); //draw flask top
                    for (int i = 1; i < 8; i++) //for each of 7 middle parts
                    {
                        DrawPic(g, flaskparts[0], flaskxpos[f], flaskypos[i]); //draw flask middle
                    }
                    DrawPic(g, flaskparts[2], flaskxpos[f], flaskypos[8]); //draw flask bottom
                }
                DrawHighlight(g); //draw highlights for any chosen balls
                if (nflasks < currentlevel.MaximumFlasks) //if number flasks is less than maximum
                {
                    DrawPic(g, flasknew, flaskxpos[12], flaskypos[4]); //display help text
                }
            }
            canvasPB.Image = canvas; //assign canvas to picturebox
            canvasPB.Invalidate(); //and redraw
            string stline = $"Welcome to BallSort!  Level {level + 1}: {currentlevel.NumberBalls} colors of balls being sorted into {nflasks} flasks (started with {currentlevel.NumberFlasks} and maximum is {currentlevel.MaximumFlasks}).  You've moved {nmoves} balls ({nsteps} steps) so far.";
            status.Text = stline; //fill out the status line
        }
        private bool IsFlaskFull(int flask)
        {
            int b = flasks[flask, 0]; //get ball from top of flask
            for (int i = 1; i < 8; i++) //for each other position
            {
                if (b != flasks[flask, i]) //if ball doesn't match
                {
                    return false; //return false
                }
            }
            return true; //return true
        }
        private bool IsFlaskEmpty(int flask)
        {
            for (int i = 0; i < 8; i++) //for each position
            {
                if (flasks[flask, i] != -1) //if it has a ball
                {
                    return false; //return false
                }
            }
            return true; //return true
        }
        private bool IsGameComplete()
        {
            for (int i = 0; i < nflasks; i++) //for each flask
            {
                if (!IsFlaskFull(i)) //if it is not full (or empty)
                {
                    return false; //return false
                }
            }
            return true; //return true
        }
        public static void DrawPic(Graphics g, Image i, int x, int y)
        {
            Rectangle src = new(0, 0, i.Width, i.Height); //make source rectangle
            Rectangle dst = new(x, y, i.Width, i.Height); //make destination rectangle
            g.DrawImage(i, dst, src, GraphicsUnit.Pixel); //draw image
        }
        private void DrawHighlight(Graphics g)
        {
            if (chosenspot != -1) //if we have a chosen spot
            {
                int b = GetTopValue(chosenspot); //get the ball color there
                if (b == -1) //if we didn't get one (flask is empty)
                {
                    chosenspot = -1; //reject the chosen spot
                    return; //and exit
                }
                int p = GetTopPos(chosenspot); //get our top position
                while (b == flasks[chosenspot, p]) //while the color is the same
                {
                    DrawPic(g, highlight, flaskxpos[chosenspot], ballypos[p] - 3); //draw the highlight there
                    p++; //increment the position
                    if (p > 7) //if we've reached the bottom
                    {
                        return; //exit
                    }
                }
            }
        }
        private int GetFlaskPos(int p)
        {
            for (int i = 0; i < 13; i++) //for each flask
            {
                if (p >= flaskxpos[i] && p <= flaskxpos[i] + 105) //if point is inside flask
                {
                    return i; //return that index
                }
            }
            return -1; //otherwise return -1 for invalid position
        }
        private void BS_MouseClick(object sender, MouseEventArgs e)
        {
            if (animating)
            {
                if (e.Button == MouseButtons.Right) //if right mouse button
                {
                    FinishUpAnimation(); //finish animation
                }
                return; //user can wait
            }
            int where = GetFlaskPos(e.Location.X); //get flask clicked on
            if (where == -1) //if we got no flask
            {
                chosenspot = -1; //erase any choice
                return; //and exit
            }
            if (e.Button == MouseButtons.Left) //if it's a left click
            {
                if (chosenspot == -1) //if we don't have a chosen spot
                {
                    chosenspot = where; //make it this one
                }
                else //otherwise
                {
                    MoveBall(chosenspot, where); //move ball from chosen spot
                    chosenspot = -1; //and remove chosen spot
                }
            }
            else if (e.Button == MouseButtons.Right) //or if its a right click
            {
                chosenspot = -1; //clear chosen spot
            }
            else //otherwise (middle click)
            {
                if (nflasks < currentlevel.MaximumFlasks) //if there is room for another flask
                {
                    nflasks++; //add it
                    for (int i = 0; i < 8; i++) //for each position
                    {
                        flasks[nflasks - 1, i] = -1; //clear it
                    }
                }
                else //otherwise
                {
                    if (!AskPlayer(strings[0], "That's it."))
                    {
                        if (AskPlayer(strings[2], "Restart level?"))
                        {
                            InitGame();
                            return;
                        }
                        if (AskPlayer(strings[3], "Reboot game?"))
                        {
                            level = 0;
                            InitGame();
                            return;
                        }
                        if (AskPlayer(strings[4], "Quit then?"))
                        {
                            Close();
                            Application.Exit();
                        }
                    }
                }
            }
            if (!animating) //we only do this part if we are not busy animating as it will get done at the end of the animation.
            {
                DrawGame(); //draw the game
                DoCompletion(); //check for game completion
            }
        }
        private void DoCompletion()
        {
            if (IsGameComplete()) //if the level has been completed
            {
                MessageBox.Show(strings[1]); //tell the player
                level++; //increment level
                InitGame(); //make a new level
            }
        }
        private static bool AskPlayer(string message, string title) => MessageBox.Show(message, title, MessageBoxButtons.YesNo) == DialogResult.Yes;
        private int GetTopPos(int flask)
        {
            for (int pos = 0; pos < 8; pos++) //for each position
            {
                if (flasks[flask, pos] != -1) //if that position is not empty
                {
                    return pos; //return position
                }
            }
            return -1; //otherwise return -1 (empty flask)
        }
        private int GetTopValue(int flask)
        {
            int pos = GetTopPos(flask); //get top position
            if (pos == -1) //if flask is empty
            {
                return -1; //then return -1 (no ball color)
            }
            return flasks[flask, pos]; //return ball in flask at position
        }
        private void MoveBall(int flaskfrom, int flaskto)
        {
            if (flaskfrom == flaskto) // If from flask is the same as to flask
            {
                return; // Exit because there are no balls to move
            }
            int topfrom = GetTopPos(flaskfrom); // Get top position of from flask
            int topto = GetTopPos(flaskto);     // Get top position of to flask
            if (topfrom == -1 || topto == 0) // If the "from" flask is empty or the "to" flask is full
            {
                return; // Exit gracefully
            }
            int ballfrom = flasks[flaskfrom, topfrom];  // Get ball from the "from" flask
            int ballto = GetTopValue(flaskto);          // Get top ball in the "to" flask
            if (ballto != -1 && ballfrom != ballto) // If the "to" flask isn't empty and the balls don't match
            {
                return; // Exit because they can't be moved
            }
            if (topto == -1) //if to flask is empty
            {
                topto = 8; //then it has room for 8 balls
            }
            // Counter for how many balls to move
            int ballsToMove = 0;

            // Now count how many balls can be moved
            while (topfrom >= 0 && topfrom < 8 && flasks[flaskfrom, topfrom] == ballfrom && ballsToMove < topto) // Count consecutive matching balls
            {
                ballsToMove++;   // Increment the number of balls to move
                topfrom++;       // Move to the next ball in the "from" flask
            }
            MoveTheBalls(ballsToMove, flaskfrom, flaskto, topto); // Now move the balls
        }
        private void MoveTheBalls(int numbertomove, int fromflask, int toflask, int topto)
        {
            nsteps++; //increment number steps
            int topfrom = GetTopPos(fromflask); //compute starting position
            int ball = flasks[fromflask, topfrom]; //get ball at position
            for (int i = topfrom; i < topfrom + numbertomove; i++) //for each position we are moving from
            {
                flasks[fromflask, i] = -1; //get rid of ball
            }
            AnimateBallMove(numbertomove, fromflask, toflask, topfrom, topto, ball); //animate the process of moving the balls
        }
        private void AnimateBallMove(int numbertomove, int fromflask, int toflask, int topfrom, int topto, int ball)
        {
            animating = true; //set flag
            //when this mewthod is called, the balls to be moved should have been cleared, so first step is to get the rest of the board
            chosenspot = -1; //disable highlighting
            DrawGame(true); //get current board on a transparent background
            using Graphics g = Graphics.FromImage(animcanvas);
            {
                g.Clear(Color.FromArgb(0, 0, 0, 0));
                DrawPic(g, canvas, 0, 0); //add canvas
            }
            animnumberballs = numbertomove; //then we set our variables so the animation can access them.
            animball = ball;
            animstrarty = ballypos[topfrom];
            animendy = animstarty2 = 0 - 100 * numbertomove; //fully "off screen"
            animx = flaskxpos[fromflask] + 3;
            animendy2 = ballypos[topto - numbertomove];
            animx2 = flaskxpos[toflask] + 3;
            animypos = animstrarty;
            animtoflask = toflask;
            animtop = topto;
            LogIt($"Begining animating moving {numbertomove} balls from flask {fromflask} to flask {toflask}.");
            balltimer.Interval = 33; //about 30 FPS
            balltimer.Elapsed += AnimateBallMove1; //handler will "pick up" balls
            balltimer.Start(); //set the timer
        }
        private void AnimateBallMove1(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (!inthisstep) //if we are not currently busy
            {
                balltimer.Stop(); //stop the timer
                LogIt($"Animating {animnumberballs} currently at {animx}, {animypos}");
                inthisstep = true; //set busy flag
                using Graphics g = Graphics.FromImage(canvas);
                {
                    g.Clear(Color.Black); //clear to black
                    for (int i = 0; i < animnumberballs; i++) //for each ball
                    {
                        DrawPic(g, ballpics[animball], animx, animypos + i * 100); //draw it
                    }
                    DrawPic(g, animcanvas, 0, 0); //draw the animated canvas to overlay the flasks, etc.
                }
                canvasPB.Image = canvas; //set picturebox to use this pic
                canvasPB.Invalidate(); //redraw picturebox
                animypos -= 50; //move  up 50 pixels
                if (animypos < animendy) //if we are high enough
                {
                    //balltimer.Stop();
                    LogIt($"Switching handler from picking balls up to dropping them");
                    balltimer.Elapsed -= AnimateBallMove1; //reset handler to no longer use this handler
                    animypos = animstarty2; //set starting position (vertical)
                    balltimer.Elapsed += AnimateBallMove2; //register handler that will "drop" balls
                }
                inthisstep = false; //clear busy flag
                balltimer.Start(); //re-start timer
            }
        }
        private void AnimateBallMove2(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (!inthisstep) //if we are not currently busy
            {
                balltimer.Stop();
                LogIt($"Animating {animnumberballs} currently at {animx2}, {animypos}");
                inthisstep = true;
                int y = animypos;
                using Graphics g = Graphics.FromImage(canvas);
                {
                    g.Clear(Color.Black);
                    for (int i = 0; i < animnumberballs; i++)
                    {
                        DrawPic(g, ballpics[animball], animx2, y + i * 100);
                    }
                    DrawPic(g, animcanvas, 0, 0);
                }
                canvasPB.Image = canvas;
                canvasPB.Invalidate();
                animypos += 50;
                if (animypos > animendy2)
                {
                    //balltimer.Stop();
                    balltimer.Elapsed -= AnimateBallMove2;
                    FinishUpAnimation();
                    inthisstep = false;
                    return;
                }
                inthisstep = false;
                balltimer.Start();
            }
        }
        private void FinishUpAnimation()
        {
            balltimer.Stop(); //we shouldn't normally need to do this
            LogIt("Finishing up.");
            animating = inthisstep = false;
            for (int i = animtop - 1; i >= animtop - animnumberballs; i--)
            {
                flasks[animtoflask, i] = animball;
                nmoves++;
            }
            DrawGame();
            DoCompletion();
        }
        int animnumberballs, animball, animstrarty, animendy, animx, animstarty2, animendy2, animx2, animypos, animtoflask, animtop;
        private void BS_Load(object sender, EventArgs e)
        {
            MessageBox.Show(helptext, "Welcome!");
            ClientSize = new Size(1728, 906); //set client size
            Location = new Point(0, 0); //and location
            canvasPB.Size = new Size(1728, 856); //set picture box size
            canvasPB.Location = new Point(0, 0); //and location
            status.Size = new Size(1728, 50); //set status line (textbox) size
            status.Location = new Point(0, 856); //and location (directly below picturebox)
            InitGame(); //initialize game
        }

        private LevelData GetLevelData(int level)
        {
            int index = 0; //we will need an index
            while (level > levels[index].StartLevel) //looking for the first levels entry that is too high for level
            {
                index++; //increment index
            }
            return levels[index - 1]; //return last working levels
        }
    }

    public struct LevelData(int start, int balls, int flasks, int maxflasks)
    {
        public int StartLevel = start;
        public int NumberBalls = balls;
        public int NumberFlasks = flasks;
        public int MaximumFlasks = maxflasks;
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Media;

namespace Simon
{
    public delegate void ButtonClickingThreadTaskCompleted(string taskId, bool isError);

    public partial class SimonWindow : Form
    {
        enum Position { none, left, top, right, bottom };
        private readonly Color leftColour = Color.Firebrick;
        private readonly Color leftHighlightColour = Color.Red;
        private readonly Color topColour = Color.Goldenrod;
        private readonly Color topHighlightColour = Color.Yellow;
        private readonly Color rightColour = Color.RoyalBlue;
        private readonly Color rightHighlightColour = Color.LightSkyBlue;
        private readonly Color bottomColour = Color.Green;
        private readonly Color bottomHighlightColour = Color.LimeGreen;
        private SoundPlayer blueSound;
        private SoundPlayer yellowSound;
        private SoundPlayer greenSound;
        private SoundPlayer redSound;
        private SoundPlayer failSound;
        private int sleepTime;
        private object _lock = new object();
        private bool threadDone;
        private bool startupDone;
        private bool cpuOrderThreadDone;
        private bool userDonePicking;
        private List<int> order;
        private List<int> userOrder;
        private int previousHighScore;
        private int currentHighScore;
        private bool gameOver;
        private bool cpuTurn;
        private int timeBetweenButtonHighlightsMilis;
        private int userSeqGuess;

        public SimonWindow()
        {
            InitializeComponent();
            Thread initialiseAll = new Thread(initialise);
            initialiseAll.IsBackground = true;
            initialiseAll.Start();
        }

        public SimonWindow(int highScoreCarriedOver)
        {
            InitializeComponent();

            // If we have played before then we don't need to show startup flashes again
            // ERROR: Not flashing buttons on second game
            //Thread initialiseAll = new Thread(initialiseWithoutFlashes);
            Thread initialiseAll = new Thread(initialise);
            initialiseAll.IsBackground = true;
            initialiseAll.Start();
            while (!initialiseAll.IsAlive) ;
            while (initialiseAll.IsAlive) ;
            lock (_lock)
            {
                previousHighScore = highScoreCarriedOver;
                updateHighScoreText();
            }
        }

        private void initialise(object state)
        {
            order = new List<int>();
            userOrder = new List<int>();
            currentHighScore = 0;
            threadDone = false;
            startupDone = false;
            bottom.BackColor = bottomColour;
            top.BackColor = topColour;
            left.BackColor = leftColour;
            right.BackColor = rightColour;
            top.Location = new Point(84, 24);
            bottom.Location = new Point(84, 209);
            left.Location = new Point(31, 77);
            right.Location = new Point(217, 77);
            blueSound = new SoundPlayer(Properties.Resources.blue);
            yellowSound = new SoundPlayer(Properties.Resources.yellow);
            greenSound = new SoundPlayer(Properties.Resources.green);
            redSound = new SoundPlayer(Properties.Resources.red);
            failSound = new SoundPlayer(Properties.Resources.failure);
            disableButtons();
            gameOver = false;
            cpuTurn = true;
            timeBetweenButtonHighlightsMilis = 1000;

            Thread startingVisuals = new Thread(doStartup);
            startingVisuals.IsBackground = true;
            startingVisuals.Start();
        }

        //private void initialiseWithoutFlashes(object state)
        //{
        //    order = new List<int>();
        //    userOrder = new List<int>();
        //    currentHighScore = 0;
        //    threadDone = false;
        //    startupDone = false;
        //    bottom.BackColor = bottomColour;
        //    top.BackColor = topColour;
        //    left.BackColor = leftColour;
        //    right.BackColor = rightColour;
        //    top.Location = new Point(84, 24);
        //    bottom.Location = new Point(84, 209);
        //    left.Location = new Point(31, 77);
        //    right.Location = new Point(217, 77);
        //    blueSound = new SoundPlayer(Properties.Resources.blue);
        //    yellowSound = new SoundPlayer(Properties.Resources.yellow);
        //    greenSound = new SoundPlayer(Properties.Resources.green);
        //    redSound = new SoundPlayer(Properties.Resources.red);
        //    failSound = new SoundPlayer(Properties.Resources.failure);
        //    disableButtons();
        //    gameOver = false;
        //    cpuTurn = true;
        //    timeBetweenButtonHighlightsMilis = 1000;

        //    lock (_lock)
        //    {
        //        startupDone = true;
        //        threadDone = true;
        //    }

        //    Thread game = new Thread(gameControl);
        //    game.IsBackground = true;
        //    game.Start();
        //}

        private void doStartup(object state)
        {
            Thread startupFlashes = new Thread(doStartUpFlashes);
            startupFlashes.IsBackground = true;
            startupFlashes.Start();

            Thread highThenDeHighThread = new Thread(highlightThenDeHighLightButtons);
            highThenDeHighThread.IsBackground = true;
            highThenDeHighThread.Start();

            while (!highThenDeHighThread.IsAlive) ;
            while (highThenDeHighThread.IsAlive) ;

            lock (_lock)
            {
                startupDone = true;
            }

            Thread game = new Thread(gameControl);
            game.IsBackground = true;
            game.Start();
        }

        private void doEnd(object state)
        {
            Thread endFlashes = new Thread(doEndFlashes);
            endFlashes.IsBackground = true;
            endFlashes.Start();
            while (!endFlashes.IsAlive) ;
            while (endFlashes.IsAlive) ;
            Thread.Sleep(1500);
        }

        private void gameControl(object state)
        {
            while (!gameOver)
            {
                while (cpuTurn)
                {
                    // Logic here: If CPU has an order, then show this in sequence
                    // Add 1 to the order, at random
                    lock (_lock)
                    {
                        cpuOrderThreadDone = false;
                        userDonePicking = false;
                        userSeqGuess = 0;
                        disableButtons();
                        // Add to order
                        Random _rand;
                        _rand = new Random();
                        int buttonToAdd = _rand.Next(1, 5);
                        order.Add(buttonToAdd);
                    }
                    // Show current order
                    Thread displayOrder = new Thread(showCpuOrder);
                    displayOrder.IsBackground = true;
                    displayOrder.Start();
                    while (!cpuOrderThreadDone) ;

                    cpuTurn = false;
                }
                while (!cpuTurn)
                {
                    // Let user guess, guesses are made with clicking buttons
                    // Ensure after each click that the guess is correct, if not user loses
                    // Update score
                    enableButtons();
                    while (!userDonePicking) ;
                    disableButtons();
                    cpuTurn = true;
                }
            }
            updateHighScoreText();
            lock (_lock)
            {
                startupDone = false;
            }
            Thread end = new Thread(doEnd);
            end.IsBackground = true;
            end.Start();
            while (!end.IsAlive) ;
            while (end.IsAlive) ;
            DialogResult res = MessageBox.Show("Game over! You scored " + currentHighScore + (currentHighScore != 1 ? " points" : " point") +
                                               ".\nWant to play again?", "Simon", MessageBoxButtons.YesNo);
            if (res == DialogResult.Yes)
            {
                launchNewWindow();
            }
            else
            {
                //Application.Exit();
                if (Application.MessageLoop)
                {
                    // WinForms app
                    Application.Exit();
                }
                else
                {
                    // Console app
                    Environment.Exit(1);
                }
            }
        }

        private void launchNewWindow()
        {
            if (InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(launchNewWindow);
                Invoke(d, new object[] { });
            }
            else
            {
                lock (_lock)
                {
                    int newHighScore = 0;
                    if (currentHighScore > previousHighScore)
                    {
                        newHighScore = currentHighScore;
                    }
                    else
                    {
                        newHighScore = previousHighScore;
                    }
                    SimonWindow sw = new SimonWindow(newHighScore);
                    sw.FormClosed += (s, args) => Close();
                    Hide();
                    sw.Show();
                }
            }
        }

        private void updateHighScoreText()
        {
            if (InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(updateHighScoreText);
                Invoke(d, new object[] { });
            }
            else
            {
                lock (_lock)
                {
                    currentHighScore = order.Count - 1;
                    if (currentHighScore > previousHighScore)
                    {
                        Text = "Simon - High Score: " + currentHighScore;
                    }
                    else
                    {
                        Text = "Simon - High Score: " + previousHighScore;
                    }
                }
            }
        }

        private void showCpuOrder(object state)
        {
            for (int i = 0; i < order.Count; i++)
            {
                Thread doClick;
                Thread.Sleep(timeBetweenButtonHighlightsMilis);
                // 1 = red, 2 = yellow, 3 = blue, 4 = green
                switch (order[i])
                {
                    case (int)Position.left:
                        doClick = new Thread(doRedClick);
                        doClick.IsBackground = true;
                        doClick.Start();
                        break;
                    case (int)Position.top:
                        doClick = new Thread(doYellowClick);
                        doClick.IsBackground = true;
                        doClick.Start();
                        break;
                    case (int)Position.right:
                        doClick = new Thread(doBlueClick);
                        doClick.IsBackground = true;
                        doClick.Start();
                        break;
                    case (int)Position.bottom:
                        doClick = new Thread(doGreenClick);
                        doClick.IsBackground = true;
                        doClick.Start();
                        break;
                }
            }
            lock (_lock)
            {
                timeBetweenButtonHighlightsMilis -= 50;
            }
            cpuOrderThreadDone = true;
        }

        private void enableButtons()
        {
            if (bottom.InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(enableButtons);
                Invoke(d, new object[] { });
            }
            else
            {
                bottom.Enabled = true;
                top.Enabled = true;
                left.Enabled = true;
                right.Enabled = true;
            }
        }

        private void disableButtons()
        {
            if (bottom.InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(disableButtons);
                Invoke(d, new object[] { });
            }
            else
            {
                bottom.Enabled = false;
                top.Enabled = false;
                left.Enabled = false;
                right.Enabled = false;
            }
        }

        private void doEndFlashes(object state)
        {
            failSound.Stream.Position = 0;
            failSound.Play();
            Thread clockFlash1 = new Thread(clockwiseFlash);
            clockFlash1.IsBackground = true;
            clockFlash1.Start();

            while (!clockFlash1.IsAlive) ;
            while (clockFlash1.IsAlive) ;
            Thread clockFlash2 = new Thread(clockwiseFlash);
            clockFlash2.IsBackground = true;
            clockFlash2.Start();
        }

        private void doStartUpFlashes(object state)
        {
            Thread.Sleep(1000);
            lock (_lock)
            {
                sleepTime = 200;
            }
            SoundPlayer startUp = new SoundPlayer(Properties.Resources.start_up);
            startUp.Play();
            Thread clockFlash1 = new Thread(clockwiseFlash);
            clockFlash1.IsBackground = true;
            clockFlash1.Start();

            while (!clockFlash1.IsAlive) ;
            while (clockFlash1.IsAlive) ;
            Thread clockFlash2 = new Thread(clockwiseFlash);
            clockFlash2.IsBackground = true;
            clockFlash2.Start();
            while (!clockFlash2.IsAlive) ;
            while (clockFlash2.IsAlive) ;
            Thread clockFlash3 = new Thread(clockwiseFlash);
            clockFlash3.IsBackground = true;
            clockFlash3.Start();
            while (!clockFlash3.IsAlive) ;
            while (clockFlash3.IsAlive) ;
            Thread.Sleep(300);
            lock (_lock)
            {
                threadDone = true;
            }
        }

        private void highlightThenDeHighLightButtons(object state)
        {
            while (!threadDone) ;
            lock (_lock)
            {
                threadDone = false;
            }
            Thread.Sleep(300);
            SoundPlayer finale = new SoundPlayer(Properties.Resources.start_up_finale);
            finale.Play();
            Thread t = new Thread(hightlightAll);
            t.IsBackground = true;
            t.Start();
            while (!t.IsAlive) ;
            while (t.IsAlive) ;
            Thread.Sleep(1000);
            Thread t2 = new Thread(deHighlightAll);
            t2.IsBackground = true;
            t2.Start();
            while (!t2.IsAlive) ;
            while (t2.IsAlive) ;
            Thread.Sleep(200);
            lock (_lock)
            {
                threadDone = true;
            }
        }

        private void hightlightAll(object state)
        {
            if (left.InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(simulateRedHighlight);
                Invoke(d, new object[] { });
            }
            else
            {
                left.BackColor = leftHighlightColour;
            }
            if (top.InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(simulateYellowHighlight);
                Invoke(d, new object[] { });
            }
            else
            {
                top.BackColor = rightHighlightColour;
            }
            if (right.InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(simulateBlueHighlight);
                Invoke(d, new object[] { });
            }
            else
            {
                right.BackColor = rightHighlightColour;
            }
            if (bottom.InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(simulateGreenHighlight);
                Invoke(d, new object[] { });
            }
            else
            {
                bottom.BackColor = bottomHighlightColour;
            }
        }

        private void deHighlightAll(object state)
        {
            if (left.InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(simulateRedDeHighlight);
                Invoke(d, new object[] { });
            }
            else
            {
                left.BackColor = leftColour;
            }
            if (top.InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(simulateYellowDeHighlight);
                Invoke(d, new object[] { });
            }
            else
            {
                top.BackColor = topColour;
            }
            if (right.InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(simulateBlueDeHighlight);
                Invoke(d, new object[] { });
            }
            else
            {
                right.BackColor = rightColour;
            }
            if (bottom.InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(simulateGreenDeHighlight);
                Invoke(d, new object[] { });
            }
            else
            {
                bottom.BackColor = bottomColour;
            }
        }

        private void clockwiseFlash(object state)
        {
            Thread green = new Thread(doGreenClick);
            green.IsBackground = true;
            Thread yellow = new Thread(doYellowClick);
            yellow.IsBackground = true;
            Thread blue = new Thread(doBlueClick);
            blue.IsBackground = true;
            Thread red = new Thread(doRedClick);
            red.IsBackground = true;

            red.Start();
            Thread.Sleep(sleepTime);
            yellow.Start();
            Thread.Sleep(sleepTime);
            blue.Start();
            Thread.Sleep(sleepTime);
            green.Start();
            Thread.Sleep(sleepTime);
        }

        delegate void WindowActionCallBack();

        private void simulateRedHighlight()
        {
            left.BackColor = leftHighlightColour;
        }

        private void simulateRedDeHighlight()
        {
            left.BackColor = leftColour;
        }

        private void doRedClick(object state)
        {
            Thread.Sleep(sleepTime);
            if (left.InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(simulateRedHighlight);
                Invoke(d, new object[] { });
                if (startupDone)
                {
                    redSound.Stream.Position = 0;
                    redSound.Play();
                }
            }
            else
            {
                left.BackColor = leftHighlightColour;
                if (startupDone)
                {
                    redSound.Stream.Position = 0;
                    redSound.Play();
                }
            }
            Thread.Sleep(sleepTime);
            if (left.InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(simulateRedDeHighlight);
                Invoke(d, new object[] { });
            }
            else
            {
                left.BackColor = leftColour;
            }
        }

        private void simulateBlueHighlight()
        {
            right.BackColor = rightHighlightColour;
        }

        private void simulateBlueDeHighlight()
        {
            right.BackColor = rightColour;
        }

        private void doBlueClick(object state)
        {
            Thread.Sleep(sleepTime);
            if (right.InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(simulateBlueHighlight);
                Invoke(d, new object[] { });
                if (startupDone)
                {
                    blueSound.Stream.Position = 0;
                    blueSound.Play();
                }
            }
            else
            {
                right.BackColor = rightHighlightColour;
                if (startupDone)
                {
                    blueSound.Stream.Position = 0;
                    blueSound.Play();
                }
            }
            Thread.Sleep(sleepTime);
            if (right.InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(simulateBlueDeHighlight);
                Invoke(d, new object[] { });
            }
            else
            {
                right.BackColor = topColour;
            }
        }

        private void simulateYellowHighlight()
        {
            top.BackColor = topHighlightColour;
        }

        private void simulateYellowDeHighlight()
        {
            top.BackColor = topColour;
        }

        private void doYellowClick(object state)
        {
            Thread.Sleep(sleepTime);
            if (top.InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(simulateYellowHighlight);
                Invoke(d, new object[] { });
                if (startupDone)
                {
                    yellowSound.Stream.Position = 0;
                    yellowSound.Play();
                }
            }
            else
            {
                top.BackColor = topHighlightColour;
                if (startupDone)
                {
                    yellowSound.Stream.Position = 0;
                    yellowSound.Play();
                }
            }
            Thread.Sleep(sleepTime);
            if (top.InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(simulateYellowDeHighlight);
                Invoke(d, new object[] { });
            }
            else
            {
                top.BackColor = topColour;
            }
        }

        private void simulateGreenHighlight()
        {
            bottom.BackColor = bottomHighlightColour;
        }

        private void simulateGreenDeHighlight()
        {
            bottom.BackColor = bottomColour;
        }

        private void doGreenClick(object state)
        {
            Thread.Sleep(sleepTime);
            if (bottom.InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(simulateGreenHighlight);
                Invoke(d, new object[] { });
                if (startupDone)
                {
                    greenSound.Stream.Position = 0;
                    greenSound.Play();
                }
            }
            else
            {
                bottom.BackColor = bottomHighlightColour;
                if (startupDone)
                {
                    greenSound.Stream.Position = 0;
                    greenSound.Play();
                }
            }
            Thread.Sleep(sleepTime);
            if (bottom.InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(simulateGreenDeHighlight);
                Invoke(d, new object[] { });
            }
            else
            {
                bottom.BackColor = bottomColour;
            }
        }

        private void right_Click(object sender, EventArgs e)
        {
            if (right.Enabled)
            {
                if (order[userSeqGuess] == (int)Position.right)
                {
                    blueSound.Stream.Position = 0;
                    blueSound.Play();
                    lock (_lock)
                    {
                        userSeqGuess++;
                    }
                    if (userSeqGuess == order.Count)
                    {
                        lock (_lock)
                        {
                            userDonePicking = true;
                        }
                    }
                }
                else
                {
                    lock (_lock)
                    {
                        userDonePicking = true;
                        gameOver = true;
                    }
                }
            }
        }

        private void top_Click(object sender, EventArgs e)
        {
            if (top.Enabled)
            {
                if (order[userSeqGuess] == (int)Position.top)
                {
                    yellowSound.Stream.Position = 0;
                    yellowSound.Play();
                    lock (_lock)
                    {
                        userSeqGuess++;
                    }
                    if (userSeqGuess == order.Count)
                    {
                        lock (_lock)
                        {
                            userDonePicking = true;
                        }
                    }
                }
                else
                {
                    lock (_lock)
                    {
                        userDonePicking = true;
                        gameOver = true;
                    }
                }
            }
        }

        private void left_Click(object sender, EventArgs e)
        {
            if (left.Enabled)
            {
                if (order[userSeqGuess] == (int)Position.left)
                {
                    redSound.Stream.Position = 0;
                    redSound.Play();
                    lock (_lock)
                    {
                        userSeqGuess++;
                    }
                    if (userSeqGuess == order.Count)
                    {
                        lock (_lock)
                        {
                            userDonePicking = true;
                        }
                    }
                }
                else
                {
                    lock (_lock)
                    {
                        userDonePicking = true;
                        gameOver = true;
                    }
                }
            }
        }

        private void bottom_Click(object sender, EventArgs e)
        {
            if (bottom.Enabled)
            {
                if (order[userSeqGuess] == (int)Position.bottom)
                {
                    greenSound.Stream.Position = 0;
                    greenSound.Play();
                    lock (_lock)
                    {
                        userSeqGuess++;
                    }
                    if (userSeqGuess == order.Count)
                    {
                        lock (_lock)
                        {
                            userDonePicking = true;
                        }
                    }
                }
                else
                {
                    lock (_lock)
                    {
                        userDonePicking = true;
                        gameOver = true;
                    }
                }
            }
        }
    }
}

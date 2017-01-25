using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Media;
using System.IO;

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
        //public readonly Color middleColour = Color.DarkMagenta;
        //public readonly Color middleHighlightColour = Color.Fuchsia;
        //private SoundPlayer purpleSound;
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
        private int highScore;
        private bool gameOver;
        private bool cpuTurn;
        private int timeBetweenButtonHighlightsMilis;
        private int userSeqGuess;

        public SimonWindow()
        {
            InitializeComponent();
            //middle.Select();
            ActiveControl = middle;
            initialise();
        }

        private void initialise()
        {
            // Note: The middle (purple) button was going to be included in this version but it is an addition to the original
            // Simon game, and would probably be too difficult for the user, so it has been removed. It is hidden in the code in
            // case I want to come back to it.
            middle.Hide();
            order = new List<int>();
            userOrder = new List<int>();
            highScore = 0;
            threadDone = false;
            startupDone = false;
            bottom.BackColor = bottomColour;
            top.BackColor = topColour;
            left.BackColor = leftColour;
            right.BackColor = rightColour;
            //middle.BackColor = middleColour;
            top.Location = new Point(84, 24);
            bottom.Location = new Point(84, 209);
            left.Location = new Point(31, 77);
            right.Location = new Point(217, 77);
            //middle.Location = new Point(124, 117);
            //purpleSound = new SoundPlayer(Properties.Resources.blue);
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
            //left.MouseHover += (s, e) =>
            //{
            //    left.BackColor = leftColour;
            //};
            ////left.MouseHover += leftColour;
            //left.MouseLeave += (s, e) =>
            //{
            //    left.BackColor = default(Color);
            //    left.UseVisualStyleBackColor = true;
            //};

        }

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
            MessageBox.Show("Game over");
            //lock (_lock)
            //{
            //    highScore = order.Count - 1;
            //    Text = "Simon - High Score: " + highScore;
            //}
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

        private void gameControlT(object state)
        {
            //Thread purpleClick = new Thread(doPurpleClick);
            //purpleClick.Start();

            //while (!purpleClick.IsAlive) ;
            //while (purpleClick.IsAlive) ;

            Thread.Sleep(300);
            Thread yellowClick = new Thread(doYellowClick);
            yellowClick.IsBackground = true;
            yellowClick.Start();

            while (!yellowClick.IsAlive) ;
            while (yellowClick.IsAlive) ;

            Thread.Sleep(300);
            Thread blueClick = new Thread(doBlueClick);
            blueClick.Start();

            while (!blueClick.IsAlive) ;
            while (blueClick.IsAlive) ;

            Thread.Sleep(300);
            Thread redClick = new Thread(doRedClick);
            redClick.Start();

            while (!redClick.IsAlive) ;
            while (redClick.IsAlive) ;

            Thread.Sleep(300);
            Thread greenClick = new Thread(doGreenClick);
            greenClick.Start();
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
                //middle.Enabled = true;
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
                //middle.Enabled = false;
            }
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
            //if (middle.InvokeRequired)
            //{
            //    WindowActionCallBack d = new WindowActionCallBack(simulatePurpleHighlight);
            //    Invoke(d, new object[] { });
            //    SoundPlayer finalHit = new SoundPlayer(Properties.Resources.start_up_finale);
            //    finalHit.Play();
            //}
            //else
            //{
            //    middle.BackColor = middleHighlightColour;
            //    SoundPlayer finalHit = new SoundPlayer(Properties.Resources.start_up_finale);
            //    finalHit.Play();
            //}
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
            //if (middle.InvokeRequired)
            //{
            //    WindowActionCallBack d = new WindowActionCallBack(simulatePurpleDeHighlight);
            //    Invoke(d, new object[] { });
            //}
            //else
            //{
            //    middle.BackColor = middleColour;
            //}
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

        //private void DoBackgroundWork()
        //{
        //    // Create a ThreadTask object.

        //    DoButtonClickingThreadTask threadTask = new DoButtonClickingThreadTask();

        //    // Create a task id.  Quick and dirty here to keep it simple.  
        //    // Read about threading and task identifiers to learn 
        //    // various ways people commonly do this for production code.

        //    threadTask.TaskId = "ButtonClickTask" + DateTime.Now.Ticks.ToString();

        //    // Set the thread up with a callback function pointer.

        //    threadTask.CompletedCallback =
        //        new ButtonClickingThreadTaskCompleted(SomeThreadTaskCompletedCallback);


        //    // Create a thread.  We only need to specify the entry point function.
        //    // Framework creates the actual delegate for thread with this entry point.

        //    Thread thread = new Thread(threadTask.ExecuteThreadTask);

        //    // Do something with our thread and threadTask object instances just created
        //    // so we could cancel the thread etc.  Can be as simple as stick 'em in a bag
        //    // or may need a complex manager, just depends.

        //    // GO!
        //    thread.Start();

        //    // Go do something else.  When task finishes we will get a callback.

        //}

        //public void SomeThreadTaskCompletedCallback(string taskId, bool isError)
        //{
        //    // Do post background work here.
        //    // Cleanup the thread and task object references, etc.
        //}

        //private void simulatePurpleHighlight()
        //{
        //    middle.BackColor = middleHighlightColour;                
        //}

        //private void simulatePurpleDeHighlight()
        //{
        //    middle.BackColor = middleColour;
        //}

        //private void doPurpleClick(object state)
        //{
        //    Thread.Sleep(sleepTime);
        //    if (left.InvokeRequired)
        //    {
        //        WindowActionCallBack d = new WindowActionCallBack(simulatePurpleHighlight);
        //        Invoke(d, new object[] { });
        //        if (startupDone)
        //        {
        //            purpleSound.Stream.Position = 0;
        //            purpleSound.Play();
        //        }
        //    }
        //    else
        //    {
        //        middle.BackColor = middleHighlightColour;
        //        if (startupDone)
        //        {
        //            purpleSound.Stream.Position = 0;
        //            purpleSound.Play();
        //        }
        //    }
        //    Thread.Sleep(sleepTime);
        //    if (left.InvokeRequired)
        //    {
        //        WindowActionCallBack d = new WindowActionCallBack(simulatePurpleDeHighlight);
        //        Invoke(d, new object[] { });
        //    }
        //    else
        //    {
        //        middle.BackColor = middleColour;
        //    }
        //}

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
                else if (order[userSeqGuess] != (int)Position.right)
                {
                    failSound.Stream.Position = 0;
                    failSound.Play();
                    lock (_lock)
                    {
                        userDonePicking = true;
                        gameOver = true;
                    }
                }
                else if (userSeqGuess >= order.Count)
                {
                    lock (_lock)
                    {
                        userDonePicking = true;
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
                else if (order[userSeqGuess] != (int)Position.top)
                {
                    failSound.Stream.Position = 0;
                    failSound.Play();
                    lock (_lock)
                    {
                        userDonePicking = true;
                        gameOver = true;
                    }
                }
                else if (userSeqGuess >= order.Count)
                {
                    lock (_lock)
                    {
                        userDonePicking = true;
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
                else if (order[userSeqGuess] != (int)Position.left)
                {
                    failSound.Stream.Position = 0;
                    failSound.Play();
                    lock (_lock)
                    {
                        userDonePicking = true;
                        gameOver = true;
                    }
                }
                else if (userSeqGuess >= order.Count)
                {
                    lock (_lock)
                    {
                        userDonePicking = true;
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
                else if (order[userSeqGuess] != (int)Position.bottom)
                {
                    failSound.Stream.Position = 0;
                    failSound.Play();
                    lock (_lock)
                    {
                        userDonePicking = true;
                        gameOver = true;
                    }
                }
                else if (userSeqGuess >= order.Count)
                {
                    lock (_lock)
                    {
                        userDonePicking = true;
                    }
                }
            }
        }
    }

    //internal class DoButtonClickingThreadTask
    //{

    //    private string _taskId;
    //    private ButtonClickingThreadTaskCompleted _completedCallback;

    //    /// <summary>
    //    /// Get. Set simple identifier that allows main thread to identify this task.
    //    /// </summary>
    //    internal string TaskId
    //    {
    //        get { return _taskId; }
    //        set { _taskId = value; }
    //    }

    //    /// <summary>
    //    /// Get, Set instance of a delegate used to notify the main thread when done.
    //    /// </summary>
    //    internal ButtonClickingThreadTaskCompleted CompletedCallback
    //    {
    //        get { return _completedCallback; }
    //        set { _completedCallback = value; }
    //    }

    //    /// <summary>
    //    /// Thread entry point function.
    //    /// </summary>
    //    internal void ExecuteThreadTask()
    //    {
    //        // Often a good idea to tell the main thread if there was an error
    //        bool isError = false;

    //        // Thread begins execution here.
    //        Control[] controls;

    //        //Thread.Sleep(1000);
    //        //bottom.PerformClick();

    //        //Thread.Sleep(1000);
    //        //bottom.PerformClick();

    //        // You would start some kind of long task here 
    //        // such as image processing, file parsing, complex query, etc.

    //        // Thread execution eventually returns to this function when complete.

    //        // Execute callback to tell main thread this task is done.
    //        _completedCallback.Invoke(_taskId, isError);


    //    }

    //}
}

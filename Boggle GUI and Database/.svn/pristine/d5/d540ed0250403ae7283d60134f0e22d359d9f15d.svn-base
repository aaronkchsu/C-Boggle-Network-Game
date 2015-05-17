using CustomNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using BoggleGUI;
using System.Windows.Forms;
using System.Windows;
using System.Text.RegularExpressions;

//Michael Zhao and Aaron Hsu, 12/5/2014.
namespace Model
{
    public class BoggleClientModel
    {
        /// <summary>
        /// Socket associated with the client.
        /// </summary>
        public StringSocket ClientSocket { get; set; }

        /// <summary>
        /// Boolean to indicate if th game is running.
        /// </summary>
        public Boolean GameRunning { get; set; }

        /// <summary>
        /// Username associated with the client.
        /// </summary>
        public String UserName { get; set; }

        /// <summary>
        /// ipAddress associated with the client.
        /// </summary>
        public String ipAddress { get; set; }
        private HashSet<string> wordsSent; //To keep track of the words sent (to appear in the GUI). 
        public event Action<String,String> UpdateScore; //To update a score.
        public event Action<String, String, String> GameStartEvent; //To start a game.
        public event Action<String> TimeChange; //To change the timer on the GUI.
        public event Action<String> UsernameReceived; 
        public event Action<String> DisplaySentWords; // Everytime a word is sent we want to change the GUI
        public event Action ERROR; // Event for errors
        public event Action CloseGame; // Closes the game
        public event Action ResetGUI; //To reset the GUI.
        public event Action<String> FinalScore; //To give a screen indicating the final score.

        /// <summary>
        /// Constructs a Boggleclient that deals with server connection and 
        /// </summary>
        public BoggleClientModel()
        {
            ClientSocket = null; // Socket is not connected yet
            GameRunning = false; // The game does not start yet.
            UserName = null; //null, rather than empty string, so we can give it a standardized username later.
            ipAddress = null; 
            wordsSent = new HashSet<string>();
        }

        /// <summary>
        /// We will use this to form a connection with the server
        /// </summary>
        public void Connect(string IpAddress, int port, string UserName)
        {
            if (ClientSocket == null)
            { // If no connection has already been made from this boggle client
                this.UserName = UserName;
                this.ipAddress = IpAddress;
                //keep track of username and ip address.

                //create a tcp client and attempt a connection creation.
                TcpClient client = null;

                try
                {
                    client = new TcpClient(IpAddress, port);
                }
                catch (Exception) { connectError(1); return; }

                //if username is null, give a default one.
                if (UserName == null)
                    UserName = "Player" + (new Random()).Next();

                ClientSocket = new StringSocket(client.Client, UTF8Encoding.Default);
                ClientSocket.BeginSend("PLAY " + UserName + "\n", (e, p) => { }, ClientSocket); // We attempt to send the first command
                ClientSocket.BeginReceive(CommandReceived, null); // We want to be waiting for a command after recieving a command
                UsernameReceived(UserName); // Sends the Username event to GUI
            }
            else
            {
                connectError(0);
            }
         }

        /// <summary>
        /// Displays errors to the user when an exceptiosn occur
        /// </summary>
        /// <param name="i"></param>
        private void connectError(int i)
        {
            if (i == 0) // This error will be used if the user attempts to connect again
            {
                System.Windows.Forms.MessageBox.Show("Already connected!", "Error: two connection attempts", MessageBoxButtons.OKCancel);
            }
            else if (i == 1) // This will occur for all other default errors
            {
                DialogResult temp = System.Windows.Forms.MessageBox.Show("Unknown error.", "Connection Error", MessageBoxButtons.OKCancel);
            }
            ERROR(); // Make the gui work better
        }

        public void replay()
        {
            DialogResult temp = System.Windows.Forms.MessageBox.Show("Do you wish to play another game?", "Replay?", MessageBoxButtons.YesNo);
            if (temp == DialogResult.Yes)
            {
                ResetGUI();
            }
            else
                CloseGame(); // Closes the game
        }

        /// <summary>
        /// Takes the command Received from the server and processes it
        /// </summary>
        /// <param name="command"></param>
        /// <param name="e"></param>
        /// <param name="p"></param>
        public void CommandReceived(String command, Exception e, object p)
        {
            if (command == null)
            {
                return;
            }

            string[] messages = Regex.Split(command.ToUpper().Trim(), "\\s+");
            switch (messages[0])
            {
                case "TIME":
                    if (GameRunning)
                        TimeChange(messages[1]); // Change time according to the message sent
                    break;
                case "SCORE":
                    UpdateScore(messages[1], messages[2]); // The first one is the player score the second is the opponent score
                    break;
                case "TERMINATED":
                    ResetGUI(); //Reset the gui. 
                    System.Windows.Forms.MessageBox.Show("Opponent terminated!");
                    GameRunning = false;
                    ClientSocket = null;
                    return;
                case "STOP":
                    System.Windows.Forms.MessageBox.Show(command.ToUpper());
                    FinalScore(command.ToUpper().Substring(5)); // Gets rid of the stop, display the scores.
                    GameRunning = false;
                    ClientSocket = null;
                    ResetGUI();
                    return; //modified 12/4, 12:56AM. Needs to be return.
                case "START":
                    GameRunning = true;
                    GameStartEvent(messages[1], messages[2], messages[3]); // Board, Time, Opponent
                    break;
            }
            
            ClientSocket.BeginReceive(CommandReceived, null);
        }

        /// <summary>
        /// This will be used when 
        /// </summary>
        public void SendWord(String word)
        {
            word = word.ToUpper();
            if (GameRunning && ClientSocket != null)
            { 
                // If we are connected and the game is running then
                if (!wordsSent.Contains(word)) //Added 12/4/2014, 12:50AM. 
                    //Otherwise, this allows multiple words to be included in
                    //the wordsSent box.
                {
                    wordsSent.Add(word);
                    DisplaySentWords(word);
                }
                ClientSocket.BeginSend("WORD " + word + "\n", (e, o) => { }, null);
            }
        }


    }
}

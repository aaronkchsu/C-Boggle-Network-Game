using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

using System.Threading;
using System.Threading.Tasks;
using CustomNetworking;
using System.Net.Sockets;

using BB;
using BoggleComm;
using System.Timers;
using System.Diagnostics;

//Author: Michael Zhao and Aaron Hsu, 11/25/2014.
namespace BoggleServer
{
    public class BogServer
    {
        private TcpListener gameServer;
        private List<Game> games;
        private Queue<Player> players;
        private int lengthOfGame;
        private static String filePath;
        private HashSet<String> legalWords;
        private static String letters;
        private Dictionary<StringSocket, Player> socketToPlayer;
        private readonly object lockObject = new object();
        private DatabaseComm data;
        private WebsiteComm web;

        /// <summary>
        /// This is the main method that gets the arguments from the command line and
        /// sets these as the arguments of the game!
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            if (args.Length < 2) // If there are less then two arguments from the command line then do not do anything
                return;

            //If there are three arguments, parse the third argument as an integer.
            if (args.Length == 3 && args[2].Length == 16)
                letters = args[2];
            else if (args.Length == 3 && args[2].Length != 16)
                return;
            
            BogServer temp = new BogServer(2000);
            //Set the length of the game and the filepath of the legal words file.
            temp.lengthOfGame = Int32.Parse(args[0]);
            filePath = args[1];
            Console.ReadLine();
        }

        /// <summary>
        /// Helper method to read from a file.
        /// </summary>
        private void readFile()
        {
            legalWords = new HashSet<string>();
            String line;
            if (filePath == null)
                return;
            lock (lockObject)
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    while ((line = sr.ReadLine()) != null)
                        legalWords.Add(line);
                }
            }
        }

        /// <summary>
        /// Constructor written for unit tests, which takes in a port as an argument.
        /// That way the tests do not interfere with each other.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="args"></param>
        public BogServer(int port, string[] args) : this(port)
        {
            if (args.Length < 2) // If there are less then two arguments from the command line then do not do anything
                return;
            //If there are three arguments, parse the third argument as an integer.
            if (args.Length == 3 && args[2].Length == 16)
                letters = args[2];
            else if (args.Length == 3 && args[2].Length != 16)
                return;

            //Set the length of the game and the filepath of the legal words file.
            this.lengthOfGame = Int32.Parse(args[0]);
            filePath = args[1];
            readFile();

        }

        /// <summary>
        /// This server is used for playing Boggle.
        /// </summary>
        /// <param name="port"></param>
        public BogServer(int port)
        {
            gameServer = new TcpListener(IPAddress.Any, port);
            socketToPlayer = new Dictionary<StringSocket, Player>();

            //Keep track of games and queue the players as they come in.
            games = new List<Game>();
            players = new Queue<Player>();
            letters = null;

            //Create the board game from the file.
            readFile();

            //Setup for database and webpage communication.
            data = new DatabaseComm();
            web = new WebsiteComm();

            gameServer.Start();
            gameServer.BeginAcceptSocket(BoggleSocketReceived, null);
        }

        /// <summary>
        /// Method that receives a socket.
        /// </summary>
        /// <param name="ar"></param>
        private void BoggleSocketReceived(IAsyncResult ar)
        {
            // Receive the socket.
            Socket socket = gameServer.EndAcceptSocket(ar);
            StringSocket ss = new StringSocket(socket, UTF8Encoding.Default);

            //Receive from the socket and begin sending.
            ss.BeginReceive(CommandReceived, ss);
            gameServer.BeginAcceptSocket(BoggleSocketReceived, null);
        }

        /// <summary>
        /// Proccesses a PLAY command from the user.
        /// </summary>
        /// <param name="ConnectionCommand"></param>
        /// <param name="e"></param>
        /// <param name="p"></param>
        private void CommandReceived(String ConnectionCommand, Exception e, object p)
        {
            StringSocket ss = (StringSocket)p;
            
            //The following checks if the socket has been disconnected (StringSocketOfficial.dll is used).
            if (ConnectionCommand == null && (e == null || e is ObjectDisposedException))
            //ObjectDisposedException is quick fix. 12/4/2014.
            {
                socketToPlayer.Remove(ss);
                //ss.Close(); // The socket should not count here because the command is null
                return;
            }

            ConnectionCommand = ConnectionCommand.ToUpper().Trim();

            //Boundary case that the following switch statement/regex split cannot handle.
            if (ConnectionCommand == "")
            {
                ss.BeginSend("IGNORING: empty command\r\n", (ee, pp) => { }, null);
                ss.BeginReceive(CommandReceived, ss);
                return;
            }

            //Process the connection command. 
            string[] tokens = Regex.Split(ConnectionCommand, "\\s+");
            switch (tokens[0])
            {
                case "PLAY":
                    Start(tokens[1], ss); //Add the user.
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        lock (lockObject)
                        {
                            createGames(); //Check if there are more than one players in the queue, creating games until there aren't.
                        }
                    });
                    ss.BeginReceive(GameCommandReceived, ss);
                    break;
                default:
                    ss.BeginSend("IGNORING: " + ConnectionCommand + "\r\n", (ee, pp) => { }, ss); //Ignore any other command.
                    ss.BeginReceive(CommandReceived, ss);
                    break;
            }

        }

        /// <summary>
        /// Handles once the player has connected to the server.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="e"></param>
        /// <param name="p"></param>
        private void GameCommandReceived(String command, Exception e, object p)
        {
            
            StringSocket ss = (StringSocket)p; // The socket is passed by the payload to make this work

            //Check if certain things are null.
            if (command == null && (e == null || e is ObjectDisposedException))
            //ObjectDisposedException is quick fix. 12/4/2014.
            {
                Player temp = socketToPlayer[ss];
                temp.game.opponent(temp).sock.BeginSend("TERMINATED\r\n", (ee, pp) => { }, null);
                socketToPlayer.Remove(ss);
                return;
            }

            if (!socketToPlayer[ss].inGame || command == "")
            {
                if (command != "")
                    ss.BeginSend("IGNORING: " + command + "\r\n", (ee, pp) => { }, ss);
                else
                    ss.BeginSend("IGNORING: empty command\r\n", (ee, pp) => { }, ss);
                ss.BeginReceive(GameCommandReceived, ss);
                return;
            }

            //Quick fix at line 221.
            try
            {
                command = command.ToUpper().Trim();
            }
            catch (NullReferenceException) {  return; }

            //An exception is thrown here when the game terminates. Command is null, but so is the exception.
            string[] tokens = Regex.Split(command, "\\s+");

            if (tokens.Length < 2)
                return;
            switch (tokens[0])
            {
                case "WORD":
                    if (tokens[1] != "")
                        Word(tokens[1], ss, true);
                    break;
                default:
                    ss.BeginSend("IGNORING: " + command + "\r\n", (ee, pp) => { }, ss);
                    break;
            }
      
            //Continue receiving game commands.
            ss.BeginReceive(GameCommandReceived, ss);
        }

        /// <summary>
        /// Helper method to create games.
        /// </summary> 
        private void createGames()
        {
            lock (lockObject)
            {
                while (players.Count > 1)
                {
                    //Get the players from the queue.
                    Player player2 = players.Dequeue();
                    Player player1 = players.Dequeue();
                    Game tempGame;

                    //Create the game, checking if we should include custom letters for the game.
                    if (letters != null)
                    {
                        tempGame = new Game(player1, player2, new BoggleBoard(letters), lengthOfGame, lengthOfGame);
                    }
                    else
                    {
                        tempGame = new Game(player1, player2, new BoggleBoard(), lengthOfGame, lengthOfGame);
                    }

                    //Set the players' games to tempGame.
                    player1.game = tempGame;
                    player2.game = tempGame;
                    games.Add(tempGame);

                    //Set the player's game to 
                    player1.inGame = true; player2.inGame = true;
                    player1.sock.BeginSend("START " + player1.game.board.ToString()
                        + " " + lengthOfGame + " " + player2.username + "\r\n", (ee, pp) => { }, player1.sock);
                    player2.sock.BeginSend("START " + player2.game.board.ToString()
                        + " " + lengthOfGame + " " + player1.username + "\r\n", (ee, pp) => { }, player2.sock);

                    time(tempGame);
                }
            }
        }

        /// <summary>
        /// Sends the time until the seconds left = 0
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        private void time(Game g)
        {
            //Create a timer, add an event handler for when the timer passes an interval (1000ms).
            System.Timers.Timer sw = new System.Timers.Timer();
            sw.Elapsed += (sender, e) => timeMessage(g);
            sw.Interval = 1000;
            sw.Enabled = true;
            sw.Start();
            
        }

        private void timeMessage(Game g)
        {
            //Inform players of the time.
            g.secondsLeft -= 1;
            g.player1.sock.BeginSend("TIME " + g.secondsLeft + "\r\n", (ee, pp) => { }, g.player1.sock);
            g.player2.sock.BeginSend("TIME " + g.secondsLeft + "\r\n", (ee, pp) => { }, g.player2.sock);

            if (g.secondsLeft == 0)
                endGame(g.player1.sock, g.player2.sock);
        }

        private void endGame(StringSocket s1, StringSocket s2)
        {
            Player p1; Player p2;
            try
            {
                p1 = socketToPlayer[s1];
                p2 = socketToPlayer[s2];
            }
            catch (KeyNotFoundException) { return; }

            s1.BeginSend("SCORE " + p1.score +
                " " + p2.score + "\r\n",
                (ee, pp) => { }, s1);
            s2.BeginSend("SCORE " + p2.score + " " + p1.score + "\r\n",
                (ee, pp) => { }, p2.sock);

            //Create lists of legal words and join them together, to send to the players.
            List<string> p1LegalWords = p1.uniqueLegalWords(legalWords);
            String p1Legal = string.Join(",", p1LegalWords.ToArray());

            List<string> commonWords = p1.commonLegalWords(legalWords);
            String common = string.Join(",", commonWords.ToArray());

            List<string> p2LegalWords = p2.uniqueLegalWords(legalWords);
            String p2Legal = string.Join(",", p2LegalWords.ToArray());

            List<string> p1Illegal = p1.illegalWords(legalWords);
            String illegal1 = string.Join(",", p1Illegal.ToArray());

            List<string> p2Illegal = p2.illegalWords(legalWords);
            String illegal2 = string.Join(",", p2Illegal.ToArray());

            String temp = "STOP " + p1LegalWords.Count + " " + p1Legal + " " + p2LegalWords.Count + " " + p2Legal + " " + commonWords.Count + " " + common + " " + p1Illegal.Count + " " + illegal1 + " " + p2Illegal.Count + " " + illegal2 + "\r\n";
            s1.BeginSend(temp, (ee, pp) => { }, null);
            s2.BeginSend("STOP " + p2LegalWords.Count + " " + p2Legal + " " + p1LegalWords.Count + " " + p1Legal + " " + commonWords.Count + " " + common + " " + p2Illegal.Count + " " + illegal2 + " " + p1Illegal.Count + " " + illegal1 + "\r\n", (ee, pp) => { }, null);
            
            //Allows data to handle the Boggle SQL database.
            data.transmitGame(p1.username, p2.username, p1.score, p2.score, DateTime.Now.ToString("YYYY-MM-dd HH:mm:ss"), this.lengthOfGame.ToString(), p1.game.board.ToString(), temp);

            s1.Close();
            s2.Close();
            
                     
        }

        private bool Word(String wordSent, StringSocket ss, bool inGame)
        {
            Player p1 = socketToPlayer[ss];

            bool changedScore;
            lock (lockObject) 
            {
                changedScore = p1.addWord(wordSent, legalWords);
            }

            if (inGame && changedScore) 
            {
                Player opponent = p1.game.opponent(p1);
                    
                ss.BeginSend("SCORE " + p1.score +
                    " " + opponent.score + "\r\n",
                    (ee, pp) => { }, ss);
                opponent.sock.BeginSend("SCORE " + opponent.score + " " + p1.score + "\r\n",
                    (ee, pp) => { }, opponent.sock);
            }
            return changedScore;
        }

        /// <summary>
        /// When they enter teh game we must add them to the queue
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="ss"></param>
        private void Start(String userName, StringSocket ss)
        {
            Player play = new Player(userName, ss, false);
            lock (lockObject)
            {
                players.Enqueue(play);
                socketToPlayer.Add(ss, play);
            }

        }

        /// <summary>
        /// Close all the sockets in the server then stops the server
        /// </summary>
        public void close()
        {
            foreach (StringSocket ss in socketToPlayer.Keys)
            {
                ss.Close();
            }

            gameServer.Stop();
        }


        /// <summary>
        /// Class representing a player.
        /// </summary>
        public class Player
        {
            public String username { get; set; }
            public int score;
            public StringSocket sock;
            public HashSet<string> wordsSent;
            public Game game;
            public bool inGame;

            public Player(String username, StringSocket socket, bool inGame)
            {
                this.username = username;
                sock = socket;
                score = 0;
                wordsSent = new HashSet<string>();
                this.inGame = inGame;
            }

            /// <summary>
            /// Words that only occur for a single client and are legal
            /// </summary>
            /// <returns></returns>
            public List<string> uniqueLegalWords(HashSet<string> legalWords)
            {
                List<string> toReturn = new List<string>();
                foreach (String s in wordsSent)
                {
                    if ((game.board.CanBeFormed(s) && legalWords.Contains(s)) && 
                        !this.game.opponent(this).wordsSent.Contains(s))
                    {
                        toReturn.Add(s);    
                    }
                }

                return toReturn;
            }
            
            /// <summary>
            /// This gets all the illegal words that were sent from the players
            /// </summary>
            /// <returns></returns>
            public List<string> illegalWords(HashSet<string> legalWords)
            {
                List<string> toReturn = new List<string>();
                foreach (String s in wordsSent)
                {
                    if (!game.board.CanBeFormed(s) || !legalWords.Contains(s))
                    {
                        toReturn.Add(s);
                    }
                }

                return toReturn;
            }

            /// <summary>
            /// Adds the word currently in s to common words that were sent from both players
            /// </summary>
            /// <returns></returns>
            public List<string> commonLegalWords(HashSet<string> legalWords)
            {
                List<string> toReturn = new List<string>();
                foreach (String s in this.game.commonWords)
                {
                    toReturn.Add(s);
                }

                return toReturn;
            }

            /// <summary>
            /// Returns true if the string s changes the score.
            /// </summary>
            /// <param name="s"></param>
            /// <returns></returns>
            public bool addWord(String s, HashSet<string> legalWords)
            {
                bool legal = game.board.CanBeFormed(s) && legalWords.Contains(s);
                //Each word with fewer than three characters is removed (whether or not it is legal).
                if (s.Length < 3)
                    return false;

                //For any word that appears more than once, all but the first occurrence is removed (whether or not it is legal).
                if (wordsSent.Contains(s))
                    return false;
                else
                    wordsSent.Add(s);

                

                Player opponent = game.opponent(this);


                //Each legal word that occurs on the opponent's list is removed.
                if (legal && opponent.wordsSent.Contains(s))
                {
                    opponent.score -= points(s); //Their score must be reduced.
                    opponent.wordsSent.Remove(s);
                    wordsSent.Remove(s);
                    this.game.commonWords.Add(s);
                    return false;
                }

                //Each remaining illegal word is worth negative one points.
                if (!legal)
                {
                    score -= 1;
                }
                else
                {
                    //Each remaining legal word earns a score that depends on its length. 
                    score += points(s);
                }

                return true;

            }

            //Returns the point value of a string s.
            private int points(String s)
            {
                //Three- and four-letter words are worth one point, five-letter words are 
                //worth two points, six-letter words are worth three points, seven-letter 
                //words are worth five points, and longer word are worth 11 points.
                int pointValue = 0;
                switch (s.Length)
                {
                    case 3:
                    case 4:
                        pointValue = 1;
                        break;
                    case 5:
                        pointValue = 2;
                        break;
                    case 6:
                        pointValue = 3;
                        break;
                    case 7:
                        pointValue = 5;//This was previously 4. It may cause us to lose points.
                        break;
                    default:
                        pointValue = 11;
                        break;
                }

                return pointValue;
            }
        }

        
        /// <summary>
        /// Represents a Boggle game.
        /// </summary>
        public class Game
        {
            public Player player1 { get; set; }
            public Player player2 { get; set; }
            public int gameLength;
            public BoggleBoard board;
            public int secondsLeft;
            public HashSet<String> commonWords;

            public Game(Player p1, Player p2, BoggleBoard board, int length, int time)
            {
                player1 = p1;
                player2 = p2;
                this.board = board;
                gameLength = length;
                secondsLeft = time;
                commonWords = new HashSet<string>();
            }

            public Player opponent(Player p1)
            {
                if (p1.sock == player1.sock)
                    return player2;
                return player1;
            }
        }

    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using Model;
using System.Windows.Threading;
using System.Collections;
using System.Text.RegularExpressions;

//Michael Zhao and Aaron Hsu, 12/5/2014.
namespace BoggleGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private BoggleClientModel model;
        private ArrayList BoggleBoard; // For funzies
        
        public MainWindow()
        {
            InitializeComponent();
            model = new BoggleClientModel();
            BoggleBoard = new ArrayList(); // Keeps boggle board letter instance
            model.GameStartEvent += StartTheGame; // This creates the game.
            model.UsernameReceived += UsernameEvent; // Starts the action to listen for a Username change
            model.TimeChange += UpdateTheTime; // Updates the time.
            model.UpdateScore += UpdateTheScores; // Updates the scores.
            model.ERROR += ErrorOccured; // Clears the IPBox, which is where an error could come from.
            model.CloseGame += EndClient; // Handle the client closing.
            model.DisplaySentWords += DisplayWords; //Update the display of sent words.
            model.ResetGUI += Reset; //Display the reset.
            model.FinalScore += EndScreen; //Display the end screen after the game is over.
            CreateBoard("ABCDEFGHIJKLMNOP");
            DigitalBoard("ABCDEFGHIJKLMNOP");
        }

        /// <summary>
        /// Update the username if the textbox changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void userBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            model.UserName = ((TextBox)sender).Text;
        }

        /// <summary>
        /// Update the ip address if the ipbox changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ipBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            model.ipAddress = ((TextBox)sender).Text;
        }


        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (model.UserName != null && model.ipAddress != null) 
            {
                IPAddress temp = null;

                //Attempt to parse the ip address. Otherwise, use Dns.GetHostAddresses like Matthew suggested.
                if (!IPAddress.TryParse(model.ipAddress, out temp))
                    try
                    {
                        temp = Dns.GetHostAddresses(model.ipAddress)[0];
                    }
                    catch (Exception) { MessageBox.Show("Invalid IP address!"); }

                if (temp != null)
                    model.Connect(temp.ToString(), 2000, model.UserName);
            }
        }

        /// <summary>
        /// In the event that a Username is Received.. We need to change the label
        /// </summary>
        /// <param name="name"></param>
        private void UsernameEvent(string name)
        {
            Dispatcher.Invoke(new Action(() => { userNameLabel.Content = name;})); // 
            userBox.Clear(); // Clear Boxes
            ipBox.Clear(); // Clear Boxes
        }

        /// <summary>
        /// In the event that "TERMINATED" is received by the client socket, reset the GUI.
        /// </summary>
        public void Reset()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                secondsLeftLabel.Content = "Game Not Started";
                userNameLabel.Content = "";
                oppNameLabel.Content = "";
                userScoreLabel.Content = "0";
                oppScoreLabel.Content = "0";
                CreateBoard("AAAAAAAAAAAAAAAA");
                DigitalBoard("AAAAAAAAAAAAAAAA");
                BoggleBoard.Clear();
                wordsPlayedBox.Clear();
                wordBox.Clear();

            }));

        }

        /// <summary>
        /// When the game starts this event will be triggered
        /// </summary>
        /// <param name="start"></param>
        public void StartTheGame(string board, string time, string opponent)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                CreateBoard(board);
                DigitalBoard(board);
                secondsLeftLabel.Content = time;
                oppNameLabel.Content = opponent;
            })); // This action will populate the board, set the opponent, and change the time label
                
        }

        /// <summary>
        /// Helper method used to created the boggle boarbd
        /// </summary>
        private void CreateBoard(string board){
            int i = 0; // starts at index 0
            board = board.ToUpper(); // Make it uppercase
            Dispatcher.Invoke(new Action(() =>
            {
            boggle11.Content = board[i++];
            boggle12.Content = board[i++];
            boggle13.Content = board[i++];
            boggle14.Content = board[i++];
            boggle21.Content = board[i++];
            boggle22.Content = board[i++];
            boggle23.Content = board[i++];
            boggle24.Content = board[i++];
            boggle31.Content = board[i++];
            boggle32.Content = board[i++];
            boggle33.Content = board[i++];
            boggle34.Content = board[i++];
            boggle41.Content = board[i++];
            boggle42.Content = board[i++];
            boggle43.Content = board[i++];
            boggle44.Content = board[i++];
            })); 
        }

        /// <summary>
        /// Creates a digital board for funzies
        /// </summary>
        private void DigitalBoard(string board)
        {
            int i = 0; // starts at index 0
            board = board.ToUpper(); // Make it uppercase
            while(i < 16)
            BoggleBoard.Insert(i, board[i++]);// Maps char
        }

        /// <summary>
        /// Everytime a word is sent we want to display it to the user
        /// </summary>
        /// <param name="word"></param>
        public void DisplayWords(string word)
        {
            Dispatcher.Invoke(new Action(() => { wordsPlayedBox.Text += word + "\r\n"; })); // Sends off the display on a word
        }

        /// <summary>
        /// Once a command is sent from base to update scores mission control will be on it!
        /// </summary>
        /// <param name="PlayerScore"></param>
        /// <param name="OpponentScore"></param>
        public void UpdateTheScores(string PlayerScore, string OpponentScore)
        {
            Dispatcher.Invoke(new Action(() => { userScoreLabel.Content = PlayerScore; oppScoreLabel.Content = OpponentScore;}));
        }

        /// <summary>
        /// Time is very important to us... it is all we have so we need to keep it up to date
        /// </summary>
        /// <param name="Time"></param>
        public void UpdateTheTime(string Time)
        {
            int temp;
            if (Int32.TryParse(Time, out temp) && temp == 0)
                Reset();
            Dispatcher.Invoke(new Action(() => {secondsLeftLabel.Content = Time;})); // Updates Time
        }

        /// <summary>
        /// If an error occurs clear some text and fix it
        /// </summary>
        private void ErrorOccured()
        {
            Dispatcher.Invoke(new Action(() => { ipBox.Clear(); }));
        }

        /// <summary>
        /// If a message from mission control says abort .. then we must abord
        /// </summary>
        private void EndClient()
        {
            Dispatcher.Invoke(new Action(() => { this.Close(); model.ClientSocket.Close(); }));
        }

        /// <summary>
        /// When user enters a word send it to the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnterWord(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter && wordBox.Text != ""){ 
                model.SendWord(wordBox.Text); // Model sends a word to server
                wordBox.Clear();
            }
        }

        /// <summary>
        /// Handles the closing of the boggle client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void boggleClientWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => { try { model.ClientSocket.Close(); } catch { } }));
        }

        /// <summary>
        /// Tries to highlight letters written on the board
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HightlightLetters(object sender, TextChangedEventArgs e)
        {
            ColorText(); // Changes to default then colors text
            for(int i = 0; i < wordBox.Text.Length; i++){
                if(BoggleBoard.Contains(Char.ToUpper(wordBox.Text[i]))){
                switch(BoggleBoard.IndexOf(Char.ToUpper(wordBox.Text[i]))){
                    case 0:
                        boggle11.Foreground = Brushes.Red;
                        break;
                    case 1:
                        boggle12.Foreground = Brushes.GreenYellow;
                        break;
                    case 2:
                        boggle13.Foreground = Brushes.Violet;
                        break;
                    case 3:
                        boggle14.Foreground = Brushes.Chocolate;
                        break;
                    case 4:
                        boggle21.Foreground = Brushes.DarkBlue;
                        break;
                    case 5:
                        boggle22.Foreground = Brushes.Yellow;
                        break;
                    case 6:
                        boggle23.Foreground = Brushes.BurlyWood;
                        break;
                    case 7:
                        boggle24.Foreground = Brushes.DarkOliveGreen;
                        break;
                    case 8:
                        boggle31.Foreground = Brushes.DodgerBlue;
                        break;
                    case 9:
                        boggle32.Foreground = Brushes.Firebrick;
                        break;
                    case 10:
                        boggle33.Foreground = Brushes.LightGoldenrodYellow;
                        break;
                    case 11:
                        boggle34.Foreground = Brushes.MediumSlateBlue;
                        break;
                    case 12:
                        boggle41.Foreground = Brushes.PeachPuff;
                        break;
                    case 13:
                        boggle42.Foreground = Brushes.PowderBlue;
                        break;
                    case 14:
                        boggle43.Foreground = Brushes.Pink;
                        break;
                    case 15:
                        boggle44.Foreground = Brushes.PapayaWhip;
                        break;
                    default:
                        break;
                }
                }
            }

        }

        /// <summary>
        /// Sets the default text color
        /// </summary>
        private void ColorText()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                boggle11.Foreground = Brushes.White;
                boggle12.Foreground = Brushes.White;
                boggle13.Foreground = Brushes.White;
                boggle14.Foreground = Brushes.White;
                boggle21.Foreground = Brushes.White;
                boggle22.Foreground = Brushes.White;
                boggle23.Foreground = Brushes.White;
                boggle24.Foreground = Brushes.White;
                boggle31.Foreground = Brushes.White;
                boggle32.Foreground = Brushes.White;
                boggle33.Foreground = Brushes.White;
                boggle34.Foreground = Brushes.White;
                boggle41.Foreground = Brushes.White;
                boggle42.Foreground = Brushes.White;
                boggle43.Foreground = Brushes.White;
                boggle44.Foreground = Brushes.White;
            }));
        }

        private void EndScreen(string score)
        {
            //string[] end = Regex.Split(score, "\\s+");
            
            Dispatcher.Invoke(new Action(() => {
            EndScreen window = new EndScreen();
            window.Show();// Displays score
           // window.Player.Content = userNameLabel.Content;
           // window.Opponent.Content = oppNameLabel.Content;
            window.PlayerEndScore.Content = userScoreLabel.Content;
            window.OpponentEndScore.Content = oppScoreLabel.Content;
            window.WordsPlay.Text = score;
            window.WordsOpp.Text = score;
            }));
        }
    }
}

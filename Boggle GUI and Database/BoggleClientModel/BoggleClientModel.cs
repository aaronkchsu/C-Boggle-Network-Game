using CustomNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class BoggleClientModel
    {

        private StringSocket ClientSocket;
        private Boolean GameRunning;
        private String UserName;

        /// <summary>
        /// Constructs a Boggleclient that deals with server connection and 
        /// </summary>
        public void BoggleClientModel()
        {
            ClientSocket = null; // Socket is not connected yet
            GameRunning = false; // The game does not start yet I believe?
        }

        /// <summary>
        /// We will use this to form a connection with the server
        /// </summary>
        public void Connect(string IpAddress, int port, string UserName)
        {
            if (ClientSocket == null) { // If no connection has already been made from this boggle client
                this.UserName = UserName;
                TcpClient client = new TcpClient(IpAddress, port);
                ClientSocket = new StringSocket(client.Client, UTF8Encoding.Default);
                ClientSocket.BeginSend("PLAY " + UserName + "\n", (e, p) => { }, ClientSocket); // We attempt to send the first command
                ClientSocket.BeginReceive(CommandReceived, null); // We want to be waiting for a command after recieving a command
            }
         }

        public void Cancel()
        {
            if (!GameRunning)
            {
                ClientSocket.Close();
            }
            else
            {
                //TODO: ask user to play a new game
            }
        }

        /// <summary>
        /// Takes the command recieved from the server and processes it
        /// </summary>
        /// <param name="command"></param>
        /// <param name="e"></param>
        /// <param name="p"></param>
        public void CommandReceived(String command, Exception e, object p)
        {
            
            if (command.StartsWith("START"))
                GameRunning = true;
            else if (command == "TERMINATE" || command.StartsWith("STOP"))
                ClientSocket.Close();

            ClientSocket.BeginReceive(CommandReceived, null);
        }

        /// <summary>
        /// This will be used when 
        /// </summary>
        public void SendWord(String word)
        {
            if(!GameRunning && ClientSocket != null){ // If we are connected and the game is running then
                ClientSocket.BeginSend("WORD " + word, (e, o) => { }, null);
            }
        }


    }
}

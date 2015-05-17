using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using CustomNetworking;
using MySql.Data.MySqlClient;

namespace BoggleComm
{
    public class WebsiteComm
    {
        private TcpListener server;
        //Used to begin the html document.
        private const string pageStart = "<!DOCTYPE html>\n" +
                                         "<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                         "<head>\n" +
                                         "    <meta charset=\"utf-8\" />\n" +
                                         "    <title>\n";
        //Sometime used to end the html document.
        private const string pageEnd = "</body>\n" +
                                       "</html>\n";
        //The connection string for the database where the boggle data is stored.
        public const string connectionString = "server=atr.eng.utah.edu;database=cs3500_ahsu;uid=cs3500_ahsu;password=333091724";

        public WebsiteComm()
        {
            //Begin by accepting sockets on port 2500.
            server = new TcpListener(IPAddress.Any, 2500);
            server.Start();
            server.BeginAcceptSocket(WebSocketReceived, null);
        }

        private void WebSocketReceived(IAsyncResult ar)
        {
            //Create a StringSocket to communicate with the connected client.
            Socket sock = server.EndAcceptSocket(ar);
            StringSocket ss = new StringSocket(sock, UTF8Encoding.Default);
            ss.BeginReceive(CommandReceived, ss);
            server.BeginAcceptSocket(WebSocketReceived, null);
        }

        private void CommandReceived(String command, Exception e, object payload)
        {
            String id;
            StringSocket ss = (StringSocket)payload;
            String page;

            if (command == "GET /players HTTP/1.1") //Bring up a page with the server stats.
            {
                page = serverStats(); 
            }
            else if (Regex.IsMatch(command, @"GET\s/games?player=(.*)\sHTTP/1.1")) //Bring up a page of games the user has played.
            {
                id = Regex.Match(command, @"GET\s/games?player=(.*)\sHTTP/1.1").Groups[1].Value;
                page = playerGames(id);
            }
            else if (Regex.IsMatch(command, @"GET\s/game?id=(\d+)\sHTTP/1.1")) //Bring up the information about a specific game.
            {
                id = Regex.Match(command, @"GET\s/game?id=(\d+)\sHTTP/1.1").Groups[1].Value;
                page = gameInfo(Int32.Parse(id));
            }
            else //Otherwise, we cannot find the page.
            {
                page = errorPage();
            }

            send(page, ss); //Use the protocol outlined in the documentation to send the string page.
        }

        /// <summary>
        /// Sends page using ss, given the protocol outlined in the documentation.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="ss"></param>
        private void send(String page, StringSocket ss)
        {
            ss.BeginSend("HTTP/1.1 200 OK\r\nConnection: close\r\nContent-Type: text/html; charset=UTF-8\r\n",
            (ee, oo) => { }, ss);

            ss.BeginSend("\r\n", (ee, oo) => { }, ss);

            ss.BeginSend(page, (ee, oo) => { }, ss);

            ss.Close();
        }

        /// <summary>
        /// Construct the string for the server stats page.
        /// </summary>
        /// <returns></returns>
        private String serverStats()
        {
            String page = "" + pageStart;
            page += "Server Stats" +
                    "</title>" +
                    "<body>" +
                    "<p>Here are all the players who have ever played and their win/loss/tie record.</p>" +
                    "<table style=\"width:100%\">";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    //Make a query to PlayerInformation for player information.
                    MySqlCommand comm = conn.CreateCommand();
                    comm.CommandText = "SELECT * FROM PlayerInformation";

                    //Use data from queries to build an HTML table.
                    using (MySqlDataReader reader = comm.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            page += "<tr>" +
                                    "  <td>" + reader["BoggleUsername"] + "</td>" +
                                    "  <td>" + reader["GamesWon"] + "</td>" +
                                    "  <td>" + reader["GamesLost"] + "</td>" +
                                    "  <td>" + reader["GamesTied"] + "</td>" +
                                    "</tr>";
                        }
                    }

                }
                catch { return null; }
            }

            //Closing tags for table, body and html. \n so StringSocket will send.
            page += "</table></body></html>\n";

            return page;
        }

        /// <summary>
        /// Return an HTML string of the error page
        /// </summary>
        /// <returns></returns>
        private String errorPage()
        {
            String page = "" + pageStart;
            page += "Page Not Found" +
                    "</title>" +
                    "<body>" +
                    "<p>We were unable to locate the page you requested!</p>" +
                    "</body>\n";

            return page;
        }

        /// <summary>
        /// Returns a string representing an HTML page that displays information about the game.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private String gameInfo(int id)
        {
            String page = "" + pageStart;

            string[] gameData = new string[8];

            getData(ref gameData, id); //Populates the gameData array with information about the game from the GameInformation database.

            //Add information about the game into a paragraph.
            page += "GAME " + gameData[0] + "</title" +
                    "<body>" +
                    "<p>This game occured between " + gameData[1] + ", who scored " + gameData[2] + " points, " +
                    "and " + gameData[3] + ", who scored " + gameData[4] + " points. " +
                    "It had a time limit of " + gameData[6] + " seconds. " +
                    "This game finished on " + gameData[7] + ". The following was the board configuration.</p>";

            //Create a table representing the boggle game
            for (int i = 0; i < 4; i++)
            {
                int j = 4*i;
                page += "<tr>" +
                        "  <td>" + gameData[5][j] + "</td>" +
                        "  <td>" + gameData[5][j + 1] + "</td>" +
                        "  <td>" + gameData[5][j + 2] + "</td>" +
                        "  <td>" + gameData[5][j + 3] + "</td>" +
                        "</tr>";
            }

            //Add the end table tag.
            page += "</table>";

            page += "<p>The following were the words sent by each of the players, in no particular order.</p>";

            //Get the data associated with the five part word summary.
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand comm = conn.CreateCommand();

                    comm.CommandText = "SELECT * FROM WordsPlayed WHERE game_id = " + id;

                    using (MySqlDataReader reader = comm.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            page += "<tr>" +
                                    "  <td>" + reader["game_id"] + "</td>" +
                                    "  <td>" + returnUser(reader["player_id"].ToString()) + "</td>" +
                                    "  <td>" + reader["word"] + "</td>" +
                                    "  <td>" + reader["legal"] + "</td>" +
                                    "</tr>";
                        }
                    }
                }
                catch { }
            }

            page += "</table></body></html>\n";

            return page;
        }

        private void getData(ref string[] gameData, int id)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    MySqlCommand comm = conn.CreateCommand();
                    comm.CommandText = "SELECT *" +
                                       " FROM GameInformation" +
                                       " WHERE GameInformation.P1ID = " + id;


                    using (MySqlDataReader reader = comm.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                gameData[0] = reader["GameNumber"].ToString();
                                gameData[1] = returnUser(reader["P1ID"].ToString());
                                gameData[2] = reader["ScoreP1"].ToString();
                                gameData[3] = returnUser(reader["P2ID"].ToString());
                                gameData[4] = reader["ScoreP2"].ToString();
                                gameData[5] = reader["BoardInfo"].ToString();
                                gameData[6] = reader["TimeLimit"].ToString();
                                gameData[7] = reader["EndTime"].ToString();

                                return;
                            }
                        }
                    }

                    comm.CommandText = "SELECT *" +
                                       " FROM GameInformation" +
                                       " WHERE GameInformation.P2ID = " + id;

                    using (MySqlDataReader reader = comm.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                gameData[0] = reader["GameNumber"].ToString();
                                gameData[1] = returnUser(reader["P2ID"].ToString());
                                gameData[2] = reader["ScoreP2"].ToString();
                                gameData[3] = returnUser(reader["P1ID"].ToString());
                                gameData[4] = reader["ScoreP1"].ToString();
                                gameData[5] = reader["BoardInfo"].ToString();
                                gameData[6] = reader["TimeLimit"].ToString();
                                gameData[7] = reader["EndTime"].ToString();

                                return;
                            }
                        }
                    }

                }
                catch { return; }
            }
        }

        private String playerGames(String play_id)
        {
            //Note: play_id is actually a username.
            String id = returnID(play_id);

            //If the id is null, the player does not yet exist in the database.
            if (id == null)
                return errorPage();

            String page = "" + pageStart;
            page += play_id + "'s Games" +
                    "</title>" +
                    "<body>" +
                    "<p>Here are all the games " + play_id + " has ever played.</p>" +
                    "<table style=\"width:100%\">";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    MySqlCommand comm = conn.CreateCommand();
                    comm.CommandText = "SELECT *" +
                                       " FROM GameInformation" +
                                       " WHERE GameInformation.P1ID = " + id;
                                       //"OR GameInformation.P2ID = " + id;


                    using (MySqlDataReader reader = comm.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            page += "<tr>" +
                                    "  <td>" + reader["GameNumber"] + "</td>" +
                                    "  <td>" + reader["EndTime"] + "</td>" +
                                    "  <td>" + returnUser(reader["P2ID"].ToString()) + "</td>" +
                                    "  <td>" + reader["ScoreP1"] + "</td>" +
                                    "  <td>" + reader["ScoreP2"] + "</td>" +
                                    "</tr>";
                        }
                    }

                }
                catch { return null; }
            }

            page += "</table></body></html>\n";

            return page;
        }

        /// <summary>
        /// Returns an ID given a username. If it returns null, the user does not exist.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private String returnID(String user)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    MySqlCommand command = conn.CreateCommand();
                    command.CommandText = "Select *" +
                                          " FROM PlayerInformation" +
                                          " WHERE PlayerInformation.BoggleUsername = " + user;


                    using (MySqlDataReader read = command.ExecuteReader())
                    {
                        while (read.Read())
                        {
                            if (read["ID"] != null)
                                return read["ID"].ToString();
                        }
                    }

                }
                catch { }
            }

            //null is returned either here or a few lines above. If that's the case, that should mean user doesn't have an ID yet.
            return null;
        }

        private String returnUser(String id)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    MySqlCommand command = conn.CreateCommand();
                    command.CommandText = "Select *" +
                                          " FROM PlayerInformation" +
                                          " WHERE PlayerInformation.ID = " + id;


                    using (MySqlDataReader read = command.ExecuteReader())
                    {
                        while (read.Read())
                        {
                            if (read["BoggleUsername"] != null)
                                return read["BoggleUsername"].ToString();
                        }
                    }

                }
                catch { }
            }

            //null is returned either here or a few lines above. If that's the case, that should mean user doesn't have an ID yet.
            return null;
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace BoggleComm
{
    
    public class DatabaseComm
    {
        /// <summary>
        /// The connection string.
        /// </summary>
        public const string connectionString = "server=atr.eng.utah.edu;database=cs3500_ahsu;uid=cs3500_ahsu;password=333091724";

        /// <summary>
        /// Transmit the game. Invariant: only used when BoggleServer's endGame method is just about to transmit the STOP command,
        /// hence we do not have to worry about incomplete games.
        /// </summary>
        /// <param name="p1Name"></param>
        /// <param name="p2Name"></param>
        /// <param name="p1Score"></param>
        /// <param name="p2Score"></param>
        /// <param name="endDate"></param>
        /// <param name="timeLimit"></param>
        /// <param name="board"></param>
        /// <param name="summary"></param>
        /// <returns></returns>
        public bool transmitGame(String p1Name, String p2Name, int p1Score, int p2Score,
            String endDate, String timeLimit, String board, String summary)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {

                    //Get the player IDs associated with the usernames, creating the IDs if necessary.
                    String p1_id = returnID(p1Name);

                    if (p1_id == null)
                        p1_id = createID(p1Name);

                    String p2_id = returnID(p2Name);
                    
                    if (p2_id == null)
                        p2_id = createID(p2Name);

                    conn.Open();

                    //Insert complete information about the game into the GameInformation table.
                    MySqlCommand command = conn.CreateCommand();
                    command.CommandText = "INSERT INTO GameInformation (P1ID, P2ID, EndTime, TimeLimit, ScoreP1, ScoreP2, BoardInfo)" +
                                          "VALUES ('" + p1_id + "','" + p2_id + "','" + endDate + "','" + timeLimit + "','" + p1Score + "','" + p2Score + "','" + board + "')";

                    command.ExecuteNonQuery();

                    //Update the information in the PlayerInformation table.
                    updateScore(p1Score, p2Score, p1_id, p2_id);

                    //Use the summary string to add to the WordsPlayed database.
                    processSummary(summary, p1_id, p2_id);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// From the STOP command, extract the words that need to be added to the database.
        /// </summary>
        /// <param name="summary"></param>
        /// <param name="p1_id"></param>
        /// <param name="p2_id"></param>
        private void processSummary(String summary, String p1_id, String p2_id)
        {
            //Get the lists of legal, illegal words. Regex.Split cannot be used in the boundary case that no words are transmitted..
            String pattern = @"STOP\s\d+\s(.*)\s\d+\s(.*)\s\d+\s(.*)\s\d+(.*)\d+\s+(.*)\r\n";
            Match mat = Regex.Match(summary, pattern);

            //Send the words to the database.
            wordsToDatabase(mat.Groups[1].Value, p1_id, p2_id, 1, true); //p1legal
            wordsToDatabase(mat.Groups[4].Value, p1_id, p2_id, 1, false); //p1illegal
            wordsToDatabase(mat.Groups[3].Value, p1_id, p2_id, 3, true); //common
            wordsToDatabase(mat.Groups[2].Value, p1_id, p2_id, 2, true); //p2legal
            wordsToDatabase(mat.Groups[5].Value, p1_id, p2_id, 2, false); //p2illegal
        }

        /// <summary>
        /// Given a string of comma separated words, write them to the database.
        /// </summary>
        /// <param name="commaSepWords">string of comma separted words (potentially empty)</param>
        /// <param name="p1_id">ID of player 1</param>
        /// <param name="p2_id">ID of player 2</param>
        /// <param name="player_no">The player whose words we are transmitting.</param>
        /// <param name="legal">Indicates whether we are transmitting legal words or illegal words.</param>
        private void wordsToDatabase(String commaSepWords, String p1_id, String p2_id, int player_no, bool legal)
        {
            //No words may have been transmitted, so we can just return.
            if (commaSepWords == "")
                return;

            //Collect the words.
            string[] tokens = Regex.Split(commaSepWords, ",");

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    //Obtain the Game ID. THIS MIGHT GO BADLY, since we are not executing the command.
                    MySqlCommand command1 = conn.CreateCommand();
                    command1.CommandText = "SELECT MAX(GameNumber) FROM GameInformation";

                    int game_id = (int) command1.ExecuteScalar();
                    MySqlCommand command = conn.CreateCommand();

                    //Add the words in. Not entirely sure about how ExecuteNonQuery() works. Supposedly this code will have command text,
                    //execute it, then change the command text, and so on.
                    foreach (string s in tokens)
                    {
                        switch (player_no)
                        {
                             //case 3 indicates a tie. Notice that case 3 and case 1 are the same, except that 3 goes into 2.
                            case 3:
                                command.CommandText = "INSERT INTO WordsPlayed" + " VALUES ('" + s + "', '" + game_id + "', '" + p1_id + "', '" + legal + "')";
                                goto case 2;
                            case 2:
                                command.CommandText = "INSERT INTO WordsPlayed" + " VALUES ('" + s + "', '" + game_id + "', '" + p2_id + "', '" + legal + "')";
                                break;
                            case 1:
                                command.CommandText = "INSERT INTO WordsPlayed" + " VALUES ('" + s + "', '" + game_id + "', '" + p1_id + "', '" + legal + "')";
                                break;
                        }
                        //if (player_no == 1)
                        //    command.CommandText = "INSERT INTO WordsPlayed" +
                        //                          "VALUES (" + s + ", " + game_id + ", " + p1_id + ", " + legal + ")";
                        //else if (player_no == 2)
                        //    command.CommandText = "INSERT INTO WordsPlayed" +
                        //                          "VALUES (" + s + ", " + game_id + ", " + p2_id + ", " + legal + ")";
                        command.ExecuteNonQuery();
                    }


                }
                catch { }
            }
        }

        /// <summary>
        /// Update the score of the player. 
        /// </summary>
        /// <param name="p1Score"></param>
        /// <param name="p2Score"></param>
        /// <param name="p1ID"></param>
        /// <param name="p2ID"></param>
        private void updateScore(int p1Score, int p2Score, String p1ID, String p2ID)
        {
         
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand command = conn.CreateCommand();

                    //Update the GamesWon, GamesLost, and GamesTied fields in the database as appropriate.
                    if (p1Score > p2Score)
                    {
                        command.CommandText = "UPDATE PlayerInformation SET GamesWon=GamesWon+1 WHERE ID=" + p1ID;
                        command.ExecuteNonQuery();
                        //Do we need to do something before setting command text to something else? Does ExecuteNonQuery work?
                        command.CommandText = "UPDATE PlayerInformation SET GamesLost=GamesLost+1 WHERE ID=" + p2ID;
                        command.ExecuteNonQuery();
                    }
                    else if (p2Score > p1Score)
                    {
                        command.CommandText = "UPDATE PlayerInformation SET GamesWon=GamesWon+1 WHERE ID=" + p2ID;
                        command.ExecuteNonQuery();
                        //Do we need to do something before setting command text to something else?
                        command.CommandText = "UPDATE PlayerInformation SET GamesLost=GamesLost+1 WHERE ID=" + p1ID;
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        command.CommandText = "UPDATE PlayerInformation SET GamesTied=GamesTied+1 WHERE ID=" + p1ID;
                        command.ExecuteNonQuery();
                        //Do we need to do something before setting command text to something else?
                        command.CommandText = "UPDATE PlayerInformation SET GamesTied=GamesTied+1 WHERE ID=" + p2ID;
                        command.ExecuteNonQuery();
                    }
                }
                catch { }
            }

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
                    command.CommandText = "SELECT *" +
                                          " FROM PlayerInformation" +
                                          " WHERE BoggleUsername = '" + user + "';";


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

        /// <summary>
        /// Creates an ID for a user if one does not already exist. Returns the ID. 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private String createID(String user)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    MySqlCommand command = conn.CreateCommand();
                    command.CommandText = "INSERT INTO PlayerInformation (BoggleUsername, GamesWon, GamesTied, GamesLost)" +
                                          " VALUES ('" + user + "', 0, 0, 0)";

                    command.ExecuteNonQuery();
                    //Do we need to do something before changing the command text? ExecuteNonQuery?

                    command.CommandText = "SELECT *" +
                                          " FROM PlayerInformation" +
                                          " WHERE BoggleUsername = '" + user + "'";

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return reader["ID"].ToString();
                        }
                    }

                }
                catch { }
            }

            return null;
        }
    }
}

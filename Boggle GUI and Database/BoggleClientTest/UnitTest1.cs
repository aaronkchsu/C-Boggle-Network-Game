using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;
using System.Net.Sockets;
using System.Net;
using CustomNetworking;
using System.Text;
using System.Threading;
using System.Collections.Generic;


namespace BoggleClientTest
{

    public static class PortClass
    {
        private static int port = 2000;

        public static int getPort()
        {
            return port++;
        }
    }

    [TestClass]
    public class UnitTest1
    {
        
        [TestMethod]
        public void BasicFunctionsTest()
        {
            new BasicClass().run(2000);
        }

        public class BasicClass
        {
            StringSocket communicate;
            TcpListener server;
            BoggleClientModel client;
            public void run(int port)
            {
                client = new BoggleClientModel(); // A Model to test
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                server.BeginAcceptSocket(Work, null);
                client.GameRunning = true; // This allows us to shoot the person otherwise the if statment blocks us
                client.GameStartEvent += GameStart;
                client.DisplaySentWords += WordTest; 
                client.UsernameReceived += UserNameTest;
                client.TimeChange += TimeTest;
                client.UpdateScore += ScoreTest;
                client.ResetGUI += Stop;
                client.FinalScore += Final;
                client.ERROR += error;
                client.CloseGame += error;
                client.Connect("localhost", port, "Noob");
                communicate.BeginSend("START 10 5 seemore\n", (o, e) => { }, this);
                communicate.BeginReceive(SendMore, null);
                Thread.Sleep(3000);
                PrivateObject obj = new PrivateObject(client);
                HashSet<string> temp = (HashSet<string>) obj.GetField("wordsSent");
                Assert.IsTrue(temp.Contains("STEVE"));
                communicate.Close();
                client.ClientSocket.Close();
                server.Stop();
            }

            private void error()
            {
            }

            /// <summary>
            /// The stars are with us in this event once connected fire everything!
            /// </summary>
            /// <param name="hi"></param>
            private void Work(IAsyncResult hi)
            {
                Socket t = server.EndAcceptSocket(hi);
                communicate = new StringSocket(t, UTF8Encoding.Default);
            }

            private void SendMore(string s, Exception e, object p)
            {
                communicate.BeginSend("SCORE 10 8\n", SendMore1, this); // For Narnia
                   
            }

            private void SendMore1(Exception e, object p)
            {
                communicate.BeginSend("TIME 300\n", SendMore2, this);
                    
                    
            }
            private void SendMore2(Exception e, object p)
            {
                client.SendWord("steve");
                communicate.BeginSend("STOP test\n", (ee, pp) => { }, this);
            }

            private void Final(String s)
            {
                Assert.IsTrue(s == "TEST");
            }

            /// <summary>
            /// Let the war of words begin
            /// </summary>
            private void GameStart(string b, string t, string o)
            {
                Assert.AreEqual(b, "10");
                Assert.AreEqual(t, "5");
                Assert.AreEqual(o, "SEEMORE");
            }

            /// <summary>
            /// Only time will tell if we will win this
            /// </summary>
            private void TimeTest(string time)
            {
                Assert.AreEqual(time, "300");
            }

            /// <summary>
            /// Tests scores method from mission control
            /// </summary>
            /// <param name="player"></param>
            /// <param name="opp"></param>
            private void ScoreTest(string player, string opp)
            {
                Assert.AreEqual(player, "10");
                Assert.AreEqual(opp, "8");
            }

            /// <summary>
            /// Some words are better then others
            /// </summary>
            /// <param name="word"></param>
            private void WordTest(string word)
            {
                Assert.AreEqual(word, "STEVE");
            }

            /// <summary>
            /// Now you see me now you don't
            /// </summary>
            /// <param name="user"></param>
            private void UserNameTest(string user)
            {
                Assert.AreEqual(user, "Noob");
            }

            private void Stop()
            {
                Assert.IsTrue(!client.GameRunning);
            }
        }

        //Tests if a game can proceed normally and 
        //the default username is assigned to the player 
        //(since null is passed in for the username).
        [TestMethod]
        public void NullUserTest()
        {
            new NullUser().run(2010);
        }

        public class NullUser
        {
            StringSocket communicate;
            TcpListener server;
            BoggleClientModel client;
            public void run(int port)
            {
                client = new BoggleClientModel(); // A Model to test
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                server.BeginAcceptSocket(Work, null);
                client.GameRunning = true; // This allows us to shoot the person otherwise the if statment blocks us
                client.GameStartEvent += GameStart;
                client.DisplaySentWords += WordTest;
                client.UsernameReceived += UserNameTest;
                client.TimeChange += TimeTest;
                client.UpdateScore += ScoreTest;
                client.ResetGUI += Stop;
                client.FinalScore += Final;
                client.ERROR += error;
                client.CloseGame += error;
                client.Connect("localhost", port, null);
                communicate.BeginSend("START 10 5 seemore\n", (o, e) => { }, this);
                communicate.BeginReceive(SendMore, null);
                Thread.Sleep(3000);
                PrivateObject obj = new PrivateObject(client);
                HashSet<string> temp = (HashSet<string>)obj.GetField("wordsSent");
                Assert.IsTrue(temp.Contains("STEVE"));
                communicate.Close();
                server.Stop();
                client.ClientSocket.Close();

            }

            private void error()
            {
            }

            /// <summary>
            /// The stars are with us in this event once connected fire everything!
            /// </summary>
            /// <param name="hi"></param>
            private void Work(IAsyncResult hi)
            {
                Socket t = server.EndAcceptSocket(hi);
                communicate = new StringSocket(t, UTF8Encoding.Default);
            }

            private void SendMore(string s, Exception e, object p)
            {
                communicate.BeginSend("SCORE 10 8\n", SendMore1, this); // For Narnia

            }

            private void SendMore1(Exception e, object p)
            {
                communicate.BeginSend("TIME 300\n", SendMore2, this);


            }
            private void SendMore2(Exception e, object p)
            {
                client.SendWord("steve");
            }

            private void SendMore3(Exception e, object p)
            {
                communicate.BeginSend("STOP test\n", (ee, pp) => { }, this);
            }

            /// <summary>
            /// Let the war of words begin
            /// </summary>
            private void GameStart(string b, string t, string o)
            {
                Assert.AreEqual(b, "10");
                Assert.AreEqual(t, "5");
                Assert.AreEqual(o, "SEEMORE");
            }

            /// <summary>
            /// Only time will tell if we will win this
            /// </summary>
            private void TimeTest(string time)
            {
                Assert.AreEqual(time, "300");
            }

            /// <summary>
            /// Tests scores method from mission control
            /// </summary>
            /// <param name="player"></param>
            /// <param name="opp"></param>
            private void ScoreTest(string player, string opp)
            {
                Assert.AreEqual(player, "10");
                Assert.AreEqual(opp, "8");
            }

            /// <summary>
            /// Some words are better then others
            /// </summary>
            /// <param name="word"></param>
            private void WordTest(string word)
            {
                Assert.AreEqual(word, "STEVE");
            }

            /// <summary>
            /// Now you see me now you don't
            /// </summary>
            /// <param name="user"></param>
            private void UserNameTest(string user)
            {
                Assert.IsTrue(user.StartsWith("Player"));
                //Assert.AreEqual(user, "Noob");
            }

            private void Stop()
            {
                Assert.IsTrue(!client.GameRunning);
            }

            private void Final(String s)
            {
                Assert.IsTrue(s == "TEST");
            }
        }


        [TestMethod]
        public void GameTerminateTest()
        {
            new TerminatedGame().run(2100);
        }

        public class TerminatedGame
        {
            StringSocket communicate;
            TcpListener server;
            BoggleClientModel client;
            public void run(int port)
            {
                client = new BoggleClientModel(); // A Model to test
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                server.BeginAcceptSocket(Work, null);
                client.GameRunning = true; // This allows us to shoot the person otherwise the if statment blocks us
                client.GameStartEvent += GameStart;
                client.DisplaySentWords += WordTest;
                client.UsernameReceived += UserNameTest;
                client.TimeChange += TimeTest;
                client.UpdateScore += ScoreTest;
                client.ResetGUI += Stop;
                client.FinalScore += Final;
                client.ERROR += error;
                client.CloseGame += error;
                client.Connect("localhost", port, "Noob");
                communicate.BeginSend("START 10 5 seemore\n", (o, e) => { }, this);
                communicate.BeginReceive(SendMore, null);
                Thread.Sleep(2000);
                PrivateObject obj = new PrivateObject(client);
                HashSet<string> temp = (HashSet<string>)obj.GetField("wordsSent");
                Assert.IsTrue(temp.Contains("STEVE"));
                Thread.Sleep(2000);
                communicate.Close();
                server.Stop();
                client.ClientSocket.Close();

            }

            private void error()
            {
            }

            /// <summary>
            /// The stars are with us in this event once connected fire everything!
            /// </summary>
            /// <param name="hi"></param>
            private void Work(IAsyncResult hi)
            {
                Socket t = server.EndAcceptSocket(hi);
                communicate = new StringSocket(t, UTF8Encoding.Default);
            }

            private void SendMore(string s, Exception e, object p)
            {
                communicate.BeginSend("SCORE 10 8\n", SendMore1, this); // For Narnia

            }

            private void SendMore1(Exception e, object p)
            {
                communicate.BeginSend("TIME 300\n", SendMore2, this);


            }
            private void SendMore2(Exception e, object p)
            {
                client.SendWord("steve");
                communicate.BeginSend("TERMINATED", (ee, pp) => { }, this);
            }

            /// <summary>
            /// Let the war of words begin
            /// </summary>
            private void GameStart(string b, string t, string o)
            {
                Assert.AreEqual(b, "10");
                Assert.AreEqual(t, "5");
                Assert.AreEqual(o, "SEEMORE");
            }

            /// <summary>
            /// Only time will tell if we will win this
            /// </summary>
            private void TimeTest(string time)
            {
                Assert.AreEqual(time, "300");
            }

            /// <summary>
            /// Tests scores method from mission control
            /// </summary>
            /// <param name="player"></param>
            /// <param name="opp"></param>
            private void ScoreTest(string player, string opp)
            {
                Assert.AreEqual(player, "10");
                Assert.AreEqual(opp, "8");
            }

            /// <summary>
            /// Some words are better then others
            /// </summary>
            /// <param name="word"></param>
            private void WordTest(string word)
            {
                Assert.AreEqual(word, "STEVE");
            }

            /// <summary>
            /// Now you see me now you don't
            /// </summary>
            /// <param name="user"></param>
            private void UserNameTest(string user)
            {
                Assert.AreEqual(user, "Noob");
            }

            private void Stop()
            {
                Assert.IsTrue(!client.GameRunning);
            }

            private void Final(String s)
            {
                Assert.IsTrue(s == "TEST");
            }
        }

        //[TestMethod]
        //public void MultipleConnect()
        //{
        //    new ErrorClass().run1(2157);
        //}

        public class ErrorClass
        {
            StringSocket communicate;
            TcpListener server;
            BoggleClientModel client;
            public void run1(int port)
            {
                client = new BoggleClientModel(); // A Model to test
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                server.BeginAcceptSocket(Work, null);
                client.GameRunning = true; // This allows us to shoot the person otherwise the if statment blocks us
                client.GameStartEvent += GameStart;
                client.DisplaySentWords += WordTest;
                client.UsernameReceived += UserNameTest;
                client.TimeChange += TimeTest;
                client.UpdateScore += ScoreTest;
                client.ResetGUI += Stop;
                client.FinalScore += Final;
                client.ERROR += error;
                client.CloseGame += error;
                client.Connect("localhost", port, "Noob");
                client.Connect("localhost", port + 1, "Noob");
                communicate.BeginSend("START 10 5 seemore\n", (o, e) => { }, this);
                communicate.BeginReceive(SendMore, null);
                Thread.Sleep(2000);
                PrivateObject obj = new PrivateObject(client);
                HashSet<string> temp = (HashSet<string>)obj.GetField("wordsSent");
                Assert.IsTrue(temp.Contains("STEVE"));
                Thread.Sleep(2000);
                communicate.Close();
                server.Stop();
                client.ClientSocket.Close();
            }

            private void error()
            {
            }

            /// <summary>
            /// The stars are with us in this event once connected fire everything!
            /// </summary>
            /// <param name="hi"></param>
            private void Work(IAsyncResult hi)
            {
                Socket t = server.EndAcceptSocket(hi);
                communicate = new StringSocket(t, UTF8Encoding.Default);
            }

            private void SendMore(string s, Exception e, object p)
            {
                communicate.BeginSend("SCORE 10 8\n", SendMore1, this); // For Narnia

            }

            private void SendMore1(Exception e, object p)
            {
                communicate.BeginSend("TIME 300\n", SendMore2, this);


            }
            private void SendMore2(Exception e, object p)
            {
                client.SendWord("steve");
                communicate.BeginSend("TERMINATED", (ee, pp) => { }, this);
            }

            /// <summary>
            /// Let the war of words begin
            /// </summary>
            private void GameStart(string b, string t, string o)
            {
                Assert.AreEqual(b, "10");
                Assert.AreEqual(t, "5");
                Assert.AreEqual(o, "SEEMORE");
            }

            /// <summary>
            /// Only time will tell if we will win this
            /// </summary>
            private void TimeTest(string time)
            {
                Assert.AreEqual(time, "300");
            }

            /// <summary>
            /// Tests scores method from mission control
            /// </summary>
            /// <param name="player"></param>
            /// <param name="opp"></param>
            private void ScoreTest(string player, string opp)
            {
                Assert.AreEqual(player, "10");
                Assert.AreEqual(opp, "8");
            }

            /// <summary>
            /// Some words are better then others
            /// </summary>
            /// <param name="word"></param>
            private void WordTest(string word)
            {
                Assert.AreEqual(word, "STEVE");
            }

            /// <summary>
            /// Now you see me now you don't
            /// </summary>
            /// <param name="user"></param>
            private void UserNameTest(string user)
            {
                Assert.AreEqual(user, "Noob");
            }

            private void Stop()
            {
                Assert.IsTrue(!client.GameRunning);
            }

            private void Final(String s)
            {
                Assert.IsTrue(s == "TEST");
            }
        }
    }

}

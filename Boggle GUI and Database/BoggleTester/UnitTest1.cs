using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BoggleServer;
using System.Net.Sockets;
using System.Text;
using CustomNetworking;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace ServerTest
{
    [TestClass]
    public class UnitTest1
    {
        private BogServer server;
        private BogClient client1;
        private BogClient client2;

        [TestInitialize]
        public void init()
        {
        }

        [TestCleanup]
        public void close()
        {
        }

        /// <summary>
        /// Test that invalid commands are returned with the appropriate message.
        /// </summary>
        [TestMethod]
        public void invalidWordTest()
        {
            server = new BogServer(2000, new string[] { "5", "boggle.txt", "AAAAAAAAAAAAAAAA" });
            client1 = new BogClient("USER1");
            client2 = new BogClient("USER2");
            client1.Connect("localhost", 2000);
            client2.Connect("localhost", 2000);
            Thread.Sleep(1000);

            client1.sendCommand("WORD @\n");
            client2.sendCommand("WORD @\n");

            Thread.Sleep(5000);
           
            
            Assert.AreEqual(true, client1.linesReceived.Contains("STOP 0  0  0  1 @ 1 @\r"));
            Assert.AreEqual(true, client2.linesReceived.Contains("STOP 0  0  0  1 @ 1 @\r"));

            client1.Close();
            client2.Close(); 
        }

        [TestMethod]
        public void BadCommand()
        {
            server = new BogServer(2025, new string[] { "5", "boggle.txt", "AAAAAAAAAAAAAAAA" });
            client1 = new BogClient("USER1");
            client2 = new BogClient("USER2");
            client1.Connect("localhost", 2025);
            client2.Connect("localhost", 2025);
            Thread.Sleep(1000);

            client1.sendCommand("INVALID\n");
            client2.sendCommand("INVALID\n");

            Thread.Sleep(5000);


            Assert.AreEqual(true, client1.linesReceived.Contains("IGNORING: INVALID\r"));
            Assert.AreEqual(true, client2.linesReceived.Contains("IGNORING: INVALID\r"));

            client1.Close();
            client2.Close();
        }

        [TestMethod]
        public void NotPlay()
        {
            server = new BogServer(2089, new string[] { "5", "boggle.txt", "AAAAAAAAAAAAAAAA" });
            client1 = new BogClient("USER1");
            client2 = new BogClient("USER2");
            client1.ConnectFail("localhost", 2089);
            client2.ConnectFail("localhost", 2089);

            Thread.Sleep(2000);

            Assert.AreEqual(true, client1.linesReceived.Contains("IGNORING: FAIL\r"));
            Assert.AreEqual(true, client2.linesReceived.Contains("IGNORING: FAIL\r"));
            
            client1.Close();
            client2.Close();
        }

        [TestMethod]
        public void emptyWordTest()
        {
            server = new BogServer(2015, new string[] { "5", "boggle.txt", "AAAAAAAAAAAAAAAA" });
            client1 = new BogClient("USER1");
            client2 = new BogClient("USER2");
            client1.Connect("localhost", 2015);
            client2.Connect("localhost", 2015);
            Thread.Sleep(1000);

            client1.sendCommand("WORD \n");
            client2.sendCommand("WORD \n");

            Thread.Sleep(5000);

            Assert.AreEqual(true, client1.linesReceived.Contains("STOP 0  0  0  0  0 \r"));
            Assert.AreEqual(true, client2.linesReceived.Contains("STOP 0  0  0  0  0 \r"));

            client1.Close();
            client2.Close();
        }

        [TestMethod]
        public void emptyStringTest()
        {
            server = new BogServer(2010, new string[] { "5", "boggle.txt", "AAAAAAAAAAAAAAAA" });
            client1 = new BogClient("USER1");
            client2 = new BogClient("USER2");
            client1.Connect("localhost", 2010);
            client2.Connect("localhost", 2010);
            Thread.Sleep(1000);

            client1.sendCommand("\n");
            client2.sendCommand("\n");

            Thread.Sleep(5000);


            Assert.AreEqual(true, client1.linesReceived.Contains("IGNORING: empty command\r"));
            Assert.AreEqual(true, client2.linesReceived.Contains("IGNORING: empty command\r"));

            client1.Close();
            client2.Close();
        }

        [TestMethod]
        public void legitimateGame()
        {
            server = new BogServer(2001, new string[] { "4", "boggle.txt", "AAAAAAAAAAAAAAAA" });
            client1 = new BogClient("USER1");
            client2 = new BogClient("USER2");
            client1.Connect("localhost", 2001);
            client2.Connect("localhost", 2001);

            Thread.Sleep(1000);

            client1.sendCommand("WORD AAAA\n");


            Thread.Sleep(10000);

            Assert.AreEqual(true, client1.linesReceived.Contains("STOP 1 AAAA 0  0  0  0 \r"));


            client1.Close();
            client2.Close();
        }

    

        /// <summary>
        /// This test should not result in an exception.
        /// </summary>
        [TestMethod]
        public void properArgs()
        {
            server = new BogServer(2005, new string[] { "1", "boggle.txt", "AAAAAAAAAAAAAAAA" });
            client1 = new BogClient("USER1");
            client1.Connect("localhost", 2005);
            client1.Close();
        }

        /// <summary>
        /// The following should not result in an exception, in addition to sending TERMINATED.
        /// </summary>
        [TestMethod]
        public void prematureClosing()
        {
            server = new BogServer(3423, new string[] { "5", "boggle.txt", "AAAAAAAAAAAAAAAA" });
            client1 = new BogClient("USER1");
            client1.Connect("localhost", 3423);
            client2 = new BogClient("USER2");
            client2.Connect("localhost", 3423);

            Thread.Sleep(1000);
            client2.sendCommand("WORD AAAA\n");
            Thread.Sleep(6000);
            Assert.IsTrue(client2.linesReceived.Contains("TERMINATED"));
        }

        [TestMethod]
        public void improperArgs1()
        {
            new Thread(o => {
                BogServer.Main(new string[] { "5", "boggle.txt", "sdfadsf" });
                client1 = new BogClient("user");
                client1.Connect("localhost", 2000);
            });
        }

        [TestMethod]
        public void improperArgs2()
        {
            new Thread(o =>
            {
                BogServer.Main(new string[] { "5" });
                client1 = new BogClient("user");
                client1.Connect("localhost", 2000);
            });
        }


        public class BogClient
        {

            private StringSocket socket;
            public String username;
            private BogServer server;
            private TcpClient client;
            public String lineReceived { get; set; }
            public HashSet<string> linesReceived { get; set; }

            public BogClient(BogServer serv, String user)
            {
                socket = null;
                username = user;
                server = serv;
                linesReceived = new HashSet<string>();
            }

            public BogClient(String user)
            {
                socket = null;
                username = user;
                linesReceived = new HashSet<string>();

            }

            /// <summary>
            /// Lets connect to a spot
            /// </summary>
            public void Connect(string hostname, int port)
            {
                if (socket == null)
                {
                    client = new TcpClient(hostname, port);
                    socket = new StringSocket(client.Client, UTF8Encoding.Default);
                    socket.BeginSend("PLAY " + username + "\n", (e, p) => { }, socket);
                    socket.BeginReceive(CommandRecieved, null);
                }
            }

            /// <summary>
            /// Lets connect fail to a spot
            /// </summary>
            public void ConnectFail(string hostname, int port)
            {
                if (socket == null)
                {
                    client = new TcpClient(hostname, port);
                    socket = new StringSocket(client.Client, UTF8Encoding.Default);
                    socket.BeginSend("FAIL\n", (e, p) => { }, socket);
                    socket.BeginReceive(CommandRecieved, null);
                }
            }

            /// <summary>
            /// Lets send a command
            /// </summary>
            public void sendCommand(String command)
            {
                socket.BeginSend(command, (e, p) => { }, null);
            }


            /// <summary>
            /// Deal with an arriving line of text.
            /// </summary>
            private void CommandRecieved(String s, Exception e, object p)
            {
                lineReceived = s;
                lock (this)
                {
                    linesReceived.Add(s);
                }
                socket.BeginReceive(CommandRecieved, null);
            }

            /// <summary>
            /// Checks to see what the scores are the client are
            /// </summary>
            /// <param name="expected"></param>
            public void checkScore(int expected)
            {
                PrivateObject obj = new PrivateObject(server);
                Dictionary<String, BoggleServer.BogServer.Player> temp = (Dictionary<String, BoggleServer.BogServer.Player>) obj.GetField("stringToPlayer");
                Assert.AreEqual(expected, temp[username].score);
                this.Close();
            }

            /// <summary>
            /// Checks the variables inside the server
            /// </summary>
            /// <param name="expected"></param>
            public void checkVariables(string expected)
            {
                PrivateObject obj = new PrivateObject(server);
                String actual = (string)obj.GetField("legalWords");
                Assert.AreEqual(expected, actual);
                this.Close();
            }

            public void Close()
            {
                socket.Close();
                client.Close();
            }



        }
    }
}

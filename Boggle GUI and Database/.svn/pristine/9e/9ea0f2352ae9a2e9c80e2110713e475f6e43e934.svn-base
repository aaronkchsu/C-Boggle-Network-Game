using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BoggleServer;
using BoggleGUI;

namespace BoggleLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            //BogServer server = new BogServer(2000);
            new Thread(() => BoggleGUI.App.Main()).Start();
            //new Thread(() => BoggleGUI.App.Main()).Start();

           // new ChatServer(5000);
           // new Thread(() => ChatClientView.Main()).Start();
            //new Thread(() => ChatClientView.Main()).Start();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TwitchRecoverCs.core.Enums;
using TwitchRecoverCs.core;

namespace TwitchRecoverGui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Console.WriteLine("Ready");
        }

        private void download()
        {
            Console.WriteLine("\nVOD downloading:");
            string url = textBox1.Text;
            VOD vod = new VOD(false);
            vod.retrieveID(url);
            Feeds feeds = vod.getVODFeeds();

            Console.WriteLine(feeds);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            download();

        }
    }
}

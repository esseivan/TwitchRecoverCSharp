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
using TwitchRecoverCs.core.Downloader;
using DownloadVOD = TwitchRecoverCs.core.Downloader.Download;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics.Tracing;
using System.Security;
using System.IO;

namespace TwitchRecoverGui
{
    public partial class Form1 : Form
    {
        private Feeds feeds = null;
        private VOD vod = null;
        private int chunkCounter = 0;
        private int chunkMax = 0;
        private float approxChunkSize_MB = 0;
        private int approxTotalSize_MB = 0;

        public Form1()
        {
            InitializeComponent();

            Console.WriteLine("Ready");

            DownloadVOD.DownloadStarted += DownloadVOD_DownloadStarted;
            DownloadVOD.ChunkDownloaded += DownloadVOD_ChunkDownloaded;
        }

        private Feeds GetFeeds()
        {
            Console.WriteLine("\nVOD URL retrieval:");
            string url = textBox1.Text;
            vod = new VOD(false);
            vod.retrieveID(url);
            feeds = vod.getVODFeeds();
            return feeds;
        }

        private async void Download(int index, string filePath)
        {
            Console.WriteLine(
                      "\nPlease enter the FILE PATH of where you want the VOD saved:"
                    + "\nFile path: "
            );

            vod.setFP(filePath);
            Console.WriteLine("\nDownloading...");
            string result = await vod.downloadVOD(feeds.getFeed(index));

            label3.Text = toolStripStatusLabel2.Text = string.Empty;
            toolStripStatusLabel1.Text = "Complete !";
            Console.WriteLine("\nFile downloaded at: " + vod.getFFP());
            if (!string.IsNullOrEmpty(result))
                Process.Start(result);
        }

        private void DownloadVOD_ChunkDownloaded(object sender, int e)
        {
            chunkCounter++;
            toolStripStatusLabel1.Text = string.Format("{0}/{1}", chunkCounter, chunkMax);
            progressBar1.Invoke((MethodInvoker)delegate
            {
                progressBar1.Value = 100 * chunkCounter / chunkMax;
            });

            // Estimate with chunk 001 (not 000 because usually smaller)
            if (e == 1)
            {
                FileInfo fi = new FileInfo(sender.ToString());
                approxChunkSize_MB = fi.Length / (float)(1024 * 1024);
                approxTotalSize_MB = (int)(chunkMax * approxChunkSize_MB);
                label3.Invoke((MethodInvoker)delegate
                {
                    label3.Text = string.Format("Estimation : {0} MB", approxTotalSize_MB);
                });
            } else if (e > 1)
            {
                int approxCurrentSize_MB = (int)(chunkCounter * approxChunkSize_MB);
                statusStrip1.Invoke((MethodInvoker)delegate
                {
                    toolStripStatusLabel2.Text = string.Format("Estimation : {0} MB / {1} MB", approxCurrentSize_MB, approxTotalSize_MB);
                });
            }
        }

        private void DownloadVOD_DownloadStarted(object sender, int e)
        {
            chunkMax = e;
            chunkCounter = 0;
            toolStripStatusLabel1.Text = string.Format("{0}/{1}", chunkCounter, chunkMax);
            progressBar1.Value = 0;
        }

        private void Download_ProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Stopwatch s = new Stopwatch();
            s.Start();

            Cursor = Cursors.WaitCursor;

            GetFeeds();
            if (feeds == null || feeds.GetCount() == 0)
                return;

            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(feeds.getQualities().ToArray());

            Cursor = Cursors.Default;

            Console.WriteLine(s.ElapsedMilliseconds);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (feeds == null || feeds.GetCount() == 0 || comboBox1.SelectedIndex == -1)
                return;

            // Get save path
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Download(comboBox1.SelectedIndex, saveFileDialog1.FileName);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string result = DownloadVOD.m3u8_retryMerge(vod.getFFP());
            if (string.IsNullOrEmpty(result))
                MessageBox.Show("Failed");
            else
            {
                MessageBox.Show("Success");
                Process.Start(result);
            }
        }
    }
}

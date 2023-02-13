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
using System.Reflection;
using System.Security.Policy;
using System.Threading;

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

        private int minChunk = -1;
        private int maxChunk = -1;
        private int chunkCount = 0;

        private bool ContinueMerge = false;

        private CancellationTokenSource cts;

        public Form1()
        {
            InitializeComponent();
            EditCheckbox1State();

            Console.WriteLine("Ready");

            DownloadVOD.DownloadStarted += DownloadVOD_DownloadStarted;
            DownloadVOD.ChunkDownloaded += DownloadVOD_ChunkDownloaded;
        }

        private async void Download(string feed, string filePath)
        {
            Console.WriteLine(
                      "\nPlease enter the FILE PATH of where you want the VOD saved:"
                    + "\nFile path: "
            );

            if (vod == null)
                vod = new VOD(false);

            vod.setFP(filePath);
            Console.WriteLine("\nDownloading...");
            cts = new CancellationTokenSource();
            if (!(minChunk == -1 && maxChunk == -1))
                Console.WriteLine(string.Format("Downloading from chunk {0} to {1}", minChunk, maxChunk));

            string result = await vod.downloadVOD(feed, minChunk, maxChunk, cts.Token);
            if (cts.IsCancellationRequested)
            {
                if (!ContinueMerge)
                    return;
                // Continue the merge although it was cancelled
                ContinueMerge = false;
            }

            if (string.IsNullOrEmpty(result))
            {
                MessageBox.Show("Download cancelled");
                return;
            }

            label3.Text = toolStripStatusLabel2.Text = string.Empty;
            toolStripStatusLabel1.Text = "Complete !";
            Console.WriteLine("\nFile downloaded at: " + vod.getFFP());
            if (!string.IsNullOrEmpty(result))
                Process.Start(result);
        }

        private void DownloadVOD_ChunkDownloaded(object sender, int e)
        {
            chunkCounter++;
            toolStripStatusLabel1.Text = string.Format("{0}/{1} chunks downloaded", chunkCounter, chunkMax);
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
            }
            else if (e > 1)
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

        private bool GetActiveVODFeeds(string url)
        {
            Console.WriteLine("\nVOD URL retrieval:");
            vod = new VOD(false);
            vod.retrieveID(url);
            feeds = vod.getVODFeeds();

            if (feeds == null || feeds.GetCount() == 0)
                return false;
            return true;
        }

        private bool GetDeletedVODFeeds(string url)
        {
            Console.WriteLine("\nVOD URL recovery:");
            vod = new VOD(true);

            vod.retrieveVODURL(url);
            vod.retrieveVOD(false);
            feeds = vod.retrieveVODFeeds();

            if (feeds == null || feeds.GetCount() == 0)
                return false;
            return true;
        }

        private void GetVOD(string url)
        {
            bool success = false;

            try
            {
                success = GetActiveVODFeeds(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show("Failed. Link invalid or VOD deleted", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (success) return;

            //try
            //{
            //    // Need OAuth
            //    //success = GetDeletedVODFeeds(url);
            //}
            //catch (Exception) { }
            //if (success) return;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Enter the url to the VOD", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Stopwatch s = new Stopwatch();
            s.Start();
            Cursor = Cursors.WaitCursor;

            comboBox1.Items.Clear();
            comboBox1.ResetText();
            textBox2.ResetText();
            GetVOD(textBox1.Text);
            if (feeds != null && feeds.GetCount() != 0)
                comboBox1.Items.AddRange(feeds.getQualities().ToArray());

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
            Cursor = Cursors.Default;
            Console.WriteLine(s.ElapsedMilliseconds);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox2.Text))
                return;

            List<string> chunks = M3U8Handler.getChunks(textBox2.Text);
            chunkCount = chunks.Count;

            if (checkBox1.Checked)
            {
                if (!int.TryParse(textBox3.Text, out minChunk))
                {
                    MessageBox.Show("Starting chunk invalid. Enter an integer value >= 1", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!int.TryParse(textBox4.Text, out maxChunk))
                {
                    MessageBox.Show("Ending chunk invalid. Enter an integer value >= 1", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (minChunk < 1)
                    minChunk = 1;
                if (maxChunk > chunkCount)
                    maxChunk = chunkCount;
                if (minChunk > maxChunk) // Invalid values
                {
                    MessageBox.Show("Ending chunk must be greater or equal to the starting chunk", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                minChunk = maxChunk = -1;
            }

            // Get save path
            saveFileDialog1.FileName = string.Format("VOD_{0}_{1}.mp4", vod.ChannelName, vod.ID);
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Download(textBox2.Text, saveFileDialog1.FileName);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string result = DownloadVOD.m3u8_retryMerge(vod.getFFP());
            if (result == null)
                return;

            if (string.IsNullOrEmpty(result))
                MessageBox.Show("Failed");
            else
            {
                MessageBox.Show("Success");
                Process.Start(result);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
                textBox2.Text = string.Empty;
            else
                textBox2.Text = feeds.getFeed(comboBox1.SelectedIndex);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox2.Text))
                Process.Start("vlc", textBox2.Text);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Ask confirmation if download in progress
            // and Cancel all tasks if asked
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (cts == null)
                return;
            if (cts.IsCancellationRequested)
                return;
            cts.Cancel();
            progressBar1.Value = 0;
            toolStripStatusLabel2.Text += " - Cancelled !";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // Cancel the task and set the flag to still merge
            if (cts == null)
                return;
            if (cts.IsCancellationRequested)
                return;

            ContinueMerge = true;
            cts.Cancel();
            progressBar1.Value = 0;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("No feed selected !", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<string> chunks = M3U8Handler.getChunks(textBox2.Text);
            chunkCount = chunks.Count;
            // Each chunk is ~ 10 second
            TimeSpan duration = TimeSpan.FromSeconds(chunkCount * 10);
            label9.Text = duration.ToString("hh':'mm':'ss");
            // Clear starttime
            dateTimePicker1.Value = new DateTime(2000, 01, 01);
            dateTimePicker3.Value = new DateTime(2000, 01, 01) + duration;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            label9.ResetText();
            chunkCount = 0;
        }

        private bool ScriptEdit = false;

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (ScriptEdit) return;

            ScriptEdit = true;
            if (int.TryParse(textBox3.Text, out int sc) && sc >= 0)
            {
                if (sc == 0)
                {
                    sc = 1;
                    textBox3.Text = 1.ToString();
                }
                dateTimePicker1.Value = new DateTime(2000, 01, 01) + TimeSpan.FromSeconds(10 * (sc - 1));
            }
            ScriptEdit = false;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (ScriptEdit) return;

            ScriptEdit = true;
            if (int.TryParse(textBox4.Text, out int ec) && ec > 0)
            {
                dateTimePicker2.Value = new DateTime(2000, 01, 01) + TimeSpan.FromSeconds(10 * ec);
                dateTimePicker3.Value = new DateTime(2000, 01, 01) + (dateTimePicker2.Value - dateTimePicker1.Value);
            }
            ScriptEdit = false;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (ScriptEdit) return;

            ScriptEdit = true;
            DateTime dt = dateTimePicker1.Value;
            TimeSpan st = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
            int chunkCount = (int)Math.Ceiling(st.TotalSeconds / 10) + 1;
            textBox3.Text = chunkCount.ToString();
            ScriptEdit = false;
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            if (ScriptEdit) return;

            ScriptEdit = true;
            DateTime dt = dateTimePicker2.Value;
            TimeSpan et = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
            int chunkCount = (int)Math.Ceiling(et.TotalSeconds / 10);
            textBox4.Text = chunkCount.ToString();
            dateTimePicker3.Value = new DateTime(2000, 01, 01) + (dateTimePicker2.Value - dateTimePicker1.Value);
            ScriptEdit = false;
        }

        private void dateTimePicker3_ValueChanged(object sender, EventArgs e)
        {
            if (ScriptEdit) return;

            ScriptEdit = true;
            DateTime dt = dateTimePicker3.Value;
            TimeSpan duration = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
            dateTimePicker2.Value = dateTimePicker1.Value + duration;

            dt = dateTimePicker2.Value;
            TimeSpan et = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
            int chunkCount = (int)Math.Ceiling(et.TotalSeconds / 10);
            textBox4.Text = chunkCount.ToString();
            ScriptEdit = false;
        }

        private void EditCheckbox1State()
        {
            bool isChecked = checkBox1.Checked;
            dateTimePicker1.Enabled = dateTimePicker2.Enabled = dateTimePicker3.Enabled = isChecked;
            textBox3.Enabled = textBox4.Enabled = isChecked;
            label6.Enabled = label7.Enabled = label8.Enabled = label10.Enabled = label11.Enabled = isChecked;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            EditCheckbox1State();
        }
    }
}

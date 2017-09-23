using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi.Session;

namespace VolumeMixer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            port1 = new SerialPort("COM8", 9600);
            port1.Open();
            defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
            label1.Text = defaultPlaybackDevice.SessionController.ToArray()[actual].DisplayName;
            progressBar1.Value = (int)defaultPlaybackDevice.SessionController.ToArray()[actual].Volume;
            try
            {
                pictureBox1.Image = Icon.ExtractAssociatedIcon(defaultPlaybackDevice.SessionController.ToArray()[actual].ExecutablePath).ToBitmap();
                pictureBox1.Load();
            }
            catch { }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            string entradaS = port1.ReadLine();
            port1.DiscardOutBuffer();
            port1.DiscardInBuffer();
            int entrada = 0;
            bool shaEntrat = false;
            try
            {
                entrada = Int32.Parse(entradaS);
                shaEntrat = true;
            }
            catch { }
            if (shaEntrat)
            {
                bool upButton = false, downButton = false, switchButton = false;
                if (entrada >= 4) { downButton = true; entrada -= 4; }
                if (entrada >= 2) { upButton = true; entrada -= 2; }
                if (entrada >= 1) { switchButton = true; entrada--; }
                if (switchButton && !lastSwitch)
                {
                    actual++;
                    if (actual >= defaultPlaybackDevice.SessionController.ToArray().Length) actual = 0;
                    try
                    {
                        label1.Text = defaultPlaybackDevice.SessionController.ToArray()[actual].DisplayName;
                        progressBar1.Value = (int)defaultPlaybackDevice.SessionController.ToArray()[actual].Volume;
                        pictureBox1.Image = Icon.ExtractAssociatedIcon(defaultPlaybackDevice.SessionController.ToArray()[actual].ExecutablePath).ToBitmap();
                        pictureBox1.Update();
                    }
                    catch { }
                }
                if (upButton && progressBar1.Value < 100) { defaultPlaybackDevice.SessionController.ToArray()[actual].Volume++; progressBar1.Value = (int)defaultPlaybackDevice.SessionController.ToArray()[actual].Volume; }
                if (downButton && progressBar1.Value > 0) { defaultPlaybackDevice.SessionController.ToArray()[actual].Volume--; progressBar1.Value = (int)defaultPlaybackDevice.SessionController.ToArray()[actual].Volume; }
                lastSwitch = switchButton;
            }
        }
        private SerialPort port1;
        private CoreAudioDevice defaultPlaybackDevice;
        private int actual = 0;
        private bool lastSwitch = false;
    }
}
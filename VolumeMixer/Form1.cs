﻿using System;
using System.IO;
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
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Threading;

namespace VolumeMixer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            port1 = new SerialPort("COM17", 115200);
            port1.Open();
            defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
            appActual = defaultPlaybackDevice.SessionController.ToArray()[actual];
            label1.Text = appActual.DisplayName;
            label2.Text = appActual.ExecutablePath;
            progressBar1.Value = (int)appActual.Volume;
            try
            {
                pictureBox1.Image = Icon.ExtractAssociatedIcon(appActual.ExecutablePath).ToBitmap();
                pictureBox1.Load();
            }
            catch { }
        }
        private void enviarMode()
        {
            port1.WriteLine("M");//Mode
            port1.WriteLine(adjustMode ? "SET" : "NOT");
        }
        private void enviarNom()
        {
            port1.WriteLine("N");//NOM                
            port1.WriteLine(label1.Text.Length <= 15 ? label1.Text : label1.Text.Substring(0, 14));
        }
        private void enviarImatge()
        {
            byte[] bitfield = new byte[1];
            bitfield[0] = 0;
            int bitfieldSize = 0;
            port1.DiscardInBuffer();
            port1.WriteLine("I");
            int comptador = 0;
            Bitmap imatge = null;
            if(appActual.ExecutablePath != null)
            {
                imatge = Icon.ExtractAssociatedIcon(appActual.ExecutablePath).ToBitmap();
            }
            else
            {
                imatge = (Bitmap)Bitmap.FromFile("error.bmp");
            }
            /*for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    var pixelTemp = imatge.GetPixel(i, j);
                    int valor = (int)(((double)pixelTemp.R + (double)pixelTemp.G + (double)pixelTemp.B) * ((double)pixelTemp.A / 255.0f));
                    valor /= 3;
                    char pixel = (char)valor;
                    bitfield[0] = (byte)((bitfield[0] << 1) | (pixel > (127 * 3 / 2) ? 1 : 0));
                    //bitfield[0] = (byte)((bitfield[0] << 1) | (j%2==0?0:1)); DEBUG PATTERN
                    bitfieldSize++;
                    if (bitfieldSize == 8)
                    {
                        bitfieldSize = 0;
                        port1.Write(bitfield, 0, 1);
                        string readResult = "";
                        while (readResult != "ACK")
                        {
                            while (port1.BytesToRead == 0) { }
                            readResult = port1.ReadLine();
                        }
                        bitfield[0] = 0;
                    }
                    comptador++;
                }
            }*/
            Bitmap edge = EdgeDetect(imatge);
            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    var pixelTemp = edge.GetPixel(i, j);
                    bitfield[0] = (byte)((bitfield[0] << 1) | (pixelTemp.R == 255 ? 1 : 0));
                    bitfieldSize++;
                    if (bitfieldSize == 8)
                    {
                        bitfieldSize = 0;
                        port1.Write(bitfield, 0, 1);
                        string readResult = "";
                        while (readResult != "ACK")
                        {
                            while (port1.BytesToRead == 0) { }
                            readResult = port1.ReadLine();
                        }
                        bitfield[0] = 0;
                    }
                    comptador++;
                }
            }
        }
        private void enviarDades()
        {
            enviarMode();
            enviarNom();
            enviarImatge();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            string entradaS = port1.ReadLine();
            //LLEGIM DEL ARDUINO
            while (entradaS != "Start")
            {
                entradaS = port1.ReadLine();
            }
            bool setButton = Convert.ToBoolean(Int32.Parse(port1.ReadLine()));
            bool upButton = Convert.ToBoolean(Int32.Parse(port1.ReadLine()));
            bool downButton = Convert.ToBoolean(Int32.Parse(port1.ReadLine()));
            int analogState = Int32.Parse(port1.ReadLine());

            //REACCIONEM A LA INPUT
            if (!connected)
            {
                connected = true;
                enviarDades();
            }
            if (setButton && setButton != lastSet)
            {
                adjustMode = !adjustMode;
                enviarMode();
            }
            if (upButton && !lastUp || downButton && !lastDown) //SI CANVIEM D APP
            {
                adjustMode = false;
                actual = (actual + (upButton?downButton?0:1:downButton?actual==0? defaultPlaybackDevice.SessionController.ToArray().Length -1: -1:0)) % defaultPlaybackDevice.SessionController.ToArray().Length;
                appActual = defaultPlaybackDevice.SessionController.ToArray()[actual];
                try
                {
                    label1.Text = Process.GetProcessById(appActual.ProcessId).MainWindowTitle;
                    Console.WriteLine(appActual.IconPath);
                    Console.WriteLine(appActual.ExecutablePath);
                    label1.Text = (appActual.IconPath.Contains("@%SystemRoot%\\System32\\AudioSrv.Dll")?"System Sounds":label1.Text.Length == 0 ? "----" : label1.Text);
                    label2.Text = appActual.ExecutablePath;
                    progressBar1.Value = (int)appActual.Volume;
                    pictureBox1.Image = null;
                    pictureBox1.Image = EdgeDetect(Icon.ExtractAssociatedIcon(appActual.ExecutablePath).ToBitmap());
                    pictureBox1.Update();
                }
                catch { }
                port1.WriteLine("M");
                port1.WriteLine(adjustMode ? "SET" : "NOT");//Mode
                port1.WriteLine("N");//NOM
                port1.WriteLine(label1.Text.Length <= 15 ? label1.Text : label1.Text.Substring(0, 14));
                //if (label1.Text != "----" && appActual.ExecutablePath != null)
                //{

                    enviarImatge();
                //}
            }
            if (adjustMode)
            {
                if (analogState > 1000) analogState = 1000;
                appActual.Volume = (double)analogState / 10.0f;
                progressBar1.Value = (int)appActual.Volume;
            }
            if (downButton && (int)appActual.Volume > 0) { appActual.Volume--; progressBar1.Value = (int)appActual.Volume; }
            lastUp = upButton;
            lastDown = downButton;
            lastSet = setButton;
        }
        private Bitmap EdgeDetect(Bitmap original)
        {
            int width = original.Width;
            int height = original.Height;

            int BitsPerPixel = Image.GetPixelFormatSize(original.PixelFormat);
            int OneColorBits = BitsPerPixel / 8;

            BitmapData bmpData = original.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, original.PixelFormat);
            int position;
            int[,] gx = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] gy = new int[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };
            int Threshold = 500;

            Bitmap dstBmp = new Bitmap(width, height, original.PixelFormat);
            BitmapData dstData = dstBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, dstBmp.PixelFormat);

            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0.ToPointer();
                byte* dst = (byte*)dstData.Scan0.ToPointer();

                for (int i = 1; i < height - 1; i++)
                {
                    for (int j = 1; j < width - 1; j++)
                    {
                        int NewX = 0, NewY = 0;

                        for (int ii = 0; ii < 3; ii++)
                        {
                            for (int jj = 0; jj < 3; jj++)
                            {
                                int I = i + ii - 1;
                                int J = j + jj - 1;
                                byte Current = *(ptr + (I * width + J) * OneColorBits);
                                NewX += gx[ii, jj] * Current;
                                NewY += gy[ii, jj] * Current;
                            }
                        }
                        position = ((i * width + j) * OneColorBits);
                        if (NewX * NewX + NewY * NewY > Threshold * Threshold)
                        {
                            dst[position] = dst[position + 1] = dst[position + 2] = 255;
                            //Console.Write("1");
                        }
                        else
                        {
                            dst[position] = dst[position + 1] = dst[position + 2] = 0;
                           // Console.Write("0");
                        }
                    }
                    //Console.WriteLine("");
                }
            }
            original.UnlockBits(bmpData);
            dstBmp.UnlockBits(dstData);

            return dstBmp;
        }
        private SerialPort port1;
        private CoreAudioDevice defaultPlaybackDevice;
        private int actual = 0;
        private bool lastSet = false;
        private bool lastUp = false;
        private bool lastDown = false;
        private bool adjustMode = false;
        private IAudioSession appActual;
        private bool connected = false;
    }
}
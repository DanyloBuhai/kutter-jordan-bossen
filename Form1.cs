using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lr2
{
    public partial class Form1 : Form
    {
        Bitmap bitmap;
        public Form1()
        {
            InitializeComponent();
        }
        private byte BitToByte(BitArray scr)
        {
            byte num = 0;
            for (int i = 0; i < scr.Count; i++)
                if (scr[i] == true)
                    num += (byte)Math.Pow(2, i);
            return num;
        }// Функція переведення бітів в байти

        private BitArray ByteToBit(byte src)
        {
            BitArray bitArray = new BitArray(8);
            bool st = false;
            for (int i = 0; i < 8; i++)
            {
                if ((src >> i & 1) == 1) { st = true; }
                else st = false;
                bitArray[i] = st;
            }
            return bitArray;
        }// Функція переведення байтів в масив бітів

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Images (*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|" + "All files (*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    bitmap = new Bitmap(ofd.FileName);
                }
                catch
                {
                    MessageBox.Show("Помилка!");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "") { MessageBox.Show("Введіть повідомлення!"); return; }
            if (bitmap == null) { MessageBox.Show("Завантажте зображення!"); return; };
           
            richTextBox1.Text = "";
            string message = textBox1.Text + "%";
            int charCounter = 0;
            int msgBit = 0;
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            

            for (int x = 1; x < bitmap.Width - 1; x+=3)
            {
                for (int y = 1; y < bitmap.Height - 1; y+=3)
                {
                    if (msgBit >= 8)
                    {
                        charCounter++;
                        msgBit = 0;
                    }
                    if (charCounter >= message.Length) break;
                    var bitMessage = ByteToBit(messageBytes[charCounter]);

                    int B = bitmap.GetPixel(x, y).B;
                    int R = bitmap.GetPixel(x, y).R;
                    int G = bitmap.GetPixel(x, y).G;
                    int Yxy = Convert.ToInt32(0.3 * R + 0.59 * G + 0.11 * B);
                    int newB;
                    if (bitMessage[msgBit]) newB = Convert.ToInt32(B + 0.1 * Yxy);
                    else newB = Convert.ToInt32(B - 0.1 * Yxy);

                    if (newB > 255) newB = 255;
                    if (newB < 0) newB = 0;
                    Color newColor = Color.FromArgb(R, G, newB);
                    bitmap.SetPixel(x, y, newColor);
                    
                    msgBit++;
                    
                }
                if (charCounter >= message.Length) break;
            }//шифрування

            //Збереження зображення з зашифрованою інформацією
            String sFilePic;
            SaveFileDialog dSavePic = new SaveFileDialog();
            dSavePic.Filter = "(*.bmp)|*.bmp|Все файлы (*.*)|*.*";
            if (dSavePic.ShowDialog() == DialogResult.OK)
            {
                sFilePic = dSavePic.FileName;
            }
            else
            {
                sFilePic = ""; return;
            };
            FileStream wFile; try
            {
                wFile = new FileStream(sFilePic, FileMode.Create);
            }
            catch (IOException)
            {
                MessageBox.Show("Помилка відкриття файлу для запису", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            bitmap.Save(wFile, System.Drawing.Imaging.ImageFormat.Bmp);
            wFile.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (bitmap == null) return;
            string letter = "2";
            string ms = "";
            int msgBit = 0;
            var charResBit = ByteToBit(bitmap.GetPixel(1, 1).B);
            for (int x = 1; x < bitmap.Width - 1; x+=3)
            {
                for (int y = 1; y < bitmap.Height - 1; y+=3)
                {
                    int B = bitmap.GetPixel(x, y).B;
                    int Bt, Bb, Br, Bl;
                    Bt = bitmap.GetPixel(x, y - 1).B;
                    Bb = bitmap.GetPixel(x, y + 1).B;
                    Br = bitmap.GetPixel(x + 1, y).B;
                    Bl = bitmap.GetPixel(x - 1, y).B;

                    float Bser = (Br + Bl + Bt + Bb) / 4;
                    if (msgBit >= 8)
                    {
                        int value = BitToByte(charResBit);
                        char c = Convert.ToChar(value);
                        letter = System.Text.Encoding.ASCII.GetString(new byte[] { Convert.ToByte(c) });
                        if (letter == "%") break;
                        ms = ms + letter;
                        msgBit = 0;
                    }
                    charResBit[msgBit] = Bser > B ? false : true;
                    msgBit++;
                }
                if (letter == "%") break;
            }
            richTextBox1.Text += ms;
        }//Розшифровка інформації з зображення
    }
}

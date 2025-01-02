//Bacoro dit Elhadji Lansar
//B211200567
//Bilisim Sistemleri Muhendisligi
//24-25-NDP-Ödev 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ColorfullPuzzle
{
    // Interface pour définir les actions communes
    public interface IFormActions
    {
        //void OpenGameForm(string playerName);
        void OpenHighScores();
        void DisplayInfo();
    }
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public void ShowMessage(string message, string title = "Message", MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
        }

        private void textbox_name_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) // Check if ENTER key is pressed
            {
                if (!string.IsNullOrWhiteSpace(textbox_name.Text))
                {
                    // Create an instance of GameForm
                    GameForm gameForm = new GameForm(textbox_name.Text);


                    // Hide the current form (Form1)
                    this.Hide();

                    // Show the new GameForm
                    gameForm.ShowDialog();

                    // Return to this form after GameForm is closed
                    this.Show();
                }
                else
                {
                    MessageBox.Show("Please enter your name before starting the game.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        public void OpenFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start("notepad.exe", filePath);
                }
                else
                {
                    ShowMessage("File not found.", "Error", MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error opening file: " + ex.Message, "Error", MessageBoxIcon.Error);
            }
        }
    
       public void OpenHighScores()
        {
            string filePath = "D:\\highScores.txt";
            OpenFile(filePath);
        }
        private void HighScore_Click(object sender, EventArgs e)
        {
            OpenHighScores();
        }

        private void infoButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Nesneleri hareket ettirmek için Mouse ile seçiniz ve/veya yön tuşlarını kullanınız. \r\nOyunu durdurmak için P tuşuna basınız.  ", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}


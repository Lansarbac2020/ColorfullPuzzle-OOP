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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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

        private void HighScore_Click(object sender, EventArgs e)
        {
            string filePath = "D:\\highScores.txt";
            try
            {
                if (File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start("notepad.exe", filePath);
                }
                else
                {
                    MessageBox.Show("High scores file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void infoButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Nesneleri hareket ettirmek için Mouse ile seçiniz ve/veya yön tuşlarını kullanınız. \r\nOyunu durdurmak için P tuşuna basınız.  ", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

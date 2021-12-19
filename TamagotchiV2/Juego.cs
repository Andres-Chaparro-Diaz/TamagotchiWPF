using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows.Forms;


namespace TamagotchiV2
{
    public partial class Juego : Form
    {


        MainWindow mw;


        int time = 20;
        int puntuacionPartida = 0;

        public Juego(MainWindow padre)
        {
            InitializeComponent();
            mw = padre;


        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (puntuacionPartida != 30)
            {
                puntuacionPartida += 2;
                label1.Text = "Puntuacion: " + puntuacionPartida;
                // mw.PBDiversion.Value += 10;
                //mw.PBDiversion.Value = this.points;
            }
            else
            {
                mw.sumarPuntos(puntuacionPartida);
                Close();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            if (time == 0)
            {
                timer1.Stop();
                timer2.Stop();
                //MessageBox.Show("Tu puntuación es " + puntuacionPartida);
                mw.sumarPuntos(puntuacionPartida);
                Console.WriteLine(mw.pbJugar.Value);
                Close();


            }
            else
            {
                time = time - 1;
                label2.Text = "tiempo: " + time;
            }


        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            Random r = new Random();
            pictureBox1.Location = new Point(r.Next(700), r.Next(400));
        }


    }
}
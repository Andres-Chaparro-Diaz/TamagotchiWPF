using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


namespace TamagotchiV2
{
    /// <summary> 
    /// Lógica de interacción para MainWindow.xaml 
    /// 
    /// </summary> 
    public partial class MainWindow : Window
    {
        DispatcherTimer t1;
        
        String nombre;
        int puntuacion = 0;
        int contador = 0;
        int decremento = 5;
        int pociones = 0;
        bool cazado = false;
        bool satisfecho = false;
        bool pocion = false;
        bool cazar = false;
        bool duranteAnimacion = false;
        string pathDirectory = Environment.CurrentDirectory.Replace("\\bin\\Debug", "");
        Dictionary<string, Jugador> jugadores;
        Vector j;
        public MainWindow()
        {
            InitializeComponent();
            jugadores = lecturaJson();
            j = new Vector(1, jugadores.Count);
            VentanaBienvenida pantallaInicio = new VentanaBienvenida(this);
            pantallaInicio.ShowDialog();
            t1 = new DispatcherTimer();
            t1.Interval = TimeSpan.FromMilliseconds(1000.0);
            t1.Tick += new EventHandler(reloj);
        }

        private void cambiarFondo(object sender, MouseButtonEventArgs e)
        {
            this.imFondo.Source = ((Image)sender).Source;

        }

        private void eventoDescansar(object sender, RoutedEventArgs e)
        {
            t1.Stop();
            animacion();
            btnDescansar.IsEnabled = false;
            btnComer.IsEnabled = false;
            btnJugar.IsEnabled = false;
            pbDescanso.Value += 15;
            puntuacion += 15;
            Storyboard sbDormir = (Storyboard)this.Resources["animacionDormir"];
            duranteAnimacion = true;
            sbDormir.Completed += new EventHandler(finDescansar);
            sbDormir.Begin();
        }

        private void eventoComer(object sender, RoutedEventArgs e)
        {
            t1.Stop();
            animacion();
            btnDescansar.IsEnabled = false;
            btnComer.IsEnabled = false;
            btnJugar.IsEnabled = false;
            pbComida.Value += 15;
            puntuacion += 15;
            string fullPath_sonidoComer = pathDirectory + "\\comer.wav";
            SoundPlayer s = new SoundPlayer(fullPath_sonidoComer);
            s.Play();
            duranteAnimacion = true;
            Storyboard sbComer = (Storyboard) this.Resources["animacionComer"];
            sbComer.Completed += new EventHandler(finComer);
            sbComer.Begin();
        }

        private void eventoJugar(object sender, RoutedEventArgs e)
        {

            t1.Stop();
            animacion();
            btnDescansar.IsEnabled = false;
            btnComer.IsEnabled = false;
            btnJugar.IsEnabled = false;
            pbJugar.Value += 15;
            puntuacion += 15;
            duranteAnimacion = true;
            Storyboard sbVolar = (Storyboard)this.Resources["animacionVolar"];
            Storyboard sbAlas = (Storyboard)this.Resources["animacionAlas"];
            sbVolar.Completed += new EventHandler(finJugar);
            sbVolar.Begin();
            sbAlas.Begin(); 
        }

        private void finDescansar(object sender, EventArgs e)
        {
            comprobarSatisfecho();
            btnComer.IsEnabled = true;
            btnJugar.IsEnabled = true;
            t1.Start();
            duranteAnimacion = false;
            btnDescansar.IsEnabled = true;  
        }

        private void finComer(object sender, EventArgs e)
        {
            comprobarSatisfecho();
            btnDescansar.IsEnabled = true;
            btnJugar.IsEnabled = true;
            t1.Start();
            duranteAnimacion = false;
            btnComer.IsEnabled = true;

        }

        private void finJugar(object sender, EventArgs e)
        {
            comprobarSatisfecho();
            btnDescansar.IsEnabled = true;
            btnComer.IsEnabled = true;
            t1.Start();
            duranteAnimacion = false;
            btnJugar.IsEnabled = true;
        }
        private void finMorir(object sender, EventArgs e)
        {
            MessageBoxResult resultado = MessageBox.Show("Ha muerto.\nGame Over\nPuntuación: "+puntuacion+"\n ¿Desea Salir?", "IPO2 Tamagotchi", MessageBoxButton.YesNo);
            switch (resultado)
            {
                case MessageBoxResult.Yes:
                    this.Close();
                    break;
            }
            tbMensajes.Text = "Fin de partida, Puntuacion final: " + puntuacion;
        }
        private void animacion()
        {
            pensamiento.Visibility = Visibility.Hidden;
            imHambre.Visibility = Visibility.Hidden;
            imNodescanso.Visibility = Visibility.Hidden;
            imAburrimiento.Visibility = Visibility.Hidden;
        }

        private void reloj(object sender, EventArgs e)
        {

            this.pbDescanso.Value -= decremento;
            this.pbComida.Value -= decremento;
            this.pbJugar.Value -= decremento;
            decremento++;
            contador++;
            if (cazar)
            {
                if (!duranteAnimacion)
                {
                    t1.Stop();
                    imColCaza.Visibility = Visibility.Hidden;
                    imColCaza.IsEnabled = false;
                    cazar = false;
                    Juego j = new Juego(this);
                    j.Show();
                }
            }
            if(pociones >= 5 && !pocion)
            {
                ChkBAlquimista.IsChecked = true;
                imColFlor.IsEnabled = true;
                imColFlor.Visibility = Visibility.Visible;
                pocion = true;
                tbMensajes.Text = "Enhorabuena, Logro obtenido!";
            }
            if (cazado)
            {
                ChkBCazado.IsChecked = true;
                imColCaza.IsEnabled = true;
                imColCaza.Visibility = Visibility.Visible;
                cazado = false;
            }

            if (contador.ToString().Contains("0"))
            {
                Storyboard sbPuntos = (Storyboard)this.Resources["animacionPuntos"];
                sbPuntos.Begin();
                if (!imColPocion.IsEnabled)
                {
                    imColPocion.IsEnabled = true;
                    imColPocion.Visibility = Visibility.Visible;
                    imColPocion.AllowDrop = true;
                }
            }
            comprobarEstados();

        }
        public void comprobarSatisfecho()
        {
            if (pbComida.Value == 100 && pbDescanso.Value == 100 && pbJugar.Value == 100 && !satisfecho)
            {
                ChkBSatisfecho.IsChecked = true;
                if (!imColGorro.IsEnabled)
                {
                    imColGorro.Visibility = Visibility.Visible;
                    imColGorro.IsEnabled = true;
                    imColGorro.AllowDrop = true;
                    satisfecho = true;
                    puntuacion += 50;
                    tbMensajes.Text = "Enhorabuena, Logro obtenido!";
                }
                puntuacion += 50;
            }
        }
        public void comprobarEstados()
        {
            if (pbDescanso.Value <= 0 && pbJugar.Value <= 0 && pbComida.Value <= 0)
            {
                escrituraJson();
                
                t1.Stop();
                btnComer.IsEnabled = false;
                btnDescansar.IsEnabled = false;
                btnJugar.IsEnabled = false;
                imColPocion.IsEnabled = false;
                imColCaza.IsEnabled = false;
                Storyboard sbMorir = (Storyboard)this.Resources["animacionMorir"];
                sbMorir.Completed += new EventHandler(finMorir);
                sbMorir.Begin();

            }
            else if (pbDescanso.Value <= 0 || pbJugar.Value <= 0 || pbComida.Value <= 0)
            {
                pensamiento.Visibility = Visibility.Visible;
                if (pbComida.Value <= 0 && imAburrimiento.Visibility == Visibility.Hidden && imNodescanso.Visibility == Visibility.Hidden)
                {
                    imHambre.Visibility = Visibility.Visible;
                }
                else
                {
                    imHambre.Visibility = Visibility.Hidden;
                }

                if (pbDescanso.Value <= 0 && imAburrimiento.Visibility == Visibility.Hidden && imHambre.Visibility == Visibility.Hidden)
                {
                    imNodescanso.Visibility = Visibility.Visible;
                }
                else
                {
                    imNodescanso.Visibility = Visibility.Hidden;
                }

                if (pbJugar.Value <= 0 && imNodescanso.Visibility == Visibility.Hidden && imHambre.Visibility == Visibility.Hidden)
                {
                    imAburrimiento.Visibility = Visibility.Visible;
                }
                else
                {
                    imAburrimiento.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                pensamiento.Visibility = Visibility.Hidden;
            }


        }

        private void acercaDe(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult resultado = MessageBox.Show("Práctica realizada por:\nAndrés Chaparro\n\n ¿Desea Salir?", "IPO2 Tamagotchi", MessageBoxButton.YesNo);
            switch (resultado)
            {
                case MessageBoxResult.Yes:
                    this.Close();
                    break;

            }
        }

        public void setNombre (string nombre)
        {
            this.nombre = nombre;
            tbMensajes.Text = "Bienvenido " + nombre;
        }

        public void sumarPuntos(int puntos)
        {
            puntuacion += puntos;
            pbJugar.Value += puntos;
            tbMensajes.Text = "Enhorabuena, ha conseguido un total de " + puntos + " puntos en el juego de la caza!";
            if (puntos >= 30)
            {
                ChkBCazador.IsChecked = true;
                tbMensajes.Text = "Enhorabuena, ha conseguido un total de " + puntos + " puntos en el juego de la caza! Logro obtenido!";
                imColGafas.Visibility = Visibility.Visible;
                imColGafas.IsEnabled = true;
            }
            else
            {
                tbMensajes.Text = "Enhorabuena, ha conseguido un total de " + puntos + " puntos en el juego de la caza!";
            }
            t1.Start();
        }

        private void inicioArrastrar(object sender, MouseButtonEventArgs e)
        {
            DataObject dobj = new DataObject(((Image)sender));
            DragDrop.DoDragDrop((Image)sender, dobj, DragDropEffects.Move);
        }

        private void colocarColeccionable(object sender, DragEventArgs e)
        {
            Image aux = (Image)e.Data.GetData(typeof(Image));
            switch (aux.Name)
            {
                case "imColGorro":
                    imGorro.Visibility = Visibility.Visible;
                    break;
                case "imColGafas":
                    imGafas.Visibility = Visibility.Visible;
                    break;
                case "imColFlor":
                    imFlor.Visibility = Visibility.Visible;
                    break;
                case "imColPocion":
                    pbComida.Value += 40;
                    pbDescanso.Value += 40;
                    pbJugar.Value += 40;
                    imColPocion.Visibility = Visibility.Hidden;
                    imColPocion.IsEnabled = false;
                    pociones++;
                    break;
            }
        }

        private void retirarColeccionable(object sender, DragEventArgs e)
        {
            Image aux = (Image)e.Data.GetData(typeof(Image));
            switch (aux.Name)
            {
                case "imGorro":
                    imGorro.Visibility = Visibility.Hidden;
                    break;
                case "imGafas":
                    imGafas.Visibility = Visibility.Hidden;
                    break;
                case "imFlor":
                    imFlor.Visibility = Visibility.Hidden;
                    break;
            }
        }

        private void pulsarPuntos(object sender, MouseButtonEventArgs e)
        {
            this.pbComida.Value = 100;
            this.pbDescanso.Value = 100;
            this.pbJugar.Value = 100;
            puntuacion += 300;
            tbMensajes.Text = "Has cazado al Doraemon de la Suerte, Logro obtenido!";
            cazado = true;

        }

        private void inicioCaza(object sender, MouseButtonEventArgs e)
        {
            cazar = true;
        }

        public Dictionary<string, Jugador> lecturaJson()
        {
            string jsonString = File.ReadAllText(pathDirectory + "\\jugadores.json");
            var objeto = Newtonsoft.Json.JsonConvert.DeserializeObject<TxtJson>(jsonString);
            Dictionary<string, Jugador> jugadores= objeto.jugadores;
            foreach (var item in jugadores.Values)
            {
                txtBrank.AppendText("Nombre: " + item.nombre + "; Puntos:" + item.puntos + "\n");
               
            }
            return jugadores;
        }

        public void escrituraJson()
        {
            puntuacion = puntuacion + contador * 2;
            string jsonString = File.ReadAllText(pathDirectory + "\\jugadores.json");
            JObject objeto = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString) as JObject;
            JObject channel = (JObject)objeto["jugadores"];
            Jugador jugador = new Jugador();
            jugador.nombre = nombre;
            jugador.puntos = puntuacion;
            JObject jugadorAux = new JObject() ;
            jugadorAux.Add("nombre", nombre);
            jugadorAux.Add("puntos", puntuacion);
            channel.Add((jugadores.Count() + 1) + "", jugadorAux);
            string updatedJsonString = objeto.ToString();
            File.WriteAllText(pathDirectory + "\\jugadores.json", updatedJsonString);
        }
    }
    public class TxtJson
    {
        public Dictionary<string, Jugador> jugadores
        {
            get;
            set;
        }
    }
    public class Jugador
    {
        public string nombre
        {
            get;
            set;
        }
        public int puntos
        {
            get;
            set;
        }
    }
}

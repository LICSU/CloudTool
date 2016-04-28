using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using Microsoft.Win32;
using System.IO;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Fusion;
using System.Globalization;
using System.Diagnostics;
using HelixToolkit.Wpf;
using System.ComponentModel;
using System.Xml.Serialization;
using HumanVolumeCalculator;
using Xceed.Wpf.Toolkit;


namespace HumanVolumeCalculator
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        #region MyVariables

        private ImportarDatos myDatos;
        private Tools myTools;

        private Model3D model;
        private Model3DGroup body;
        private readonly IHelixViewport3D _viewport;

        private List<SkeletonPoint> spine;
        private List<SkeletonPoint> cadera;
        private List<SkeletonPoint> cuello;
        private List<SkeletonPoint> cabeza;
        private List<SkeletonPoint> chest;
        private List<SkeletonPoint> Nipple;
        private List<SkeletonPoint> Brazo;

        private List<SkeletonPoint> Joints;
        double valorCadera = 0.00;
        double valorCintura = 0.00;

        private readonly Dispatcher dispatcher;
        private OpenFileDialog dialog;
        private string modelPath;

        ResultMedida paciente;

        /// <summary>
        /// Para ejcutar el Binding con el XML
        /// </summary>

        public ICommand FileOpenCommand { get; set; }
        public ICommand CalcAlturaCommand { get; set; }
        public ICommand CalcCabezaCommand { get; set; }
        public ICommand CalcCuelloCommand { get; set; }
        public ICommand CalcCaderaCommand { get; set; }
        public ICommand CalcCinturaCommand { get; set; }
        public ICommand CalcTodasCommand { get; set; }
        public ICommand ExportarResultadoCommand { get; set; }
        public ICommand CalcPechoCommand { get; set; }
        public ICommand JointsCommand { get; set; }

        public float Tracking { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private BackgroundWorker bgW;

        protected void RaisePropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion



        //Databinding con el XML para visualizar el modelo en el modelvisual3D

        public Model3D my_Model
        {
            get
            {
                return this.model;
            }

            set
            {
                this.model = value;
                this.RaisePropertyChanged("my_Model");
            }
        }


        public MainWindow()
        {
            this.InitializeComponent();
            myDatos = new ImportarDatos(Colors.White);
            this._viewport = helix_viewport;
            myTools = new Tools(myDatos.Vertices, 1.0022);
            spine = new List<SkeletonPoint>();
            cadera = new List<SkeletonPoint>();
            cuello = new List<SkeletonPoint>();
            cabeza = new List<SkeletonPoint>();
            chest = new List<SkeletonPoint>();
            Nipple = new List<SkeletonPoint>();
            Brazo = new List<SkeletonPoint>();
            Joints = new List<SkeletonPoint>();

            dispatcher = Dispatcher.CurrentDispatcher;
            dialog = new OpenFileDialog();
            dialog.Filter= "OBJ MeshFiles|*.obj|All Files|*.*";
            body = new Model3DGroup();

            bgW = new BackgroundWorker();
            bgW.DoWork += new DoWorkEventHandler(CargarVertices);
            bgW.WorkerSupportsCancellation = false;

            paciente = new ResultMedida();

            //FileOpenCommand = new DelegateCommand(CargarNube);
            CalcAlturaCommand = new DelegateCommand(CalcularAltura);
            //ExportarResultadoCommand = new DelegateCommand(AppExit);
            CalcCabezaCommand = new DelegateCommand(CalcularCabeza);
            CalcCuelloCommand = new DelegateCommand(CalcularCuello);
            CalcCaderaCommand = new DelegateCommand(CalcularCadera);
            CalcCinturaCommand = new DelegateCommand(CalcularCintura);
            CalcTodasCommand = new DelegateCommand(CalcularTodas);
            CalcPechoCommand = new DelegateCommand(CalcularPecho);
            JointsCommand = new DelegateCommand(MostrarJoints);

            radioButtonMasc.IsChecked = true;
            UpDown.Value = 20;
			Loaded += MainWindow_Loaded;
        }

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			CargarNubeAutomatico();
		}

		private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            MedicionItem.IsEnabled = false;
            labelStatus.Visibility = Visibility.Hidden;
            labelRecurso.Visibility = Visibility.Hidden;

            Nombre.Visibility = Visibility.Hidden;
            UpDown.Visibility = Visibility.Hidden;
            textBoxApellidos.Visibility = Visibility.Hidden;
            Paciente.Visibility = Visibility.Hidden;
            labelApellidos.Visibility = Visibility.Hidden;
            LabelNombre.Visibility = Visibility.Hidden;
            radioButtonFeme.Visibility = Visibility.Hidden;
            radioButtonMasc.Visibility = Visibility.Hidden;
            textBoxId.Visibility = Visibility.Hidden;
            labelEdad.Visibility = Visibility.Hidden;
            labelSexo.Visibility = Visibility.Hidden;
            labelId.Visibility = Visibility.Hidden;

		}


		private async void CargarNubeAutomatico()
		{
			if (body.Children.Count > 0)
			{
				Clear();
			}

			//Ocultar el label cuando no hay medicions
			//cinturaLabel.Visibility = Visibility.Hidden;
			labelStatus.Visibility = Visibility.Visible;
			labelRecurso.Content = "nube";
			labelRecurso.Visibility = Visibility.Visible;

			/*if (false == dialog.ShowDialog())
            {
                labelStatus.Visibility = Visibility.Hidden;
                labelRecurso.Visibility = Visibility.Hidden;
                return;
            }
           
            if (dialog.FileName == null)
            {
                return;
            }*/
			modelPath = Directory.GetCurrentDirectory() + @"/MeshedReconstruction.obj";
			if (!File.Exists(modelPath))
			{
				labelStatus.Visibility = Visibility.Hidden;
				labelRecurso.Visibility = Visibility.Hidden;
				return;
			}


			bgW.RunWorkerAsync();
			model = await this.LoadAsync(modelPath);


			if (model != null)
			{
				body.Children.Add(model);
				this.my_Model = body;
				overall_grid.DataContext = this;
				_viewport.ZoomExtents(0);
				// buttonCalcular.IsEnabled = true;
				MedicionItem.IsEnabled = true;
			}


			//myDatos.CargarModelo();
			labelRecurso.Content = "vertices";
			myDatos.CargarJoints(modelPath);

			labelStatus.Visibility = Visibility.Hidden;
			labelRecurso.Visibility = Visibility.Hidden;

			//Calculo tdas las medidas.
			CalcularTodas();

			//Guardo el archivo.
			GuardarXML();
		}


		private async void CargarNube()
        {
            if (body.Children.Count > 0)
            {
                Clear();
            }

            //Ocultar el label cuando no hay medicions
            //cinturaLabel.Visibility = Visibility.Hidden;
            labelStatus.Visibility = Visibility.Visible;
            labelRecurso.Content = "nube";
            labelRecurso.Visibility = Visibility.Visible;

            if (false == dialog.ShowDialog())
            {
                labelStatus.Visibility = Visibility.Hidden;
                labelRecurso.Visibility = Visibility.Hidden;
                return;
            }
           
            if (dialog.FileName == null)
            {
                return;
            }
            modelPath = dialog.FileName;
				

			bgW.RunWorkerAsync();
            model = await this.LoadAsync(modelPath);
          

            if (model != null)
            {
                body.Children.Add(model);
                this.my_Model = body;
                overall_grid.DataContext = this;
                _viewport.ZoomExtents(0);
                // buttonCalcular.IsEnabled = true;
                MedicionItem.IsEnabled = true;
            }


            //myDatos.CargarModelo();
            labelRecurso.Content = "vertices";
            myDatos.CargarJoints(modelPath);

            labelStatus.Visibility = Visibility.Hidden;
            labelRecurso.Visibility = Visibility.Hidden;

			//Calculo tdas las medidas.
			CalcularTodas();

			//Guardo el archivo.
			GuardarXML();
		 }

        void Clear()
        {
            body.Children.Clear();
            myDatos.Vertices.Clear();
            myDatos.Normals.Clear();
            MedicionItem.IsEnabled = false;
            labelAlturaResult.Content = "0.00 cm";
            caderaLabel.Content = "0.00 cm";
            cuelloLabelResult.Content = "0.00 cm";
            cinturaLabel.Content = "0.00 cm";
            labelPechoResult.Content = "0.00 cm";
            labelCabezaResult.Content = "0.00 cm";


        }

        void CargarVertices(object sender, DoWorkEventArgs e)
        {
            myDatos.GuararModelValues(modelPath);
        }

        private async Task<Model3DGroup> LoadAsync(string path)
        {
            return await Task.Factory.StartNew(() =>
            {
                var mi = new ModelImporter();
                //Material material= new DiffuseMaterial(new SolidColorBrush(Colors.White));
                //mi.DefaultMaterial= material;
                return mi.Load(path, this.dispatcher);

            });
        }

        string Round(string value)
        {
            if (value.Contains<char>(','))
            {
                value = ReplaceComma(value);
            }

            string round = string.Empty;
            string delimeter = ".";
            char[] _delimeter = delimeter.ToCharArray();
            string[] cadena = value.Split(_delimeter, 2);
            round = cadena[0] + "." + cadena[1].Substring(0, 2)+" cm";
            return round;
        }

        private string ReplaceComma(string value)
        {
          return value.Replace(',', '.'); ;
        }        

        public void GuardarXML()
        {
            //paciente.Nombre = Nombre.Text;
            //paciente.Apellidos = textBoxApellidos.Text;

            //if (radioButtonMasc.IsChecked==true)
            //{
            //    paciente.Masculino = true;
            //}
            //else
            //{
            //    paciente.Masculino = false;
            //}

            //paciente.Edad = (int)UpDown.Value;
            if (valorCintura==0 || valorCadera==0)
            {
                System.Windows.MessageBox.Show("Revise que se haya calculado tanto la cadera como la cintura", "Advertencia", MessageBoxButton.OK,MessageBoxImage.Warning);
                if (valorCintura==0)
                {
                    paciente.IndiceCaderaCintura = (decimal)(valorCadera / 1);
                }
            }
            else paciente.IndiceCaderaCintura = (decimal)(valorCadera / valorCintura);
            //paciente.Id = textBoxId.Text;

            //if (paciente.Id=="")
            //{
            //    Xceed.Wpf.Toolkit.MessageBox.Show("Debe rellenar el campo Cédula","Campo Incompleto",MessageBoxButton.OK,MessageBoxImage.Error);
            //    return;
            //}
                        
            XmlSerializer XMLwriter = new XmlSerializer(typeof(ResultMedida));
            // var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "//"+paciente.Id+ ".xml";
            var path = Directory.GetCurrentDirectory() + "//"+ "Results.xml";
          

            try
            {
                FileStream XmlFile = System.IO.File.Create(path);
                XMLwriter.Serialize(XmlFile, paciente);
                XmlFile.Close();
                CleanPaciente();
                //Nombre.Text = "";
                //textBoxApellidos.Text = "";
                //radioButtonMasc.IsChecked = true;
                //UpDown.Value = 20;
                //textBoxId.Text = "";
            }
            catch (Exception)
            {

                Xceed.Wpf.Toolkit.MessageBox.Show("Compruebe que tiene permiso de Administración para escribir en esta dirección","Error al guardar XML",MessageBoxButton.OK,MessageBoxImage.Error);
            }           


            
        }

        private void CleanPaciente()
        {
            //paciente.Nombre = null;
            //paciente.Apellidos = null;
            //paciente.Edad = 0;
            //paciente.Masculino = true;
            paciente.PerimetroCabeza = 0;
            paciente.PerimetroCadera = 0;
            paciente.PerimetroCintura = 0;
            paciente.PerimetroCuello = 0;
            paciente.PerimetroPecho = 0;
            paciente.Altura = 0;

        }

        private void CalcularAltura()
        {
            myTools.CalcularAlturaPorY(myDatos.Vertices);
            labelAlturaResult.Visibility = Visibility.Visible;
            paciente.Altura = (decimal)(myTools.Altura);
            labelAlturaResult.Content = Round(myTools.Altura.ToString());
        }

        private void AppExit()
        {
            Application.Current.Shutdown();
        }
     
        private void Cargar_Click(object sender, RoutedEventArgs e)
        {
            CargarNube();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            AppExit();
        }

        private void ExportarXML_Click(object sender, RoutedEventArgs e)
        {
            GuardarXML();
        }

        private void CalcularCabeza()
        {
            double medicion;
            SkeletonPoint temp = new SkeletonPoint();
            temp.X = myDatos.MyJoints.HeadCoo.X;
            temp.Y = (myDatos.MyJoints.HeadCoo.Y) + Tracking;
            temp.Z = myDatos.MyJoints.HeadCoo.Z;

            cabeza = myTools.SelectFranjaHorizontal(temp, false, 0.07f);
            labelCabezaResult.Visibility = Visibility.Visible;
            medicion = myTools.DistanciaPorRadiosPromedios(cabeza);
            labelCabezaResult.Content = Round((medicion).ToString());
            paciente.PerimetroCabeza =(decimal) medicion;

            if (body.Children.Count>1)
            {
                body.Children.RemoveAt(body.Children.Count - 1);
            }

            body.Children.Add(myTools.GraficarAreaMedida(cabeza, Colors.Purple));
            my_Model = body;
        }

        private void CalcularPecho()
        {
            double medicion;
            SkeletonPoint temp = new SkeletonPoint();
            SkeletonPoint _chest = myTools.EstimarTorax(myDatos.MyJoints.SpineShoulderCoo, myDatos.MyJoints.SpineMidCoo);
            //if (Tracking==0)
            //{
            //    Tracking = 0.04f;
            //}
            temp.X =_chest.X;
            temp.Y = _chest.Y + Tracking;
            temp.Z =_chest.Z;

            chest = myTools.SelectFranjaHorizontal(temp, true, 0.1f);
            labelPechoResult.Visibility = Visibility.Visible;
            medicion = myTools.DistanciaPorRadiosPromedios(chest);
            labelPechoResult.Content = Round((medicion).ToString());
            paciente.PerimetroPecho = (decimal)(medicion);

            if (body.Children.Count > 1)
            {
                body.Children.RemoveAt(body.Children.Count - 1);
            }

            body.Children.Add(myTools.GraficarAreaMedida(chest, Colors.DeepPink));
            my_Model = body;
        }

        private void CalcularCuello()
        {
            double medicion;
            SkeletonPoint temp = new SkeletonPoint();
            temp.X = myDatos.MyJoints.NeckCoo.X;
            temp.Y = (myDatos.MyJoints.NeckCoo.Y) + Tracking;
            temp.Z = myDatos.MyJoints.NeckCoo.Z;

            cuello = myTools.SelectFranjaHorizontal(temp, false, 0.07f);
            cuelloLabelResult.Visibility = Visibility.Visible;
            medicion = myTools.DistanciaPorRadiosPromedios(cuello);
            cuelloLabelResult.Content = Round((medicion).ToString());
            paciente.PerimetroCuello = (decimal)medicion;

            if (body.Children.Count >1)
            {
                body.Children.RemoveAt(body.Children.Count - 1);
            }

            body.Children.Add(myTools.GraficarAreaMedida(cuello, Colors.DeepSkyBlue));            
            my_Model = body;

        }
        
        private void CalcularCadera()
        {
            double medicion;
            SkeletonPoint temp = new SkeletonPoint();
            temp.X = myDatos.MyJoints.SpineBaseCoo.X;
            temp.Y = (myDatos.MyJoints.SpineBaseCoo.Y) + Tracking;
            temp.Z = myDatos.MyJoints.SpineBaseCoo.Z;

            cadera = myTools.SelectFranjaHorizontal(temp, true, 0.4f);
            caderaLabel.Visibility = Visibility.Visible;
            medicion = myTools.DistanciaPorRadiosPromedios(cadera);
            valorCadera = medicion;
            caderaLabel.Content = Round((medicion).ToString());
            paciente.PerimetroCadera = (decimal)medicion;

            if (body.Children.Count >1)
            {
                body.Children.RemoveAt(body.Children.Count - 1);
            }

            body.Children.Add(myTools.GraficarAreaMedida(cadera, Colors.Green));
            my_Model = body;
        }

        private void CalcularCintura()
        {
            double medicion;
            SkeletonPoint temp = new SkeletonPoint();
            temp.X = myDatos.MyJoints.SpineMidCoo.X;
            temp.Y = (myDatos.MyJoints.SpineMidCoo.Y)+Tracking;
            temp.Z = myDatos.MyJoints.SpineMidCoo.Z;
            //Metodo de radios promedios del circulo
            spine = myTools.SelectFranjaHorizontal(temp, true, 0.2f);
            cinturaLabel.Visibility = Visibility.Visible;
            medicion = myTools.DistanciaPorRadiosPromedios(spine);
            valorCintura = medicion;
            cinturaLabel.Content = Round((medicion).ToString());
            paciente.PerimetroCintura = (decimal)medicion;

            if (body.Children.Count >1)
            {
                body.Children.RemoveAt(body.Children.Count - 1);
            }

            body.Children.Add(myTools.GraficarAreaMedida(spine, Colors.Red));
            my_Model = body;
        }

        private void CalcularCinturaXDist()
        {
            double medicion;
            SkeletonPoint temp = new SkeletonPoint();
            temp.X = myDatos.MyJoints.SpineMidCoo.X;
            temp.Y = (myDatos.MyJoints.SpineMidCoo.Y) + Tracking;
            temp.Z = myDatos.MyJoints.SpineMidCoo.Z;
            spine = myTools.SelectFranjaHorizontal(temp, true, 0.1f);
            medicion = myTools.DistanciaPointAbs(spine);
            cinturaLabel.Content = Round((medicion).ToString());
            paciente.PerimetroCintura = (decimal)medicion;
            if (body.Children.Count >= 1)
            {
                body.Children.RemoveAt(body.Children.Count - 1);
            }

            body.Children.Add(myTools.GraficarAreaMedida(spine, Colors.Red));
            my_Model = body;
        }

        private void CalcularTodas()
        {
            double medicion;
            //Metodo de radios promedios del circulo
            spine = myTools.SelectFranjaHorizontal(myDatos.MyJoints.SpineMidCoo, true, 0.07f);
            cinturaLabel.Visibility = Visibility.Visible;
            medicion = myTools.DistanciaPorRadiosPromedios(spine);
            valorCintura = medicion;
            cinturaLabel.Content = Round((medicion).ToString());
            paciente.PerimetroCintura = (decimal)medicion;
            //body.Children.Add(myTools.GraficarAreaMedida(spine, Colors.Red));
            //my_Model = body;

            cadera = myTools.SelectFranjaHorizontal(myDatos.MyJoints.SpineBaseCoo, true, 0.07f);
            caderaLabel.Visibility = Visibility.Visible;
            medicion = myTools.DistanciaPorRadiosPromedios(cadera);
            valorCadera = medicion;
            caderaLabel.Content = Round((medicion).ToString());
            paciente.PerimetroCadera = (decimal)medicion;
            //body.Children.Add(myTools.GraficarAreaMedida(cadera, Colors.Green));
            //my_Model = body;

            cabeza = myTools.SelectFranjaHorizontal(myDatos.MyJoints.HeadCoo, false, 0.07f);
            labelCabezaResult.Visibility = Visibility.Visible;
            medicion = myTools.DistanciaPorRadiosPromedios(cabeza);
            labelCabezaResult.Content = Round((medicion).ToString());
            paciente.PerimetroCabeza = (decimal)medicion;
            //body.Children.Add(myTools.GraficarAreaMedida(cabeza, Colors.Purple));
            //my_Model = body;


            cuello = myTools.SelectFranjaHorizontal(myDatos.MyJoints.NeckCoo, false, 0.07f);
            cuelloLabelResult.Visibility = Visibility.Visible;
            medicion = myTools.DistanciaPorRadiosPromedios(cuello);
            cuelloLabelResult.Content = Round((medicion).ToString());
            paciente.PerimetroCuello = (decimal)medicion;
           // body.Children.Add(myTools.GraficarAreaMedida(cuello, Colors.DeepSkyBlue));
            //my_Model = body;

            //myTools.ErrorAltura = 0.1325f;
            ////myTools.CalcularAltura(myDatos.MyJoints.HeadCoo, myDatos.MyJoints.FootLeftCoo,myDatos.MyJoints.FootRightCoo);
            myTools.CalcularAlturaPorY(myDatos.Vertices);
            labelAlturaResult.Visibility = Visibility.Visible;
            paciente.Altura = (decimal)(myTools.Altura);
            labelAlturaResult.Content = Round(myTools.Altura.ToString());

            chest = myTools.SelectFranjaHorizontal(myTools.EstimarTorax(myDatos.MyJoints.SpineShoulderCoo, myDatos.MyJoints.SpineMidCoo), true, 0.025f);
            labelPechoResult.Visibility = Visibility.Visible;
            medicion = myTools.DistanciaPorRadiosPromedios(chest);
            labelPechoResult.Content = Round((medicion).ToString());
            paciente.PerimetroPecho = (decimal)medicion;
            //body.Children.Add(myTools.GraficarAreaMedida(chest, Colors.DeepPink));
            //my_Model = body;
        }

        private void MostrarJoints()
        {
            //SkeletonPoint tem = new SkeletonPoint();
            //tem.X = myDatos.MyJoints.HeadCoo.X - 0.2f;
            //tem.Y = myDatos.MyJoints.HeadCoo.Y;
            //tem.Z = myDatos.MyJoints.HeadCoo.Z;
            Joints.Add(myDatos.MyJoints.AnkleLeftCoo);
            Joints.Add(myDatos.MyJoints.AnkleRightCoo);
            Joints.Add(myDatos.MyJoints.ElbowLeftCoo);
            Joints.Add(myDatos.MyJoints.ElbowRightCoo);
            Joints.Add(myDatos.MyJoints.FootLeftCoo);
            Joints.Add(myDatos.MyJoints.FootRightCoo);
            Joints.Add(myDatos.MyJoints.HandLeftCoo);
            Joints.Add(myDatos.MyJoints.HandRightCoo);
            Joints.Add(myDatos.MyJoints.HandTipLeftCoo);
            Joints.Add(myDatos.MyJoints.HandTipRightCoo);
            Joints.Add(myDatos.MyJoints.HeadCoo);
            Joints.Add(myDatos.MyJoints.HipLeftCoo);
            Joints.Add(myDatos.MyJoints.HipRightCoo);
            Joints.Add(myDatos.MyJoints.KneeLeftCoo);
            Joints.Add(myDatos.MyJoints.KneeRightCoo);
            Joints.Add(myDatos.MyJoints.NeckCoo);
            Joints.Add(myDatos.MyJoints.ShoulderLeftCoo);
            Joints.Add(myDatos.MyJoints.ShoulderRightCoo);
            Joints.Add(myDatos.MyJoints.SpineBaseCoo);
            Joints.Add(myDatos.MyJoints.SpineMidCoo);
            Joints.Add(myDatos.MyJoints.SpineShoulderCoo);
            Joints.Add(myDatos.MyJoints.ThumbLeftCoo);
            Joints.Add(myDatos.MyJoints.ThumbRightCoo);
            Joints.Add(myDatos.MyJoints.WirstLeftCoo);
            Joints.Add(myDatos.MyJoints.WristRightCoo);

            if (body.Children.Count > 1)
            {
                body.Children.RemoveAt(body.Children.Count - 1);
            }
           // body.Children.RemoveAt(0);

            body.Children.Add(myTools.GraficarAreaMedida(Joints, Colors.YellowGreen));
            my_Model = body;

        }

    }

        

  }


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Kinect;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;
using Microsoft.Win32;
using System.Windows.Media;
using HumanVolumeCalculator.Loaders;
using System.Globalization;
using static HumanVolumeCalculator.Tools;

namespace HumanVolumeCalculator
{
	public class ImportarDatos
	{
		private ModelImporter importer;
		private Model3DGroup body;
		private Model3D model;
		//private Model3D bodySeccion;
		private Material bodyMaterial;
		private JointsCoo myjointsCoo;
		//private int currentLineNo;

		private StreamReader jointReader;
		private StreamReader ReaderObj;

		private List<SkeletonPoint> verticesDelantero;
		private List<SkeletonPoint> verticesPosterior;
		private List<SkeletonPoint> vert;
		private List<Normal> normals;
		private List<int> position;

		OpenFileDialog dialog;

		//Constructores

		public ImportarDatos(Color _colorBodyMaterial)
		{
			dialog = new OpenFileDialog();
			dialog.Filter = "OBJ MeshFiles|*.obj|All Files|*.*";
			importer = new ModelImporter();
			body = new Model3DGroup();
			bodyMaterial = new DiffuseMaterial(new SolidColorBrush(_colorBodyMaterial));
			importer.DefaultMaterial = bodyMaterial;
			verticesDelantero = new List<SkeletonPoint>();
			verticesPosterior = new List<SkeletonPoint>();
			vert = new List<SkeletonPoint>();
			normals = new List<Normal>();
			position = new List<int>();
		}


		#region MyProperties

		public Model3DGroup Body
		{
			get { return body; }
			set { body = value; }
		}

		public JointsCoo MyJoints
		{
			get { return myjointsCoo; }
			set { myjointsCoo = value; }
		}

		public Model3D Model
		{
			get { return model; }
			set { model = value; }
		}

		public List<Normal> Normals
		{
			get { return normals; }
			set { normals = value; }
		}

		public List<SkeletonPoint> VerticesDelantero
		{
			get { return verticesDelantero; }
			set { verticesDelantero = value; }
		}

		public List<SkeletonPoint> VerticesPosterior
		{
			get { return verticesPosterior; }
			set { verticesPosterior = value; }
		}

		#endregion

		#region MyMethods

		public void CargarModelo()
		{
			if (true == dialog.ShowDialog())
			{
				if (dialog.FileName == null)
				{
					return;
				}
				model = importer.Load(dialog.FileName);
				GuararModelValues(dialog.FileName, TipoAvatar.Delantero);
				body.Children.Add(model);

				RotateTransform3D myRotateTransform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90));
				myRotateTransform.CenterX = 0;
				myRotateTransform.CenterY = 0;
				myRotateTransform.CenterZ = 0;
				body.Transform = myRotateTransform;
			}
		}

		public void GuararModelValues(string path, TipoAvatar tipoAvatar)
		{
			vert.Clear();
			position.Clear();
			if (path == null)
			{
				return;
			}
			try
			{


				using (ReaderObj = new StreamReader(path))
				{
					while (!ReaderObj.EndOfStream)
					{
						var line = ReaderObj.ReadLine();
						if (line == null)
						{
							break;
						}

						line = line.Trim();

						if (line.StartsWith("#") || line.Length == 0)
						{
							continue;
						}
						string keyword, values;

						SplitLine(line, out keyword, out values);

						if (keyword.ToLower() == "v" || keyword.ToLower() == "vn")
						{
							AddVertex(values);
						}

						if (keyword.ToLower() == "f")
						{
							string[] fields = values.Trim().SplitOnWhitespace();
							for (int i = 0; i < fields.Count(); i++)
							{
								position.Add(int.Parse(fields[i].Substring(0, fields[i].IndexOf('/'))));
							}
						}
					}
				}
				for (int i = 0; i < position.Count(); i++)
				{
					if (tipoAvatar == TipoAvatar.Delantero)
						VerticesDelantero.Add(vert[position[i]]);
					else
						VerticesPosterior.Add(vert[position[i]]);
					/*using (StreamWriter sw = new StreamWriter(@"C:\puntos.txt", true, Encoding.UTF8))
					{
						sw.WriteLine(string.Format(@"{0} {1} {2}", vert[position[i]].X, vert[position[i]].Y, vert[position[i]].Z));
					}*/
				}
			}
			catch(Exception)
			{

			}
		}

		private void AddVertex(string values)
		{
			SkeletonPoint temp = new SkeletonPoint();
			var fields = Split(values);
			temp.X = (float)fields[0];
			temp.Y = (float)fields[1];
			//temp.Y = (float)fields[1] * -1;           //invertir eje
			temp.Z = (float)fields[2];
			vert.Add(temp);
		}

		private void AddNormal(string values)
		{
			var fields = Split(values);
			this.Normals.Add(new Normal(fields[0], fields[1], fields[2]));
		}


		private static IList<double> Split(string input)
		{
			input = input.Trim();
			var fields = input.SplitOnWhitespace();
			var result = new double[fields.Length];
			for (int i = 0; i < fields.Length; i++)
			{
				result[i] = double.Parse(fields[i], CultureInfo.InvariantCulture);
			}

			return result;
		}


		private static void SplitLine(string line, out string keyword, out string arguments)
		{
			int idx = line.IndexOf(' ');
			if (idx < 0)
			{
				keyword = line;
				arguments = null;
				return;
			}

			keyword = line.Substring(0, idx);
			arguments = line.Substring(idx + 1);
		}


		public void CargarJoints(string modelPath)
		{
			string path = string.Empty;
			if (modelPath == null)
			{
				return;
			}

			path = modelPath.Substring(0, modelPath.Count() - 4);
			path += "JointsCoo.txt";
			jointReader = new StreamReader(path);
			var value = string.Empty;

			while (!jointReader.EndOfStream)
			{
				value = jointReader.ReadLine();
				switch (value)
				{
					case "Head":
						value = jointReader.ReadLine();
						myjointsCoo.HeadCoo = GuardarJoints(value);
						break;

					case "Neck":
						value = jointReader.ReadLine();
						myjointsCoo.NeckCoo = GuardarJoints(value);
						break;

					case "ShoulderCenter":
						value = jointReader.ReadLine();
						myjointsCoo.SpineShoulderCoo = GuardarJoints(value);

						break;

					case "ShoulderLeft":
						value = jointReader.ReadLine();
						myjointsCoo.ShoulderLeftCoo = GuardarJoints(value);
						break;

					case "ElbowLeft":
						value = jointReader.ReadLine();
						myjointsCoo.ElbowLeftCoo = GuardarJoints(value);
						break;

					case "WristLeft":
						value = jointReader.ReadLine();
						myjointsCoo.WirstLeftCoo = GuardarJoints(value);
						break;

					case "HandLeft":
						value = jointReader.ReadLine();
						myjointsCoo.HandLeftCoo = GuardarJoints(value);
						break;

					case "ShoulderRight":
						value = jointReader.ReadLine();
						myjointsCoo.ShoulderRightCoo = GuardarJoints(value);
						break;

					case "ElbowRight":
						value = jointReader.ReadLine();
						myjointsCoo.ElbowRightCoo = GuardarJoints(value);
						break;

					case "WristRight":
						value = jointReader.ReadLine();
						myjointsCoo.WristRightCoo = GuardarJoints(value);
						break;

					case "HandRight":
						value = jointReader.ReadLine();
						myjointsCoo.HandRightCoo = GuardarJoints(value);
						break;

					case "SpineMid":
						value = jointReader.ReadLine();
						myjointsCoo.SpineMidCoo = GuardarJoints(value);
						break;

					case "SpineBase":
						value = jointReader.ReadLine();
						myjointsCoo.SpineBaseCoo = GuardarJoints(value);
						break;

					case "HipLeft":
						value = jointReader.ReadLine();
						myjointsCoo.HipLeftCoo = GuardarJoints(value);
						break;

					case "KneeLeft":
						value = jointReader.ReadLine();
						myjointsCoo.KneeLeftCoo = GuardarJoints(value);
						break;

					case "AnkleLeft":
						value = jointReader.ReadLine();
						myjointsCoo.AnkleLeftCoo = GuardarJoints(value);
						break;

					case "FootLeft":
						value = jointReader.ReadLine();
						myjointsCoo.FootLeftCoo = GuardarJoints(value);
						break;

					case "HipRight":
						value = jointReader.ReadLine();
						myjointsCoo.HipRightCoo = GuardarJoints(value);
						break;

					case "KneeRight":
						value = jointReader.ReadLine();
						myjointsCoo.KneeRightCoo = GuardarJoints(value);
						break;

					case "AnkleRight":
						value = jointReader.ReadLine();
						myjointsCoo.AnkleRightCoo = GuardarJoints(value);
						break;

					case "FootRight":
						value = jointReader.ReadLine();
						myjointsCoo.FootRightCoo = GuardarJoints(value);
						break;

					default:
						break;
				}

			}

		}

		public List<SkeletonPoint> JointsToList()
		{
			List<SkeletonPoint> myJoints = new List<SkeletonPoint>();
			myJoints.Add(MyJoints.AnkleLeftCoo);
			myJoints.Add(MyJoints.AnkleRightCoo);
			myJoints.Add(MyJoints.ElbowLeftCoo);
			myJoints.Add(MyJoints.ElbowRightCoo);
			myJoints.Add(MyJoints.FootLeftCoo);
			myJoints.Add(MyJoints.FootRightCoo);
			myJoints.Add(MyJoints.HandLeftCoo);
			myJoints.Add(MyJoints.HandRightCoo);
			myJoints.Add(MyJoints.HandTipLeftCoo);
			myJoints.Add(MyJoints.HandTipRightCoo);
			myJoints.Add(MyJoints.HeadCoo);
			myJoints.Add(MyJoints.HipLeftCoo);
			myJoints.Add(MyJoints.HipRightCoo);
			myJoints.Add(MyJoints.KneeLeftCoo);
			myJoints.Add(MyJoints.KneeRightCoo);
			myJoints.Add(MyJoints.NeckCoo);
			myJoints.Add(MyJoints.ShoulderLeftCoo);
			myJoints.Add(MyJoints.ShoulderRightCoo);
			myJoints.Add(MyJoints.SpineBaseCoo);
			myJoints.Add(MyJoints.SpineMidCoo);
			myJoints.Add(MyJoints.SpineShoulderCoo);
			myJoints.Add(MyJoints.ThumbLeftCoo);
			myJoints.Add(MyJoints.ThumbRightCoo);
			myJoints.Add(MyJoints.WirstLeftCoo);
			myJoints.Add(MyJoints.WristRightCoo);
			return myJoints;
		}

		SkeletonPoint GuardarJoints(string line)
		{
			var fields = Split(line);

			SkeletonPoint point = new SkeletonPoint();
			point.X = (float)fields[0];
			point.Y = (float)fields[1];
			point.Z = (float)fields[2];
			return point;


		}

		#endregion




	}
}

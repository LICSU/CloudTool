using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using HelixToolkit.Wpf;


namespace HumanVolumeCalculator
{
	public class Tools
	{
		private double ancho;
		private List<SkeletonPoint> verticesDelanteros;
		private double epsilon;
		private List<double> angulos;
		//private KinectSensor sensor;
		private float altura;
		private double anchoHombros;
		private float errorAltura;
		//SkeletonPoint Zmax;
		//bool menor = false;
		List<SkeletonPoint> brazos;

		public Tools(List<SkeletonPoint> values)
		{
			epsilon = 0.0022;
			verticesDelanteros = new List<SkeletonPoint>();
			verticesDelanteros = values;
			angulos = new List<double>();
			brazos = new List<SkeletonPoint>();


		}

		public Tools(List<SkeletonPoint> values, double _epsilon)
		{
			epsilon = _epsilon;
			verticesDelanteros = new List<SkeletonPoint>();
			verticesDelanteros = values;
			angulos = new List<double>();
		}




		#region MyProperties

		public List<SkeletonPoint> VerticesDelanteros
		{
			get { return verticesDelanteros; }
			set { verticesDelanteros = value; }
		}

		public double Epsilon
		{
			get { return epsilon; }
			set { epsilon = value; }
		}

		public float Altura
		{
			get { return altura; }
			set { altura = value; }
		}

		public double AnchoHombros
		{
			get { return anchoHombros; }
			set { anchoHombros = value; }
		}
		public float ErrorAltura
		{
			get { return errorAltura; }
			set { errorAltura = value; }
		}

		#endregion



		#region MyMethods

		//Obtener todos los vertices en la franja epsilon - Y epsilon + Y
		public List<SkeletonPoint> SelectFranjaHorizontal(SkeletonPoint jointCoo, bool tronco, float histeresis)
		{
			List<SkeletonPoint> franja = new List<SkeletonPoint>();
			//franja.Add(jointCoo);

			foreach (var item in verticesDelanteros)
			{
				if (item.Y >= jointCoo.Y - epsilon && item.Y <= jointCoo.Y + epsilon)
				{
					franja.Add(item);
				}
			}

			//List<SkeletonPoint>franjaOrdenada=OrdenarByDist(franja);
			EliminarDuplicados(franja);
			InitialPoint(franja, jointCoo);

			if (tronco)
			{
				return OrdenarByDistanciaMayorX(franja, histeresis);
			}

			return franja;
			//return FranjaRefinadaxDsitancias(franjaOrdenada);
		}

		/*private List<SkeletonPoint> FranjaRefinadaxDsitancias(List<SkeletonPoint> franja)
		{
			List<SkeletonPoint> fRefinada = new List<SkeletonPoint>();
			double dist = 0;
			double distPromedio = 0;
			double lastDist = 0;
			int contador = 1;


			for (int i = 1; i < franja.Count - 1; i++)
			{
				dist = FormulaDistanciaEntrePtos(franja[i], franja[i + 1]);

				if (dist != 0)
				{
					distPromedio += dist;
					distPromedio /= contador;
				}


				if ((dist <= lastDist + 0.0002 && dist != 0) || (i == 1))
				{
					fRefinada.Add(franja[i]);
					contador++;
					lastDist = dist;

				}
				else
				{
					int j = 0;
				}
			}

			return fRefinada;
		}*/

		/*private List<SkeletonPoint> RefinarFranja(List<SkeletonPoint> franja, SkeletonPoint joint)
		{
			double distMax = DistaciaTranversal(joint);
			distMax = distMax / 2;
			List<SkeletonPoint> temp = new List<SkeletonPoint>();

			foreach (var item in franja)
			{
				if (item.X >= joint.X - distMax && item.X <= joint.X + distMax)
				{
					temp.Add(item);
				}
				else
				{
					temp.Add(new SkeletonPoint());
				}
			}
			return temp;
		}

		private double DistaciaTranversal(SkeletonPoint joint)
		{
			return FormulaDistanciaEntrePtos(joint, Zmax);
		}*/



		private SkeletonPoint CalcularZmax(List<SkeletonPoint> franja)
		{
			SkeletonPoint Zmax = franja[0];
			double distMax = 0.0005;

			foreach (var item in franja)
			{
				if (Math.Abs(item.Z) > Math.Abs(Zmax.Z) && (item.X >= franja[0].X - distMax && item.X <= franja[0].X + distMax))
				{
					Zmax = item;
				}
			}

			return Zmax;
		}

		//Distancia entre dos ptos Absoluta plano X-Z
		public double DistanciaPointAbs(List<SkeletonPoint> points)
		{
			var suma = 0.0;

			for (int i = 0, j = 1; j <= points.Count<SkeletonPoint>() - 1; i++, j++)
			{
				suma += Math.Sqrt((Math.Abs(points[j].X) - Math.Abs(points[i].X)) * (Math.Abs(points[j].X) - Math.Abs(points[i].X)) + (Math.Abs(points[j].Z) - Math.Abs(points[i].Z)) * (Math.Abs(points[j].Z) - Math.Abs(points[i].Z)));
			}
			suma += Math.Sqrt((Math.Abs(points[points.Count() - 1].X) - Math.Abs(points[0].X)) * (Math.Abs(points[points.Count() - 1].X) - Math.Abs(points[0].X)) + (Math.Abs(points[points.Count() - 1].Y) - Math.Abs(points[0].Y)) * (Math.Abs(points[points.Count() - 1].Y) - Math.Abs(points[0].Y)) + (Math.Abs(points[points.Count() - 1].Z) - Math.Abs(points[0].Z)) * (Math.Abs(points[points.Count() - 1].Z) - Math.Abs(points[0].Z)));

			return suma;
		}

		//Distancia entre dos ptos no absoluta espacia X-Y-Z
		public double DistanciaPointXYZ(List<SkeletonPoint> points)
		{
			var suma = 0.0;

			for (int i = 0, j = 1; j <= points.Count<SkeletonPoint>() - 1; i++, j++)
			{
				suma += Math.Sqrt(((points[j].X - points[i].X) * (points[j].X - points[i].X)) + (points[j].Y - points[i].Y) * (points[j].Y - points[i].Y) + (points[j].Z - points[i].Z) * (points[j].Z - points[i].Z));
			}
			suma += Math.Sqrt((points[points.Count() - 1].X - points[0].X) * (points[points.Count() - 1].X - points[0].X) + (points[points.Count() - 1].Y - points[0].Y) * (points[points.Count() - 1].Y - points[0].Y) + (points[points.Count() - 1].Z - points[0].Z) * (points[points.Count() - 1].Z - points[0].Z));

			return suma;
		}

		//Distancia entre dos ptos no absoluta espacia X-Z
		public double DistanciaPointXZ(List<SkeletonPoint> points)
		{
			var suma = 0.0;

			for (int i = 0, j = 1; j < points.Count<SkeletonPoint>(); i++, j++)
			{
				suma += Math.Sqrt((Math.Round(points[j].X, 2) - Math.Round(points[i].X, 2)) * (Math.Round(points[j].X, 2) - Math.Round(points[i].X, 2)) + (Math.Round(points[j].Z, 2) - Math.Round(points[i].Z, 2)) * (Math.Round(points[j].Z, 2) - Math.Round(points[i].Z, 2)));
			}
			//suma += Math.Sqrt((points[points.Count() - 1].X - points[0].X) * (points[points.Count() - 1].X - points[0].X) + (points[points.Count() - 1].Z - points[0].Z) * (points[points.Count() - 1].Z - points[0].Z));

			return suma;
		}

		//Ordenar los puntos en el perimetro mediante las distancias mas cortas
		public List<SkeletonPoint> OrdenarByDist(List<SkeletonPoint> points)
		{
			//List<SkeletonPoint> tempList = new List<SkeletonPoint>();
			//tempList.Add(points[0]);  
			int indexToSwap = 0;
			SkeletonPoint tempPoint = new SkeletonPoint();


			for (int i = 0; i < points.Count() - 1; i++)
			{

				indexToSwap = IndiceMenorDistanciaRounded(i, points);

				if (i + 1 != indexToSwap)
				{
					tempPoint = points[indexToSwap];
					points[indexToSwap] = points[i + 1];
					points[i + 1] = tempPoint;
				}

			}

			return points;
		}

		private List<SkeletonPoint> OrdenarByDistanciaMayorX(List<SkeletonPoint> franja, float histeresis)
		{
			List<SkeletonPoint> sorted = new List<SkeletonPoint>();
			SkeletonPoint swapPos = new SkeletonPoint();
			double dist;
			double menorDist = 1000; //Una distancia inicial grande
			int indice = 0;
			bool ready = false;
			int j = 1;
			double substraction;
			double menorSub = 1000;

			sorted.Add(franja[0]);
			//bool Positiva = PrimerPto(franja);

			for (int i = 0; i < franja.Count - 1; i++)
			{
				j = i + 1;
				for (; j < franja.Count; j++)
				{
					dist = FormulaDistanciaEntrePtos(franja[i], franja[j]);
					substraction = Math.Abs(franja[i].X - franja[j].X);

					if ((indice != j) && (dist != 0) && (menorSub > substraction) && (substraction < histeresis) && (franja[i].X != franja[j].X))
					{
						indice = j;
						menorDist = dist;
						ready = true;
						menorSub = substraction;
					}
				}

				if (ready)
				{
					//i = indice - 1;
					sorted.Add(franja[indice]);
					swapPos = franja[indice];
					franja[indice] = franja[i + 1];
					franja[i + 1] = swapPos;

					ready = false;

					indice = 0;
					menorDist = 1000;
					menorSub = 1000;
				}
				else
				{
					//for (int w = j; w < franja.Count; w++)
					//{
					//    brazos.Add(franja[w]);
					//}
					return sorted;

				}


			}

			//for (int w = j; w < franja.Count; w++)
			//{
			//    brazos.Add(franja[w]);
			//}
			return sorted;
		}




		//return true si el punto mas cercano el valor de X es positivo
		/*private bool PrimerPto(List<SkeletonPoint> franja)
		{
			double dist;
			double menorDist = 1000;
			int indice = 0;


			for (int j = 1; j < franja.Count; j++)
			{
				dist = FormulaDistanciaEntrePtos(franja[0], franja[j]);
				if (menorDist > dist)
				{
					menorDist = dist;
					indice = j;
				}
			}
			if (franja[indice].X < franja[0].X)
			{
				menor = true;
			}
			else
			{
				menor = false;
			}

			if (franja[indice].X >= 0)
			{
				return true;

			}
			else { return false; }

		}*/

		private void InitialPoint(List<SkeletonPoint> franja, SkeletonPoint joint)
		{
			//float Xdist = 100;
			float mayorZ = -5;
			SkeletonPoint temp = new SkeletonPoint();
			int indice = 0;
			float dist;

			for (int i = 0; i < franja.Count; i++)
			{
				dist = dist = joint.X - franja[i].X;
				if (Math.Abs(dist) < 0.00027)
				{
					if (franja[i].Z > mayorZ)
					{
						indice = i;
					}
				}
			}
			temp = franja[0];
			franja[0] = franja[indice];
			franja[indice] = temp;


		}

		private void EliminarDuplicados(List<SkeletonPoint> franja)
		{
			int j = 1;
			for (int i = 0; i < franja.Count - 1; i++)
			{
				j = i + 1;
				for (; j < franja.Count; j++)
				{
					if (Equals(franja[i], franja[j]))
					{
						franja.RemoveAt(j);
					}
				}

			}
		}

		private bool Equals(SkeletonPoint p1, SkeletonPoint p2)
		{
			if (p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z)
			{
				return true;
			}
			return false;
		}

		//Metodo para retornar el indice del elemento q menor distancia tiene con el pto point[index]
		int IndiceMenorDistancia(int index, List<SkeletonPoint> points)
		{
			int indexToReturn = index + 1;
			double menorDist = 1000;
			double dist;


			for (int j = index + 1; j < points.Count(); j++)
			{
				dist = Math.Sqrt((points[index].X - points[j].X) * (points[index].X - points[j].X) + (points[index].Z - points[j].Z) * (points[index].Z - points[j].Z));

				if (j == index + 1 || menorDist > dist)
				{
					menorDist = dist;
					indexToReturn = j;
				}
			}

			ancho += menorDist;

			return indexToReturn;
		}

		int IndiceMenorDistanciaRounded(int index, List<SkeletonPoint> points)
		{
			int indexToReturn = index + 1;
			double menorDist = 1000;
			double dist;

			for (int j = index + 1; j < points.Count(); j++)
			{
				dist = Math.Sqrt((Math.Round(points[index].X, 2) - Math.Round(points[j].X, 2)) * (Math.Round(points[index].X, 2) - Math.Round(points[j].X, 2)) + (Math.Round(points[index].Z, 2) - Math.Round(points[j].Z, 2)) * (Math.Round(points[index].Z, 2) - Math.Round(points[j].Z, 2)));

				if (j == index + 1 || menorDist > dist)
				{
					//if (points[j].X)
					//{

					//}
					menorDist = dist;
					//anchoMet2Spine += dist;
					indexToReturn = j;
				}
			}

			ancho += menorDist;
			return indexToReturn;
		}

		//Metodo por implementar
		public void ObtenerAngulos(List<SkeletonPoint> points)
		{
			for (int i = 0; i < points.Count(); i++)
			{
				if (points[i].X >= 0) //Primer cuadrante
				{
					angulos.Add(CalcularAngulo(points[i], true));
				}
				else
				{
					angulos.Add(CalcularAngulo(points[i], false));
				}
			}
		}

		double CalcularAngulo(SkeletonPoint point, bool pos)
		{
			double theta;

			if (pos)
			{
				theta = 90 - ((180 / Math.PI) * Math.Atan2(point.Z, point.X));
			}
			else
			{
				theta = 90 - ((180 / Math.PI) * Math.Atan2(point.Z, point.X)) + 180;
			}


			return theta;
		}

		public void OrdenarAngulos(ref List<SkeletonPoint> points)
		{
			double anguloSwap;
			SkeletonPoint pointSwap1;

			for (int i = 0; i < angulos.Count; i++)
			{
				for (int j = 1; j < angulos.Count; j++)
				{
					if (angulos[i] > angulos[j])
					{
						anguloSwap = angulos[j];
						angulos[j] = angulos[i];
						angulos[i] = anguloSwap;

						pointSwap1 = points[j];
						points[j] = points[i];
						points[i] = pointSwap1;
					}
				}
			}

		}

		//2do Metodo para la aprximacion del perimetro
		public double DistanciaPorRadiosPromedios(List<SkeletonPoint> points)
		{
			//Calcular la Xpromedio y la Zpromedio
			Point PointProm = XpZp(points);
			double radioProm = 0.0;


			List<double> Distancias = DistanciasPromNoRounded(points, PointProm);

			foreach (var item in Distancias)
			{
				radioProm += item;
			}

			radioProm = radioProm / Distancias.Count();

			//List<SkeletonPoint> ordenados = new List<SkeletonPoint>();
			//ordenados=OrdenarByDist(points);
			//GraficarAreaMedida(ordenados);

			double result = 2 * Math.PI * radioProm;
			return result * 100;      //LLevar de metros a cm
		}

		List<double> DistanciasProm(List<SkeletonPoint> points, Point XpZp)
		{
			List<double> Distancias = new List<double>();

			for (int i = 0; i < points.Count(); i++)
			{
				Distancias.Add(Math.Sqrt((Math.Round(points[i].X, 3) - Math.Round(XpZp.X, 3)) * (Math.Round(points[i].X, 3) - Math.Round(XpZp.X, 3)) + (Math.Round(points[i].Z, 3) - Math.Round(XpZp.Y, 3)) * (Math.Round(points[i].Z, 3) - Math.Round(XpZp.Y, 3))));
			}

			return Distancias;
		}

		List<double> DistanciasPromNoRounded(List<SkeletonPoint> points, Point XpZp)
		{
			List<double> Distancias = new List<double>();

			for (int i = 0; i < points.Count(); i++)
			{
				Distancias.Add(Math.Sqrt((points[i].X - XpZp.X) * (points[i].X - XpZp.X) + (points[i].Z - XpZp.Y) * (points[i].Z - XpZp.Y)));
			}

			return Distancias;
		}

		//Calcular la Xpromedio y la Zpromedio
		Point XpZp(List<SkeletonPoint> points)
		{
			var Xp = 0.0;
			var Zp = 0.0;

			foreach (var varp in points)
			{
				Xp += (varp.X);
				Zp += (varp.Z);
			}
			Xp = Xp / points.Count();
			Zp = Zp / points.Count();

			Point XpZp = new Point(Xp, Zp);
			return XpZp;
		}

		public Model3DGroup GraficarAreaMedida(List<SkeletonPoint> points, Color color)
		{
			//Crear un modelo
			var modelGroup = new Model3DGroup();
			var meshBuiler = new MeshBuilder(false, false);
			/*Point3D x1, x2, x3;
			x1 = new Point3D();
			x2 = new Point3D();
			x3 = new Point3D();*/
			int count = 0;
			foreach (var item in points)
			{
				meshBuiler.AddSphere(new Point3D(item.X, item.Y, item.Z), 0.005);
				count++;
			}
			//for (int i = 0; i < points.Count-3; i+=3)
			//{
			//    x1.X = points[i].X;
			//    x1.Y = -points[i].Y;
			//    x1.Z = points[i].Z;
			//    x2.X = points[i+1].X;
			//    x2.Y = -points[i+1].Y;
			//    x2.Z = points[i+1].Z;
			//    x3.X = points[i+2].X;
			//    x3.Y = -points[i+2].Y;
			//    x3.Z = points[i+2].Z;
			//    meshBuiler.AddTriangle(x1, x2, x3);

			//}


			var mesh = meshBuiler.ToMesh(true);

			var redMaterial = MaterialHelper.CreateMaterial(color);
			var insideMaterial = MaterialHelper.CreateMaterial(color);
			modelGroup.Children.Add(new GeometryModel3D { Geometry = mesh, Material = redMaterial, BackMaterial = insideMaterial });
			return modelGroup;
		}

		//Estimar la altura de la persona
		public void CalcularAltura(SkeletonPoint jointHead, SkeletonPoint jointFootRigth, SkeletonPoint jointFootLeft)
		{
			var temporal = 0.0f;
			altura = (Math.Abs(jointHead.Y - jointFootRigth.Y) + errorAltura) * 100;
			temporal = (Math.Abs(jointHead.Y - jointFootLeft.Y) + errorAltura) * 100;
			altura = (altura + temporal) / 2;

		}

		public void CalcularAlturaPorY(List<SkeletonPoint> vertices)
		{
			float mayorY = vertices.Max(x => x.Y);
			float menorY = vertices.Min(x => x.Y);
			altura = Math.Abs(mayorY - menorY);
		}

		public decimal[] CentroDeMasa(List<SkeletonPoint> vertices)
		{
			decimal[] centroMasa = new decimal[3];
			decimal xCm = 0m;
			decimal yCm = 0m;
			decimal zCm = 0m;
			for (int i = 0; i < vertices.Count(); i++)
			{
				xCm += (decimal)vertices[i].X;
				yCm += (decimal)vertices[i].Y;
				zCm += (decimal)vertices[i].Z;
			}
			xCm = xCm / vertices.Count;
			yCm = yCm / vertices.Count;
			zCm = zCm / vertices.Count;
			centroMasa[0] = xCm;
			centroMasa[1] = yCm;
			centroMasa[2] = zCm;
			return centroMasa;
		}

		public List<SkeletonPoint> GraficarCentrodeMasa(List<SkeletonPoint> vertices, decimal[] centroMasa, float mayorY, float menorY)
		{
			List<SkeletonPoint> sCentroMasa = new List<SkeletonPoint>(vertices.Count());
			float aumento = ((mayorY - menorY) / vertices.Count()) * 2000;
			float yPosition = menorY;
			for (int i = 0; i < vertices.Count(); i++)
			{
				yPosition += aumento;
				sCentroMasa.Add(new SkeletonPoint { X = (float)centroMasa[0], Z = (float)centroMasa[2], Y = i == 0 ? menorY : i == vertices.Count ? mayorY : yPosition });
				if (yPosition >= mayorY)
				{
					sCentroMasa.Add(new SkeletonPoint
					{
						X = (float)centroMasa[0],
						Z = (float)centroMasa[2],
						Y = mayorY
					});
					break;
				}
			}
			return sCentroMasa;
		}

		public List<SkeletonPoint> CalcularPuntosMedidas(List<SkeletonPoint> vertices, int edad, out double epsilon, TipoAvatar tipoAvatar)
		{
			double Ycabeza = 0d, Ycuello = 0d, Ycuello1 = 0d, Ypecho = 0d, Ypecho1 = 0d, Ypecho2 = 0d, Ycintura = 0d, Ycadera = 0d, Ycadera1 = 0d, Ycadera2 = 0d, Ycinturap = 0d, Ep = 0d;
			epsilon = 0;
			CalcularAlturaPorY(vertices);
			float MaxPointY = vertices.Max(x => x.Y);
			decimal[] centroMasa = CentroDeMasa(vertices);
			List<SkeletonPoint> medidas = new List<SkeletonPoint>(11);
			if (edad > 16)
			{
				Ycabeza = (MaxPointY - (Altura * 0.125d) * 0.39d);
				Ypecho = (MaxPointY - (Altura * 0.25d));
				Ypecho1 = (MaxPointY - (Altura) * (0.25d) * (0.9d));
				Ypecho2 = (MaxPointY - (Altura) * (0.25d) * (1.03d));
				Ycintura = (MaxPointY - ((Altura) * (0.375d)));
				Ycinturap = (MaxPointY - ((Altura) * (0.375d)) - (Altura / 24));
				Ycadera = (MaxPointY - ((Altura) * (0.5d)));
				Ycadera1 = (MaxPointY - ((Altura) * (0.5d) * (0.89d)));
				Ycadera2 = (MaxPointY - ((Altura) * (0.5d) * (0.91d)));
				if (tipoAvatar == TipoAvatar.Delantero)
				{
					Ycuello = (MaxPointY - ((Altura * 0.125d) * 1.03d));
					Ycuello1 = (MaxPointY - ((Altura * 0.125d) * 1.18d));
					Ep = Altura / (128 * 2);
				}
				else
				{
					Ycuello = (MaxPointY - ((Altura / 8) * 1.0));
					Ycuello1 = (MaxPointY - ((Altura / 8) * 1.01));
					Ep = Altura / (128 * 2);
					Ep = Ep * 0.8;
				}
			}
			else if (edad > 8 && edad <= 16)
			{
				Ycabeza = (MaxPointY - (Altura / 7.0d) * 0.25d);
				Ycuello = (MaxPointY - (Altura / 7.0d));
				Ypecho = (MaxPointY - ((Altura * 2.0d) / 7.0d));
				Ycintura = (MaxPointY - ((Altura * 3.0d) / 7.0d));
				Ycadera = (MaxPointY - ((Altura * 4.0d) / 7.0d) + (Altura / 14.0d));
				Ep = Altura / (224.0d);
			}
			else if (edad <= 8 && edad > 5)
			{
				Ycabeza = (MaxPointY - ((Altura / 6.0d) * 0.25d));
				Ycuello = (MaxPointY - (Altura / 6.0d));
				Ypecho = (MaxPointY - ((Altura / 3.0d)));
				Ycintura = (MaxPointY - (Altura / 2.0d));
				Ycadera = (MaxPointY - ((Altura * 4.0d) / 6.0d) + (Altura / 12.0d));
				Ep = Altura / (192.0d);
			}
			else if (edad > 1 && edad <= 5)
			{
				Ycabeza = (MaxPointY - (Altura / 5.0d) * 0.250d);
				Ycuello = (MaxPointY - (Altura / 5.0d));
				Ypecho = (MaxPointY - ((Altura * 2.0d) / 5.0d));
				Ycintura = (MaxPointY - ((Altura * 3.0d) / 5.0d) + (Altura * (0.1d)));
				Ycadera = (MaxPointY - ((Altura * 3.0d) / 5.0d));
				Ep = Altura / (160.0d);
			}
			else if (edad <= 1)
			{
				Ycabeza = (MaxPointY - (Altura / 4.0d) * 0.25d);
				Ycuello = (MaxPointY - (Altura / 4.0d));
				Ypecho = (MaxPointY - ((Altura) * (0.5d)) + ((Altura / 8.0d)));
				Ycintura = (MaxPointY - ((Altura) * (0.5d)));
				Ycadera = (MaxPointY - ((Altura * 3.0d) / 4.0d) + (Altura / 8.0d));
				Ep = Altura / (128.0d);
			}
			//Cabeza
			medidas.Add(new SkeletonPoint { X = (float)centroMasa[0], Z = (float)centroMasa[2], Y = (float)Ycabeza });
			//Cuello
			medidas.Add(new SkeletonPoint { X = (float)centroMasa[0], Z = (float)centroMasa[2], Y = (float)Ycuello });
			//Cuello1
			medidas.Add(new SkeletonPoint { X = (float)centroMasa[0], Z = (float)centroMasa[2], Y = (float)Ycuello1 });
			//Pecho
			medidas.Add(new SkeletonPoint { X = (float)centroMasa[0], Z = (float)centroMasa[2], Y = (float)Ypecho });
			//Cintura
			medidas.Add(new SkeletonPoint { X = (float)centroMasa[0], Z = (float)centroMasa[2], Y = (float)Ycintura });
			//Cadera
			medidas.Add(new SkeletonPoint { X = (float)centroMasa[0], Z = (float)centroMasa[2], Y = (float)Ycadera });
			//CinturaP
			medidas.Add(new SkeletonPoint { X = (float)centroMasa[0], Z = (float)centroMasa[2], Y = (float)Ycinturap });
			//Cadera 1
			medidas.Add(new SkeletonPoint { X = (float)centroMasa[0], Z = (float)centroMasa[2], Y = (float)Ycadera1 });
			//Cadera 2
			medidas.Add(new SkeletonPoint { X = (float)centroMasa[0], Z = (float)centroMasa[2], Y = (float)Ycadera2 });
			//Pecho 1
			medidas.Add(new SkeletonPoint { X = (float)centroMasa[0], Z = (float)centroMasa[2], Y = (float)Ypecho1 });
			//Pecho 2
			medidas.Add(new SkeletonPoint { X = (float)centroMasa[0], Z = (float)centroMasa[2], Y = (float)Ypecho2 });
			epsilon = Ep;
			return medidas;
		}

		public List<SkeletonPoint> CalcularPuntosCuelloP(List<SkeletonPoint> vertices, int edad, out double epsilon)
		{
			float MaxPointY = vertices.Max(x => x.Y);
			double Ycuellop1 = 0d, Ycuellop2 = 0d;
			epsilon = 0;
			List<SkeletonPoint> medidas = new List<SkeletonPoint>(2);
			if (edad > 16)
			{
				//% Para persona mayor a 20 años
				Ycuellop1 = (MaxPointY - ((Altura * 0.125d) * 1.0d));
				Ycuellop2 = (MaxPointY - ((Altura * 0.125d) * 1.02d));
				epsilon = Altura / (256.0d);
				epsilon = epsilon * 0.8d;
			}
			else if (edad > 8.0m && edad <= 16.0m)
			{
				Ycuellop1 = (MaxPointY - (Altura / 7.0d) * 1.02d);
				Ycuellop2 = (MaxPointY - (Altura / 7.0d));
				epsilon = Altura / (224.0d);
			}
			else if (edad <= 8.0m && edad > 5.0m)
			{
				Ycuellop1 = (MaxPointY - (Altura / 6.0d));
				Ycuellop2 = (MaxPointY - (Altura / 6.0d) * 1.02d);
				epsilon = Altura / (192.0d);
			}
			else if (edad > 1.0m && edad <= 5.0m)
			{
				Ycuellop1 = (MaxPointY - (Altura / 5.0d));
				Ycuellop2 = (MaxPointY - (Altura / 5.0d) * 1.02d);
				epsilon = Altura / (160.0d);
			}
			else if (edad <= 1.0m)
			{
				Ycuellop1 = (MaxPointY - (Altura / 4.0d));
				Ycuellop2 = (MaxPointY - (Altura / 4.0d) * 1.02d);
				epsilon = Altura / (128.0d);
			}
			//CuelloP1
			medidas.Add(new SkeletonPoint { X = 0, Z = 0, Y = (float)Ycuellop1 });
			//CuelloP2
			medidas.Add(new SkeletonPoint { X = 0, Z = 0, Y = (float)Ycuellop2 });
			return medidas;
		}

		//Para crear una franja en base a un punto.
		public List<SkeletonPoint> DibujarFranjaHorizontal(List<SkeletonPoint> vertices, SkeletonPoint jointCoo, double epsilon)
		{
			List<SkeletonPoint> franja = new List<SkeletonPoint>();
			foreach (var item in vertices)
			{
				if (item.Y >= jointCoo.Y - epsilon && item.Y <= jointCoo.Y + epsilon)
				{
					franja.Add(item);
				}
			}
			EliminarDuplicados(franja);
			return franja;
		}

		public List<SkeletonPoint> CrearFranjaHorizontal(List<SkeletonPoint> vertices, SkeletonPoint jointCoo, double epsilon)
		{
			List<SkeletonPoint> franja = new List<SkeletonPoint>();
			foreach (var item in vertices)
			{
				if (item.Y >= (jointCoo.Y - epsilon) && item.Y <= (jointCoo.Y + epsilon))
				{
					franja.Add(item);
				}
			}
			return franja;
		}

		public List<SkeletonPoint> CrearFranjaHorizontal(List<SkeletonPoint> vertices, SkeletonPoint jointCoo, double epsilon, double increment)
		{
			List<SkeletonPoint> franja = new List<SkeletonPoint>();
			foreach (var item in vertices)
			{
				if (item.Y >= (jointCoo.Y - epsilon * increment) && item.Y <= (jointCoo.Y + epsilon * increment))
				{
					franja.Add(item);
				}
			}
			return franja;
		}

		public List<SkeletonPoint> CrearFranjaHorizontal(List<SkeletonPoint> vertices, SkeletonPoint jointCoo, double epsilon, int increment)
		{
			List<SkeletonPoint> franja = new List<SkeletonPoint>();
			foreach (var item in vertices)
			{
				if (item.Y >= (jointCoo.Y - increment * epsilon) && item.Y <= (jointCoo.Y + increment * epsilon))
				{
					franja.Add(item);
				}
			}
			return franja;
		}

		public Model3D PaintPoints()
		{
			/*double epsilon;
			List<SkeletonPoint> medidas = CalcularPuntosMedidas(26, out epsilon, TipoAvatar.Delantero);
			return GraficarAreaMedida(DibujarFranjaHorizontal(medidas[5], epsilon), Colors.Red);*/
			return new Model3DGroup();
		}

		public List<Model3D> PaintPointsCadera(List<SkeletonPoint> vertices, TipoAvatar tipoAvatar, char sexo, int edad)
		{
			List<Model3D> lstModelCadera = new List<Model3D>();
			double epsilon;
			List<SkeletonPoint> medidas = CalcularPuntosMedidas(vertices, edad, out epsilon, tipoAvatar);
			lstModelCadera.Add(GraficarAreaMedida(DibujarFranjaHorizontal(vertices, medidas[0], epsilon), Colors.Red));
			//lstModelCadera.Add(GraficarAreaMedida(DibujarFranjaHorizontal(vertices, medidas[2], epsilon), Colors.White));
			//List<SkeletonPoint> medidasCuello = CalcularPuntosCuelloP(vertices, edad, out epsilon);
			//lstModelCadera.Add(GraficarAreaMedida(DibujarFranjaHorizontal(vertices, medidasCuello[0], epsilon), Colors.Yellow));
			//lstModelCadera.Add(GraficarAreaMedida(DibujarFranjaHorizontal(vertices, medidasCuello[1], epsilon), Colors.Green));
			return lstModelCadera;
		}

		public decimal[] CalcularPerimetro(List<SkeletonPoint> vertices, TipoAvatar tipoAvatar, char sexo, int edad)
		{
			double epsilon;
			List<SkeletonPoint> pointsCabeza, pointsCuello, pointsPecho, pointsCintura, pointsCadera, pointsCinturaP, pointsCuello1, pointsCadera1, pointsCadera2, pointsPecho1, pointsPecho2, pointsCuelloP1, pointsCuelloP2;
			List<SkeletonPoint> pointsCabezaPri, pointsCuelloPri, pointsPechoPri, pointsCinturaPri, pointsCaderaPri, pointsCinturaPPri, pointsCuello1Pri, pointsCadera1Pri, pointsCadera2Pri, pointsPecho1Pri, pointsPecho2Pri, pointsCuelloP1Pri, pointsCuelloP2Pri;
			decimal zCabezaMinimo, zCuelloMinimo, zPechoMinimo, zCinturaMinimo, zCaderaMinimo, zCinturaPMinimo, zCuello1Minimo, zCabezaCm, zCuelloCm, zPechoCm, zCinturaCm, zCaderaCm, zCinturaPCm, zCuello1Cm, zCadera1Minimo, zCadera2Minimo, zCadera1Cm, zCadera2Cm, zPecho1Minimo, zPecho2Minimo, zPecho1Cm, zPecho2Cm;
			decimal[] centroMasa = CentroDeMasa(vertices);
			//cálculo de anillos alrededor de zonas de interes
			List<SkeletonPoint> medidas = CalcularPuntosMedidas(vertices, edad, out epsilon, tipoAvatar);
			pointsCabeza = CrearFranjaHorizontal(vertices, medidas[0], epsilon);
			pointsCuello = CrearFranjaHorizontal(vertices, medidas[1], epsilon);
			pointsPecho = CrearFranjaHorizontal(vertices, medidas[3], epsilon);
			pointsCintura = CrearFranjaHorizontal(vertices, medidas[4], epsilon);
			pointsCadera = CrearFranjaHorizontal(vertices, medidas[5], epsilon);
			pointsCinturaP = CrearFranjaHorizontal(vertices, medidas[6], epsilon);
			pointsCuello1 = CrearFranjaHorizontal(vertices, medidas[2], epsilon, 0.4d);
			pointsCadera1 = CrearFranjaHorizontal(vertices, medidas[7], epsilon);
			pointsCadera2 = CrearFranjaHorizontal(vertices, medidas[8], epsilon);
			pointsPecho1 = CrearFranjaHorizontal(vertices, medidas[9], epsilon);
			pointsPecho2 = CrearFranjaHorizontal(vertices, medidas[10], epsilon);

			List<SkeletonPoint> medidasCuelloP = CalcularPuntosCuelloP(vertices, edad, out epsilon);
			pointsCuelloP1 = CrearFranjaHorizontal(vertices, medidasCuelloP[0], epsilon, 10);
			pointsCuelloP2 = CrearFranjaHorizontal(vertices, medidasCuelloP[1], epsilon, 10);

			pointsCabezaPri = new List<SkeletonPoint>(pointsCabeza.Count);
			pointsCuelloPri = new List<SkeletonPoint>(pointsCuello.Count);
			pointsPechoPri = new List<SkeletonPoint>(pointsPecho.Count);
			pointsCinturaPri = new List<SkeletonPoint>(pointsCintura.Count);
			pointsCaderaPri = new List<SkeletonPoint>(pointsCadera.Count);
			pointsCinturaPPri = new List<SkeletonPoint>(pointsCinturaP.Count);
			pointsCuello1Pri = new List<SkeletonPoint>(pointsCuello1.Count);
			pointsCadera1Pri = new List<SkeletonPoint>(pointsCadera1.Count);
			pointsCadera2Pri = new List<SkeletonPoint>(pointsCadera2.Count);
			pointsPecho1Pri = new List<SkeletonPoint>(pointsPecho1.Count);
			pointsPecho2Pri = new List<SkeletonPoint>(pointsPecho2.Count);
			pointsCuelloP1Pri = new List<SkeletonPoint>(pointsCuelloP1.Count);
			pointsCuelloP2Pri = new List<SkeletonPoint>(pointsCuelloP2.Count);

			//calculo del centro de masa optimo para eje "z"
			zCabezaMinimo = (decimal)pointsCabeza.Min(x => x.Z);
			zCuelloMinimo = (decimal)pointsCuello.Min(x => x.Z);
			zPechoMinimo = (decimal)pointsPecho.Min(x => x.Z);
			zCinturaMinimo = (decimal)pointsCintura.Min(x => x.Z);
			zCaderaMinimo = (decimal)pointsCadera.Min(x => x.Z);
			zCinturaPMinimo = (decimal)pointsCinturaP.Min(x => x.Z);
			zCuello1Minimo = (decimal)pointsCuello1.Min(x => x.Z);
			zCadera1Minimo = (decimal)pointsCadera1.Min(x => x.Z);
			zCadera2Minimo = (decimal)pointsCadera2.Min(x => x.Z);
			zPecho1Minimo = (decimal)pointsPecho1.Min(x => x.Z);
			zPecho2Minimo = (decimal)pointsPecho2.Min(x => x.Z);

			if (zCabezaMinimo < centroMasa[2])
				zCabezaCm = (centroMasa[2] + zCabezaMinimo) / 2;
			else
				zCabezaCm = zCabezaMinimo;

			if (zCuelloMinimo < centroMasa[2])
				zCuelloCm = (centroMasa[2] + zCuelloMinimo) / 2;
			else
				zCuelloCm = zCuelloMinimo;

			if (zPechoMinimo < centroMasa[2])
				zPechoCm = centroMasa[2];
			else
				zPechoCm = zPechoMinimo;

			if (zCinturaMinimo < centroMasa[2])
				zCinturaCm = (centroMasa[2] + zCinturaMinimo) / 2;
			else
				zCinturaCm = zCinturaMinimo;

			if (zCaderaMinimo < centroMasa[2])
				zCaderaCm = (centroMasa[2] + zCaderaMinimo) / 2;
			else
				zCaderaCm = zCaderaMinimo;

			if (zCinturaPMinimo < centroMasa[2])
				zCinturaPCm = (centroMasa[2] + zCinturaPMinimo) / 2;
			else
				zCinturaPCm = zCinturaPMinimo;

			if (zCuello1Minimo < centroMasa[2])
				zCuello1Cm = (centroMasa[2] + zCuello1Minimo) / 2;
			else
				zCuello1Cm = zCuello1Minimo;

			if (zCadera1Minimo < centroMasa[2])
				zCadera1Cm = (centroMasa[2] + zCadera1Minimo) / 2;
			else
				zCadera1Cm = zCadera1Minimo;

			if (zCadera2Minimo < centroMasa[2])
				zCadera2Cm = (centroMasa[2] + zCadera2Minimo) / 2;
			else
				zCadera2Cm = zCadera2Minimo;

			if (zPecho1Minimo < centroMasa[2])
				zPecho1Cm = centroMasa[2];
			else
				zPecho1Cm = zPecho1Minimo;

			if (zPecho2Minimo < centroMasa[2])
				zPecho2Cm = centroMasa[2];
			else
				zPecho2Cm = zPecho2Minimo;

			//Calculo de lo que ve el nuevo sistema de referencia de los anillos donde se calcula el perimetro
			for (int i = 0; i < pointsCabeza.Count; i++)
			{
				pointsCabezaPri.Add(new SkeletonPoint
				{
					X = pointsCabeza[i].X - (float)centroMasa[0],
					Y = pointsCabeza[i].Y - (float)centroMasa[1],
					Z = pointsCabeza[i].Z - (float)zCabezaCm
				});
			}

			for (int i = 0; i < pointsCuello.Count; i++)
			{
				pointsCuelloPri.Add(new SkeletonPoint
				{
					X = pointsCuello[i].X - (float)centroMasa[0],
					Y = pointsCuello[i].Y - (float)centroMasa[1],
					Z = pointsCuello[i].Z - (float)zCuelloCm
				});
			}

			for (int i = 0; i < pointsPecho.Count; i++)
			{
				pointsPechoPri.Add(new SkeletonPoint
				{
					X = pointsPecho[i].X - (float)centroMasa[0],
					Y = pointsPecho[i].Y - (float)centroMasa[1],
					Z = pointsPecho[i].Z - (float)zPechoCm
				});
			}

			for (int i = 0; i < pointsCintura.Count; i++)
			{
				pointsCinturaPri.Add(new SkeletonPoint
				{
					X = pointsCintura[i].X - (float)centroMasa[0],
					Y = pointsCintura[i].Y - (float)centroMasa[1],
					Z = pointsCintura[i].Z - (float)zCinturaCm
				});
			}

			for (int i = 0; i < pointsCadera.Count; i++)
			{
				pointsCaderaPri.Add(new SkeletonPoint
				{
					X = pointsCadera[i].X - (float)centroMasa[0],
					Y = pointsCadera[i].Y - (float)centroMasa[1],
					Z = pointsCadera[i].Z - (float)zCaderaCm
				});
			}

			for (int i = 0; i < pointsCinturaP.Count; i++)
			{
				pointsCinturaPPri.Add(new SkeletonPoint
				{
					X = pointsCinturaP[i].X - (float)centroMasa[0],
					Y = pointsCinturaP[i].Y - (float)centroMasa[1],
					Z = pointsCinturaP[i].Z - (float)zCinturaPCm
				});
			}

			for (int i = 0; i < pointsCuello1.Count; i++)
			{
				pointsCuello1Pri.Add(new SkeletonPoint
				{
					X = pointsCuello1[i].X - (float)centroMasa[0],
					Y = pointsCuello1[i].Y - (float)centroMasa[1],
					Z = pointsCuello1[i].Z - (float)zCuello1Cm
				});
			}

			for (int i = 0; i < pointsCadera1.Count; i++)
			{
				pointsCadera1Pri.Add(new SkeletonPoint
				{
					X = pointsCadera1[i].X - (float)centroMasa[0],
					Y = pointsCadera1[i].Y - (float)centroMasa[1],
					Z = pointsCadera1[i].Z - (float)zCadera1Cm
				});
			}

			for (int i = 0; i < pointsCadera2.Count; i++)
			{
				pointsCadera2Pri.Add(new SkeletonPoint
				{
					X = pointsCadera2[i].X - (float)centroMasa[0],
					Y = pointsCadera2[i].Y - (float)centroMasa[1],
					Z = pointsCadera2[i].Z - (float)zCadera2Cm
				});
			}

			for (int i = 0; i < pointsPecho1.Count; i++)
			{
				pointsPecho1Pri.Add(new SkeletonPoint
				{
					X = pointsPecho1[i].X - (float)centroMasa[0],
					Y = pointsPecho1[i].Y - (float)centroMasa[1],
					Z = pointsPecho1[i].Z - (float)zPecho1Cm
				});
			}

			for (int i = 0; i < pointsPecho2.Count; i++)
			{
				pointsPecho2Pri.Add(new SkeletonPoint
				{
					X = pointsPecho2[i].X - (float)centroMasa[0],
					Y = pointsPecho2[i].Y - (float)centroMasa[1],
					Z = pointsPecho2[i].Z - (float)zPecho2Cm
				});
			}

			for (int i = 0; i < pointsCuelloP1.Count; i++)
			{
				pointsCuelloP1Pri.Add(new SkeletonPoint
				{
					X = pointsCuelloP1[i].X - (float)centroMasa[0],
					Y = pointsCuelloP1[i].Y - medidasCuelloP[0].Y,
					Z = pointsCuelloP1[i].Z - (float)centroMasa[2]
				});
			}

			for (int i = 0; i < pointsCuelloP2.Count; i++)
			{
				pointsCuelloP2Pri.Add(new SkeletonPoint
				{
					X = pointsCuelloP2[i].X - (float)centroMasa[0],
					Y = pointsCuelloP2[i].Y - medidasCuelloP[1].Y,
					Z = pointsCuelloP2[i].Z - (float)centroMasa[2]
				});
			}

			decimal[] perimetros = new decimal[14];
			perimetros[0] = CalcularPerimetroPecho(pointsPechoPri, centroMasa, zPechoCm, tipoAvatar, zPechoMinimo);
			perimetros[1] = CalcularPerimetroCintura(pointsCinturaPri, centroMasa, zCinturaCm, tipoAvatar, zCinturaMinimo);
			perimetros[2] = CalcularPerimetroCuello(pointsCuelloPri, centroMasa, zCuelloCm, tipoAvatar);
			perimetros[3] = CalcularPerimetroCabeza(pointsCabezaPri, centroMasa, zCabezaCm, tipoAvatar);
			perimetros[4] = CalcularPerimetroCadera(pointsCaderaPri, centroMasa, zCaderaCm, sexo, zCaderaMinimo);
			perimetros[5] = CalcularPerimetroCinturaP(pointsCinturaPPri, centroMasa, zCinturaPCm, tipoAvatar, zCinturaPMinimo);
			perimetros[6] = CalcularPerimetroCuello1(pointsCuello1Pri, centroMasa, zCuello1Cm, zCuello1Minimo, (decimal)pointsCuelloPri.Min(x => x.X), (decimal)pointsCuelloPri.Max(x => x.X));
			perimetros[7] = CalcularCuelloVariante(vertices, edad, tipoAvatar);
			perimetros[8] = CalcularPerimetroCadera(pointsCadera1Pri, centroMasa, zCadera1Cm, sexo, zCadera1Minimo);
			perimetros[9] = CalcularPerimetroCadera(pointsCadera2Pri, centroMasa, zCadera2Cm, sexo, zCadera2Minimo);
			perimetros[10] = CalcularPerimetroPecho(pointsPecho1Pri, centroMasa, zPecho1Cm, tipoAvatar, zPecho1Minimo);
			perimetros[11] = CalcularPerimetroPecho(pointsPecho2Pri, centroMasa, zPecho2Cm, tipoAvatar, zPecho2Minimo);
			perimetros[12] = CalcularCuelloP(pointsCuelloP1Pri, centroMasa, (decimal)medidasCuelloP[0].Y, (decimal)epsilon, tipoAvatar);
			perimetros[13] = CalcularCuelloP(pointsCuelloP2Pri, centroMasa, (decimal)medidasCuelloP[1].Y, (decimal)epsilon, tipoAvatar);
			return perimetros;
		}

		public decimal CalcularPerimetroPecho(List<SkeletonPoint> PointsPechoPri, decimal[] centroMasa, decimal zPechoCm, TipoAvatar tipoAvatar, decimal zPechoMinimo)
		{
			decimal SUMPPE = 0.0m;
			decimal[] SUMPPES = new decimal[PointsPechoPri.Count];
			for (int k = 0; k < PointsPechoPri.Count; k++)
			{
				SUMPPES[k] = (decimal)Math.Sqrt(Math.Pow(PointsPechoPri[k].X, 2) + Math.Pow(PointsPechoPri[k].Z, 2));
			}

			decimal ProPE = SUMPPES.Sum() / SUMPPES.Length;
			decimal DPE = 0.0m;
			for (int k = 0; k < SUMPPES.Length; k++)
			{
				DPE = DPE + (decimal)Math.Pow((double)(SUMPPES[k] - ProPE), 2);
			}
			DPE = DPE / (SUMPPES.Length - 1);
			DPE = (decimal)Math.Sqrt((double)DPE);

			decimal PorPE;
			PorPE = DPE / ProPE;
			decimal APE;
			if (PorPE > 0.35m)
			{
				APE = (1.2m) * DPE * (1.0m - PorPE) + ProPE;
			}
			else
			{
				APE = (3.0m) * DPE * (1.0m - PorPE) + ProPE;
			}

			decimal Delta = 0;
			if (tipoAvatar == TipoAvatar.Delantero)
				Delta = 2.0m;
			else
				Delta = 2.4m;

			int nmax = Convert.ToInt32(360 / Delta);

			decimal Titai = 0, Titaf = 0, Tita = 0;
			decimal[] Xpeprif = new decimal[nmax];
			decimal[] Zpeprif = new decimal[nmax];
			int j;
			for (int q = 0; q < nmax; q++)
			{
				j = 0;
				decimal[] R = new decimal[PointsPechoPri.Count];
				decimal[] Xpepri1 = new decimal[PointsPechoPri.Count];
				decimal[] Zpepri1 = new decimal[PointsPechoPri.Count];

				Titai = q * Delta;
				Titaf = (q + 1) * Delta;

				for (int k = 0; k < PointsPechoPri.Count; k++)
				{
					if (PointsPechoPri[k].X >= 0 && PointsPechoPri[k].Z >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsPechoPri[k].X / (PointsPechoPri[k].Z)));
					}
					if (PointsPechoPri[k].X < 0 && PointsPechoPri[k].Z >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsPechoPri[k].X / (PointsPechoPri[k].Z)));
					}
					if (PointsPechoPri[k].X <= 0 && PointsPechoPri[k].Z < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsPechoPri[k].X / (PointsPechoPri[k].Z))) + 180.0m;
					}
					if (PointsPechoPri[k].X > 0 && PointsPechoPri[k].Z < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsPechoPri[k].X / (PointsPechoPri[k].Z))) + 180.0m;
					}

					if (Tita > Titai && Tita <= Titaf && Tita <= 180.0m && (decimal)PointsPechoPri[k].Z >= (-zPechoCm + centroMasa[2]) && (decimal)Math.Sqrt(Math.Pow(PointsPechoPri[k].X, 2) + Math.Pow(PointsPechoPri[k].Z, 2)) < APE)
					{
						Xpepri1[j] = (decimal)PointsPechoPri[k].X;
						Zpepri1[j] = (decimal)PointsPechoPri[k].Z;
						R[j] = (decimal)Math.Sqrt(Math.Pow(PointsPechoPri[k].X, 2) + Math.Pow(PointsPechoPri[k].Z, 2));
						j++;
					}
				}

				int l4;
				decimal minf1;
				l4 = j;
				if (l4 > 0)
				{
					minf1 = R[0];
					j = 0;
					for (int i = 0; i < l4 - 1; i++)
					{
						if (minf1 > R[i + 1])
						{
							minf1 = R[i + 1];
							j = i + 1;
						}
					}
					Xpeprif[q] = Xpepri1[j];
					Zpeprif[q] = Zpepri1[j];
				}
				else
				{
					Xpeprif[q] = 0;
					Zpeprif[q] = 0;
				}
				Array.Clear(R, 0, PointsPechoPri.Count);
				Array.Clear(Xpepri1, 0, PointsPechoPri.Count);
				Array.Clear(Zpepri1, 0, PointsPechoPri.Count);
			}

			int l = 0;
			int[] SJJ = new int[Xpeprif.Length];
			for (int k = 0; k < Xpeprif.Length; k++)
			{
				if (Xpeprif[k] != 0 && Zpeprif[k] != 0)
				{
					SJJ[l] = k;
					l++;
				}
			}
			int b1;
			b1 = l;
			decimal[] Xpeprifn = new decimal[b1];
			decimal[] Zpeprifn = new decimal[b1];

			if (b1 > 0)
			{
				for (int k = 0; k < b1; k++)
				{
					Xpeprifn[k] = Xpeprif[SJJ[k]];
					Zpeprifn[k] = Zpeprif[SJJ[k]];
				}
				Array.Clear(Xpeprif, 0, Xpeprif.Length);
				Array.Clear(Zpeprif, 0, Zpeprif.Length);

				for (int k = 0; k < b1; k++)
				{
					Xpeprif[k] = Xpeprifn[k];
					Zpeprif[k] = Zpeprifn[k];
				}
				decimal[] Tita1 = new decimal[b1];
				for (int k = 0; k < b1; k++)
				{
					if (Xpeprif[k] >= 0 && Zpeprif[k] >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(((double)Xpeprif[k]) / ((double)Zpeprif[k])));
					}
					if (Xpeprif[k] < 0 && Zpeprif[k] >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(((double)Xpeprif[k]) / ((double)Zpeprif[k])));
					}
					if (Xpeprif[k] <= 0 && Zpeprif[k] < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(((double)Xpeprif[k]) / ((double)Zpeprif[k]))) + 180.0m;
					}
					if (Xpeprif[k] > 0 && Zpeprif[k] < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(((double)Xpeprif[k]) / ((double)Zpeprif[k]))) + 180.0m;
					}
					Tita1[k] = Tita;
				}

				//Ordeno en funcion del angulo segun el angulo de menor a mayor
				decimal a, b, c, minf2;
				int l6 = b1;
				if (l6 > 0)
				{
					for (int k = 0; k < l6 - 1; k++)
					{
						minf2 = Tita1[k];
						j = k;
						for (int i = k; i < l6 - 1; i++)
						{
							if (minf2 > Tita1[i + 1])
							{
								minf2 = Tita1[i + 1];
								j = i + 1;
							}
						}
						a = Tita1[k];
						b = Xpeprif[k];
						c = Zpeprif[k];
						Xpeprif[k] = Xpeprif[j];
						Zpeprif[k] = Zpeprif[j];
						Xpeprif[j] = b;
						Zpeprif[j] = c;
						Tita1[k] = Tita1[j];
						Tita1[j] = a;
					}
				}
				//Perimetro
				decimal[] SUMPPE1 = new decimal[l6 - 1];
				for (int k = 0; k < l6 - 1; k++)
				{
					SUMPPE = SUMPPE + (decimal)Math.Sqrt(Math.Pow(Convert.ToDouble(Xpeprif[k + 1] - Xpeprif[k]), 2) + Math.Pow(Convert.ToDouble(Zpeprif[k + 1] - Zpeprif[k]), 2));
					SUMPPE1[k] = (decimal)Math.Sqrt(Math.Pow(Convert.ToDouble(Xpeprif[k + 1] - Xpeprif[k]), 2) + Math.Pow(Convert.ToDouble(Zpeprif[k + 1] - Zpeprif[k]), 2));
				}
			}
			else
			{
				SUMPPE = 3000.0m;
			}
			decimal varAux = Zpeprif[0];
			j = 0;
			for (int i = 0; i < Zpeprif.Length - 1; i++)
			{
				if (varAux > Zpeprif[i + 1] && Xpeprif[i + 1] > 0)
				{
					varAux = Zpeprif[i + 1];
					j = i + 1;
				}
			}
			decimal ZPE1 = Math.Abs(Zpeprif[j]);
			varAux = Zpeprif[0];
			j = 0;
			for (int i = 0; i < Zpeprif.Length - 1; i++)
			{
				if (varAux > Zpeprif[i + 1] && Xpeprif[i + 1] < 0)
				{
					varAux = Zpeprif[i + 1];
					j = i + 1;
				}
			}
			decimal ZPE2 = Math.Abs(Zpeprif[j]);
			SUMPPE = SUMPPE + ZPE1 + ZPE2;

			return SUMPPE;
		}

		public decimal CalcularPerimetroCintura(List<SkeletonPoint> PointsCinturaPri, decimal[] centroMasa, decimal zCinturaCm, TipoAvatar tipoAvatar, decimal zCinturaMinimo)
		{
			decimal SUMPCI = 0.0m;
			decimal[] SUMPCIS = new decimal[PointsCinturaPri.Count];
			for (int k = 0; k < PointsCinturaPri.Count; k++)
			{
				SUMPCIS[k] = (decimal)Math.Sqrt(Math.Pow(PointsCinturaPri[k].X, 2) + Math.Pow(PointsCinturaPri[k].Z, 2));
			}
			decimal ProCI = SUMPCIS.Sum() / SUMPCIS.Length;

			decimal DCI = 0.0m;
			for (int k = 0; k < SUMPCIS.Length; k++)
			{
				DCI = DCI + (decimal)(Math.Pow(Convert.ToDouble(SUMPCIS[k] - ProCI), 2));
			}
			DCI = DCI / (SUMPCIS.Length - 1.0m);
			DCI = (decimal)Math.Sqrt((double)DCI);

			decimal PorCI;
			PorCI = DCI / ProCI;

			decimal ACI;
			if (PorCI > 0.45m)
				ACI = (2.0m) * DCI * (1.0m - PorCI) + ProCI;
			else
				ACI = (4.0m) * DCI * (1.0m - PorCI) + ProCI;
			decimal Delta1 = 2.0m;
			int nmax = Convert.ToInt32(360 / Delta1);
			decimal[] Xcinprif = new decimal[nmax];
			decimal[] Zcinprif = new decimal[nmax];
			decimal Titai = 0, Titaf = 0, Tita = 0;
			int j;
			for (int q = 0; q < nmax; q++)
			{
				decimal[] R1 = new decimal[PointsCinturaPri.Count];
				decimal[] Xcinpri1 = new decimal[PointsCinturaPri.Count];
				decimal[] Zcinpri1 = new decimal[PointsCinturaPri.Count];

				Titai = q * Delta1;
				Titaf = (q + 1) * Delta1;

				j = 0;
				for (int k = 0; k < PointsCinturaPri.Count; k++)
				{
					if (PointsCinturaPri[k].X >= 0 && PointsCinturaPri[k].Z >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCinturaPri[k].X / PointsCinturaPri[k].Z));
					}
					if (PointsCinturaPri[k].X < 0 && PointsCinturaPri[k].Z >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCinturaPri[k].X / PointsCinturaPri[k].Z));
					}
					if (PointsCinturaPri[k].X <= 0 && PointsCinturaPri[k].Z < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCinturaPri[k].X / PointsCinturaPri[k].Z)) + 180.0m;
					}
					if (PointsCinturaPri[k].X > 0 && PointsCinturaPri[k].Z < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCinturaPri[k].X / PointsCinturaPri[k].Z)) + 180.0m;
					}

					if (Tita > Titai && Tita <= Titaf && Tita <= 180 && (decimal)PointsCinturaPri[k].Z >= (-zCinturaCm + centroMasa[2]) && (decimal)Math.Sqrt(Math.Pow(PointsCinturaPri[k].X, 2) + Math.Pow(PointsCinturaPri[k].Z, 2)) < ACI)
					{
						Xcinpri1[j] = (decimal)PointsCinturaPri[k].X;
						Zcinpri1[j] = (decimal)PointsCinturaPri[k].Z;
						R1[j] = (decimal)Math.Sqrt(Math.Pow(PointsCinturaPri[k].X, 2) + Math.Pow(PointsCinturaPri[k].Z, 2));
						j++;
					}
				}
				int l4;
				decimal minf1;
				l4 = j;
				if (l4 > 0)
				{
					minf1 = R1[0];
					j = 0;
					for (int i = 0; i < l4 - 1; i++)
					{
						if (minf1 > R1[i + 1])
						{
							minf1 = R1[i + 1];
							j = i + 1;
						}
					}
					Xcinprif[q] = Xcinpri1[j];
					Zcinprif[q] = Zcinpri1[j];
				}
				else
				{
					Xcinprif[q] = 0;
					Zcinprif[q] = 0;
				}
			}
			j = 0;
			int[] SJH = new int[Xcinprif.Length];
			for (int k = 0; k < Xcinprif.Length; k++)
			{
				if (Xcinprif[k] != 0 && Zcinprif[k] != 0)
				{
					SJH[j] = k;
					j++;
				}
			}
			int b1 = j;
			decimal[] Xcinprifn = new decimal[b1];
			decimal[] Zcinprifn = new decimal[b1];

			if (b1 > 0)
			{
				for (int k = 0; k < b1; k++)
				{
					Xcinprifn[k] = Xcinprif[SJH[k]];
					Zcinprifn[k] = Zcinprif[SJH[k]];
				}
				Array.Clear(Xcinprif, 0, Xcinprif.Length);
				Array.Clear(Zcinprif, 0, Zcinprif.Length);
				for (int k = 0; k < b1; k++)
				{
					Xcinprif[k] = Xcinprifn[k];
					Zcinprif[k] = Zcinprifn[k];
				}

				decimal[] Tita2 = new decimal[b1];
				for (int k = 0; k < b1; k++)
				{
					if (Xcinprif[k] >= 0 && Zcinprif[k] >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(Xcinprif[k] / Zcinprif[k])));
					}
					if (Xcinprif[k] < 0 && Zcinprif[k] >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(Xcinprif[k] / Zcinprif[k])));
					}
					if (Xcinprif[k] <= 0 && Zcinprif[k] < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(Xcinprif[k] / Zcinprif[k]))) + 180.0m;
					}
					if (Xcinprif[k] > 0 && Zcinprif[k] < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(Xcinprif[k] / Zcinprif[k]))) + 180.0m;
					}
					Tita2[k] = Tita;
				}

				// Ordeno en funcion del angulo segun el angulo de menor a mayor
				int l6 = b1;
				decimal a, b, c, minf2;
				if (l6 > 0)
				{
					for (int k = 0; k < l6 - 1; k++)
					{
						minf2 = Tita2[k];
						j = k;
						for (int i = k; i < l6 - 1; i++)
						{
							if (minf2 > Tita2[i + 1])
							{
								minf2 = Tita2[i + 1];
								j = i + 1;
							}
						}
						a = Tita2[k];
						b = Xcinprif[k];
						c = Zcinprif[k];
						Xcinprif[k] = Xcinprif[j];
						Zcinprif[k] = Zcinprif[j];
						Xcinprif[j] = b;
						Zcinprif[j] = c;
						Tita2[k] = Tita2[j];
						Tita2[j] = a;
					}
				}
				// Perimetro
				decimal[] SUMPCI1 = new decimal[l6 - 1];
				for (int k = 0; k < l6 - 1; k++)
				{
					SUMPCI = SUMPCI + (decimal)Math.Sqrt(Math.Pow(Convert.ToDouble(Xcinprif[k + 1] - Xcinprif[k]), 2) + Math.Pow(Convert.ToDouble(Zcinprif[k + 1] - Zcinprif[k]), 2));

					SUMPCI1[k] = (decimal)Math.Sqrt(Math.Pow(Convert.ToDouble(Xcinprif[k + 1] - Xcinprif[k]), 2) + Math.Pow(Convert.ToDouble(Zcinprif[k + 1] - Zcinprif[k]), 2));
				}
			}
			else
			{
				SUMPCI = 3000.0m;
			}
			decimal varAux = Zcinprif[0];
			j = 0;
			for (int i = 0; i < Zcinprif.Length - 1; i++)
			{
				if (varAux > Zcinprif[i + 1] && Xcinprif[i + 1] > 0)
				{
					varAux = Zcinprif[i + 1];
					j = i + 1;
				}
			}
			decimal ZCIN1 = Math.Abs(Zcinprif[j]);
			varAux = Zcinprif[0];
			j = 0;
			for (int i = 0; i < Zcinprif.Length - 1; i++)
			{
				if (varAux > Zcinprif[i + 1] && Xcinprif[i + 1] < 0)
				{
					varAux = Zcinprif[i + 1];
					j = i + 1;
				}
			}
			decimal ZCIN2 = Math.Abs(Zcinprif[j]);
			SUMPCI = SUMPCI + ZCIN1 + ZCIN2;

			return SUMPCI;
		}

		public decimal CalcularPerimetroCuello(List<SkeletonPoint> PointsCuelloPri, decimal[] centroMasa, decimal zCuelloCm, TipoAvatar tipoAvatar)
		{
			decimal SUMPCU = 0.0m;
			decimal[] SUMPCUS = new decimal[PointsCuelloPri.Count];
			for (int k = 0; k < PointsCuelloPri.Count; k++)
			{
				SUMPCUS[k] = (decimal)Math.Sqrt(Math.Pow(PointsCuelloPri[k].X, 2) + Math.Pow(PointsCuelloPri[k].Z, 2));
			}
			decimal ProCu = SUMPCUS.Sum() / SUMPCUS.Length;
			decimal DCu = 0.0m;
			for (int k = 0; k < SUMPCUS.Length; k++)
			{
				DCu = DCu + (decimal)(Math.Pow((double)(SUMPCUS[k] - ProCu), 2));
			}
			DCu = DCu / (SUMPCUS.Length - 1.0m);
			DCu = (decimal)Math.Sqrt((double)DCu);
			decimal PorCu, ACu;
			PorCu = DCu / ProCu;
			if (PorCu > 0.45m)
				ACu = (1.2m) * DCu * (1.0m - PorCu) + ProCu;
			else
				ACu = (3.0m) * DCu * (1.0m - PorCu) + ProCu;
			decimal Delta2 = 2.0m;
			int nmax = Convert.ToInt32(360 / Delta2);

			decimal[] XCuprif = new decimal[nmax];
			decimal[] ZCuprif = new decimal[nmax];
			decimal Titai = 0, Titaf = 0, Tita = 0;
			int j;
			for (int q = 0; q < nmax; q++)
			{
				decimal[] R2 = new decimal[PointsCuelloPri.Count];
				decimal[] XCupri1 = new decimal[PointsCuelloPri.Count];
				decimal[] ZCupri1 = new decimal[PointsCuelloPri.Count];

				Titai = q * Delta2;
				Titaf = (q + 1) * Delta2;

				j = 0;
				for (int k = 0; k < PointsCuelloPri.Count; k++)
				{
					if (PointsCuelloPri[k].X >= 0 && PointsCuelloPri[k].Z >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCuelloPri[k].X / PointsCuelloPri[k].Z));
					}
					if (PointsCuelloPri[k].X < 0 && PointsCuelloPri[k].Z >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCuelloPri[k].X / PointsCuelloPri[k].Z));
					}
					if (PointsCuelloPri[k].X <= 0 && PointsCuelloPri[k].Z < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCuelloPri[k].X / PointsCuelloPri[k].Z)) + 180.0m;
					}
					if (PointsCuelloPri[k].X > 0 && PointsCuelloPri[k].Z < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCuelloPri[k].X / PointsCuelloPri[k].Z)) + 180.0m;
					}
					if (Tita > Titai && Tita <= Titaf && Tita <= 180.0m && (decimal)PointsCuelloPri[k].Z >= (-zCuelloCm + centroMasa[2]) && (decimal)Math.Sqrt(Math.Pow(PointsCuelloPri[k].X, 2) + Math.Pow(PointsCuelloPri[k].Z, 2)) < ACu)

					{
						XCupri1[j] = (decimal)PointsCuelloPri[k].X;
						ZCupri1[j] = (decimal)PointsCuelloPri[k].Z;
						R2[j] = (decimal)Math.Sqrt(Math.Pow(PointsCuelloPri[k].X, 2) + Math.Pow(PointsCuelloPri[k].Z, 2));
						j++;
					}
				}
				int l4 = j;
				decimal minf1;
				if (l4 > 0)
				{
					minf1 = R2[0];
					j = 0;
					for (int i = 0; i < l4 - 1; i++)
					{
						if (minf1 > R2[i + 1])
						{
							minf1 = R2[i + 1];
							j = i + 1;
						}
					}
					XCuprif[q] = XCupri1[j];
					ZCuprif[q] = ZCupri1[j];
				}
				else
				{
					XCuprif[q] = 0;
					ZCuprif[q] = 0;
				}
			}
			j = 0;
			int[] SJHC = new int[XCuprif.Length];
			for (int k = 0; k < XCuprif.Length; k++)
			{
				if (XCuprif[k] != 0 && ZCuprif[k] != 0)
				{
					SJHC[j] = k;
					j++;
				}
			}
			int b1 = j;
			decimal[] XCuprifn = new decimal[b1];
			decimal[] ZCuprifn = new decimal[b1];
			if (b1 > 0)
			{
				for (int k = 0; k < b1; k++)
				{
					XCuprifn[k] = XCuprif[SJHC[k]];
					ZCuprifn[k] = ZCuprif[SJHC[k]];
				}
				Array.Clear(XCuprif, 0, XCuprif.Length);
				Array.Clear(ZCuprif, 0, ZCuprif.Length);
				for (int k = 0; k < b1; k++)
				{
					XCuprif[k] = XCuprifn[k];
					ZCuprif[k] = ZCuprifn[k];
				}

				decimal[] Tita3 = new decimal[b1];
				for (int k = 0; k < b1; k++)
				{
					if (XCuprif[k] >= 0 && ZCuprif[k] >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCuprif[k] / ZCuprif[k])));
					}
					if (XCuprif[k] < 0 && ZCuprif[k] >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCuprif[k] / ZCuprif[k])));
					}
					if (XCuprif[k] <= 0 && ZCuprif[k] < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCuprif[k] / ZCuprif[k]))) + 180.0m;
					}
					if (XCuprif[k] > 0 && ZCuprif[k] < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCuprif[k] / ZCuprif[k]))) + 180.0m;
					}
					Tita3[k] = Tita;
				}
				//Ordeno en funcion del angulo segun el angulo de menor a mayor
				int l6 = b1;
				decimal a, b, c, minf2;
				if (l6 > 0)
				{
					for (int k = 0; k < l6 - 1; k++)
					{
						minf2 = Tita3[k];
						j = k;
						for (int i = k; i < l6 - 1; i++)
						{
							if (minf2 > Tita3[i + 1])
							{
								minf2 = Tita3[i + 1];
								j = i + 1;
							}
						}
						a = Tita3[k];
						b = XCuprif[k];
						c = ZCuprif[k];
						XCuprif[k] = XCuprif[j];
						ZCuprif[k] = ZCuprif[j];
						XCuprif[j] = b;
						ZCuprif[j] = c;
						Tita3[k] = Tita3[j];
						Tita3[j] = a;
					}
				}
				//Perimetro
				decimal[] SUMPCU1 = new decimal[l6 - 1];
				for (int k = 0; k < l6 - 1; k++)
				{
					SUMPCU = SUMPCU + (decimal)Math.Sqrt(Math.Pow(Convert.ToDouble(XCuprif[k + 1] - XCuprif[k]), 2) + Math.Pow(Convert.ToDouble(ZCuprif[k + 1] - ZCuprif[k]), 2));
					SUMPCU1[k] = (decimal)Math.Sqrt(Math.Pow(Convert.ToDouble(XCuprif[k + 1] - XCuprif[k]), 2) + Math.Pow(Convert.ToDouble(ZCuprif[k + 1] - ZCuprif[k]), 2));
				}
			}
			else
			{
				SUMPCU = 3000.0m;
			}
			decimal varAux = ZCuprif[0];
			j = 0;
			for (int i = 0; i < ZCuprif.Length - 1; i++)
			{
				if (varAux > ZCuprif[i + 1] && XCuprif[i + 1] > 0)
				{
					varAux = ZCuprif[i + 1];
					j = i + 1;
				}
			}
			decimal ZC1 = Math.Abs(ZCuprif[j]);
			varAux = ZCuprif[0];
			j = 0;
			for (int i = 0; i < ZCuprif.Length - 1; i++)
			{
				if (varAux > ZCuprif[i + 1] && XCuprif[i + 1] < 0)
				{
					varAux = ZCuprif[i + 1];
					j = i + 1;
				}
			}
			decimal ZC2 = Math.Abs(ZCuprif[j]);
			//SUMPCU = SUMPCU + ZC1 + ZC2;
			return SUMPCU;
		}

		public decimal CalcularPerimetroCabeza(List<SkeletonPoint> PointsCabezaPri, decimal[] centroMasa, decimal zCabezaCm, TipoAvatar tipoAvatar)
		{
			decimal SUMPCA = 0.0m;
			decimal[] SUMPCAS = new decimal[PointsCabezaPri.Count];
			for (int k = 0; k < PointsCabezaPri.Count; k++)
			{
				SUMPCAS[k] = (decimal)Math.Sqrt(Math.Pow(PointsCabezaPri[k].X, 2) + Math.Pow(PointsCabezaPri[k].Z, 2));
			}
			decimal ProCA = SUMPCAS.Sum() / SUMPCAS.Length;
			decimal DCA = 0.0m;
			for (int k = 0; k < SUMPCAS.Length; k++)
			{
				DCA = DCA + (decimal)Math.Pow(Convert.ToDouble(SUMPCAS[k] - ProCA), 2);
			}
			DCA = DCA / (SUMPCAS.Length - 1.0m);
			DCA = (decimal)Math.Sqrt((double)DCA);
			decimal PorCA, ACA;
			PorCA = DCA / ProCA;
			if (PorCA > 0.45m)
				ACA = (1.2m) * DCA * (1.0m - PorCA) + ProCA;
			else
				ACA = (3.0m) * DCA * (1.0m - PorCA) + ProCA;

			decimal Delta3 = 2.0m;
			int nmax = Convert.ToInt32(360 / Delta3);
			decimal[] XCabprif = new decimal[nmax];
			decimal[] Zcabprif = new decimal[nmax];
			decimal Titai = 0, Titaf = 0, Tita = 0;
			int j;
			for (int q = 0; q < nmax; q++)
			{
				decimal[] R3 = new decimal[PointsCabezaPri.Count];
				decimal[] XCabpri1 = new decimal[PointsCabezaPri.Count];
				decimal[] Zcabpri1 = new decimal[PointsCabezaPri.Count];

				Titai = q * Delta3;
				Titaf = (q + 1) * Delta3;
				j = 0;
				for (int k = 0; k < PointsCabezaPri.Count; k++)
				{
					if (PointsCabezaPri[k].X >= 0 && PointsCabezaPri[k].Z >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCabezaPri[k].X / PointsCabezaPri[k].Z));
					}
					if (PointsCabezaPri[k].X < 0 && PointsCabezaPri[k].Z >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCabezaPri[k].X / PointsCabezaPri[k].Z));
					}
					if (PointsCabezaPri[k].X <= 0 && PointsCabezaPri[k].Z < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCabezaPri[k].X / PointsCabezaPri[k].Z)) + 180.0m;
					}
					if (PointsCabezaPri[k].X > 0 && PointsCabezaPri[k].Z < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCabezaPri[k].X / PointsCabezaPri[k].Z)) + 180.0m;
					}
					if (Tita > Titai && Tita <= Titaf && Tita <= 180.0m && (decimal)PointsCabezaPri[k].Z >= (-zCabezaCm + centroMasa[2]) && (decimal)Math.Sqrt(Math.Pow(PointsCabezaPri[k].X, 2) + Math.Pow(PointsCabezaPri[k].Z, 2)) < ACA)
					{
						XCabpri1[j] = (decimal)PointsCabezaPri[k].X;
						Zcabpri1[j] = (decimal)PointsCabezaPri[k].Z;
						R3[j] = (decimal)Math.Sqrt(Math.Pow(PointsCabezaPri[k].X, 2) + Math.Pow(PointsCabezaPri[k].Z, 2));
						j++;
					}
				}
				int l4 = j;
				decimal minf1;
				if (l4 > 0)
				{
					minf1 = R3[0];
					j = 0;
					for (int i = 0; i < l4 - 1; i++)
					{
						if (tipoAvatar == TipoAvatar.Delantero)
						{
							if (minf1 > R3[i + 1])
							{
								minf1 = R3[i + 1];
								j = i + 1;
							}
						}
						else
						{
							if (minf1 < R3[i + 1])
							{
								minf1 = R3[i + 1];
								j = i + 1;
							}
						}
					}
					XCabprif[q] = XCabpri1[j];
					Zcabprif[q] = Zcabpri1[j];
				}
				else
				{
					XCabprif[q] = 0;
					Zcabprif[q] = 0;
				}
			}
			j = 0;
			int[] SJHCA = new int[XCabprif.Length];
			for (int k = 0; k < XCabprif.Length; k++)
			{
				if (XCabprif[k] != 0 && Zcabprif[k] != 0)
				{
					SJHCA[j] = k;
					j++;
				}
			}
			int b1 = j;
			decimal[] XCabprifn = new decimal[b1];
			decimal[] Zcabprifn = new decimal[b1];

			for (int k = 0; k < b1; k++)
			{
				XCabprifn[k] = XCabprif[SJHCA[k]];
				Zcabprifn[k] = Zcabprif[SJHCA[k]];
			}
			Array.Clear(XCabprif, 0, XCabprif.Length);
			Array.Clear(Zcabprif, 0, Zcabprif.Length);
			for (int k = 0; k < b1; k++)
			{
				XCabprif[k] = XCabprifn[k];
				Zcabprif[k] = Zcabprifn[k];
			}
			decimal[] Tita4 = new decimal[b1];
			for (int k = 0; k < b1; k++)
			{
				if (XCabprif[k] >= 0 && Zcabprif[k] >= 0)
				{
					Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCabprif[k] / Zcabprif[k])));
				}
				if (XCabprif[k] < 0 && Zcabprif[k] >= 0)
				{
					Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCabprif[k] / Zcabprif[k])));
				}
				if (XCabprif[k] <= 0 && Zcabprif[k] < 0)
				{
					Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCabprif[k] / Zcabprif[k]))) + 180.0m;
				}
				if (XCabprif[k] > 0 && Zcabprif[k] < 0)
				{
					Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCabprif[k] / Zcabprif[k]))) + 180.0m;
				}
				Tita4[k] = Tita;
			}
			//Ordeno en funcion del angulo segun el angulo de menor a mayor
			int l6 = b1;
			decimal a, b, c, minf2;
			if (l6 > 0)
			{
				for (int k = 0; k < l6 - 1; k++)
				{
					minf2 = Tita4[k];
					j = k;
					for (int i = k; i < l6 - 1; i++)
					{
						if (minf2 > Tita4[i + 1])
						{
							minf2 = Tita4[i + 1];
							j = i + 1;
						}
					}
					a = Tita4[k];
					b = XCabprif[k];
					c = Zcabprif[k];
					XCabprif[k] = XCabprif[j];
					Zcabprif[k] = Zcabprif[j];
					XCabprif[j] = b;
					Zcabprif[j] = c;
					Tita4[k] = Tita4[j];
					Tita4[j] = a;
				}
			}
			//Perimetro
			decimal[] SUMPCA1 = new decimal[l6 - 1];
			for (int k = 0; k < l6 - 1; k++)
			{
				SUMPCA = SUMPCA + (decimal)Math.Sqrt(Math.Pow(Convert.ToDouble(XCabprif[k + 1] - XCabprif[k]), 2) + Math.Pow(Convert.ToDouble(Zcabprif[k + 1] - Zcabprif[k]), 2));
				SUMPCA1[k] = (decimal)Math.Sqrt(Math.Pow(Convert.ToDouble(XCabprif[k + 1] - XCabprif[k]), 2) + Math.Pow(Convert.ToDouble(Zcabprif[k + 1] - Zcabprif[k]), 2));
			}
			return SUMPCA;
		}

		public decimal CalcularPerimetroCadera(List<SkeletonPoint> PointsCaderaPri, decimal[] centroMasa, decimal zCaderaCm, char sexo, decimal zCaderaMinimo)
		{
			decimal SUMPCAD = 0.0m;
			decimal SM2;
			if (sexo == 'M')
				SM2 = 0.6m;
			else
				SM2 = 0.45m;
			decimal[] SUMPCADS = new decimal[PointsCaderaPri.Count];
			for (int k = 0; k < PointsCaderaPri.Count; k++)
			{
				SUMPCADS[k] = (decimal)Math.Sqrt(Math.Pow(PointsCaderaPri[k].X, 2) + Math.Pow(PointsCaderaPri[k].Z, 2));
			}

			decimal ProCAD = SUMPCADS.Sum() / SUMPCADS.Length;
			decimal DCAD = 0.0m;
			for (int k = 0; k < SUMPCADS.Length; k++)
			{
				DCAD = DCAD + (decimal)Math.Pow(Convert.ToDouble(SUMPCADS[k] - ProCAD), 2);
			}
			DCAD = DCAD / (SUMPCADS.Length - 1.0m);
			DCAD = (decimal)Math.Sqrt((double)DCAD);

			decimal PorCAD, ACAD;
			PorCAD = DCAD / ProCAD;
			if (PorCAD > SM2)
				ACAD = (2.0m) * DCAD * (1.0m - PorCAD) + ProCAD;
			else
				ACAD = (4.0m) * DCAD * (1.0m - PorCAD) + ProCAD;
			decimal Delta4 = 2.0m;
			int nmax = Convert.ToInt32(360 / Delta4);

			decimal[] Xcadeprif = new decimal[nmax];
			decimal[] Zcadeprif = new decimal[nmax];
			decimal Titai = 0, Titaf = 0, Tita = 0;
			int j;
			for (int q = 0; q < nmax; q++)
			{
				decimal[] R4 = new decimal[PointsCaderaPri.Count];
				decimal[] Xcadepri1 = new decimal[PointsCaderaPri.Count];
				decimal[] Zcadepri1 = new decimal[PointsCaderaPri.Count];

				Titai = q * Delta4;
				Titaf = (q + 1) * Delta4;

				j = 0;
				for (int k = 0; k < PointsCaderaPri.Count; k++)
				{
					if (PointsCaderaPri[k].X >= 0 && PointsCaderaPri[k].Z >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCaderaPri[k].X / PointsCaderaPri[k].Z));
					}
					if (PointsCaderaPri[k].X < 0 && PointsCaderaPri[k].Z >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCaderaPri[k].X / PointsCaderaPri[k].Z));
					}
					if (PointsCaderaPri[k].X <= 0 && PointsCaderaPri[k].Z < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCaderaPri[k].X / PointsCaderaPri[k].Z)) + 180.0m;
					}
					if (PointsCaderaPri[k].X > 0 && PointsCaderaPri[k].Z < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCaderaPri[k].X / PointsCaderaPri[k].Z)) + 180.0m;
					}
					if (Tita > Titai && Tita <= Titaf && Tita <= 180.0m && (decimal)PointsCaderaPri[k].Z >= (-zCaderaCm + centroMasa[2]) && (decimal)Math.Sqrt(Math.Pow(PointsCaderaPri[k].X, 2) + Math.Pow(PointsCaderaPri[k].Z, 2)) < ACAD)
					{
						Xcadepri1[j] = (decimal)PointsCaderaPri[k].X;
						Zcadepri1[j] = (decimal)PointsCaderaPri[k].Z;
						R4[j] = (decimal)Math.Sqrt(Math.Pow(PointsCaderaPri[k].X, 2) + Math.Pow(PointsCaderaPri[k].Z, 2));
						j++;
					}
				}
				int l4 = j;
				decimal minf1;
				if (l4 > 0)
				{
					minf1 = R4[0];
					j = 0;
					for (int i = 0; i < l4 - 1; i++)
					{
						if (minf1 > R4[i + 1])
						{
							minf1 = R4[i + 1];
							j = i + 1;
						}
					}
					Xcadeprif[q] = Xcadepri1[j];
					Zcadeprif[q] = Zcadepri1[j];
				}
				else
				{
					Xcadeprif[q] = 0;
					Zcadeprif[q] = 0;
				}
			}
			j = 0;
			int[] SJHCAD = new int[Xcadeprif.Length];
			for (int k = 0; k < Xcadeprif.Length; k++)
			{
				if (Xcadeprif[k] != 0 && Zcadeprif[k] != 0)
				{
					SJHCAD[j] = k;
					j++;
				}
			}
			int b1 = j;
			decimal[] Xcadeprifn = new decimal[b1];
			decimal[] Zcadeprifn = new decimal[b1];
			if (b1 > 0)
			{
				for (int k = 0; k < b1; k++)
				{
					Xcadeprifn[k] = Xcadeprif[SJHCAD[k]];
					Zcadeprifn[k] = Zcadeprif[SJHCAD[k]];
				}
				Array.Clear(Xcadeprif, 0, Xcadeprif.Length);
				Array.Clear(Zcadeprif, 0, Zcadeprif.Length);
				for (int k = 0; k < b1; k++)
				{
					Xcadeprif[k] = Xcadeprifn[k];
					Zcadeprif[k] = Zcadeprifn[k];
				}
				decimal[] Tita5 = new decimal[b1];
				for (int k = 0; k < b1; k++)
				{
					if (Xcadeprif[k] >= 0 && Zcadeprif[k] >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(Xcadeprif[k] / Zcadeprif[k])));
					}
					if (Xcadeprif[k] < 0 && Zcadeprif[k] >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(Xcadeprif[k] / Zcadeprif[k])));
					}
					if (Xcadeprif[k] <= 0 && Zcadeprif[k] < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(Xcadeprif[k] / Zcadeprif[k]))) + 180.0m;
					}
					if (Xcadeprif[k] > 0 && Zcadeprif[k] < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(Xcadeprif[k] / Zcadeprif[k]))) + 180.0m;
					}
					Tita5[k] = Tita;
				}
				//Ordeno en funcion del angulo segun el angulo de menor a mayor
				decimal a, b, c, minf2;
				int l6 = b1;
				if (l6 > 0)
				{
					for (int k = 0; k < l6 - 1; k++) //1:l6-1
					{
						minf2 = Tita5[k];
						j = k;
						for (int i = k; i < l6 - 1; i++) // :l6-1
						{
							if (minf2 > Tita5[i + 1])
							{
								minf2 = Tita5[i + 1];
								j = i + 1;
							}
						}
						a = Tita5[k];
						b = Xcadeprif[k];
						c = Zcadeprif[k];
						Xcadeprif[k] = Xcadeprif[j];
						Zcadeprif[k] = Zcadeprif[j];
						Xcadeprif[j] = b;
						Zcadeprif[j] = c;
						Tita5[k] = Tita5[j];
						Tita5[j] = a;
					}
				}
				//Perimetro
				decimal[] SUMPCAD1 = new decimal[l6 - 1];
				for (int k = 0; k < l6 - 1; k++)
				{
					SUMPCAD = SUMPCAD + (decimal)Math.Sqrt(Math.Pow(Convert.ToDouble(Xcadeprif[k + 1] - Xcadeprif[k]), 2) + Math.Pow(Convert.ToDouble(Zcadeprif[k + 1] - Zcadeprif[k]), 2));
					SUMPCAD1[k] = (decimal)Math.Sqrt(Math.Pow(Convert.ToDouble(Xcadeprif[k + 1] - Xcadeprif[k]), 2) + Math.Pow(Convert.ToDouble(Zcadeprif[k + 1] - Zcadeprif[k]), 2));
				}
			}
			else
			{
				SUMPCAD = 3000.0m;
			}
			decimal varAux = Zcadeprif[0];
			j = 0;
			for (int i = 0; i < Zcadeprif.Length - 1; i++)
			{
				if (varAux > Zcadeprif[i + 1] && Xcadeprif[i + 1] > 0)
				{
					varAux = Zcadeprif[i + 1];
					j = i + 1;
				}
			}
			decimal ZCAD1 = Math.Abs(Zcadeprif[j]);
			varAux = Zcadeprif[0];
			j = 0;
			for (int i = 0; i < Zcadeprif.Length - 1; i++)
			{
				if (varAux > Zcadeprif[i + 1] && Xcadeprif[i + 1] < 0)
				{
					varAux = Zcadeprif[i + 1];
					j = i + 1;
				}
			}
			decimal ZCAD2 = Math.Abs(Zcadeprif[j]);
			SUMPCAD = SUMPCAD + ZCAD1 + ZCAD2;

			return SUMPCAD;
		}

		public decimal CalcularPerimetroCinturaP(List<SkeletonPoint> PointsCinturaPPri, decimal[] centroMasa, decimal zCinturaPCm, TipoAvatar tipoAvatar, decimal zCinturaPMinimo)
		{
			decimal[] SUMPCIPS = new decimal[PointsCinturaPPri.Count];
			decimal SUMPCIP = 0.0m;
			for (int k = 0; k < PointsCinturaPPri.Count; k++)
			{
				SUMPCIPS[k] = (decimal)Math.Sqrt(Math.Pow(PointsCinturaPPri[k].X, 2) + Math.Pow(PointsCinturaPPri[k].Z, 2));
			}
			decimal ProCIp = SUMPCIPS.Sum() / SUMPCIPS.Length;
			decimal DCIP = 0.0m;
			for (int k = 0; k < SUMPCIPS.Length; k++)
			{
				DCIP = DCIP + (decimal)Math.Pow(Convert.ToDouble(SUMPCIPS[k] - ProCIp), 2);
			}
			DCIP = DCIP / (SUMPCIPS.Length - 1.0m);
			DCIP = (decimal)Math.Sqrt((double)DCIP);
			decimal PorCIp, ACIP;
			PorCIp = DCIP / ProCIp;
			if (PorCIp > 0.45m)
				ACIP = (2.0m) * DCIP * (1.0m - PorCIp) + ProCIp;
			else
				ACIP = (4.0m) * DCIP * (1.0m - PorCIp) + ProCIp;

			decimal Delta1p = 2.0m;
			int nmax = Convert.ToInt32(360 / Delta1p);
			decimal[] Xcinpprif = new decimal[nmax];
			decimal[] Zcinpprif = new decimal[nmax];
			decimal Titai = 0, Titaf = 0, Tita = 0;
			int j;
			for (int q = 0; q < nmax; q++)
			{
				Titai = q * Delta1p;
				Titaf = (q + 1) * Delta1p;
				decimal[] R1p = new decimal[PointsCinturaPPri.Count];
				decimal[] Xcinppri1 = new decimal[PointsCinturaPPri.Count];
				decimal[] Zcinppri1 = new decimal[PointsCinturaPPri.Count];
				j = 0;
				for (int k = 0; k < PointsCinturaPPri.Count; k++)
				{
					if (PointsCinturaPPri[k].X >= 0 && PointsCinturaPPri[k].Z >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCinturaPPri[k].X / PointsCinturaPPri[k].Z));
					}
					if (PointsCinturaPPri[k].X < 0 && PointsCinturaPPri[k].Z >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCinturaPPri[k].X / PointsCinturaPPri[k].Z));
					}
					if (PointsCinturaPPri[k].X <= 0 && PointsCinturaPPri[k].Z < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCinturaPPri[k].X / PointsCinturaPPri[k].Z)) + 180.0m;
					}
					if (PointsCinturaPPri[k].X > 0 && PointsCinturaPPri[k].Z < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCinturaPPri[k].X / PointsCinturaPPri[k].Z)) + 180.0m;
					}
					if (Tita > Titai && Tita <= Titaf && Tita <= 180.0m && (decimal)PointsCinturaPPri[k].Z >= (-zCinturaPCm + centroMasa[2]) && (decimal)Math.Sqrt(Math.Pow(PointsCinturaPPri[k].X, 2) + Math.Pow(PointsCinturaPPri[k].Z, 2)) < ACIP)
					{
						Xcinppri1[j] = (decimal)PointsCinturaPPri[k].X;
						Zcinppri1[j] = (decimal)PointsCinturaPPri[k].Z;
						R1p[j] = (decimal)Math.Sqrt(Math.Pow(PointsCinturaPPri[k].X, 2) + Math.Pow(PointsCinturaPPri[k].Z, 2));
						j++;
					}
				}
				int l4 = j;
				decimal minf1;
				if (l4 > 0)
				{
					minf1 = R1p[0];
					j = 0;
					for (int i = 0; i < l4 - 1; i++)
					{
						if (minf1 > R1p[i + 1])
						{
							minf1 = R1p[i + 1];
							j = i + 1;
						}
					}
					Xcinpprif[q] = Xcinppri1[j];
					Zcinpprif[q] = Zcinppri1[j];
				}
				else
				{
					Xcinpprif[q] = 0;
					Zcinpprif[q] = 0;
				}
			}
			j = 0;
			int[] SJHp = new int[Xcinpprif.Length];
			for (int k = 0; k < Xcinpprif.Length; k++)
			{
				if (Xcinpprif[k] != 0 && Zcinpprif[k] != 0)
				{
					SJHp[j] = k;
					j++;
				}
			}
			int b1 = j;
			decimal[] Xcinpprifn = new decimal[b1];
			decimal[] Zcinpprifn = new decimal[b1];
			if (b1 > 0)
			{
				for (int k = 0; k < b1; k++)
				{
					Xcinpprifn[k] = Xcinpprif[SJHp[k]];
					Zcinpprifn[k] = Zcinpprif[SJHp[k]];
				}
				Array.Clear(Xcinpprif, 0, Xcinpprif.Length);
				Array.Clear(Zcinpprif, 0, Zcinpprif.Length);
				for (int k = 0; k < b1; k++)
				{
					Xcinpprif[k] = Xcinpprifn[k];
					Zcinpprif[k] = Zcinpprifn[k];
				}
				decimal[] Tita2p = new decimal[b1];
				for (int k = 0; k < b1; k++)
				{
					if (Xcinpprif[k] >= 0 && Zcinpprif[k] >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(Xcinpprif[k] / Zcinpprif[k])));
					}
					if (Xcinpprif[k] < 0 && Zcinpprif[k] >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(Xcinpprif[k] / Zcinpprif[k])));
					}
					if (Xcinpprif[k] <= 0 && Zcinpprif[k] < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(Xcinpprif[k] / Zcinpprif[k]))) + 180.0m;
					}
					if (Xcinpprif[k] > 0 && Zcinpprif[k] < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(Xcinpprif[k] / Zcinpprif[k]))) + 180.0m;
					}
					Tita2p[k] = Tita;
				}
				//Ordeno en funcion del angulo segun el angulo de menor a mayor
				int l6 = b1;
				decimal a, b, c, minf2;
				if (l6 > 0)
				{
					for (int k = 0; k < l6 - 1; k++)
					{
						minf2 = Tita2p[k];
						j = k;
						for (int i = k; i < l6 - 1; i++)
						{
							if (minf2 > Tita2p[i + 1])
							{
								minf2 = Tita2p[i + 1];
								j = i + 1;
							}
						}
						a = Tita2p[k];
						b = Xcinpprif[k];
						c = Zcinpprif[k];
						Xcinpprif[k] = Xcinpprif[j];
						Zcinpprif[k] = Zcinpprif[j];
						Xcinpprif[j] = b;
						Zcinpprif[j] = c;
						Tita2p[k] = Tita2p[j];
						Tita2p[j] = a;
					}
				}
				// Perimetro
				decimal[] SUMPCI1P = new decimal[l6 - 1];
				for (int k = 0; k < l6 - 1; k++)
				{
					SUMPCIP = SUMPCIP + (decimal)Math.Sqrt(Math.Pow(Convert.ToDouble(Xcinpprif[k + 1] - Xcinpprif[k]), 2) + Math.Pow(Convert.ToDouble(Zcinpprif[k + 1] - Zcinpprif[k]), 2));
					SUMPCI1P[k] = (decimal)Math.Sqrt(Math.Pow(Convert.ToDouble(Xcinpprif[k + 1] - Xcinpprif[k]), 2) + Math.Pow(Convert.ToDouble(Zcinpprif[k + 1] - Zcinpprif[k]), 2));
				}
			}
			else
			{
				SUMPCIP = 3000.0m;
			}
			decimal varAux = Zcinpprif[0];
			j = 0;
			for (int i = 0; i < Zcinpprif.Length - 1; i++)
			{
				if (varAux > Zcinpprif[i + 1] && Xcinpprif[i + 1] > 0)
				{
					varAux = Zcinpprif[i + 1];
					j = i + 1;
				}
			}
			decimal ZCINP1 = Math.Abs(Zcinpprif[j]);
			varAux = Zcinpprif[0];
			j = 0;
			for (int i = 0; i < Zcinpprif.Length - 1; i++)
			{
				if (varAux > Zcinpprif[i + 1] && Xcinpprif[i + 1] < 0)
				{
					varAux = Zcinpprif[i + 1];
					j = i + 1;
				}
			}
			decimal ZCINP2 = Math.Abs(Zcinpprif[j]);
			SUMPCIP = SUMPCIP + ZCINP1 + ZCINP2;

			return SUMPCIP;
		}

		public decimal CalcularPerimetroCuello1(List<SkeletonPoint> PointsCuello1Pri, decimal[] centroMasa, decimal zCuello1Cm, decimal zCuello1Minimo, decimal MinX, decimal MaxX)
		{
			decimal SUMPCUP = 0.0m;
			decimal[] SUMPCUPS = new decimal[PointsCuello1Pri.Count];
			for (int k = 0; k < PointsCuello1Pri.Count; k++)
			{
				SUMPCUPS[k] = (decimal)Math.Sqrt(Math.Pow(PointsCuello1Pri[k].X, 2) + Math.Pow(PointsCuello1Pri[k].Z, 2));
			}

			decimal ProCup = SUMPCUPS.Sum() / SUMPCUPS.Length;

			decimal DCup = 0.0m;
			for (int k = 0; k < SUMPCUPS.Length; k++)
			{
				DCup = DCup + (decimal)Math.Pow((double)(SUMPCUPS[k] - ProCup), 2);
			}
			DCup = DCup / SUMPCUPS.Length;
			DCup = (decimal)Math.Sqrt((double)DCup);

			decimal PorCup;
			PorCup = DCup / ProCup;

			decimal ACup;
			if (PorCup > 0.45m)
			{
				ACup = (1.2m) * DCup * (1.0m - PorCup) + ProCup;
			}
			else
			{
				ACup = (3.0m) * DCup * (1.0m - PorCup) + ProCup;
			}

			decimal Delta2p = 3.0m;
			int nmax = Convert.ToInt32(360 / Delta2p);
			decimal[] XCupprif = new decimal[nmax];
			decimal[] ZCupprif = new decimal[nmax];
			decimal Titai = 0, Titaf = 0, Tita = 0;
			int j;
			for (int q = 0; q < nmax; q++)
			{
				Titai = q * Delta2p;
				Titaf = (q + 1) * Delta2p;
				decimal[] R2p = new decimal[PointsCuello1Pri.Count];
				decimal[] XCuppri1 = new decimal[PointsCuello1Pri.Count];
				decimal[] ZCuppri1 = new decimal[PointsCuello1Pri.Count];
				j = 0;
				for (int k = 0; k < PointsCuello1Pri.Count; k++)
				{
					if (PointsCuello1Pri[k].X >= 0 && PointsCuello1Pri[k].Z >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCuello1Pri[k].X / PointsCuello1Pri[k].Z));
					}
					if (PointsCuello1Pri[k].X < 0 && PointsCuello1Pri[k].Z >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCuello1Pri[k].X / PointsCuello1Pri[k].Z));
					}
					if (PointsCuello1Pri[k].X <= 0 && PointsCuello1Pri[k].Z < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCuello1Pri[k].X / PointsCuello1Pri[k].Z)) + 180.0m;
					}
					if (PointsCuello1Pri[k].X > 0 && PointsCuello1Pri[k].Z < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan(PointsCuello1Pri[k].X / PointsCuello1Pri[k].Z)) + 180.0m;
					}
					if (Tita > Titai && Tita <= Titaf && Tita <= 180.0m && (decimal)PointsCuello1Pri[k].Z >= (-zCuello1Cm + centroMasa[2]) && (decimal)Math.Sqrt(Math.Pow(PointsCuello1Pri[k].X, 2) + Math.Pow(PointsCuello1Pri[k].Z, 2)) < ACup && (decimal)PointsCuello1Pri[k].X >= MinX && (decimal)PointsCuello1Pri[k].X <= MaxX)
					{
						XCuppri1[j] = (decimal)PointsCuello1Pri[k].X;
						ZCuppri1[j] = (decimal)PointsCuello1Pri[k].Z;
						R2p[j] = (decimal)Math.Sqrt(Math.Pow(PointsCuello1Pri[k].X, 2) + Math.Pow(PointsCuello1Pri[k].Z, 2));
						j++;
					}
				}
				int l4 = j;
				decimal minf1;
				if (l4 > 0)
				{
					minf1 = R2p[0];
					j = 0;
					for (int i = 0; i < l4 - 1; i++)
					{
						if (minf1 > R2p[i + 1])
						{
							minf1 = R2p[i + 1];
							j = i + 1;
						}
					}
					XCupprif[q] = XCuppri1[j];
					ZCupprif[q] = ZCuppri1[j];
				}
				else
				{
					XCupprif[q] = 0;
					ZCupprif[q] = 0;
				}
			}
			j = 0;
			int[] SJHp = new int[XCupprif.Length];
			for (int k = 0; k < XCupprif.Length; k++)
			{
				if (XCupprif[k] != 0 && ZCupprif[k] != 0)
				{
					SJHp[j] = k;
					j++;
				}
			}
			int b1 = j;
			decimal[] XCupprifn = new decimal[b1];
			decimal[] ZCupprifn = new decimal[b1];
			if (b1 > 0)
			{
				for (int k = 0; k < b1; k++)
				{
					XCupprifn[k] = XCupprif[SJHp[k]];
					ZCupprifn[k] = ZCupprif[SJHp[k]];
				}
				Array.Clear(XCupprif, 0, XCupprif.Length);
				Array.Clear(ZCupprif, 0, ZCupprif.Length);
				for (int k = 0; k < b1; k++)
				{
					XCupprif[k] = XCupprifn[k];
					ZCupprif[k] = ZCupprifn[k];
				}
				decimal[] Tita3p = new decimal[b1];
				for (int k = 0; k < b1; k++)
				{
					if (XCupprif[k] >= 0 && ZCupprif[k] >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCupprif[k] / ZCupprif[k])));
					}
					if (XCupprif[k] < 0 && ZCupprif[k] >= 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCupprif[k] / ZCupprif[k])));
					}
					if (XCupprif[k] <= 0 && ZCupprif[k] < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCupprif[k] / ZCupprif[k]))) + 180.0m;
					}
					if (XCupprif[k] > 0 && ZCupprif[k] < 0)
					{
						Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCupprif[k] / ZCupprif[k]))) + 180.0m;
					}
					Tita3p[k] = Tita;
				}
				//Ordeno en funcion del angulo segun el angulo de menor a mayor
				int l6 = b1;
				decimal a, b, c, minf2;
				if (l6 > 0)
				{
					for (int k = 0; k < l6 - 1; k++)
					{
						minf2 = Tita3p[k];
						j = k;
						for (int i = k; i < l6 - 1; i++)
						{
							if (minf2 > Tita3p[i + 1])
							{
								minf2 = Tita3p[i + 1];
								j = i + 1;
							}
						}
						a = Tita3p[k];
						b = XCupprif[k];
						c = ZCupprif[k];
						XCupprif[k] = XCupprif[j];
						ZCupprif[k] = ZCupprif[j];
						XCupprif[j] = b;
						ZCupprif[j] = c;
						Tita3p[k] = Tita3p[j];
						Tita3p[j] = a;
					}
				}
				// Perimetro
				decimal[] SUMPCUP1 = new decimal[l6 - 1];
				for (int k = 0; k < l6 - 1; k++)
				{
					SUMPCUP = SUMPCUP + (decimal)Math.Sqrt(Math.Pow(Convert.ToDouble(XCupprif[k + 1] - XCupprif[k]), 2) + Math.Pow(Convert.ToDouble(ZCupprif[k + 1] - ZCupprif[k]), 2));
					SUMPCUP1[k] = (decimal)Math.Sqrt(Math.Pow(Convert.ToDouble(XCupprif[k + 1] - XCupprif[k]), 2) + Math.Pow(Convert.ToDouble(ZCupprif[k + 1] - ZCupprif[k]), 2));
				}
			}
			else
			{
				SUMPCUP = 3000.0m;
			}
			decimal varAux = ZCupprif[0];
			j = 0;
			for (int i = 0; i < ZCupprif.Length - 1; i++)
			{
				if (varAux > ZCupprif[i + 1] && XCupprif[i + 1] > 0)
				{
					varAux = ZCupprif[i + 1];
					j = i + 1;
				}
			}
			decimal ZC1 = Math.Abs(ZCupprif[j]);
			varAux = ZCupprif[0];
			j = 0;
			for (int i = 0; i < ZCupprif.Length - 1; i++)
			{
				if (varAux > ZCupprif[i + 1] && XCupprif[i + 1] < 0)
				{
					varAux = ZCupprif[i + 1];
					j = i + 1;
				}
			}
			decimal ZC2 = Math.Abs(ZCupprif[j]);
			if (zCuello1Minimo - centroMasa[2] < 0.05m)
				SUMPCUP = SUMPCUP + ZC1 + ZC2;
			return SUMPCUP;
		}

		public decimal CalcularCuelloP(List<SkeletonPoint> PointsCuelloP, decimal[] centroMasa, decimal YCuelloP, decimal Ep, TipoAvatar tipoAvatar)
		{
			decimal[] SUMPCU = new decimal[25];
			decimal[] ZC1 = new decimal[25];
			decimal[] ZC2 = new decimal[25];
			decimal[] ZCuminimo = new decimal[25];
			decimal[] Zcmp11 = new decimal[25];
			int[] cold = new int[25];
			int j1 = 0;
			for (int kw = 0; kw < 25; kw++)
			{
				int lj = PointsCuelloP.Count;
				decimal incli;
				if (tipoAvatar == TipoAvatar.Delantero)
					incli = -(kw + 1);
				else
					incli = kw + 1;
				decimal[] ZCNP = new decimal[lj];
				decimal[] YCNP = new decimal[lj];
				decimal[] XCNP = new decimal[lj];
				for (int k = 0; k < lj; k++)
				{
					ZCNP[k] = (decimal)PointsCuelloP[k].Z * (decimal)Math.Cos((double)(incli * ((decimal)Math.PI / 180.0m))) + (decimal)PointsCuelloP[k].Y * (decimal)Math.Sin((double)(incli * ((decimal)Math.PI / 180.0m)));
					YCNP[k] = -(decimal)PointsCuelloP[k].Z * (decimal)Math.Sin((double)(incli * ((decimal)Math.PI / 180.0m))) + (decimal)PointsCuelloP[k].Y * (decimal)Math.Cos((double)(incli * ((decimal)Math.PI / 180.0m)));
					XCNP[k] = (decimal)PointsCuelloP[k].X;
				}

				int j = 0;
				int[] col = new int[lj];
				for (int k = 0; k < lj; k++)
				{
					if (Math.Abs(YCNP[k]) <= Ep)
					{
						col[j] = k;
						j++;
					}
				}

				decimal[] XCu = new decimal[j];
				decimal[] YCu = new decimal[j];
				decimal[] ZCu = new decimal[j];

				for (int k = 0; k < j; k++)
				{
					XCu[k] = XCNP[col[k]];
					YCu[k] = YCNP[col[k]];
					ZCu[k] = ZCNP[col[k]];
				}
				decimal minf = ZCu[0];
				j = 0;
				for (int i = 0; i < ZCu.Length - 1; i++)
				{
					if (minf > ZCu[i + 1])
					{
						minf = ZCu[i + 1];
						j++;
					}
				}

				ZCuminimo[kw] = minf;
				decimal Zcmp1;
				//Zcmp1 = centroMasa[2] * (decimal)Math.Cos((double)(incli * ((decimal)Math.PI / 180.0m)));
				Zcmp1 = 0;
				decimal ZCucm;
				if (ZCuminimo[kw] < Zcmp1)
				{
					ZCucm = (Zcmp1 + ZCuminimo[kw]) / 2.0m;
				}
				else
				{
					ZCucm = ZCuminimo[kw];
				}
				decimal[] XCupri = new decimal[YCu.Length];
				decimal[] YCupri = new decimal[YCu.Length];
				decimal[] ZCupri = new decimal[YCu.Length];
				for (int k = 0; k < YCu.Length; k++)
				{
					XCupri[k] = XCu[k];
					YCupri[k] = YCu[k];
					ZCupri[k] = ZCu[k];
				}
				decimal[] SUMPCUS = new decimal[YCupri.Length];
				for (int k = 0; k < YCupri.Length; k++)
				{
					SUMPCUS[k] = (decimal)Math.Sqrt((double)((XCupri[k] * XCupri[k]) + ((ZCupri[k] * ZCupri[k]))));
				}
				decimal ProCu = 0.0m;
				decimal n1 = 0.0m;
				for (int k = 0; k < SUMPCUS.Length; k++)
				{
					ProCu = ProCu + SUMPCUS[k];
					n1 = n1 + 1;
				}

				ProCu = ProCu / n1;
				decimal DCu = 0.0m;
				n1 = 0.0m;
				for (int k = 0; k < SUMPCUS.Length; k++)
				{
					DCu = DCu + ((SUMPCUS[k] - ProCu) * (SUMPCUS[k] - ProCu));
					n1 = n1 + 1;
				}
				DCu = DCu / (n1 - 1.0m);
				DCu = (decimal)Math.Sqrt((double)DCu);
				decimal PorCu, ACu;
				PorCu = DCu / ProCu;
				if (PorCu > 0.45m && (1.0m - PorCu) >= 0)
				{
					ACu = (1.2m) * DCu * (1.0m - PorCu) + ProCu;
				}
				else if (PorCu > 0.45m && (1.0m - PorCu) < 0)
				{
					ACu = ProCu;
				}
				else
				{
					ACu = (3.0m) * DCu * (1.0m - PorCu) + ProCu;
				}

				decimal Delta2 = 2.0m;
				int nmax = Convert.ToInt32((360.0m) / Delta2);
				decimal Titai, Titaf, Tita = 0.0m;
				decimal[] XCuprif = new decimal[nmax];
				decimal[] ZCuprif = new decimal[nmax];
				int l4, l6;
				for (int q = 0; q < nmax; q++)
				{
					decimal[] R2 = new decimal[XCupri.Length];
					decimal[] XCupri1 = new decimal[XCupri.Length];
					decimal[] ZCupri1 = new decimal[XCupri.Length];
					Titai = ((decimal)q) * Delta2;
					Titaf = ((decimal)(q + 1)) * Delta2;
					j = 0;
					for (int k = 0; k < XCupri.Length; k++)
					{
						if (XCupri[k] >= 0 && ZCupri[k] >= 0)
						{
							Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCupri[k] / ZCupri[k])));
						}
						if (XCupri[k] < 0 && ZCupri[k] >= 0)
						{
							Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCupri[k] / ZCupri[k])));
						}
						if (XCupri[k] <= 0 && ZCupri[k] < 0)
						{
							Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCupri[k] / ZCupri[k]))) + 180.0m;
						}
						if (XCupri[k] > 0 && ZCupri[k] < 0)
						{
							Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCupri[k] / ZCupri[k]))) + 180.0m;
						}

						if (Tita > Titai && Tita <= Titaf && Tita <= 180.0m && ZCupri[k] >= (Zcmp1) && (decimal)Math.Sqrt((double)(((XCupri[k] * XCupri[k])) + ((ZCupri[k] * ZCupri[k])))) < ACu)

						{
							XCupri1[j] = XCupri[k];
							ZCupri1[j] = ZCupri[k];
							R2[j] = (decimal)Math.Sqrt((double)(((XCupri[k] * XCupri[k])) + ((ZCupri[k] * ZCupri[k]))));
							j = j + 1;
						}
					}
					decimal minf1;
					l4 = j;
					if (l4 > 0)
					{
						minf1 = R2[0];
						j = 0;
						for (int i = 0; i < l4 - 1; i++)
						{
							if (minf1 > R2[i + 1])
							{
								minf1 = R2[i + 1];
								j = i + 1;
							}
						}
						XCuprif[q] = XCupri1[j];
						ZCuprif[q] = ZCupri1[j];
					}
					else
					{
						XCuprif[q] = 0;
						ZCuprif[q] = 0;
					}
				}
				j = 0;
				int[] SJHC = new int[XCuprif.Length];
				for (int k = 0; k < XCuprif.Length; k++)
				{
					if (XCuprif[k] != 0 && ZCuprif[k] != 0)
					{
						SJHC[j] = k;
						j = j + 1;
					}
				}
				decimal a, b, c, minf2;
				int b1;
				b1 = j;
				decimal[] XCuprifn = new decimal[b1];
				decimal[] ZCuprifn = new decimal[b1];
				if (b1 > 0)
				{
					for (int k = 0; k < b1; k++)
					{
						XCuprifn[k] = XCuprif[SJHC[k]];
						ZCuprifn[k] = ZCuprif[SJHC[k]];
					}
					Array.Clear(XCuprif, 0, XCuprif.Length);
					Array.Clear(ZCuprif, 0, ZCuprif.Length);
					for (int k = 0; k < b1; k++)
					{
						XCuprif[k] = XCuprifn[k];
						ZCuprif[k] = ZCuprifn[k];
					}
					decimal[] Tita3 = new decimal[b1];
					for (int k = 0; k < b1; k++)
					{
						if (XCuprif[k] >= 0 && ZCuprif[k] >= 0)
						{
							Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCuprif[k] / ZCuprif[k])));
						}
						if (XCuprif[k] < 0 && ZCuprif[k] >= 0)
						{
							Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCuprif[k] / ZCuprif[k])));
						}
						if (XCuprif[k] <= 0 && ZCuprif[k] < 0)
						{
							Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCuprif[k] / ZCuprif[k]))) + 180.0m;
						}
						if (XCuprif[k] > 0 && ZCuprif[k] < 0)
						{
							Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCuprif[k] / ZCuprif[k]))) + 180.0m;
						}
						Tita3[k] = Tita;
					}
					l6 = b1;
					if (l6 > 0)
					{
						for (int k = 0; k < l6 - 1; k++)
						{
							minf2 = Tita3[k];
							j = k;
							for (int i = k; i < l6 - 1; i++)
							{
								if (minf2 > Tita3[i + 1])
								{
									minf2 = Tita3[i + 1];
									j = i + 1;
								}
							}
							a = Tita3[k];
							b = XCuprif[k];
							c = ZCuprif[k];
							XCuprif[k] = XCuprif[j];
							ZCuprif[k] = ZCuprif[j];
							XCuprif[j] = b;
							ZCuprif[j] = c;
							Tita3[k] = Tita3[j];
							Tita3[j] = a;
						}
					}
					decimal[] SUMPCU1 = new decimal[l6 - 1];
					for (int k = 0; k < l6 - 1; k++)
					{
						SUMPCU[kw] = SUMPCU[kw] + (decimal)Math.Sqrt((double)(((XCuprif[k + 1] - XCuprif[k]) * (XCuprif[k + 1] - XCuprif[k])) + ((ZCuprif[k + 1] - ZCuprif[k]) * (ZCuprif[k + 1] - ZCuprif[k]))));

						SUMPCU1[k] = (decimal)Math.Sqrt((double)(((XCuprif[k + 1] - XCuprif[k]) * (XCuprif[k + 1] - XCuprif[k])) + ((ZCuprif[k + 1] - ZCuprif[k]) * (ZCuprif[k + 1] - ZCuprif[k]))));
					}
					cold[j1] = kw;
					j1 = j1 + 1;
				}
				else
				{
					SUMPCU[kw] = 3000.0m;
					cold[j1] = kw;
					j1 = j1 + 1;
				}

				minf = ZCuprif[0];
				j = 0;
				for (int i = 0; i < b1 - 1; i++)
				{
					if (minf > ZCuprif[i + 1] && XCuprif[i + 1] > 0)
					{
						minf = ZCuprif[i + 1];
						j = i + 1;
					}
				}
				ZC1[kw] = Math.Abs(ZCuprif[j]);
				int l9 = b1;
				minf = ZCuprif[l9];
				j = l9;
				for (int i = l9 - 1; i >= 1; i--)
				{
					if (minf > ZCuprif[i - 1] && XCuprif[i - 1] < 0)
					{
						minf = ZCuprif[i - 1];
						j = i - 1;
					}
				}
				ZC2[kw] = Math.Abs(ZCuprif[j]);

				decimal[] ZN = new decimal[YCu.Length];
				decimal[] YN = new decimal[YCu.Length];
				decimal[] XN = new decimal[YCu.Length];
				for (int k = 0; k < YCu.Length; k++)
				{
					ZN[k] = ZCupri[k] * (decimal)Math.Cos((double)(incli * ((decimal)Math.PI / 180.0m))) - YCupri[k] * (decimal)Math.Sin((double)(incli * ((decimal)Math.PI / 180.0m))) + centroMasa[2];
					YN[k] = ZCupri[k] * (decimal)Math.Sin((double)(incli * ((decimal)Math.PI / 180.0m))) + YCupri[k] * (decimal)Math.Cos((double)(incli * ((decimal)Math.PI / 180.0m))) + YCuelloP;
					XN[k] = XCupri[k] + centroMasa[0];
				}
				Zcmp11[kw] = Zcmp1;

			}
			int l41 = j1;
			decimal minfe = SUMPCU[cold[0]];
			int l = cold[0];
			for (int i = 0; i < l41 - 1; i++)
			{
				if (minfe > SUMPCU[cold[i + 1]])
				{
					minfe = SUMPCU[cold[i + 1]];
					l = cold[i + 1];
				}
			}
			decimal ZCuminimof, SUMPCUF;
			decimal minfa = ZCuminimo[0];
			int jn = 0;
			for (int i = 0; i < ZCuminimo.Length - 1; i++)
			{
				if (minfa > ZCuminimo[i + 1])
				{
					minfa = ZCuminimo[i + 1];
					jn = i + 1;
				}
			}
			ZCuminimof = minfa;
			if ((ZCuminimof - Zcmp11[jn]) >= 0.05m)
			{
				SUMPCUF = minfe;
			}
			else
			{
				SUMPCUF = minfe + ZC1[l] + ZC2[l];
			}
			return SUMPCUF;
		}

		public decimal CalcularCuelloVariante(List<SkeletonPoint> vertices, int edad, TipoAvatar tipoAvatar)
		{
			decimal[] SUMPCU = new decimal[25];
			decimal[] ZC1 = new decimal[25];
			decimal[] ZC2 = new decimal[25];
			decimal[] ZCuminimo = new decimal[25];
			int j1 = 0;
			decimal[] Zcmp11 = new decimal[25];
			int[] cold = new int[25];
			decimal minf = (decimal)vertices.Min(x => x.Y);
			decimal maxf = (decimal)vertices.Max(x => x.Y);
			decimal Altura;
			Altura = Math.Abs(maxf - minf);
			decimal[] centroMasa = CentroDeMasa(vertices);
			decimal Xcm = centroMasa[0];
			decimal Ycm = centroMasa[1];
			decimal Zcm = centroMasa[2];
			for (int kw = 0; kw < 25; kw++)
			{
				decimal Ycuellop = 0;
				decimal Ep = 0;
				if (edad > 16)
				{
					Ycuellop = (maxf - ((Altura * 0.125m) * 1.0m));
					Ep = Altura / (256.0m);
					Ep = Ep * 0.8m;
				}
				else if (edad > 8.0m && edad <= 16.0m)
				{
					Ycuellop = (maxf - (Altura / 7.0m));
					Ep = Altura / (224.0m);
				}
				else if (edad <= 8.0m && edad > 5.0m)
				{
					Ycuellop = (maxf - (Altura / 6.0m));
					Ep = Altura / (192.0m);
				}
				else if (edad > 1.0m && edad <= 5.0m)
				{
					Ycuellop = (maxf - (Altura / 5.0m));
					Ep = Altura / (160.0m);
				}
				else if (edad <= 1.0m)
				{
					Ycuellop = (maxf - (Altura / 4.0m));
					Ep = Altura / (128.0m);
				}
				//% cálculo de anillos alrededor de zonas de interés.
				//% Para cuello
				int[] colN = new int[vertices.Count];
				int j = 0;
				for (int k = 0; k < vertices.Count; k++)
				{
					if ((decimal)vertices[k].Y > (Ycuellop - 10.0m * Ep) && (decimal)vertices[k].Y < (Ycuellop + 10.0m * Ep))
					{
						colN[j] = k;
						j++;
					}
				}
				int lj = j;
				decimal incli;
				if (tipoAvatar == TipoAvatar.Delantero)
					incli = -(kw + 1);
				else
					incli = kw + 1;
				decimal[] XCN = new decimal[lj];
				decimal[] YCN = new decimal[lj];
				decimal[] ZCN = new decimal[lj];
				decimal[] ZCNP = new decimal[lj];
				decimal[] YCNP = new decimal[lj];
				decimal[] XCNP = new decimal[lj];
				for (int k = 0; k < lj; k++)
				{
					XCN[k] = (decimal)vertices[colN[k]].X - Xcm;
					YCN[k] = (decimal)vertices[colN[k]].Y - Ycuellop;
					ZCN[k] = (decimal)vertices[colN[k]].Z - Zcm;

					ZCNP[k] = ZCN[k] * (decimal)Math.Cos((double)(incli * ((decimal)Math.PI / 180.0m))) + YCN[k] * (decimal)Math.Sin((double)(incli * ((decimal)Math.PI / 180.0m)));
					YCNP[k] = -ZCN[k] * (decimal)Math.Sin((double)(incli * ((decimal)Math.PI / 180.0m))) + YCN[k] * (decimal)Math.Cos((double)(incli * ((decimal)Math.PI / 180.0m)));
					XCNP[k] = XCN[k];
				}

				j = 0;
				int[] col = new int[lj];
				for (int k = 0; k < lj; k++)
				{
					if (Math.Abs(YCNP[k]) <= Ep)
					{
						col[j] = k;
						j++;
					}
				}

				decimal[] XCu = new decimal[j];
				decimal[] YCu = new decimal[j];
				decimal[] ZCu = new decimal[j];

				for (int k = 0; k < j; k++)
				{
					XCu[k] = XCNP[col[k]];
					YCu[k] = YCNP[col[k]];
					ZCu[k] = ZCNP[col[k]];
				}
				minf = ZCu[0];
				j = 0;
				for (int i = 0; i < ZCu.Length - 1; i++)
				{
					if (minf > ZCu[i + 1])
					{
						minf = ZCu[i + 1];
						j++;
					}
				}

				ZCuminimo[kw] = minf;
				decimal Zcmp1;
				//Zcmp1 = Zcm * (decimal)Math.Cos((double)(incli * ((decimal)Math.PI / 180.0m)));
				Zcmp1 = 0;
				decimal ZCucm;
				if (ZCuminimo[kw] < Zcmp1)
				{
					ZCucm = (Zcmp1 + ZCuminimo[kw]) / 2.0m;
				}
				else
				{
					ZCucm = ZCuminimo[kw];
				}
				decimal[] XCupri = new decimal[YCu.Length];
				decimal[] YCupri = new decimal[YCu.Length];
				decimal[] ZCupri = new decimal[YCu.Length];
				for (int k = 0; k < YCu.Length; k++)
				{
					XCupri[k] = XCu[k];
					YCupri[k] = YCu[k];
					ZCupri[k] = ZCu[k];
				}
				decimal[] SUMPCUS = new decimal[YCupri.Length];
				for (int k = 0; k < YCupri.Length; k++)
				{
					SUMPCUS[k] = (decimal)Math.Sqrt((double)((XCupri[k] * XCupri[k]) + ((ZCupri[k] * ZCupri[k]))));
				}
				decimal ProCu = 0.0m;
				decimal n1 = 0.0m;
				for (int k = 0; k < SUMPCUS.Length; k++)
				{
					ProCu = ProCu + SUMPCUS[k];
					n1 = n1 + 1;
				}

				ProCu = ProCu / n1;
				decimal DCu = 0.0m;
				n1 = 0.0m;
				for (int k = 0; k < SUMPCUS.Length; k++)
				{
					DCu = DCu + ((SUMPCUS[k] - ProCu) * (SUMPCUS[k] - ProCu));
					n1 = n1 + 1;
				}
				DCu = DCu / (n1 - 1.0m);
				DCu = (decimal)Math.Sqrt((double)DCu);
				decimal PorCu, ACu;
				PorCu = DCu / ProCu;
				if (PorCu > 0.45m && (1.0m - PorCu) >= 0)
				{
					ACu = (1.2m) * DCu * (1.0m - PorCu) + ProCu;
				}
				else if (PorCu > 0.45m && (1.0m - PorCu) < 0)
				{
					ACu = ProCu;
				}
				else
				{
					ACu = (3.0m) * DCu * (1.0m - PorCu) + ProCu;
				}

				decimal Delta2 = 2.0m;
				int nmax = Convert.ToInt32((360.0m) / Delta2);
				decimal Titai, Titaf, Tita = 0.0m;
				decimal[] XCuprif = new decimal[nmax];
				decimal[] ZCuprif = new decimal[nmax];
				int l4, l6;
				for (int q = 0; q < nmax; q++)
				{
					decimal[] R2 = new decimal[XCupri.Length];
					decimal[] XCupri1 = new decimal[XCupri.Length];
					decimal[] ZCupri1 = new decimal[XCupri.Length];
					Titai = ((decimal)q) * Delta2;
					Titaf = ((decimal)(q + 1)) * Delta2;
					j = 0;
					for (int k = 0; k < XCupri.Length; k++)
					{
						if (XCupri[k] >= 0 && ZCupri[k] >= 0)
						{
							Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCupri[k] / ZCupri[k])));
						}
						if (XCupri[k] < 0 && ZCupri[k] >= 0)
						{
							Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCupri[k] / ZCupri[k])));
						}
						if (XCupri[k] <= 0 && ZCupri[k] < 0)
						{
							Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCupri[k] / ZCupri[k]))) + 180.0m;
						}
						if (XCupri[k] > 0 && ZCupri[k] < 0)
						{
							Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCupri[k] / ZCupri[k]))) + 180.0m;
						}

						if (Tita > Titai && Tita <= Titaf && Tita <= 180.0m && ZCupri[k] >= (Zcmp1) && (decimal)Math.Sqrt((double)(((XCupri[k] * XCupri[k])) + ((ZCupri[k] * ZCupri[k])))) < ACu)

						{
							XCupri1[j] = XCupri[k];
							ZCupri1[j] = ZCupri[k];
							R2[j] = (decimal)Math.Sqrt((double)(((XCupri[k] * XCupri[k])) + ((ZCupri[k] * ZCupri[k]))));
							j = j + 1;
						}
					}
					decimal minf1;
					l4 = j;
					if (l4 > 0)
					{
						minf1 = R2[0];
						j = 0;
						for (int i = 0; i < l4 - 1; i++)
						{
							if (minf1 > R2[i + 1])
							{
								minf1 = R2[i + 1];
								j = i + 1;
							}
						}
						XCuprif[q] = XCupri1[j];
						ZCuprif[q] = ZCupri1[j];
					}
					else
					{
						XCuprif[q] = 0;
						ZCuprif[q] = 0;
					}
				}
				j = 0;
				int[] SJHC = new int[XCuprif.Length];
				for (int k = 0; k < XCuprif.Length; k++)
				{
					if (XCuprif[k] != 0 && ZCuprif[k] != 0)
					{
						SJHC[j] = k;
						j = j + 1;
					}
				}
				decimal a, b, c, minf2;
				int b1;
				b1 = j;
				decimal[] XCuprifn = new decimal[b1];
				decimal[] ZCuprifn = new decimal[b1];
				if (b1 > 0)
				{
					for (int k = 0; k < b1; k++)
					{
						XCuprifn[k] = XCuprif[SJHC[k]];
						ZCuprifn[k] = ZCuprif[SJHC[k]];
					}
					Array.Clear(XCuprif, 0, XCuprif.Length);
					Array.Clear(ZCuprif, 0, ZCuprif.Length);
					for (int k = 0; k < b1; k++)
					{
						XCuprif[k] = XCuprifn[k];
						ZCuprif[k] = ZCuprifn[k];
					}
					decimal[] Tita3 = new decimal[b1];
					for (int k = 0; k < b1; k++)
					{
						if (XCuprif[k] >= 0 && ZCuprif[k] >= 0)
						{
							Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCuprif[k] / ZCuprif[k])));
						}
						if (XCuprif[k] < 0 && ZCuprif[k] >= 0)
						{
							Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCuprif[k] / ZCuprif[k])));
						}
						if (XCuprif[k] <= 0 && ZCuprif[k] < 0)
						{
							Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCuprif[k] / ZCuprif[k]))) + 180.0m;
						}
						if (XCuprif[k] > 0 && ZCuprif[k] < 0)
						{
							Tita = 90.0m - ((180.0m / (decimal)Math.PI) * (decimal)Math.Atan((double)(XCuprif[k] / ZCuprif[k]))) + 180.0m;
						}
						Tita3[k] = Tita;
					}
					l6 = b1;
					if (l6 > 0)
					{
						for (int k = 0; k < l6 - 1; k++)
						{
							minf2 = Tita3[k];
							j = k;
							for (int i = k; i < l6 - 1; i++)
							{
								if (minf2 > Tita3[i + 1])
								{
									minf2 = Tita3[i + 1];
									j = i + 1;
								}
							}
							a = Tita3[k];
							b = XCuprif[k];
							c = ZCuprif[k];
							XCuprif[k] = XCuprif[j];
							ZCuprif[k] = ZCuprif[j];
							XCuprif[j] = b;
							ZCuprif[j] = c;
							Tita3[k] = Tita3[j];
							Tita3[j] = a;
						}
					}
					decimal[] SUMPCU1 = new decimal[l6 - 1];
					for (int k = 0; k < l6 - 1; k++)
					{
						SUMPCU[kw] = SUMPCU[kw] + (decimal)Math.Sqrt((double)(((XCuprif[k + 1] - XCuprif[k]) * (XCuprif[k + 1] - XCuprif[k])) + ((ZCuprif[k + 1] - ZCuprif[k]) * (ZCuprif[k + 1] - ZCuprif[k]))));

						SUMPCU1[k] = (decimal)Math.Sqrt((double)(((XCuprif[k + 1] - XCuprif[k]) * (XCuprif[k + 1] - XCuprif[k])) + ((ZCuprif[k + 1] - ZCuprif[k]) * (ZCuprif[k + 1] - ZCuprif[k]))));
					}
					cold[j1] = kw;
					j1 = j1 + 1;
				}
				else
				{
					SUMPCU[kw] = 3000.0m;
					cold[j1] = kw;
					j1 = j1 + 1;
				}

				minf = ZCuprif[0];
				j = 0;
				for (int i = 0; i < b1 - 1; i++)
				{
					if (minf > ZCuprif[i + 1] && XCuprif[i + 1] > 0)
					{
						minf = ZCuprif[i + 1];
						j = i + 1;
					}
				}
				ZC1[kw] = Math.Abs(ZCuprif[j]);
				minf = ZCuprif[0];
				j = 0;
				for (int i = 0; i < b1; i++)
				{
					if (minf > ZCuprif[i + 1] && XCuprif[i + 1] < 0)
					{
						minf = ZCuprif[i + 1];
						j = i + 1;
					}
				}
				ZC2[kw] = Math.Abs(ZCuprif[j]);

				decimal[] ZN = new decimal[YCu.Length];
				decimal[] YN = new decimal[YCu.Length];
				decimal[] XN = new decimal[YCu.Length];
				for (int k = 0; k < YCu.Length; k++)
				{
					ZN[k] = ZCupri[k] * (decimal)Math.Cos((double)(incli * ((decimal)Math.PI / 180.0m))) - YCupri[k] * (decimal)Math.Sin((double)(incli * ((decimal)Math.PI / 180.0m))) + Zcm;
					YN[k] = ZCupri[k] * (decimal)Math.Sin((double)(incli * ((decimal)Math.PI / 180.0m))) + YCupri[k] * (decimal)Math.Cos((double)(incli * ((decimal)Math.PI / 180.0m))) + Ycuellop;
					XN[k] = XCupri[k] + Xcm;
				}
				Zcmp11[kw] = Zcmp1;

			}
			int l41 = j1;
			decimal minfe = SUMPCU[cold[0]];
			int l = cold[0];
			for (int i = 0; i < l41 - 1; i++)
			{
				if (minfe > SUMPCU[cold[i + 1]])
				{
					minfe = SUMPCU[cold[i + 1]];
					l = cold[i + 1];
				}
			}
			decimal ZCuminimof, SUMPCUF;
			decimal minfa = ZCuminimo[0];
			int jn = 0;
			for (int i = 0; i < ZCuminimo.Length - 1; i++)
			{
				if (minfa > ZCuminimo[i + 1])
				{
					minfa = ZCuminimo[i + 1];
					jn = i + 1;
				}
			}
			ZCuminimof = minfa;
			if ((ZCuminimof - Zcmp11[jn]) >= 0.05m)
			{
				SUMPCUF = minfe;
			}
			else
			{
				SUMPCUF = minfe + ZC1[l] + ZC2[l];
			}
			return SUMPCUF;
		}

		/*public Model3D PaintCentroMasa()
		{
			float MaxPointY = Vertices.Max(x => x.Y);
			float MinPointY = Vertices.Min(x => x.Y);
			decimal[] centroMasa = CentroDeMasa(Vertices);
			return GraficarAreaMedida(GraficarCentrodeMasa(centroMasa, MaxPointY, MinPointY), Colors.Red);
		}*/

		public SkeletonPoint EstimarTorax(SkeletonPoint shoulderCenter, SkeletonPoint SpineMid)
		{
			SkeletonPoint chest = new SkeletonPoint();
			chest.X = SpineMid.X;
			chest.Y = (shoulderCenter.Y + SpineMid.Y) / 2;
			chest.Z = SpineMid.Z;
			return chest;
		}

		public SkeletonPoint EncontrarPezon(SkeletonPoint shoulderCenter, SkeletonPoint Chest, List<SkeletonPoint> fullBody)
		{
			SkeletonPoint nipple = new SkeletonPoint();
			double mayorZ = 1;

			foreach (var item in fullBody)
			{
				if (item.Y >= shoulderCenter.Y && item.Y <= Chest.Y)
				{
					if (item.Z >= mayorZ)
					{
						mayorZ = item.Z;
						nipple = item;
					}
				}
			}

			return nipple;

		}

		public SkeletonPoint EstimarBrazo(SkeletonPoint ShoulderLeft, SkeletonPoint elbowLeft)
		{
			SkeletonPoint brazo = new SkeletonPoint();
			brazo.X = (elbowLeft.X + ShoulderLeft.X) / 2;
			brazo.Y = (ShoulderLeft.Y + elbowLeft.Y) / 2;
			brazo.Z = ShoulderLeft.Z;
			return brazo;
		}


		public double AlturaEntrePiernas(SkeletonPoint hipCenter, List<SkeletonPoint> fullBody, SkeletonPoint foot)
		{
			double menorY = 1000;

			foreach (var item in fullBody)
			{
				if (item.X == hipCenter.X && menorY < item.Y)
				{
					menorY = item.Y;
				}
			}

			return menorY - foot.Y;

		}

		public double AlturaEntrePiernasHip(SkeletonPoint hipCenter, SkeletonPoint foot)
		{
			return Math.Abs(hipCenter.Y - foot.Y) * 100;           //Conversion a cm

		}

		public void AnchoEntreHombros(SkeletonPoint point1, SkeletonPoint point2)
		{
			anchoHombros = FormulaDistanciaEntrePtos(point1, point2) * 100;
		}

		private double FormulaDistanciaEntrePtos(SkeletonPoint point1, SkeletonPoint point2)
		{
			return Math.Sqrt((point1.X - point2.X) * (point1.X - point2.X) + (point1.Y - point2.Y) * (point1.Y - point2.Y) + (point1.Z - point2.Z) * (point1.Z - point2.Z));
		}

		public enum TipoAvatar
		{
			Delantero = 0,
			Posterior = 1
		}
		#endregion

	}
}

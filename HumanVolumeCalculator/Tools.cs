using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Media.Media3D;
using Microsoft.Win32;
using System.Windows.Media;
using System.Globalization;
using HelixToolkit.Wpf;


namespace HumanVolumeCalculator
{
   public class Tools
    {
        private double ancho;
        private List<SkeletonPoint> vertices;
        private double epsilon;
        private List<double> angulos;
        private KinectSensor sensor;
        private float altura;
        private double anchoHombros;
        private float errorAltura;
        SkeletonPoint Zmax;
        bool menor = false;
        List<SkeletonPoint> brazos;

        public Tools(List<SkeletonPoint> values)
        {
            epsilon = 0.0022;
            vertices = new List<SkeletonPoint>();
            vertices = values;
            angulos = new List<double>();
            brazos = new List<SkeletonPoint>();
           
        
        }

        public Tools(List<SkeletonPoint> values, double _epsilon)
        {
            epsilon = _epsilon;
            vertices = new List<SkeletonPoint>();
            vertices = values;
            angulos = new List<double>();
        }




        #region MyProperties

        public List<SkeletonPoint> Vertices
        {
            get { return vertices; }
            set { vertices = value; }
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
        public List<SkeletonPoint> SelectFranjaHorizontal(SkeletonPoint jointCoo,bool tronco,float histeresis)
        {
            List<SkeletonPoint> franja = new List<SkeletonPoint>();
            //franja.Add(jointCoo);

           foreach (var item in vertices)
            {
                if (item.Y>=jointCoo.Y-epsilon && item.Y <=jointCoo.Y+epsilon)
                {
                   franja.Add(item);
                }
            }

            //List<SkeletonPoint>franjaOrdenada=OrdenarByDist(franja);
           EliminarDuplicados(franja);
            InitialPoint(franja, jointCoo); 

            if (tronco)
            {
                return OrdenarByDistanciaMayorX(franja,histeresis);
            }

            return franja;
            //return FranjaRefinadaxDsitancias(franjaOrdenada);
        }
        
        private List<SkeletonPoint> FranjaRefinadaxDsitancias(List<SkeletonPoint> franja)
        {
            List<SkeletonPoint> fRefinada = new List<SkeletonPoint>();
            double dist = 0;
            double distPromedio = 0;
            double lastDist = 0;
            int contador = 1;


            for (int i = 1; i < franja.Count-1; i++)
            {
                dist = FormulaDistanciaEntrePtos(franja[i],franja[i+1]);

                if (dist!=0)
                {
                    distPromedio += dist;
                    distPromedio /= contador;
                }
                

                if ((dist<=lastDist+0.0002 && dist!=0) || (i==1))
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
        }

        private List<SkeletonPoint> RefinarFranja(List<SkeletonPoint>franja,SkeletonPoint joint)
        {
            double distMax = DistaciaTranversal(joint);
            distMax = distMax / 2;
            List<SkeletonPoint> temp = new List<SkeletonPoint>();

            foreach (var item in franja)
            {
                if (item.X >=joint.X- distMax && item.X <= joint.X + distMax)
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
           return FormulaDistanciaEntrePtos(joint,Zmax);
        }



        private SkeletonPoint CalcularZmax(List<SkeletonPoint> franja)
        {
            SkeletonPoint Zmax = franja[0];
            double distMax = 0.0005;

            foreach (var item in franja)
            {
                if (Math.Abs(item.Z)>Math.Abs(Zmax.Z) && (item.X >= franja[0].X - distMax && item.X <= franja[0].X + distMax))
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


            for (int i = 0; i < points.Count()-1; i++)        
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

        private List<SkeletonPoint> OrdenarByDistanciaMayorX(List<SkeletonPoint> franja,float histeresis)
        {
            List<SkeletonPoint> sorted = new List<SkeletonPoint>();
            SkeletonPoint swapPos = new SkeletonPoint();
            double dist;
            double menorDist = 1000; //Una distancia inicial grande
            int indice = 0;
            bool ready = false;
            int j = 1;
            double substraction;
            double menorSub=1000;

            sorted.Add(franja[0]);
            //bool Positiva = PrimerPto(franja);
          
            for (int i = 0; i < franja.Count - 1; i++)
            {
                j = i + 1;
                for (; j < franja.Count; j++)
                {
                    dist = FormulaDistanciaEntrePtos(franja[i], franja[j]);
                    substraction = Math.Abs(franja[i].X - franja[j].X);

                    if ((indice != j) && (dist != 0) && (menorSub > substraction) && (substraction < histeresis)&&(franja[i].X!=franja[j].X))
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
       private bool PrimerPto(List<SkeletonPoint> franja)
        {
            double dist;
           double menorDist=1000;
           int indice=0;

           
                for (int j = 1; j < franja.Count; j++)
                {
                    dist = FormulaDistanciaEntrePtos(franja[0], franja[j]);
                    if (menorDist>dist)
                    {
                        menorDist = dist;
                        indice = j;
                    }
                }
                if (franja[indice].X<franja[0].X)
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
            
        }

        private void InitialPoint(List<SkeletonPoint> franja,SkeletonPoint joint)
        {
            float Xdist = 100;
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
            for (int i = 0; i < franja.Count-1; i++)
            {
                j = i + 1;
                for (; j < franja.Count; j++)
                {
                    if (Equals(franja[i],franja[j]))
                    {
                        franja.RemoveAt(j);
                    }
                }

            }
        }

        private bool Equals(SkeletonPoint p1, SkeletonPoint p2)
        {
            if (p1.X==p2.X && p1.Y==p2.Y && p1.Z ==p2.Z)
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

            ancho+= menorDist;

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

            ancho+= menorDist;
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
                    if (angulos[i]>angulos[j])
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

        public Model3DGroup GraficarAreaMedida(List<SkeletonPoint> points,Color color)
        {
            //Crear un modelo
            var modelGroup = new Model3DGroup();
            var meshBuiler = new MeshBuilder(false, false);
            Point3D x1, x2, x3;
            x1 = new Point3D();
            x2 = new Point3D();
            x3 = new Point3D();
          
            foreach (var item in points)
            {
                meshBuiler.AddSphere(new Point3D(item.X, -(item.Y), item.Z), 0.005);
                
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
            double mayorY=vertices[0].Y;
            double menorY=vertices[0].Y;

            for (int i = 0; i < vertices.Count(); i++)
            {
                if (vertices[i].Y>mayorY)
                {
                    mayorY = vertices[i].Y;
                }
                if (vertices[i].Y<menorY)
                {
                    menorY = vertices[i].Y;
                }
            }

            altura = (float)(mayorY - menorY);
        }

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
                if (item.Y>=shoulderCenter.Y && item.Y<= Chest.Y)
                {
                    if (item.Z>= mayorZ)
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
            brazo.X = (elbowLeft.X + ShoulderLeft.X)/2;
            brazo.Y = (ShoulderLeft.Y+elbowLeft.Y)/2;
            brazo.Z = ShoulderLeft.Z;
            return brazo;
        }


        public double AlturaEntrePiernas(SkeletonPoint hipCenter,List<SkeletonPoint> fullBody,SkeletonPoint foot)
        {
            double menorY = 1000;

            foreach (var item in fullBody)
            {
                if (item.X==hipCenter.X && menorY<item.Y)
                {
                    menorY = item.Y;
                }    
            }

            return menorY - foot.Y;

        }

        public double AlturaEntrePiernasHip(SkeletonPoint hipCenter,SkeletonPoint foot)
        {
            return Math.Abs(hipCenter.Y-foot.Y)*100;           //Conversion a cm
             
        }

       

        public void AnchoEntreHombros(SkeletonPoint point1,SkeletonPoint point2)
        {
            anchoHombros = FormulaDistanciaEntrePtos(point1,point2)*100;
        }

        private double FormulaDistanciaEntrePtos(SkeletonPoint point1, SkeletonPoint point2)
        {
            return Math.Sqrt((point1.X-point2.X)*(point1.X - point2.X)+(point1.Y-point2.Y)*(point1.Y - point2.Y)+(point1.Z-point2.Z)*(point1.Z - point2.Z));
        }


        #endregion



    }
}

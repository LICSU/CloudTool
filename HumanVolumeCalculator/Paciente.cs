using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HumanVolumeCalculator
{
    public class Paciente
    {
        //public string Nombre { get; set; }
        //public string Apellidos { get; set; }
        //public int Edad { get; set; }
        //public bool Masculino { get; set; }
        //public string Id { get; set; }

        /// <summary>
        /// Mediciones
        /// </summary>
        public decimal Altura { get; set; }
        public decimal PerimetroCabeza { get; set; }
        public decimal PerimetroCuello { get; set; }
        public decimal PerimetroPecho { get; set; }
        public decimal PerimetroCintura { get; set; }
        public decimal PerimetroCadera { get; set; }
        public decimal IndiceCaderaCintura { get; set; }

        //public double MedicionPecho { get; set; }
        //public double MedicionPecho { get; set; }
        //public double MedicionPecho { get; set; }
        //public enum Medicion{Cabeza,Pecho,Cintura,Cadera,Cuello,Altura};
        //public Dictionary<string, double> Mediciones { get; set; }
        //public Medicion _medicion { get; set; }

        //public Paciente()
        //{
        //    Mediciones = new Dictionary<string, double>();
        //}

    }
}

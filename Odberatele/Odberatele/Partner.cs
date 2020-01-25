using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace Odberatele
{
    public enum ProductType {Etiketa = 1, Sleeve, Tuba, Obal}
    public abstract class Partner
    {
        public abstract ProductType Produkt { get; set; }
        public abstract double RozmerX { get; set; }
        public abstract double RozmerY { get; set; }
        public abstract int Mnozstvi { get; set; }
        public abstract string Jednotky { get; set; }
        public abstract string OrderDate { get; set; }

        public abstract void SequenceDecoding(string sequence);
        public abstract string SequenceVerification(string sequence);
        
        protected void StoreProduct(string[] set, string substitute)
        {
            for (int i = 1; i <= set.Length; i++)
            {
                if (substitute == set[i - 1])
                {
                    this.Produkt = (ProductType) i;
                }
            }
        }
    }

    //--------------------ALFA----------------------
    public class Alfa : Partner
    {
        public override ProductType Produkt { get; set; }
        public override double RozmerX { get; set; }
        public override double RozmerY { get; set; }
        public override int Mnozstvi { get; set; }
        public override string Jednotky { get; set; }
        
        public override string OrderDate { get; set; }
        
        public override void SequenceDecoding(string sequence)        //Tvar: T00X0W0Y0W0MJ
        {
            var foundIndexes = new List<int>();     //Indexes of 'W' 
            for (int i = 0; i < sequence.Length; i++)
            {
                if (sequence[i] == 'W')
                    foundIndexes.Add(i);
            }
            //Typ produktu 
            string zkratka = sequence.Substring(0, 1);
            StoreProduct(new string[]{"E", "S", "T", "O"}, zkratka);
            
            //Rozmer X
            RozmerX = Convert.ToDouble(sequence.Substring(3, foundIndexes[0] - 3));
            //Rozmer Y
            RozmerY = Convert.ToDouble(sequence.Remove(foundIndexes[1] - 1).Substring(foundIndexes[0] + 2));
            //Jednotky J
            Jednotky = sequence.Substring(sequence.Length - 2);
            //Mnozstvi M
            Mnozstvi = Convert.ToInt32(sequence.Remove(sequence.Length - 2).Substring(foundIndexes[1] + 2));
        }

        public override string SequenceVerification(string sequence)
        {
            string adj = sequence.Remove(sequence.Length - 2);
            var x = from c in adj
                where char.IsNumber(c) && c != '0'
                select Convert.ToInt32(char.GetNumericValue(c));
            
            int soucetCislic = x.Sum();
            return (soucetCislic % 17).ToString();
        }
    }

    //--------------------BETA----------------------
    public class Beta : Partner
    {
        public override ProductType Produkt { get; set; }
        public override double RozmerX { get; set; }
        public override double RozmerY { get; set; }
        public override int Mnozstvi { get; set; }
        public override string Jednotky { get; set; }
        
        public override string OrderDate { get; set; }

        private int _pocet5;
        
        public override void SequenceDecoding(string sequence)        //Tvar: MJUXxYtT
        {
            //Mnozstvi
            int indexPrvni5 = sequence.IndexOf('5');
            string mnozstvi = sequence.Substring(0, indexPrvni5 - 2);
            Mnozstvi = Convert.ToInt32(mnozstvi);
            //Jednotka
            Jednotky = sequence.Substring(indexPrvni5 - 2, 2);
            
            //Typ
            string zkratka = sequence.Substring(sequence.IndexOf('t') + 1);
            StoreProduct(new string[]{"lbl", "slee", "lam", "flex"}, zkratka);
            
            //RozmerX
            RozmerX = Convert.ToDouble(Pomocne.ReturnFromTo(15, 'x', sequence));
            
            //RozmerY
            RozmerY = Convert.ToDouble(Pomocne.ReturnFromTo(sequence.IndexOf('x') + 1, 't', sequence));
            
            //Overeni
            _pocet5 = 15 - mnozstvi.Length - Jednotky.Length;
        }

        public override string SequenceVerification(string sequence)
        {
            return (_pocet5 + Mnozstvi).ToString();
        }
    }

    //--------------------GAMA----------------------
    public class Gama : Partner
    {
        public override ProductType Produkt { get; set; }
        public override double RozmerX { get; set; }
        public override double RozmerY { get; set; }
        public override int Mnozstvi { get; set; }
        public override string Jednotky { get; set; }
        
        public override string OrderDate { get; set; }

        public override void SequenceDecoding(string sequence)        //Tvar: GUYUXAMJ|T
        {
            //Typ
            string zkratka = sequence.Substring(sequence.IndexOf('|') + 1);
            StoreProduct(new string[]{"ETIKETA", "SLEEVE", "TUBA", "OBAL"}, zkratka);

            //Mnozstvi a Jednotka
            int a = sequence.IndexOf('A');
            string mnozstvi = Pomocne.ReturnFromTo(a + 1, '|', sequence); //Mnozstvi + jednotka
            char jednotka = Convert.ToChar(mnozstvi.Substring(mnozstvi.Length - 1)); //Jednotka
            Mnozstvi = Convert.ToInt32(mnozstvi.Remove(mnozstvi.Length - 1)); //Mnozstvi

            //Prevod jednotek
            switch (jednotka)
            {
                case 'p':
                    Jednotky = "ks";
                    break;
                case 'w':
                    Jednotky = "kg";
                    break;
                case 's':
                    Jednotky = "m2";
                    break;
            }
            
            //RozmerY
            string first7 = sequence.Substring(1, 7);
            RozmerY = Convert.ToDouble(Pomocne.RemoveZeros(first7));
            
            //RozmerX
            string second7 = Pomocne.ReturnFromTo(8, 'A', sequence);
            RozmerX = Convert.ToDouble(Pomocne.RemoveZeros(second7));
        }

        public override string SequenceVerification(string sequence)
        {
            var delkaSekv = Pomocne.ReturnFromTo(0, '|', sequence).Length;
            return "AB" + delkaSekv;
        }
    }
}
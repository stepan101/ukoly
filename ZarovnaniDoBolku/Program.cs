using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace zarovnani
{
    /// <summary>
    /// Kontrola argumentů a vstupních a výstupních souborů.
    /// </summary>
    class Kontrola
    {
        private string[] argumenty;
        public Kontrola(string[] argumenty)
        {
            this.argumenty = argumenty;

        }
        public bool Write()
        {
            try
            {
                StreamWriter write = new StreamWriter(argumenty[1]);
                write.Close();
                return true;
            }
            catch (IOException)
            {
                return false;

            }
        }
        public bool Read()
        {
            try
            {
                StreamReader read = new StreamReader(argumenty[0]);
                read.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool Arg()
        {
            if (argumenty.Length != 3) return false;
            else return true;
        }
        public bool Pocet()
        {
            try
            {
                Int32.Parse(argumenty[2]);
                if (Int32.Parse(argumenty[2]) > 0) return true;
                else return false;
            }
            catch
            {
                return false;
            }

        }
        public bool Vse()
        {
            if (Arg() && Pocet())
            {
                if (Write() && Read())
                {
                    return true;
                }
                else return false;
            }
            else return false;
        }
        public void Chyba()
        {
            if (!Arg() || !Pocet())
            {
                Console.WriteLine("Argument Error");
            }
            else
            {
                Console.WriteLine("File Error");
            }
        }
    }
    /// <summary>
    /// Nalezení jednoho slova
    /// </summary>
    class Slovo
    {
        private StringBuilder builder = new StringBuilder();
        private int zarazka = 0;



        public string CteniZnaku(StreamReader reader)
        {
            builder.Length = 0;
            int i;
            char znak;
            while ((i = reader.Read()) != -1)
            {

                znak = (char)i;
                if (znak.Equals('\t') || znak.Equals(' ') || znak.Equals('\n'))
                {
                    if (znak.Equals('\n'))
                    {
                        zarazka++;
                    }

                    if (builder.Length > 0)
                    {

                        return builder.ToString();
                    }
                    else
                    {
                        if (zarazka > 1 && znak.Equals('\n'))
                        {
                            zarazka = 0;

                            return " ";
                        }
                    }

                }
                else
                {
                    builder.Append(znak);
                    zarazka = 0;
                }

            }
            return builder.ToString();
        }

    }
    /// <summary>
    /// Výpočet mezer a zarovnání
    /// </summary>
    class Vypocet
    {
        private List<string> slova = new List<string>();
        private StringBuilder spojeni = new StringBuilder();
        private StreamWriter writer;
        private int PocetPozic;
        private bool OdstavecZarazka = false;
        private int VsechnySlovo;
        private int deleni;
        private int zbytek;
        private int velikost = 0;


        public Vypocet(StreamWriter writer, int PocetPozic)
        {
            this.writer = writer;
            this.PocetPozic = PocetPozic;
        }
        private void Mezery()
        {

            if (slova.Count() > 1)
            {
                deleni = (PocetPozic - velikost) / (slova.Count() - 1);
                zbytek = (PocetPozic - velikost) - (deleni * (slova.Count() - 1));

                foreach (string abc in slova)
                {
                    spojeni.Append(abc);
                    if (zbytek > 0)
                    {
                        for (int l = 0; l <= deleni; l++)
                        {
                            spojeni.Append(" ");

                        }
                        zbytek--;
                    }
                    else
                    {
                        for (int k = 0; k < deleni; k++)
                        {
                            spojeni.Append(" ");
                        }

                    }

                }
                spojeni.Length = PocetPozic;
                writer.Write(spojeni.ToString());
                writer.Write("\n");



            }
            else if (slova.Count() == 1)
            {
                writer.Write(slova[0]);
                writer.Write("\n");

            }
            VsechnySlovo = 0;
            slova.Clear();
            spojeni.Length = 0;
            velikost = 0;
        }


        public void KontrolaSlova(string slovo)
        {
            if (slovo == " " || slovo == "")
            {
                KonecOdstavce();

                velikost = 0;
            }
            else
            {

                if (OdstavecZarazka)
                {
                    writer.Write("\n");
                    OdstavecZarazka = false;
                }

                if ((velikost + slovo.Length + slova.Count()) > PocetPozic)
                {
                    if (slova.Count() > 0)
                    {
                        Mezery();
                    }

                }

                velikost = velikost + slovo.Length;
                slova.Add(slovo);

            }
        }

        private void KonecOdstavce()
        {

            int i = 1;
            foreach (string slovo in slova)
            {
                if (i < slova.Count()) writer.Write("{0} ", slovo);
                else { writer.Write(slovo); writer.Write("\n"); }
                i++;
            }
            slova.Clear();
            OdstavecZarazka = true;

        }

        private void VelikostSlov()
        {
            foreach (string ac in slova)
            {
                VsechnySlovo = VsechnySlovo + ac.Length;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            Kontrola kontrola = new Kontrola(args);
            if (kontrola.Vse())
            {
                StreamReader reader = new StreamReader(args[0]);
                StreamWriter writer = new StreamWriter(args[1]);

                int PocetPozic = Int32.Parse(args[2]);
                bool zarazka = true;
                string jedno;

                Slovo slovo = new Slovo();
                Vypocet vypocet = new Vypocet(writer, PocetPozic);

                while (zarazka)
                {
                    jedno = slovo.CteniZnaku(reader);
                    vypocet.KontrolaSlova(jedno);

                    if (jedno == "")
                    {
                        zarazka = false;
                        break;
                    }

                }
                reader.Close();
                writer.Close();
            }
            else
            {
            }


            kontrola.Chyba();
        }
    }
}

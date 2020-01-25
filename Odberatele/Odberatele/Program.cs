using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace Odberatele
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create new file or rewrite old one if it already exist
            FileWriter fw = new FileWriter();
            fw.Dispose();
            
            //Paths
            string inputPath = @"input.csv";
            string productionPath = @"production.xml";
            
            //(used for xml later)
            List<Partner> partners = new List<Partner>();   //List of partners
            
            
            try
            {
                using (StreamReader sr = new StreamReader(inputPath))
                {
                    string headerLine = sr.ReadLine();
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        //Count and verify columns
                        int collumnCount = headerLine.Count(c => c == ';') + 1;
                        if (line.Count(c => c == ';') + 1 != collumnCount)
                        {
                            Console.WriteLine("Nespravny pocet polozek.");
                        }

                        string[] split_line_data = line.Split(';');
                        
                        //Get date
                        split_line_data[0] = split_line_data[0].Replace('.', '-');
                        string date = DateTime.ParseExact(split_line_data[0], "dd-MM-yyyy", CultureInfo.CurrentCulture).ToString("yyyy-MM-dd");
                        
                        //Full name of subscriber - Namespace.ClassName
                        string subscriber = "Odberatele." + split_line_data[1];

                        Partner partner;
                        try
                        {
                            //Creates instance of subsriber
                            partner = (Partner) Activator.CreateInstance(Type.GetType(subscriber));

                            //Decoding of sequence => data gathering
                            partner.SequenceDecoding(split_line_data[2]);
                        
                            //Lastly verify with last collumn
                            if ( (partner.SequenceVerification(split_line_data[2])) == split_line_data[3] )
                            {
                                //Insert partner into successfully verifed.
                                partners.Add(partner);
                                partners.Last().OrderDate = date;
                            }
                            else 
                            {    
                                fw.Logger();
                                fw.Log("Pro partnera " + split_line_data[1] + " a EAN " + split_line_data[2] + " nesouhlasí kontrolní sekvence.");
                                fw.Dispose();
                            }
                        }
                        catch (Exception e)  //If className of partner is unknown.
                        {
                            fw.Logger();
                            fw.Log("Partner " + split_line_data[1] + " nebyl nalezen.");
                            fw.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            //Write subscribers into production.xml
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            using (XmlWriter xw = XmlWriter.Create(productionPath, settings))
            {
                xw.WriteStartDocument();
                xw.WriteStartElement("Production");
                xw.WriteStartElement("Items");

                foreach (Partner p in partners)
                {
                    xw.WriteStartElement("Item");
                    xw.WriteAttributeString("type", p.Produkt.ToString());
                    xw.WriteAttributeString("date", p.OrderDate); 
                    xw.WriteStartElement("Partner");
                    xw.WriteValue(p.GetType().Name); 
                    xw.WriteEndElement();
                    xw.WriteStartElement("Width");
                    xw.WriteValue(p.RozmerX);
                    xw.WriteEndElement();
                    xw.WriteStartElement("Height");
                    xw.WriteValue(p.RozmerY);
                    xw.WriteEndElement();
                    xw.WriteStartElement("Amount");
                    xw.WriteAttributeString("unit", p.Jednotky);
                    xw.WriteValue(p.Mnozstvi);
                    xw.WriteEndElement();   
                    xw.WriteEndElement();
                }
                
                xw.WriteEndElement(); 
                xw.WriteEndElement(); 
                xw.WriteEndDocument();
                xw.Flush();
            }
        }
    }
}
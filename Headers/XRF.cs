using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace XRF_Data_Analysis.Headers
{
    /// <summary>
    /// Structure to store the list of all possible elements and lines
    /// </summary>
    public class Element_Line
    {
        public string Element;
        public string Line;
    }

    /// <summary>
    /// Structure to store the data row inside the XRF tables
    /// </summary>
    public class XRF_Data
    {
        public string Element;
        public string Line;
        public float Energy_KeV;
        public float Net;
        public float Background;
        public float Sigma;
        public float Chi;
        public int NumberOfMeasurements;
        public List<double> Measurements = new List<double>();

        public XRF_Data DeepCopy()
        {
            XRF_Data XRF_Data_DeepCopy = new XRF_Data();
            XRF_Data_DeepCopy.Element = Element;
            XRF_Data_DeepCopy.Line = Line;
            XRF_Data_DeepCopy.Energy_KeV = Energy_KeV;
            XRF_Data_DeepCopy.Net = Net;
            XRF_Data_DeepCopy.Background = Background;
            XRF_Data_DeepCopy.Sigma = Sigma;
            XRF_Data_DeepCopy.Chi = Chi;
            XRF_Data_DeepCopy.NumberOfMeasurements = NumberOfMeasurements;

            foreach (double Msr in Measurements)
            {
                XRF_Data_DeepCopy.Measurements.Add(Msr);
            }

            return XRF_Data_DeepCopy;
        }
    }

    /// <summary>
    /// Structure to store the information in XRF tables
    /// </summary>
    public class XRF_Table
    {
        public string Full_File_Name;
        public string Folio_Source;
        public string Folio_Source_Measurement;
        public string Source;
        public string Folio;
        public string Measurement;
        public int No_Of_Measurements;

        public List<XRF_Data> Data = new List<XRF_Data>();

        // method for cloning object 
        public XRF_Table DeepCopy()
        {
            XRF_Table XRF_Table_DeepCopy = new XRF_Table();
            XRF_Table_DeepCopy.Folio = Folio;
            XRF_Table_DeepCopy.Measurement = Measurement;
            XRF_Table_DeepCopy.Source = Source;
            XRF_Table_DeepCopy.Folio_Source = Folio_Source;
            XRF_Table_DeepCopy.Folio_Source_Measurement = Folio_Source_Measurement;
            XRF_Table_DeepCopy.Full_File_Name = Full_File_Name;
            XRF_Table_DeepCopy.No_Of_Measurements = No_Of_Measurements;

            List<XRF_Data> XRF_Data_DeepCopy = new List<XRF_Data>();
            foreach (XRF_Data data in Data)
            {
                XRF_Data Data_Temp = new XRF_Data();
                Data_Temp = data.DeepCopy();
                XRF_Data_DeepCopy.Add(Data_Temp);
            }

            XRF_Table_DeepCopy.Data.AddRange(XRF_Data_DeepCopy);

            return XRF_Table_DeepCopy;
        }
    }

    public class XRF
    {
        public List<string> Support_Words = new List<string>()
            {
                "paper",
                "papier",
                "support",
                "parchment",
                "papyrus",
                "pergament",
                "parch",
                "pap",
                "p"
            };

        /// <summary>
        /// Read the list of all possible elements an lines and store it in a structure
        /// </summary>
        /// <param name="Path"> The storing path</param>
        /// <returns> Element-Line structure </returns>
        public List<Element_Line> Read_Element_Line_List(string Path)
        {
            StreamReader reader = new StreamReader(File.OpenRead(@Path));
            List<Element_Line> All_Element_Line = new List<Element_Line>();

            bool FirstLine = true;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (!String.IsNullOrWhiteSpace(line) && !FirstLine)
                {
                    Element_Line Element_Line_Temp = new Element_Line();
                    string[] values = line.Split(',', ';');
                    if (values.Length == 2)
                    {
                        Element_Line_Temp.Element = values[0];
                        Element_Line_Temp.Line = values[1];
                        All_Element_Line.Add(Element_Line_Temp);
                    }
                }
                FirstLine = false;
            }

            return All_Element_Line;
        }

        /// <summary>
        /// Read all XRF tables in a folder and store the information in a list of structures
        /// </summary>
        /// <param name="Path"> folder path of XRF tables </param>
        /// <returns>List of structures holding information of XRF tables</returns>
        public List<XRF_Table> Read_XRF_Tables(string Path)
        {
            List<XRF_Table> All_XRF_Tables = new List<XRF_Table>();

            // loop over all valid files with ".csv" extension
            IEnumerable<string> files_CSV;
            files_CSV = Directory.EnumerateFiles(Path, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".csv"));
            //List<string> files_CSV = Directory.EnumerateFiles(Path).ToList();

            foreach (string csv_table in files_CSV)
            {
                string Current_Path = System.IO.Path.Combine(Path, csv_table);
                StreamReader reader = new StreamReader(File.OpenRead(@Current_Path));

                // fill the folio and source
                XRF_Table XRF_Table_Temp = new XRF_Table();
                string OnlyFileName = System.IO.Path.GetFileNameWithoutExtension(csv_table);
                XRF_Table_Temp.Full_File_Name = OnlyFileName;
                string[] FileName = OnlyFileName.Split('_');

                if (FileName.Length > 2)
                {
                    XRF_Table_Temp.Folio = FileName[0];
                    XRF_Table_Temp.Source = FileName[1];
                    XRF_Table_Temp.Measurement = FileName[2];
                    XRF_Table_Temp.Folio_Source = FileName[0] + "_" + FileName[1];
                    XRF_Table_Temp.Folio_Source_Measurement = FileName[0] + "_" + FileName[1] + "_" + FileName[2];
                    XRF_Table_Temp.No_Of_Measurements = 1;
                }
                else
                    if (FileName.Length == 2)
                {
                    XRF_Table_Temp.Folio = FileName[0];
                    XRF_Table_Temp.Source = FileName[1];
                    XRF_Table_Temp.Measurement = "NoID";
                    XRF_Table_Temp.Folio_Source = FileName[0] + "_" + FileName[1];
                    XRF_Table_Temp.Folio_Source_Measurement = FileName[0] + "_" + FileName[1] + "_" + "NoID";
                    XRF_Table_Temp.No_Of_Measurements = 1;
                }

                bool FirstLine = true;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (!String.IsNullOrWhiteSpace(line) && !FirstLine)
                    {
                        if (line.Contains(';'))
                        {
                            line = line.Replace(',', '.');
                        }

                        XRF_Data XRF_Data_Temp = new XRF_Data();
                        string[] values = line.Split(',', ';');
                        if (values.Length == 11)
                        {
                            XRF_Data_Temp.Element = values[0];
                            XRF_Data_Temp.Line = values[1];
                            XRF_Data_Temp.Energy_KeV = float.Parse(values[2]);
                            XRF_Data_Temp.Net = float.Parse(values[4]);
                            XRF_Data_Temp.Background = float.Parse(values[5]);
                            XRF_Data_Temp.Sigma = float.Parse(values[6]);
                            XRF_Data_Temp.Chi = float.Parse(values[7]);
                            XRF_Data_Temp.NumberOfMeasurements = 1;

                            XRF_Table_Temp.Data.Add(XRF_Data_Temp);
                        }
                    }
                    FirstLine = false;
                }
                reader.Close();
                All_XRF_Tables.Add(XRF_Table_Temp);
            }

            return All_XRF_Tables;
        }

        /// <summary>
        /// Write XRF table to a file in Path
        /// </summary>
        /// <param name="Table"> Structure holding information of XRF table </param>
        /// <param name="Path"> storage path</param>
        public void Write_XRF_Table(XRF_Table Table, string Path, string Name_Extension = "")
        {
            var XRF_Table = new System.Text.StringBuilder();

            XRF_Table.AppendLine("Element, Line, Energy/keV, Net, Backgr., Sigma, Chi, Mean_Net, Max_Net, Min_Net");
            foreach (XRF_Data data in Table.Data)
            {
                if (data.Measurements.Count() > 1)
                {
                    double Mean, Max, Min;
                    Max = data.Measurements.Max();
                    Min = data.Measurements.Min();
                    Mean = data.Measurements.Sum() / data.Measurements.Count();

                    XRF_Table.AppendLine(data.Element + "," + data.Line + "," + Convert.ToString(data.Energy_KeV) + "," + Convert.ToString(data.Net)
                        + "," + Convert.ToString(data.Background) + "," + Convert.ToString(data.Sigma) + "," + Convert.ToString(data.Chi) + "," +
                        Convert.ToString(Mean) + "," + Convert.ToString(Max) + "," + Convert.ToString(Min));
                }
                else
                {
                    XRF_Table.AppendLine(data.Element + "," + data.Line + "," + Convert.ToString(data.Energy_KeV) + "," + Convert.ToString(data.Net)
                        + "," + Convert.ToString(data.Background) + "," + Convert.ToString(data.Sigma) + "," + Convert.ToString(data.Chi) + "," +
                        "NA" + "," + "NA" + "," + "NA");
                }
            }

            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }

            string Table_FileName;   
            Table_FileName = Path + "/" + Table.Folio_Source_Measurement + Name_Extension + ".csv";
            System.IO.File.WriteAllText(Table_FileName, XRF_Table.ToString());
        }

        /// <summary>
        /// Eliminate any table entry with Chi value more than "Chi"
        /// </summary>
        /// <param name="Tables">List of structures holding information of XRF tables</param>
        /// <param name="Chi">Chi Threshold value</param>
        /// <returns>List of structures holding information of XRF tables</returns>
        public List<XRF_Table> Chi_Filter(List<XRF_Table> Tables, float Chi = 10)
        {
            float Chi_Value = new float();
            if (Chi <= 0)
                Chi_Value = 10;
            else
                Chi_Value = Chi;

            List<XRF_Table> Tables_Filtered = new List<XRF_Table>();

            for (int tables = 0; tables < Tables.Count(); tables++)
            {
                XRF_Table Temp_Table = new XRF_Table();
                Temp_Table.Folio = Tables[tables].Folio;
                Temp_Table.Source = Tables[tables].Source;
                Temp_Table.Measurement = Tables[tables].Measurement;
                Temp_Table.Folio_Source = Tables[tables].Folio_Source;
                Temp_Table.Folio_Source_Measurement = Tables[tables].Folio_Source_Measurement;
                Temp_Table.Full_File_Name = Tables[tables].Full_File_Name;
                Temp_Table.No_Of_Measurements = Tables[tables].No_Of_Measurements;

                for (int data = 0; data < Tables[tables].Data.Count(); data++)
                {
                    if (Tables[tables].Data[data].Chi < Chi_Value)
                    {
                        Temp_Table.Data.Add(Tables[tables].Data[data]);
                    }
                }
                Tables_Filtered.Add(Temp_Table.DeepCopy());
            }

            return Tables_Filtered;
        }

        /// <summary>
        /// Keep only the line with highest energy for each existing element in a table
        /// </summary>
        /// <param name="Tables">List of structures holding information of XRF tables</param>
        /// <returns>List of structures holding information of XRF tables</returns>
        public List<XRF_Table> XRF_Main_Lines(List<XRF_Table> Tables)
        {
            List<XRF_Table> Tables_Result = new List<XRF_Table>();

            for (int tables = 0; tables < Tables.Count(); tables++)
            {
                XRF_Table Temp_Table = new XRF_Table();
                Temp_Table.Folio = Tables[tables].Folio;
                Temp_Table.Source = Tables[tables].Source;
                Temp_Table.Measurement = Tables[tables].Measurement;
                Temp_Table.Folio_Source = Tables[tables].Folio_Source;
                Temp_Table.Folio_Source_Measurement = Tables[tables].Folio_Source_Measurement;
                Temp_Table.Full_File_Name = Tables[tables].Full_File_Name;
                Temp_Table.No_Of_Measurements = Tables[tables].No_Of_Measurements;

                /// Sorting the XRF Data structures according to the energy descendingly
                Tables[tables].Data.OrderByDescending(i => i.Energy_KeV);

                for (int data1 = 0; data1 < Tables[tables].Data.Count(); data1++)
                {
                    bool found = false;
                    for (int data2 = 0; data2 < Temp_Table.Data.Count(); data2++)
                    {
                        if (Temp_Table.Data[data2].Element.ToLower() == Tables[tables].Data[data1].Element.ToLower())
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        Temp_Table.Data.Add(Tables[tables].Data[data1]);
                    }
                }
                Tables_Result.Add(Temp_Table.DeepCopy());
            }

            return Tables_Result;
        }

        /// <summary>
        /// Normalise the Net values by a given Element or the total Net summation
        /// </summary>
        /// <param name="Tables">List of structures holding information of XRF tables</param>
        /// <param name="Factor"> Normalisation factor</param>
        /// <returns></returns>
        public List<XRF_Table> NormaliseBy(List<XRF_Table> Tables, string Factor = "Fe")
        {
            List<XRF_Table> Tables_Result = new List<XRF_Table>();
            if (Factor.ToLower() == "total")
            {
                for (int tables = 0; tables < Tables.Count(); tables++)
                {
                    XRF_Table Temp_Table = Tables[tables];

                    float Net_Summation = 0;
                    for (int data = 0; data < Tables[tables].Data.Count(); data++)
                    {
                        Net_Summation += Tables[tables].Data[data].Net;
                    }

                    for (int norm = 0; norm < Temp_Table.Data.Count(); norm++)
                    {
                        if (Net_Summation == 0)
                        {
                            Temp_Table.Data[norm].Net = float.NaN;
                        }
                        else if (Temp_Table.Data[norm].Net == 0)
                        {
                            Temp_Table.Data[norm].Net = 0;
                        }
                        else
                            Temp_Table.Data[norm].Net /= Net_Summation;
                    }
                    Tables_Result.Add(Temp_Table.DeepCopy());
                }

            }
            else
            {
                for (int tables = 0; tables < Tables.Count(); tables++)
                {
                    XRF_Table Temp_Table = Tables[tables];

                    float Norm_Factor = new float();
                    bool Found = false;
                    for (int data = 0; data < Tables[tables].Data.Count(); data++)
                    {
                        if (Tables[tables].Data[data].Element.ToLower() == Factor.ToLower())
                        {
                            Norm_Factor = Tables[tables].Data[data].Net;
                            Found = true;
                            break;
                        }
                    }
                    if (!Found)
                        Norm_Factor = 1;

                    for (int norm = 0; norm < Temp_Table.Data.Count(); norm++)
                    {
                        if (Norm_Factor == 0)
                        {
                            Temp_Table.Data[norm].Net = float.NaN;
                        }
                        else if (Temp_Table.Data[norm].Net == 0)
                        {
                            Temp_Table.Data[norm].Net = 0;
                        }
                        else
                            Temp_Table.Data[norm].Net /= Norm_Factor;
                    }
                    Tables_Result.Add(Temp_Table.DeepCopy());
                }

            }
            return Tables_Result;
        }

        /// <summary>
        /// Subtracting the Net values corresponding to the support measurements from the net values corresponding to the ink measurements within the same folio
        /// </summary>
        /// <param name="Tables">List of structures holding information of XRF tables</param>
        /// <returns>List of structures holding information of XRF tables</returns>
        public List<XRF_Table> Subtracting_Support_From_Ink(List<XRF_Table> Tables)
        {
            List<XRF_Table> Tables_Result = new List<XRF_Table>();

            for (int outer = 0; outer < Tables.Count(); outer++)
            {
                XRF_Table Temp_Table = Tables[outer].DeepCopy();

                for (int inner = 0; inner < Tables.Count(); inner++)
                {
                    if (Tables[outer].Folio.ToLower() == Tables[inner].Folio.ToLower() &&
                        Support_Words.Contains(Tables[inner].Source.ToLower()) &&
                        !Support_Words.Contains(Tables[outer].Source.ToLower()))
                    {
                        for (int data_outer = 0; data_outer < Tables[outer].Data.Count(); data_outer++)
                        {
                            for (int data_inner = 0; data_inner < Tables[inner].Data.Count(); data_inner++)
                            {
                                if (Tables[outer].Data[data_outer].Element.ToLower() == Tables[inner].Data[data_inner].Element.ToLower() &&
                                   Tables[outer].Data[data_outer].Line.ToLower() == Tables[inner].Data[data_inner].Line.ToLower())
                                {
                                    float Subtraction;
                                    Subtraction = Tables[outer].Data[data_outer].Net - Tables[inner].Data[data_inner].Net;
                                    if (Subtraction < 0)
                                        Subtraction = 0;
                                    Temp_Table.Data[data_outer].Net = Subtraction;
                                }
                            }
                        }
                    }
                }
                Tables_Result.Add(Temp_Table.DeepCopy());
            }

            return Tables_Result;
        }

        /// <summary>
        /// Reordering the rows within the XRF tables based on the standard order in the "List of Elements"
        /// </summary>
        /// <param name="Tables">List of structures holding information of XRF tables</param>
        /// <param name="ListOfElements">a list of all possible elements with their energy lines</param>
        /// <returns>List of structures holding information of XRF tables</returns>
        public List<XRF_Table> Reorder_Elements(List<XRF_Table> Tables, List<Element_Line> ListOfElements)
        {
            List<XRF_Table> Tables_Result = new List<XRF_Table>();
            for (int table = 0; table < Tables.Count(); table++)
            {
                XRF_Table Temp_Table = new XRF_Table();
                Temp_Table.Folio = Tables[table].Folio;
                Temp_Table.Source = Tables[table].Source;
                Temp_Table.Measurement = Tables[table].Measurement;
                Temp_Table.Folio_Source = Tables[table].Folio_Source;
                Temp_Table.Folio_Source_Measurement = Tables[table].Folio_Source_Measurement;
                Temp_Table.Full_File_Name = Tables[table].Full_File_Name;
                Temp_Table.No_Of_Measurements = Tables[table].No_Of_Measurements;

                for (int element = 0; element < ListOfElements.Count(); element++)
                {
                    for (int data = 0; data < Tables[table].Data.Count(); data++)
                    {
                        if (ListOfElements[element].Element.ToLower() == Tables[table].Data[data].Element.ToLower() &&
                                ListOfElements[element].Line.ToLower() == Tables[table].Data[data].Line.ToLower())
                        {
                            Temp_Table.Data.Add(Tables[table].Data[data]);
                            break;
                        }
                    }
                }
                Tables_Result.Add(Temp_Table);
            }

            return Tables_Result;
        }

        /// <summary>
        /// Reordering the rows within the XRF tables based on the standard order in the "List of Elements",
        /// if an element is missing from the XRF table, this function will fill it with zeros.
        /// </summary>
        /// <param name="Tables">List of structures holding information of XRF tables</param>
        /// <param name="ListOfElements">a list of all possible elements with their energy lines</param>
        /// <returns>List of structures holding information of XRF tables</returns>
        public List<XRF_Table> Reorder_Fill_Missing_ListOfElements(List<XRF_Table> Tables, List<Element_Line> ListOfElements)
        {
            List<XRF_Table> Tables_Result = new List<XRF_Table>();
            for (int table = 0; table < Tables.Count(); table++)
            {
                XRF_Table Temp_Table = new XRF_Table();
                Temp_Table.Folio = Tables[table].Folio;
                Temp_Table.Source = Tables[table].Source;
                Temp_Table.Measurement = Tables[table].Measurement;
                Temp_Table.Folio_Source = Tables[table].Folio_Source;
                Temp_Table.Folio_Source_Measurement = Tables[table].Folio_Source_Measurement;
                Temp_Table.Full_File_Name = Tables[table].Full_File_Name;
                Temp_Table.No_Of_Measurements = Tables[table].No_Of_Measurements;

                for (int element = 0; element < ListOfElements.Count(); element++)
                {
                    bool found = false;
                    for (int data = 0; data < Tables[table].Data.Count(); data++)
                    {
                        if (ListOfElements[element].Element.ToLower() == Tables[table].Data[data].Element.ToLower() &&
                                ListOfElements[element].Line.ToLower() == Tables[table].Data[data].Line.ToLower())
                        {
                            Temp_Table.Data.Add(Tables[table].Data[data]);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {

                        XRF_Data Temp_Data = new XRF_Data();
                        Temp_Data.Element = ListOfElements[element].Element;
                        Temp_Data.Line = ListOfElements[element].Line;
                        Temp_Data.Energy_KeV = 0;
                        Temp_Data.Net = 0;
                        Temp_Data.Background = 0;
                        Temp_Data.Sigma = 0;
                        Temp_Data.Chi = 0;

                        Temp_Table.Data.Add(Temp_Data);

                    }
                }
                Tables_Result.Add(Temp_Table);
            }

            return Tables_Result;
        }

        /// <summary>
        /// Reordering the rows within the XRF tables based on the standard order in the "List of Elements",
        /// if an element is missing from the XRF table, this function will fill it with zeros.
        /// </summary>
        /// <param name="Tables">List of structures holding information of XRF tables</param>
        /// <param name="ListOfElements">a list of all possible elements with their energy lines</param>
        /// <returns>List of structures holding information of XRF tables</returns>
        public List<XRF_Table> Reorder_Fill_Missing_ExistingElements(List<XRF_Table> Tables, List<string> ExistingElements)
        {
            List<XRF_Table> Tables_Result = new List<XRF_Table>();
            for (int table = 0; table < Tables.Count(); table++)
            {
                XRF_Table Temp_Table = new XRF_Table();
                Temp_Table.Folio = Tables[table].Folio;
                Temp_Table.Source = Tables[table].Source;
                Temp_Table.Measurement = Tables[table].Measurement;
                Temp_Table.Folio_Source = Tables[table].Folio_Source;
                Temp_Table.Folio_Source_Measurement = Tables[table].Folio_Source_Measurement;
                Temp_Table.Full_File_Name = Tables[table].Full_File_Name;
                Temp_Table.No_Of_Measurements = Tables[table].No_Of_Measurements;

                for (int element = 0; element < ExistingElements.Count(); element++)
                {
                    bool found = false;
                    for (int data = 0; data < Tables[table].Data.Count(); data++)
                    {
                        if (ExistingElements[element].ToLower() == Tables[table].Data[data].Element.ToLower())
                        {
                            Temp_Table.Data.Add(Tables[table].Data[data]);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        XRF_Data Temp_Data = new XRF_Data();
                        Temp_Data.Element = ExistingElements[element];
                        Temp_Data.Line = "MainLine";
                        Temp_Data.Energy_KeV = 0;
                        Temp_Data.Net = 0;
                        Temp_Data.Background = 0;
                        Temp_Data.Sigma = 0;
                        Temp_Data.Chi = 0;

                        Temp_Table.Data.Add(Temp_Data);

                    }
                }
                Tables_Result.Add(Temp_Table);
            }

            return Tables_Result;
        }

        /// <summary>
        /// Extract a feature vector from an XRF table using the Net values
        /// </summary>
        /// <param name="Table">A structure holding information of an XRF table</param>
        /// <returns>a Matrix containing the net values in the XRF tables as a feature vector</returns>
        public Mat Extract_XRF_Features(XRF_Table Table)
        {
            Mat Current_XRF_Features = new Mat();
            foreach (XRF_Data data in Table.Data)
            {
                Current_XRF_Features.Add(data.Net);
            }

            return Current_XRF_Features.T();
        }

        public List<XRF_Table> Remove_Zero_Net(List<XRF_Table> Tables)
        {
            List<XRF_Table> Tables_Result = new List<XRF_Table>();

            for (int table = 0; table < Tables.Count(); table++)
            {
                XRF_Table Temp_Table = new XRF_Table();
                Temp_Table.Folio = Tables[table].Folio;
                Temp_Table.Source = Tables[table].Source;
                Temp_Table.Measurement = Tables[table].Measurement;
                Temp_Table.Folio_Source = Tables[table].Folio_Source;
                Temp_Table.Folio_Source_Measurement = Tables[table].Folio_Source_Measurement;
                Temp_Table.Full_File_Name = Tables[table].Full_File_Name;
                Temp_Table.No_Of_Measurements = Tables[table].No_Of_Measurements;

                for (int data = 0; data < Tables[table].Data.Count(); data++)
                {
                    if (Tables[table].Data[data].Net > 0)
                    {
                        Temp_Table.Data.Add(Tables[table].Data[data]);
                    }
                }
                Tables_Result.Add(Temp_Table);
            }

            return Tables_Result;
        }

        /// <summary>
        /// Generate a table containing net values of all existing elements in a list of XRF tables to facilitate producing scatter plots
        /// </summary>
        /// <param name="Tables">List of structures holding information of XRF tables</param>
        /// <param name="Existing_Elements">List of elements, where each element exist at least in one XRF table</param>
        /// <param name="Path">storage path of the generated scatter plots table</param>
        public void Scatter_Plot_Table(List<XRF_Table> Tables, List<string> Existing_Elements, string Path, string Name_Extension = "")
        {
            var Scatter_Table = new System.Text.StringBuilder();

            // Generate the titles row in the scatter table
            string Titles_Row = "File_Name";
            foreach (string element in Existing_Elements)
            {
                Titles_Row += "," + element;
            }
            Scatter_Table.AppendLine(Titles_Row);

            // Generate the data row which contains the corresponding Net values
            foreach (XRF_Table table in Tables)
            {
                string Data_Row = table.Folio_Source_Measurement;
                foreach (string element in Existing_Elements)
                {
                    // Add the Net value if found in table data; otherwise, add zero
                    float Net_Value = 0;
                    foreach (XRF_Data data in table.Data)
                    {
                        if (element.ToLower() == data.Element.ToLower())
                        {
                            Net_Value = data.Net;
                            break;
                        }
                    }
                    Data_Row += "," + Convert.ToString(Net_Value);
                }
                Scatter_Table.AppendLine(Data_Row);
            }

            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
            string Table_FileName = Path + "/Scatter_Plots_Table" + Name_Extension + ".csv";
            System.IO.File.WriteAllText(Table_FileName, Scatter_Table.ToString());
        }

        public List<XRF_Table> DeepCopy(List<XRF_Table> Tables)
        {
            List<XRF_Table> Tables_Results = new List<XRF_Table>();

            foreach (XRF_Table table in Tables)
            {
                XRF_Table temp = table.DeepCopy();
                Tables_Results.Add(temp);
            }

            return Tables_Results;
        }

        /// <summary>
        /// Filter the entries of XRF tables according to their energy in KeV. Only entries within pre-defined range are kept.
        /// </summary>
        /// <param name="Tables"></param>
        /// <param name="Energy_Min"> Minimum energy in KeV</param>
        /// <param name="Energy_Max"> Maximum energy in KeV</param>
        /// <returns>List of structures holding information of XRF tables</returns>
        public List<XRF_Table> Energy_Filter_Include(List<XRF_Table> Tables, float Energy_Min = 0.11F, float Energy_Max = 100F)
        {
            if (Energy_Min < 0.11F)
                Energy_Min = 0.11F;

            if (Energy_Max > 100F)
                Energy_Max = 100F;

            List<XRF_Table> Tables_Filtered = new List<XRF_Table>();

            for (int tables = 0; tables < Tables.Count(); tables++)
            {
                XRF_Table Temp_Table = new XRF_Table();
                Temp_Table.Folio = Tables[tables].Folio;
                Temp_Table.Source = Tables[tables].Source;
                Temp_Table.Measurement = Tables[tables].Measurement;
                Temp_Table.Folio_Source = Tables[tables].Folio_Source;
                Temp_Table.Folio_Source_Measurement = Tables[tables].Folio_Source_Measurement;
                Temp_Table.Full_File_Name = Tables[tables].Full_File_Name;
                Temp_Table.No_Of_Measurements = Tables[tables].No_Of_Measurements;

                for (int data = 0; data < Tables[tables].Data.Count(); data++)
                {
                    if (Tables[tables].Data[data].Energy_KeV >= Energy_Min && Tables[tables].Data[data].Energy_KeV <= Energy_Max)
                    {
                        Temp_Table.Data.Add(Tables[tables].Data[data]);
                    }
                }
                Tables_Filtered.Add(Temp_Table.DeepCopy());
            }

            return Tables_Filtered;
        }

        public List<XRF_Table> Energy_Filter_Exclude(List<XRF_Table> Tables, float Energy_Min = 0.11F, float Energy_Max = 100F)
        {
            if (Energy_Min < 0.11F)
                Energy_Min = 0.11F;

            if (Energy_Max > 100F)
                Energy_Max = 100F;

            List<XRF_Table> Tables_Filtered = new List<XRF_Table>();

            for (int tables = 0; tables < Tables.Count(); tables++)
            {
                XRF_Table Temp_Table = new XRF_Table();
                Temp_Table.Folio = Tables[tables].Folio;
                Temp_Table.Source = Tables[tables].Source;
                Temp_Table.Measurement = Tables[tables].Measurement;
                Temp_Table.Folio_Source = Tables[tables].Folio_Source;
                Temp_Table.Folio_Source_Measurement = Tables[tables].Folio_Source_Measurement;
                Temp_Table.Full_File_Name = Tables[tables].Full_File_Name;
                Temp_Table.No_Of_Measurements = Tables[tables].No_Of_Measurements;

                for (int data = 0; data < Tables[tables].Data.Count(); data++)
                {
                    if (Tables[tables].Data[data].Energy_KeV < Energy_Min || Tables[tables].Data[data].Energy_KeV > Energy_Max)
                    {
                        Temp_Table.Data.Add(Tables[tables].Data[data]);
                    }
                }
                Tables_Filtered.Add(Temp_Table.DeepCopy());
            }

            return Tables_Filtered;
        }

        public List<XRF_Table> Avg_Measurements(List<XRF_Table> Tables, bool Avg_Support = true, bool Avg_Ink = false)
        {
            List<XRF_Table> Tables_Result = new List<XRF_Table>();

            foreach (XRF_Table Input_Table in Tables)
            {
                if (!Avg_Ink && !Avg_Support)
                {
                    Tables_Result.Add(Input_Table.DeepCopy());
                    continue;
                }
                else if (Support_Words.Contains(Input_Table.Source) && !Avg_Support)
                {
                    Tables_Result.Add(Input_Table.DeepCopy());
                    continue;
                }
                else if (!Support_Words.Contains(Input_Table.Source) && !Avg_Ink)
                {
                    Tables_Result.Add(Input_Table.DeepCopy());
                    continue;
                }

                bool exist = false;
                foreach (XRF_Table Avg_Table in Tables_Result)
                {
                    if (Input_Table.Folio_Source.ToLower() == Avg_Table.Folio_Source.ToLower())
                    {
                        Avg_Table.Measurement = "Multiple";
                        Avg_Table.Folio_Source_Measurement = Input_Table.Folio_Source + "_Multiple";
                        Avg_Table.No_Of_Measurements++;

                        foreach (XRF_Data data_Input in Input_Table.Data)
                        {

                            foreach (XRF_Data data_Avg in Avg_Table.Data)
                            {
                                if (data_Input.Element.ToLower() == data_Avg.Element.ToLower() && data_Input.Line.ToLower() == data_Avg.Line.ToLower())
                                {
                                    data_Avg.NumberOfMeasurements++;
                                    data_Avg.Net += data_Input.Net;
                                    data_Avg.Measurements.Add(data_Input.Net);
                                }
                            }
                        }

                        exist = true;
                        break;
                    }
                }
                if (!exist)
                {
                    XRF_Table Temp_Table = new XRF_Table();
                    Temp_Table = Input_Table.DeepCopy();
                    Temp_Table.No_Of_Measurements = 1;

                    foreach (XRF_Data data in Temp_Table.Data)
                    {
                        data.Measurements.Add(data.Net);
                        data.NumberOfMeasurements = 1;
                    }
                    Tables_Result.Add(Temp_Table.DeepCopy());
                }
            }

            foreach (XRF_Table Avg_Table in Tables_Result)
            {
                foreach (XRF_Data data_Avg in Avg_Table.Data)
                {
                    if (data_Avg.NumberOfMeasurements > 0)
                    {
                        data_Avg.Net /= data_Avg.NumberOfMeasurements;
                    }
                }
            }

            return Tables_Result;
        }
    }
}

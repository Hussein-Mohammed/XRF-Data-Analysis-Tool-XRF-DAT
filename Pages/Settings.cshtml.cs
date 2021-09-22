using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCvSharp;
using OpenCvSharp.Flann;
using XRF_Data_Analysis.Headers;

namespace XRF_Data_Analysis.Pages
{
    public class SettingsModel : PageModel
    {
        private IWebHostEnvironment _hostingEnvironment;
        private readonly XRF_Tables_Uploaded _XRF_Tables_Uploaded;

        // Default parameters for Tables processing
        public bool Chi_Filter_Tables { get; set; } = false;
        public float Chi_Value_Tables { get; set; } = 10;
        public bool Sub_Support_Tables { get; set; } = true;
        public bool Remove_Zero_Net_Tables { get; set; } = true;
        public string NormaliseBy_Tables { get; set; } = "Fe";
        public bool Normalise_Tables { get; set; } = true;
        public bool Energy_Filter_Include_Tables { get; set; } = false;
        public bool Energy_Filter_Exclude_Tables { get; set; } = false;
        public float Min_Energy_Include_Tables { get; set; } = 0.11F;
        public float Max_Energy_Include_Tables { get; set; } = 30;
        public float Min_Energy_Exclude_Tables { get; set; } = 0.11F;
        public float Max_Energy_Exclude_Tables { get; set; } = 30;
        public bool Avg_Ink_Tables { get; set; } = false;
        public bool Avg_Support_Tables { get; set; } = true;

        // Default parameters for Generating scatter plots tables
        public bool Chi_Filter_Scatter { get; set; } = false;
        public float Chi_Value_Scatter { get; set; } = 10;
        public bool Sub_Support_Scatter { get; set; } = true;
        public bool Remove_Zero_Net_Scatter { get; set; } = false;
        public string NormaliseBy_Scatter { get; set; } = "Fe";
        public bool Normalise_Scatter { get; set; } = true;
        public bool Energy_Filter_Include_Scatter { get; set; } = false;
        public bool Energy_Filter_Exclude_Scatter { get; set; } = false;
        public float Min_Energy_Include_Scatter { get; set; } = 0.11F;
        public float Max_Energy_Include_Scatter { get; set; } = 30;
        public float Min_Energy_Exclude_Scatter { get; set; } = 0.11F;
        public float Max_Energy_Exclude_Scatter { get; set; } = 30;
        public bool Avg_Ink_Scatter { get; set; } = false;
        public bool Avg_Support_Scatter { get; set; } = true;

        // Default parameters for similarity calculation
        public bool Chi_Filter_Similarity { get; set; } = false;
        public float Chi_Value_Similarity { get; set; } = 10;
        public bool Sub_Support_Similarity { get; set; } = true;
        public bool Remove_Zero_Net_Similarity { get; set; } = false;
        public string NormaliseBy_Similarity { get; set; } = "Total";
        public bool Normalise_Similarity { get; set; } = true;
        public bool Energy_Filter_Include_Similarity { get; set; } = false;
        public bool Energy_Filter_Exclude_Similarity { get; set; } = false;
        public float Min_Energy_Include_Similarity { get; set; } = 0.11F;
        public float Max_Energy_Include_Similarity { get; set; } = 30;
        public float Min_Energy_Exclude_Similarity { get; set; } = 0.11F;
        public float Max_Energy_Exclude_Similarity { get; set; } = 30;
        public bool Avg_Ink_Similarity { get; set; } = false;
        public bool Avg_Support_Similarity { get; set; } = true;

        public SettingsModel(IWebHostEnvironment hostingEnvironment, XRF_Tables_Uploaded XRF_Tables_Uploaded)
        {
            _hostingEnvironment = hostingEnvironment;
            _XRF_Tables_Uploaded = XRF_Tables_Uploaded;
        }

        public void OnGet()
        {

        }

        /// <summary>
        /// Set the selected parameters by user, and process the uploaded tables according to the specified settings. Then download the processed tables for the user
        /// </summary>
        /// <param name="Chi_Filter">apply Chi filter, or not</param>
        /// <param name="Chi_Value">set the Chi value</param>
        /// <param name="Sub_Support">subtract the measurements of support from the measurements of the ink within the same folio, or not</param>
        /// <param name="Remove_Zero_Net">remove any entry with Net equals zero, or not</param>
        /// <param name="NormaliseBy">normalisation factor (element)</param>
        /// <param name="Normalise">Normalise all Net values by the specified element, or not</param>
        /// <returns></returns>
        public ActionResult OnPostSettingsTables(bool Chi_Filter, float Chi_Value, bool Sub_Support, bool Remove_Zero_Net,
            string NormaliseBy, bool Normalise, float Min_Energy_Include, float Max_Energy_Include, float Min_Energy_Exclude, float Max_Energy_Exclude, bool Energy_Filter_Include, 
            bool Energy_Filter_Exclude, bool Avg_Ink, bool Avg_Support)
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            string Tables_Results_Folder = "XRF_Tables_Results";
            string Element_Line_Folder = "Element_Line";
            string Temp_Folder = "Temp";
            string Element_Line_Path = Path.Combine(webRootPath, Element_Line_Folder);
            string Tables_Results_Path = Path.Combine(webRootPath, Tables_Results_Folder);
            string TempPath = Path.Combine(webRootPath, Temp_Folder);
            if (!Directory.Exists(Tables_Results_Path))
            {
                Directory.CreateDirectory(Tables_Results_Path);
            }
            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }
            Directory.EnumerateFiles(Tables_Results_Path).ToList().ForEach(f => System.IO.File.Delete(f));
            Directory.EnumerateFiles(TempPath).ToList().ForEach(f => System.IO.File.Delete(f));

            XRF XRF_Data = new XRF();
            List<XRF_Table> Tables_Results = new List<XRF_Table>();

            _XRF_Tables_Uploaded.XRF_Tables_Avg_Measurement = XRF_Data.Avg_Measurements(_XRF_Tables_Uploaded.All_XRF_Tables, Avg_Support, Avg_Ink);
            Tables_Results = XRF_Data.DeepCopy(_XRF_Tables_Uploaded.XRF_Tables_Avg_Measurement);

            List<Element_Line> Element_Line_List = new List<Element_Line>();

            // Apply the fixed part of the processing on the uploaded tables (regardless of the selected settings)
            Element_Line_List = XRF_Data.Read_Element_Line_List(Element_Line_Path + "/ElementsList.csv");
            Tables_Results = XRF_Data.XRF_Main_Lines(Tables_Results);
            Tables_Results = XRF_Data.Reorder_Elements(Tables_Results, Element_Line_List);

            // Apply the optional part of the processing on the uploaded tables (based on the selected settings)
            string Name_Extension = "";
            if (Avg_Support)
            {
                Name_Extension += "_AvgSup";
            }
            
            if(Avg_Ink)
            {
                Name_Extension += "_AvgInk";
            }

            if (Energy_Filter_Include)
            {
                Tables_Results = XRF_Data.Energy_Filter_Include(Tables_Results, Min_Energy_Include, Max_Energy_Include);
                Name_Extension += "_EFI" + "-" + Min_Energy_Include.ToString() + "-" + Max_Energy_Include.ToString();
            }

            if (Energy_Filter_Exclude)
            {
                Tables_Results = XRF_Data.Energy_Filter_Exclude(Tables_Results, Min_Energy_Exclude, Max_Energy_Exclude);
                Name_Extension += "_EFE" + "-" + Min_Energy_Exclude.ToString() + "-" + Max_Energy_Exclude.ToString();
            }

            if (Chi_Filter)
            {
                Tables_Results = XRF_Data.Chi_Filter(Tables_Results, Chi_Value);
                Name_Extension += "_CF" + "-" + Chi_Value.ToString();
            }

            if (Sub_Support)
            {
                Tables_Results = XRF_Data.Subtracting_Support_From_Ink(Tables_Results);
                Name_Extension += "_Sub";
            }

            if (Remove_Zero_Net)
            {
                Tables_Results = XRF_Data.Remove_Zero_Net(Tables_Results);
                Name_Extension += "_RZ";
            }

            if (Normalise)
            {
                Tables_Results = XRF_Data.NormaliseBy(Tables_Results, NormaliseBy);
                Name_Extension += "_N" + "-" + NormaliseBy;
            }            

            // write the resulted tables to ".csv" files
            foreach (XRF_Table table in Tables_Results)
            {
                XRF_Data.Write_XRF_Table(table, Tables_Results_Path, Name_Extension);
            }

            // create a new archive
            string archive = TempPath + "\\Tables_Results.zip";
            ZipFile.CreateFromDirectory(Tables_Results_Path, archive);

            return File("/Temp/Tables_Results.zip", "application/zip", "Tables_Results.zip");
        }

        /// <summary>
        /// Generate a table for scatter plots. Then download the generated table for the user
        /// </summary>
        /// <returns></returns>
        public ActionResult OnPostSettingsScatter(bool Chi_Filter, float Chi_Value, bool Sub_Support, 
            string NormaliseBy, bool Normalise, float Min_Energy_Include, float Max_Energy_Include, float Min_Energy_Exclude, float Max_Energy_Exclude, bool Energy_Filter_Include,
            bool Energy_Filter_Exclude, bool Avg_Ink, bool Avg_Support)
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            string Scatter_Table_Folder = "Scatter_Table";
            string Temp_Folder = "Temp";
            string Element_Line_Folder = "Element_Line";
            string Element_Line_Path = Path.Combine(webRootPath, Element_Line_Folder);
            string Scatter_Table_Path = Path.Combine(webRootPath, Scatter_Table_Folder);
            string TempPath = Path.Combine(webRootPath, Temp_Folder);
            if (!Directory.Exists(Scatter_Table_Path))
            {
                Directory.CreateDirectory(Scatter_Table_Path);
            }
            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }
            Directory.EnumerateFiles(Scatter_Table_Path).ToList().ForEach(f => System.IO.File.Delete(f));
            Directory.EnumerateFiles(TempPath).ToList().ForEach(f => System.IO.File.Delete(f));

            XRF XRF_Data = new XRF();
            List<XRF_Table> Tables_Results = new List<XRF_Table>();

            _XRF_Tables_Uploaded.XRF_Tables_Avg_Measurement = XRF_Data.Avg_Measurements(_XRF_Tables_Uploaded.All_XRF_Tables, Avg_Support, Avg_Ink);
            Tables_Results = XRF_Data.DeepCopy(_XRF_Tables_Uploaded.XRF_Tables_Avg_Measurement);

            List<string> Existing_Elements = new List<string>();
            List<Element_Line> Element_Line_List = new List<Element_Line>();

            // Apply the fixed part of the processing on the uploaded tables (regardless of the selected settings)
            Element_Line_List = XRF_Data.Read_Element_Line_List(Element_Line_Path + "/ElementsList.csv");
            Tables_Results = XRF_Data.XRF_Main_Lines(Tables_Results);
            Tables_Results = XRF_Data.Reorder_Elements(Tables_Results, Element_Line_List);

            // Apply the optional part of the processing on the uploaded tables (based on the selected settings)
            string Name_Extension = "";
            
            if (Avg_Support)
            {
                Name_Extension += "_AvgSup";
            }

            if (Avg_Ink)
            {
                Name_Extension += "_AvgInk";
            }
            if (Energy_Filter_Include)
            {
                Tables_Results = XRF_Data.Energy_Filter_Include(Tables_Results, Min_Energy_Include, Max_Energy_Include);
                Name_Extension += "_EFI" + "-" + Min_Energy_Include.ToString() + "-" + Max_Energy_Include.ToString();
            }

            if (Energy_Filter_Exclude)
            {
                Tables_Results = XRF_Data.Energy_Filter_Exclude(Tables_Results, Min_Energy_Exclude, Max_Energy_Exclude);
                Name_Extension += "_EFE" + "-" + Min_Energy_Exclude.ToString() + "-" + Max_Energy_Exclude.ToString();
            }

            if (Sub_Support)
            {
                Tables_Results = XRF_Data.Subtracting_Support_From_Ink(Tables_Results);
                Name_Extension += "_Sub";
            }

            //if (Remove_Zero_Net)
            //{
            //    Tables_Results = XRF_Data.Remove_Zero_Net(Tables_Results);
            //    Name_Extension += "_RZ";
            //}

            if (Normalise)
            {
                Tables_Results = XRF_Data.NormaliseBy(Tables_Results, NormaliseBy);
                Name_Extension += "_N" + "-" + NormaliseBy;
            }

            // Find out which element exist in at least one table
            foreach (Element_Line Element_Line in Element_Line_List)
            {
                bool found = false;
                foreach (XRF_Table table in Tables_Results)
                {
                    foreach (XRF_Data data in table.Data)
                    {
                        if (Element_Line.Element.ToLower() == data.Element.ToLower())
                        {
                            Existing_Elements.Add(Element_Line.Element);
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        break;
                }
            }
            Existing_Elements = Existing_Elements.Distinct().ToList();

            // Generate a table for scatter plots and write it to a ".csv" file
            XRF_Data.Scatter_Plot_Table(Tables_Results, Existing_Elements, Scatter_Table_Path, Name_Extension);

            // create a new archive
            string archive = TempPath + "\\Scatter_Table.zip";
            ZipFile.CreateFromDirectory(Scatter_Table_Path, archive);
            return File("/Temp/Scatter_Table.zip", "application/zip", "Scatter_Table.zip");
        }

        /// <summary>
        /// Calculate similarity tables between the different measurements (XRF_tables). Then download the calculated tables for the user
        /// </summary>
        /// <returns></returns>
        public ActionResult OnPostSettingsNLNBNN(bool Chi_Filter, float Chi_Value, bool Sub_Support, 
            string NormaliseBy, bool Normalise, float Min_Energy_Include, float Max_Energy_Include, float Min_Energy_Exclude, float Max_Energy_Exclude, bool Energy_Filter_Include,
            bool Energy_Filter_Exclude, bool Avg_Ink, bool Avg_Support)
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            string Similarities_Tables_Folder = "Similarities_Tables";
            string Element_Line_Folder = "Element_Line";
            string Temp_Folder = "Temp";
            string Element_Line_Path = Path.Combine(webRootPath, Element_Line_Folder);
            string Similarities_Tables_Path = Path.Combine(webRootPath, Similarities_Tables_Folder);
            string TempPath = Path.Combine(webRootPath, Temp_Folder);
            if (!Directory.Exists(Similarities_Tables_Path))
            {
                Directory.CreateDirectory(Similarities_Tables_Path);
            }
            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }
            Directory.EnumerateFiles(Similarities_Tables_Path).ToList().ForEach(f => System.IO.File.Delete(f));
            Directory.EnumerateFiles(TempPath).ToList().ForEach(f => System.IO.File.Delete(f));

            XRF XRF_Data = new XRF();
            List<XRF_Table> Tables_Results = new List<XRF_Table>();

            _XRF_Tables_Uploaded.XRF_Tables_Avg_Measurement = XRF_Data.Avg_Measurements(_XRF_Tables_Uploaded.All_XRF_Tables, Avg_Support, Avg_Ink);
            Tables_Results = XRF_Data.DeepCopy(_XRF_Tables_Uploaded.XRF_Tables_Avg_Measurement);

            List<Element_Line> Element_Line_List = new List<Element_Line>();

            Element_Line_List = XRF_Data.Read_Element_Line_List(Element_Line_Path + "/ElementsList.csv");
            Tables_Results = XRF_Data.XRF_Main_Lines(Tables_Results);
            Tables_Results = XRF_Data.Reorder_Elements(Tables_Results, Element_Line_List);

            // Apply the optional part of the processing on the uploaded tables (based on the selected settings)
            string Name_Extension = "";
            if (Avg_Support)
            {
                Name_Extension += "_AvgSup";
            }

            if (Avg_Ink)
            {
                Name_Extension += "_AvgInk";
            }
            
            if (Energy_Filter_Include)
            {
                Tables_Results = XRF_Data.Energy_Filter_Include(Tables_Results, Min_Energy_Include, Max_Energy_Include);
                Name_Extension += "_EFI" + "-" + Min_Energy_Include.ToString() + "-" + Max_Energy_Include.ToString();
            }

            if (Energy_Filter_Exclude)
            {
                Tables_Results = XRF_Data.Energy_Filter_Exclude(Tables_Results, Min_Energy_Exclude, Max_Energy_Exclude);
                Name_Extension += "_EFE" + "-" + Min_Energy_Exclude.ToString() + "-" + Max_Energy_Exclude.ToString();
            }

            if (Chi_Filter)
            {
                Tables_Results = XRF_Data.Chi_Filter(Tables_Results, Chi_Value);
                Name_Extension += "_CF" + "-" + Chi_Value.ToString();
            }

            if (Sub_Support)
            {
                Tables_Results = XRF_Data.Subtracting_Support_From_Ink(Tables_Results);
                Name_Extension += "_Sub";
            }

            //if (Remove_Zero_Net)
            //{
            //    Tables_Results = XRF_Data.Remove_Zero_Net(Tables_Results);
            //    Name_Extension += "_RZ";
            //}

            if (Normalise)
            {
                Tables_Results = XRF_Data.NormaliseBy(Tables_Results, NormaliseBy);
                Name_Extension += "_N" + "-" + NormaliseBy;
            }

            // Find out which element exist in at least one table
            List<string> Existing_Elements = new List<string>();
            foreach (Element_Line Element_Line in Element_Line_List)
            {
                bool found = false;
                foreach (XRF_Table table in Tables_Results)
                {
                    foreach (XRF_Data data in table.Data)
                    {
                        if (Element_Line.Element.ToLower() == data.Element.ToLower())
                        {
                            Existing_Elements.Add(Element_Line.Element);
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        break;
                }
            }
            Existing_Elements = Existing_Elements.Distinct().ToList();

            Tables_Results = XRF_Data.Reorder_Fill_Missing_ExistingElements(Tables_Results, Existing_Elements);

            // Extract feature vectors from all tables according to "Leave-one-out" test scenario
            foreach (XRF_Table table_outer in Tables_Results)
            {
                Mat Current_XRF_Features = new Mat();
                if (table_outer.Data.Count() > 0)
                {
                    Current_XRF_Features = XRF_Data.Extract_XRF_Features(table_outer);
                }
                else
                {
                    continue;
                }

                Mat All_XRF_Features = new Mat();
                List<int> Labels = new List<int>();
                int Label_Counter = 0;
                List<string> Tables_Names = new List<string>();
                List<double> Current_Distances = new List<double>();

                foreach (XRF_Table table_inner in Tables_Results)
                {
                    if (table_inner.Folio_Source_Measurement.ToLower() == table_outer.Folio_Source_Measurement.ToLower())
                        continue;

                    Tables_Names.Add(table_inner.Folio_Source_Measurement);
                    Mat Current_Feature = new Mat();
                    if(table_inner.Data.Count() > 0)
                    {
                        Current_Feature = XRF_Data.Extract_XRF_Features(table_inner);
                    }
                    else
                    {
                        continue;
                    }

                    All_XRF_Features.PushBack(Current_Feature);
                    Current_Distances.Add(Cv2.Norm(Current_XRF_Features, Current_Feature, NormTypes.L2));

                    Labels.Add(Label_Counter);
                    Label_Counter++;
                }

                List<int> Features_Num = new List<int>();
                Features_Num.AddRange(Enumerable.Repeat(1, All_XRF_Features.Rows));

                // Use the NLNBNN Classifier
                /*
                KDTreeIndexParams Index_Param = new KDTreeIndexParams(4);
                OpenCvSharp.Flann.Index Descs_Index = new OpenCvSharp.Flann.Index(All_XRF_Features, Index_Param, FlannDistance.L2);

                NormalisedLNBNN XRF_Classifier = new NormalisedLNBNN();

                List<LocalNBNN_Results> CurrentResult = new List<LocalNBNN_Results>();
                CurrentResult = XRF_Classifier.NNSearch_LNBNN_Priori(Current_XRF_Features, Labels, Descs_Index,
                                                       All_XRF_Features.Rows, Features_Num);

                // Prepare the results and write them to ".csv" files
                CurrentResult = CurrentResult.OrderByDescending(x => x.Votes).ToList();
                */

                var Results_csv = new System.Text.StringBuilder();
                Results_csv.AppendLine("Results for " + table_outer.Folio_Source_Measurement);
                Results_csv.AppendLine("FileName,L2_Distances");
                //Results_csv.AppendLine("Rank,FileName,Similarity(NLNBNN),Distance(L2)");

                List<Tuple<string, double>> Measurements_Distances = new List<Tuple<string, double>>();
                int RankCounter = 0;
                foreach (double Dist in Current_Distances)
                {
                    Measurements_Distances.Add(new Tuple<string, double>(Tables_Names[RankCounter], Math.Round(Dist, 2)));
                    RankCounter++;
                }

                Measurements_Distances = Measurements_Distances.OrderBy(t => t.Item2).ToList();

                foreach (var Dist in Measurements_Distances)
                {
                    Results_csv.AppendLine(Dist.Item1 + "," + Convert.ToString(Dist.Item2));
                }

                Results_csv.AppendLine("");

                System.IO.File.WriteAllText(Similarities_Tables_Path + "/" + table_outer.Folio_Source_Measurement + Name_Extension + ".csv", Results_csv.ToString());

                Results_csv.Clear();
            }
            // create a new archive          
            string archive = TempPath + "\\L2_Distances.zip";
            ZipFile.CreateFromDirectory(Similarities_Tables_Path, archive);
            return File("/Temp/L2_Distances.zip", "application/zip", "L2_Distances.zip");
        }
    }
}
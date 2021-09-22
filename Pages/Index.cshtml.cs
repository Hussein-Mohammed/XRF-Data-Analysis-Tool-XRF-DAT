using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using OpenCvSharp;
using OpenCvSharp.Flann;
using XRF_Data_Analysis.Headers;

namespace XRF_Data_Analysis.Pages
{
    public class IndexModel : PageModel
    {
        public IEnumerable<string> ValidFiles { get; set; }
        private readonly ILogger<IndexModel> _logger;

        // needed to get the wwwroot directory
        private IWebHostEnvironment _hostingEnvironment;
        private readonly XRF_Tables_Uploaded _XRF_Tables_Uploaded;

        public IndexModel(ILogger<IndexModel> logger, IWebHostEnvironment hostingEnvironment, XRF_Tables_Uploaded XRF_Tables_Uploaded)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _XRF_Tables_Uploaded = XRF_Tables_Uploaded;
        }

        public void OnGet()
        {
            /// Empty All existing folders and create non-existing ones 
            string Tables_Folder = "XRF_Tables";
            string Tables_Results_Folder = "XRF_Tables_Results";
            string Element_Line_Folder = "Element_Line";
            string Temp_Folder = "Temp";
            string Similarities_Tables_Folder = "Similarities_Tables";
            string Scatter_Table_Folder = "Scatter_Table";

            string webRootPath = _hostingEnvironment.WebRootPath;
            string Tables_Path = Path.Combine(webRootPath, Tables_Folder);
            string Element_Line_Path = Path.Combine(webRootPath, Element_Line_Folder);
            string Tables_Results_Path = Path.Combine(webRootPath, Tables_Results_Folder);
            string TempPath = Path.Combine(webRootPath, Temp_Folder);
            string Similarities_Tables_Path = Path.Combine(webRootPath, Similarities_Tables_Folder);
            string Scatter_Table_Path = Path.Combine(webRootPath, Scatter_Table_Folder);

            if (!Directory.Exists(Tables_Results_Path))
            {
                Directory.CreateDirectory(Tables_Results_Path);
            }
            if (!Directory.Exists(Tables_Path))
            {
                Directory.CreateDirectory(Tables_Path);
            }
            if (!Directory.Exists(Similarities_Tables_Path))
            {
                Directory.CreateDirectory(Similarities_Tables_Path);
            }
            if (!Directory.Exists(Scatter_Table_Path))
            {
                Directory.CreateDirectory(Scatter_Table_Path);
            }
            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }
            Directory.EnumerateFiles(Tables_Results_Path).ToList().ForEach(f => System.IO.File.Delete(f));
            Directory.EnumerateFiles(Tables_Path).ToList().ForEach(f => System.IO.File.Delete(f));
            Directory.EnumerateFiles(TempPath).ToList().ForEach(f => System.IO.File.Delete(f));
            Directory.EnumerateFiles(Similarities_Tables_Path).ToList().ForEach(f => System.IO.File.Delete(f));
            Directory.EnumerateFiles(Scatter_Table_Path).ToList().ForEach(f => System.IO.File.Delete(f));

            /// Clear all Singleton variables and re-assign default values.
            _XRF_Tables_Uploaded.All_XRF_Tables.Clear();
            _XRF_Tables_Uploaded.XRF_Tables_Avg_Measurement.Clear();
        }

        public IActionResult OnPostUpload(IFormFile[] files)
        {
            string Tables_Folder = "XRF_Tables";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string Tables_Path = Path.Combine(webRootPath, Tables_Folder);
            if (!Directory.Exists(Tables_Path))
            {
                Directory.CreateDirectory(Tables_Path);
            }
            Directory.EnumerateFiles(Tables_Path).ToList().ForEach(f => System.IO.File.Delete(f));

            if (files != null && files.Count() > 0)
            {
                // Iterate through uploaded files array
                foreach (var file in files)
                {
                    string ext = System.IO.Path.GetExtension(file.FileName);
                    if (ext != ".csv")
                        continue;
                    if (file.Length > 0)
                    {
                        // Extract file name from whatever was posted by browser
                        var fileName = System.IO.Path.GetFileName(file.FileName);

                        // If file with same name exists delete it
                        if (System.IO.File.Exists(fileName))
                        {
                            System.IO.File.Delete(fileName);
                        }

                        // Create new local file and copy contents of uploaded file
                        using (var localFile = System.IO.File.OpenWrite(Tables_Path + "/" + fileName))
                        using (var uploadedFile = file.OpenReadStream())
                        {
                            uploadedFile.CopyTo(localFile);
                        }
                    }
                }

                // Check whether there is at least one file with a valid extension
                ValidFiles = Directory.EnumerateFiles(Tables_Path, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".csv"));
                ViewData["ValidFiles"] = ValidFiles.Count();
                if(ValidFiles.Count() <=0)
                {
                    ViewData["Message"] = "Uploaded files are not valid";
                    return Page();
                }

                XRF XRF_Data = new XRF();
                //_XRF_Tables_Uploaded.All_XRF_Tables = XRF_Data.Read_XRF_Tables(Tables_Path);

                List<XRF_Table> ListOfTables = new List<XRF_Table>();
                ListOfTables = XRF_Data.Read_XRF_Tables(Tables_Path);

                for (int tabel_Index = 0; tabel_Index < ListOfTables.Count(); tabel_Index++)
                {
                    if (ListOfTables[tabel_Index].Data.Count() > 0)
                    {
                        _XRF_Tables_Uploaded.All_XRF_Tables.Add(ListOfTables[tabel_Index]);
                    }
                }

                //_XRF_Tables_Uploaded.XRF_Tables_Avg_Measurement = XRF_Data.Avg_Measurements(_XRF_Tables_Uploaded.All_XRF_Tables);

                if (_XRF_Tables_Uploaded.All_XRF_Tables.Count() < 1)
                {
                    ViewData["Message"] = "Uploaded files does not contain any data!";
                    return Page();
                }

                // Find out which element exist in at least one table
                List<Element_Line> Element_Line_List = new List<Element_Line>();
                string Element_Line_Folder = "Element_Line";
                string Element_Line_Path = Path.Combine(webRootPath, Element_Line_Folder);
                if (!Directory.Exists(Element_Line_Path))
                {
                    ViewData["Message"] = "Add the list of all possible element lines to this directory: " + Element_Line_Path;
                    return Page();
                }
                Element_Line_List = XRF_Data.Read_Element_Line_List(Element_Line_Path + "/ElementsList.csv");
                List<string> Existing_Elements = new List<string>();
                foreach (Element_Line Element_Line in Element_Line_List)
                {
                    bool found = false;
                    foreach (XRF_Table table in _XRF_Tables_Uploaded.All_XRF_Tables)
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

                if (Existing_Elements.Count() < 1)
                {
                    ViewData["Message"] = "Uploaded files does not contain valid elements entries!";
                    return Page();
                }

                //Delete all uploaded files afer reading and storing the contents
                Directory.EnumerateFiles(Tables_Path).ToList().ForEach(f => System.IO.File.Delete(f));

                return RedirectToPage("TablesList");
            }
            ViewData["Message"] = "Uploaded files are not valid";
            return Page();
        }
    }
}


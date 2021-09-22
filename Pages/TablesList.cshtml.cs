using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using XRF_Data_Analysis.Headers;

namespace XRF_Data_Analysis.Pages
{
    public class TablesListModel : PageModel
    {
        private readonly XRF_Tables_Uploaded _XRF_Tables_Uploaded;

        public TablesListModel(XRF_Tables_Uploaded XRF_Tables_Uploaded)
        {
            _XRF_Tables_Uploaded = XRF_Tables_Uploaded;
        }

        public void OnGet()
        {

        }

        /// <summary>
        /// Delete specific table from the XRF_Table structures
        /// </summary>
        /// <param name="FS">The Folio_Source of the selected table to be deleted, the folio_source serves as an ID in this context </param>
        /// <returns></returns>
        public ActionResult OnPostDeleteTable(string FS)
        {
            for (int SameFS = 0; SameFS < _XRF_Tables_Uploaded.All_XRF_Tables.Count(); SameFS++)
            {
                if (FS == _XRF_Tables_Uploaded.All_XRF_Tables[SameFS].Folio_Source_Measurement)
                {
                    _XRF_Tables_Uploaded.All_XRF_Tables.Remove(_XRF_Tables_Uploaded.All_XRF_Tables[SameFS]);
                    break;
                }
            }

            return RedirectToPage("TablesList");
        }
    }
}
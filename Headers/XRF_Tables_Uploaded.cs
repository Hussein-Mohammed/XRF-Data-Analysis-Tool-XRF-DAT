using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XRF_Data_Analysis.Headers
{
    public class XRF_Tables_Uploaded
    {
        public List<XRF_Table> All_XRF_Tables = new List<XRF_Table>();
        public List<XRF_Table> XRF_Tables_Avg_Measurement = new List<XRF_Table>();
        public List<Element_Line> Element_Line_List = new List<Element_Line>();

        //public bool Chi_Filter { get; set; } = false;
        //public float Chi_Value { get; set; } = 10;
        //public bool Sub_Support { get; set; } = true;
        //public bool Remove_Zero_Net { get; set; } = true;
        //public string NormaliseBy { get; set; } = "Fe";
        //public bool Normalise { get; set; } = true;
    }
}

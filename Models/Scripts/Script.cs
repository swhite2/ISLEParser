using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISLEParser.Models.WorkspaceItems;
using ISLEParser.Models.RgbMatrices;

namespace ISLEParser.Models.Scripts
{
    public class Script : WorkspaceItem
    {
        //Script should also reference a list of RgbMatrices, as referenced by the "Path" attribute of an rgbmatrix
        public List<RgbMatrix> RgbMatrices { get; set; } = new List<RgbMatrix>();
        public int SpeedFadeInAttribute { get; set; }
        public int SpeedFadeOutAttribute { get; set; }
        public int SpeedDurationAttribute { get; set; }
        public string Direction { get; set; } = "Forward";
        public string RunOrder { get; set; } = "Loop";
        public List<string> Commands { get; set; } = new List<string>();

    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISLEParser.Models.WorkspaceItems;
using ISLEParser.Models.RgbMatrices;
using ISLEParser.Models.Home;

namespace ISLEParser.Models.Scripts
{
    public class Script : WorkspaceItem
    {
        //Script should also reference a list of RgbMatrices, as referenced by the "Path" attribute of an rgbmatrix
        public List<RgbMatrix> RgbMatrices { get; set; } = new List<RgbMatrix>();
        public string SpeedFadeInAttribute { get; set; } = "0";
        public string SpeedFadeOutAttribute { get; set; } = "0";
        public string SpeedDurationAttribute { get; set; } = "300";
        public string Direction { get; set; } = "Forward";
        public string RunOrder { get; set; } = "Loop";
        public List<string> Commands { get; set; } = new List<string>();
        public string CommandsAsString
        {
            get
            {
                return string.Join("\n", Commands);
            }
            set
            {
                Commands = value.Split(new char[] { '\n' }).Select(x => x.Trim()).ToList();
            }
        }

    }


}

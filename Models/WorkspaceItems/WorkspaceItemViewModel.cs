using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISLEParser.Models.Scripts;
using ISLEParser.Models.RgbMatrices;
using ISLEParser.Models.Home;

namespace ISLEParser.Models.WorkspaceItems
{
    public class WorkspaceItemViewModel
    {
        //public WorkspaceItem WorkspaceItem { get; set; }
        public RgbMatrix RgbMatrix { get; set; }
        public Script Script { get; set; }
        public FilesViewModel filesViewModel { get; set; }
        public string WorkspaceName { get; set; }
        public List<string> scriptNames { get; set; } = new List<string>();

        public enum Direction
        {
            Forward,
            Backwards
        }

        public enum RunOrder
        {
            Loop,
            PingPong,
            SingleShot
        }
    }
}

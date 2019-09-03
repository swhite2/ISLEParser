using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISLEParser.Models.Scripts;
using ISLEParser.Models.RgbMatrices;

namespace ISLEParser.Models.WorkspaceItems
{
    public class WorkspaceItemViewModel
    {
        //public WorkspaceItem WorkspaceItem { get; set; }
        public RgbMatrix RgbMatrix { get; set; }
        public Script Script { get; set; }

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

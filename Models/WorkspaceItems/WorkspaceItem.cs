using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISLEParser.Models.Scripts;
using ISLEParser.Models.RgbMatrices;

namespace ISLEParser.Models.WorkspaceItems
{
    public abstract class WorkspaceItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public Script Script { get; set; }
        public RgbMatrix RgbMatrix { get; set; }
    }
}

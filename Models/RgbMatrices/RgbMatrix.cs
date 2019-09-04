using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISLEParser.Models.WorkspaceItems;
using ISLEParser.Models.Scripts;

namespace ISLEParser.Models.RgbMatrices
{
    public class RgbMatrix : WorkspaceItem
    {
        //Path is specific for the function node RGBMatrix
        public Script Path { get; set; }

        //Below are the single children nodes of 'Function'
        public string SpeedFadeInAttribute { get; set; }
        public string SpeedFadeOutAttribute { get; set; }
        public string SpeedDurationAttribute { get; set; }
        
        public string Direction { get; set; }
        public string RunOrder { get; set; }
        public string AlgorithmTypeAttribute { get; } = "Script";
        public string AlgorithmName { get; set; }
        public string FixtureGroup { get; set; }
        public string DimmerControl { get; } = "1";
        public string MonoColor { get; set; }
        

    }

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

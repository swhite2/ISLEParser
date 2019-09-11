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
        public string SpeedFadeInAttribute { get; set; } = "0";
        public string SpeedFadeOutAttribute { get; set; } = "0";
        public string SpeedDurationAttribute { get; set; } = "300";


        public string Direction { get; set; } = "Forward";
        public string RunOrder { get; set; } = "Loop";
        public string AlgorithmTypeAttribute { get; } = "Script";
        public string AlgorithmName { get; set; }
        public string FixtureGroup { get; set; }
        public string DimmerControl { get; } = "1";
        public string MonoColor { get; set; } = "0";
        

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

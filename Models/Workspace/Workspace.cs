using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace ISLEParser.Models.Workspace
{
    public class Workspace
    {
        //public int Id { get; set; }
        public string Name { get; set; }
        public XmlDocument Content { get; set; }

    }
}

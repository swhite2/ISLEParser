using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISLEParser.Models.Home;
using System.Xml;
using System.IO;
using Microsoft.Extensions.FileProviders;
using ISLEParser.Models.WorkspaceItems;
using ISLEParser.Models.RgbMatrices;
using ISLEParser.Models.Scripts;
using System.Xml.Linq;
using ISLEParser.util;

namespace ISLEParser.Models.Workspace
{
    public class WorkspaceRepository : IWorkspaceRepository
    {
        private Dictionary<string, Workspace> WorkspaceDictionary;
        private IFileProvider fileProvider;
        public WorkspaceRepository(IFileProvider fileProvider)
        {
            WorkspaceDictionary = new Dictionary<string, Workspace>();
            this.fileProvider = fileProvider;
            foreach(var item in this.fileProvider.GetDirectoryContents(""))
            {

                
                    //XmlDocument xmlDoc = new XmlDocument();
                    //xmlDoc.Load(item.PhysicalPath);
                    WorkspaceDictionary.Add(item.Name, new Workspace { Name = item.Name, Content = null });


            }

        }

        public Script GetWorkspaceScript(string Id, string WorkspaceName)
        {
            Workspace ws = WorkspaceDictionary[WorkspaceName];
            List<RgbMatrix> rgbMatrices = new List<RgbMatrix>();
            XmlNodeList list = ws.Content.GetElementsByTagName("Function");
            for (int i = 0; i < list.Count; i++)
            {
                var attributeId = GetAttributeValue(list[i], "ID");
                if (attributeId.Equals(Id))
                {
                    var attributeType = GetAttributeValue(list[i], "Type");
                    if (attributeType == "Script")
                    {
                        Script script = new Script();
                        var attributeNameScript = GetAttributeValue(list[i], "Name");
                        for (int x = 0; x < list.Count; x++)
                        {
                            var attributeTypeRgbMatrixLoop = GetAttributeValue(list[x], "Type");
                            var attributePathRgbMatrixLoop = GetAttributeValue(list[x], "Path");
                            if (attributeTypeRgbMatrixLoop == "RGBMatrix" && attributeNameScript.Equals(attributePathRgbMatrixLoop))
                            {
                                var attributeNameRgbMatrix = GetAttributeValue(list[x], "Name");
                                var attributeIdRgbMatrix = GetAttributeValue(list[x], "ID");
                                var attributeTypeRgbMatrix = GetAttributeValue(list[x], "Type");
                                //Parse details of the rgbmatrix (maybe use the below method of GetWorkspaceRgbMatrix?
                                RgbMatrix rgbMatrix = new RgbMatrix
                                {
                                    Name = attributeNameRgbMatrix,
                                    Id = attributeIdRgbMatrix,
                                    Type = attributeTypeRgbMatrix,
                                    Path = script
                                };
                                XmlNodeList rgbMatrixNodes = list[x].ChildNodes;
                                for (int y = 0; y < rgbMatrixNodes.Count; y++)
                                {
                                    switch (rgbMatrixNodes[y].Name)
                                    {
                                        case "Speed":
                                            rgbMatrix.SpeedFadeInAttribute = Int32.Parse(GetAttributeValue(rgbMatrixNodes[y], "FadeIn"));
                                            rgbMatrix.SpeedFadeOutAttribute = Int32.Parse(GetAttributeValue(rgbMatrixNodes[y], "FadeOut"));
                                            rgbMatrix.SpeedDurationAttribute = Int32.Parse(GetAttributeValue(rgbMatrixNodes[y], "Duration"));
                                            break;
                                        case "Direction":
                                            rgbMatrix.Direction = rgbMatrixNodes[y].InnerText;
                                            break;
                                        case "RunOrder":
                                            rgbMatrix.RunOrder = rgbMatrixNodes[y].InnerText;
                                            break;
                                        case "Algorithm":
                                            rgbMatrix.AlgorithmName = rgbMatrixNodes[y].InnerText;
                                            break;
                                        case "DimmerControl":
                                            break;
                                        case "MonoColor":
                                            rgbMatrix.MonoColor = rgbMatrixNodes[y].InnerText;
                                            break;
                                        case "FixtureGroup":
                                            rgbMatrix.FixtureGroup = rgbMatrixNodes[y].InnerText;
                                            break;
                                        default:
                                            break;
                                    }

                                }
                                rgbMatrices.Add(rgbMatrix);
                            }

                        }
                        script.Id = attributeId;
                        script.Name = attributeNameScript;
                        script.Type = attributeType;
                        script.RgbMatrices = rgbMatrices;

                        XmlNodeList childNodes = list[i].ChildNodes;
                        for (int y = 0; y < childNodes.Count; y++)
                        {
                            switch (childNodes[y].Name)
                            {
                                case "Speed":
                                    script.SpeedFadeInAttribute = Int32.Parse(GetAttributeValue(childNodes[y], "FadeIn"));
                                    script.SpeedFadeOutAttribute = Int32.Parse(GetAttributeValue(childNodes[y], "FadeOut"));
                                    script.SpeedDurationAttribute = Int32.Parse(GetAttributeValue(childNodes[y], "Duration"));
                                    break;
                                case "Direction":
                                    script.Direction = childNodes[y].InnerText;
                                    break;
                                case "RunOrder":
                                    script.RunOrder = childNodes[y].InnerText;
                                    break;
                                case "Command":
                                    script.Commands.Add(childNodes[y].InnerText);
                                    break;
                                default:
                                    break;
                            }
                        }

                        return script;
                    }
                }
            }
            throw new NotImplementedException();
        }

        public void DeleteWorkspaceScript(string Id, string Name)
        {
            XDocument doc = XDocument.Load(new XmlNodeReader(WorkspaceDictionary[Name].Content));
            XNamespace ns = "http://www.qlcplus.org/Workspace";

            //EXAMPLE:

            //var values = doc.Root
            //    .Element(ns + "Engine")
            //    .Elements(ns + "Function")
            //    .Where(item => (string)item.Attribute("Type") == "Script")
            //    .Select(item => (string)item.Attribute("Name"))
            //    .ToList();

            //.Select(item => (string)item.Attribute("Name"))
            //.FirstOrDefault();

            //foreach (var value in values)
            //{
            //    Console.WriteLine(value);
            //}

            doc.Root
                .Element(ns + "Engine")
                .Elements(ns + "Function")
                //.Where(item => (string)item.Attribute("Type") == "Script")
                .Where(item => (string)item.Attribute("ID") == Id && (string)item.Attribute("Type") == "Script")
                .Remove();

            WorkspaceDictionary[Name].Content = doc.ToXmlDocument();
            UpdateWorkspace(Name);
            
        }

        public void DeleteWorkspaceRgbMatrix(string Id, string WorkspaceName)
        {

        }

        public RgbMatrix GetWorkspaceRgbMatrix(string Id, string WorkspaceName)
        {
            Workspace ws = WorkspaceDictionary[WorkspaceName];
            XmlNodeList list = ws.Content.GetElementsByTagName("Function");
            for(int i = 0; i < list.Count; i++)
            {
                var attributeId = GetAttributeValue(list[i], "ID");
                if (attributeId.Equals(Id))
                {
                    var attributeType = GetAttributeValue(list[i], "Type");
                    if (attributeType.Equals("RGBMatrix"))
                    {

                        Script script = new Script();
                        var attributeNameRgbMatrix = GetAttributeValue(list[i], "Name");
                        var attributePathRgbMatrix = GetAttributeValue(list[i], "Path");
                    for (int x = 0; x < list.Count; x++)
                    {
                        var attributeTypeScript = GetAttributeValue(list[x], "Type");
                        var attributeNameScript = GetAttributeValue(list[x], "Name");
                        if (attributeTypeScript == "Script" && attributePathRgbMatrix.Equals(attributeNameScript))
                        {
                            var attributeIdScript = GetAttributeValue(list[x], "ID");

                            script = GetWorkspaceScript(attributeIdScript, WorkspaceName);
                        }
                    }
                    RgbMatrix rgbMatrix = new RgbMatrix
                    {
                        Name = attributeNameRgbMatrix,
                        Id = attributeId,
                        Type = attributeType,
                        Path = script
                    };
                    XmlNodeList childNodes = list[i].ChildNodes;
                    for (int y = 0; y < childNodes.Count; y++)
                    {
                        switch (childNodes[y].Name)
                        {
                            case "Speed":
                                rgbMatrix.SpeedFadeInAttribute = Int32.Parse(GetAttributeValue(childNodes[y], "FadeIn"));
                                rgbMatrix.SpeedFadeOutAttribute = Int32.Parse(GetAttributeValue(childNodes[y], "FadeOut"));
                                rgbMatrix.SpeedDurationAttribute = Int32.Parse(GetAttributeValue(childNodes[y], "Duration"));
                                break;
                            case "Direction":
                                rgbMatrix.Direction = childNodes[y].InnerText;
                                break;
                            case "RunOrder":
                                rgbMatrix.RunOrder = childNodes[y].InnerText;
                                break;
                            case "Algorithm":
                                rgbMatrix.AlgorithmName = childNodes[y].InnerText;
                                break;
                            case "DimmerControl":
                                break;
                            case "MonoColor":
                                rgbMatrix.MonoColor = childNodes[y].InnerText;
                                break;
                            case "FixtureGroup":
                                rgbMatrix.FixtureGroup = childNodes[y].InnerText;
                                break;
                            default:
                                break;
                        }

                    }
                    return rgbMatrix;
                }
                }
            }
            throw new NotImplementedException();

        }     

        public Workspace AddWorkspace(string name)
        {
            if (WorkspaceDictionary.ContainsKey(name))
            {
                throw new NotImplementedException();
            }
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Workspaces", name);            
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            if (xmlDoc.DocumentType.Name == null || !xmlDoc.DocumentType.Name.Equals("Workspace"))
            {
                Console.WriteLine("Wrong Doctype! Doctype = " + xmlDoc.DocumentType.Name);
                //TODO: Handle error/wrong input
            }
            Workspace ws = new Workspace {
                Name = name,
                Content = xmlDoc
            };
            
            WorkspaceDictionary.Add(name, ws);
            return ws;

        }

        public void DeleteWorkspace(string Name)
        {
            //throw new NotImplementedException();
            //Delete workspace from dictionary AND file from server
            if (WorkspaceDictionary.ContainsKey(Name))
            {
                WorkspaceDictionary.Remove(Name);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Workspaces", Name);
                File.Delete(path);
            }
            else
            {
                throw new Exception();
            }
        }

        public IEnumerable<Workspace> GetAllWorkspaces()
        {
            List<Workspace> list = new List<Workspace>();
            foreach(KeyValuePair<string, Workspace> item in WorkspaceDictionary)
            {
                list.Add(item.Value);
            }
            //throw new NotImplementedException();
            return list;
        }



        public WorkspaceItemListViewModel GetWorkspaceScripts(string Name)
        {
            Workspace ws = WorkspaceDictionary[Name];
            XmlNodeList list = ws.Content.GetElementsByTagName("Function");
            WorkspaceItemListViewModel model = new WorkspaceItemListViewModel();
            for (int i = 0; i < list.Count; i++)
            {
                var attributeValue = GetAttributeValue(list[i], "Type");
                if (attributeValue == "Script")
                {
                    var attributeName = GetAttributeValue(list[i], "Name");
                    var attributeId = GetAttributeValue(list[i], "ID");
                    try
                    {
                        var result = attributeId;
                        model.WorkspaceItems.Add(new Script { Name = attributeName, Id = result, Type = attributeValue});
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine($"Unable to parse '{attributeId}'");
                    }
                }
            }
            return model;

        }

        public WorkspaceItemListViewModel GetWorkspaceRgbMatrices(string Name)
        {
            //XmlDocument doc = new XmlDocument();
            //WorkspaceDictionary[Name].Content = doc;
            Workspace ws = WorkspaceDictionary[Name];
            XmlNodeList list = ws.Content.GetElementsByTagName("Function");
            WorkspaceItemListViewModel model = new WorkspaceItemListViewModel();
            for(int i = 0; i < list.Count; i++)
            {
                var attributeValue = GetAttributeValue(list[i], "Type");
                if(attributeValue == "RGBMatrix")
                {
                    var attributeName = GetAttributeValue(list[i], "Name");
                    var attributePath = GetAttributeValue(list[i], "Path");
                    var attributeId = GetAttributeValue(list[i], "ID");
                    var result = attributeId;
                    model.WorkspaceItems.Add(new RgbMatrix { Id = result, Name = attributeName, Path = new Script { Name = attributePath }, Type = attributeValue });
                }
            }
            return model;        
        }

        public WorkspaceItemListViewModel GetWorkspaceAllItems(string Name)
        {
            //XmlDocument doc = new XmlDocument();
            //WorkspaceDictionary[Name].Content = doc;
            Workspace ws = WorkspaceDictionary[Name];

            XmlNodeList list = ws.Content.GetElementsByTagName("Function");

            var model = new WorkspaceItemListViewModel();
            for (int i = 0; i < list.Count; i++)
            {
                var attributeValue = GetAttributeValue(list[i], "Type");
                if (attributeValue == "RGBMatrix")
                {
                    var attributeName = GetAttributeValue(list[i], "Name");
                    var attributePath = GetAttributeValue(list[i], "Path");
                    var attributeId = GetAttributeValue(list[i], "ID");

                    var result = attributeId;
                    model.WorkspaceItems.Add(new RgbMatrix { Id = result, Name = attributeName, Path = new Script { Name = attributePath }, Type = attributeValue });
                    


                }
                else if (attributeValue == "Script")
                {
                    var attributeName = GetAttributeValue(list[i], "Name");
                    var attributeId = GetAttributeValue(list[i], "ID");
                    model.WorkspaceItems.Add(new Script { Id = attributeId, Name = attributeName, Type = attributeValue });
                }
            }

            return model;

        }


        public async void UpdateWorkspace(string Name)
        {
            //This method Flushes the content of the current XmlFile to the same file, replacing all data.
            //IMPORTANT
            //In order to properly generate a unique ID, both the RGBMatrices' and the Scripts' IDs need to be read, and start from the highest of those two (assuming there are no other functions)
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Workspaces", Name);
            using(FileStream DestinationStream = File.Create(path))
            {
                await WriteXmlAsync(DestinationStream, WorkspaceDictionary[Name].Content);
                //await DestinationStream.FlushAsync();
            }
        }

        private async Task WriteXmlAsync(Stream stream, XmlDocument doc)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Async = true;
            settings.Indent = true;
            settings.CloseOutput = true;

            using(XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                await writer.WriteRawAsync(doc.InnerXml);
                await writer.FlushAsync();
                writer.Dispose();
                writer.Close();
            }
        }



        private string GetAttributeValue(XmlNode xNode, string attributeToFind)
        {
            string returnValue = string.Empty;
            XmlElement ele = xNode as XmlElement;

            if (ele.HasAttribute(attributeToFind))
                returnValue = ele.GetAttribute(attributeToFind);

            return returnValue;
        }
    }
}

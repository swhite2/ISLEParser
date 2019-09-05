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
using System.Threading;

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


                WorkspaceDictionary.Add(item.Name, new Workspace { Name = item.Name});


            }

        }

        public Script GetWorkspaceScript(string Id, string WorkspaceName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Workspaces", WorkspaceName);
            XDocument doc = XDocument.Load(path);
            XNamespace ns = "http://www.qlcplus.org/Workspace";

            var value = doc.Root
                .Element(ns + "Engine")
                .Elements(ns + "Function")
                .Where(item => (string)item.Attribute("Type") == "Script" && (string)item.Attribute("ID") == Id)
                .FirstOrDefault();
            var commandValues = doc.Root
                .Element(ns + "Engine")
                .Elements(ns + "Function")
                .Where(item => (string)item.Attribute("Type") == "Script" && (string)item.Attribute("ID") == Id)
                .Elements(ns + "Command")
                .ToList();
            List<string> commands = new List<string>();
            List<RgbMatrix> rgbMatrices = new List<RgbMatrix>();
            var scriptName = value.Attribute("Name").Value;
            var rgbMatricesValues = doc.Root
                .Element(ns + "Engine")
                .Elements(ns + "Function")
                .Where(item => (string)item.Attribute("Type") == "RGBMatrix" && (string)item.Attribute("Path") == scriptName)
                .ToList();
            foreach (var item in commandValues)
            {
                commands.Add(item.Value);
            }

            foreach (var item in rgbMatricesValues)
            {
                rgbMatrices.Add(new RgbMatrix
                {
                    Id = item.Attribute("ID").Value,
                    Type = item.Attribute("Type").Value,
                    Name = item.Attribute("Name").Value,
                    SpeedFadeInAttribute = item.Element(ns + "Speed").Attribute("FadeIn").Value,
                    SpeedFadeOutAttribute = item.Element(ns + "Speed").Attribute("FadeOut").Value,
                    SpeedDurationAttribute = item.Element(ns + "Speed").Attribute("Duration").Value,
                    Direction = item.Element(ns + "Direction").Value,
                    RunOrder = item.Element(ns + "RunOrder").Value,
                    AlgorithmName = item.Element(ns + "Algorithm").Value,
                    MonoColor = item.Element(ns + "MonoColor").Value,
                    FixtureGroup = item.Element(ns + "FixtureGroup").Value
                });
            }

            Script script = new Script
            {
                Name = scriptName,
                Id = value.Attribute("ID").Value,
                Type = value.Attribute("Type").Value,
                SpeedFadeInAttribute = value.Element(ns + "Speed").Attribute("FadeIn").Value,
                SpeedFadeOutAttribute = value.Element(ns + "Speed").Attribute("FadeOut").Value,
                SpeedDurationAttribute = value.Element(ns + "Speed").Attribute("Duration").Value,
                Direction = value.Element(ns + "Direction").Value,
                RunOrder = value.Element(ns + "RunOrder").Value,
                Commands = commands,
                RgbMatrices = rgbMatrices
            };

            foreach(var item in rgbMatrices)
            {
                item.Path = script;
            }
            return script;
        }

        public void DeleteWorkspaceScript(string Id, string Name)
        {
            XNamespace ns = "http://www.qlcplus.org/Workspace";
            WorkspaceDictionary[Name].Content.Root
                .Element(ns + "Engine")
                .Elements(ns + "Function")
                //.Where(item => (string)item.Attribute("Type") == "Script")
                .Where(item => (string)item.Attribute("ID") == Id && (string)item.Attribute("Type") == "Script")
                .FirstOrDefault()
                .Remove();
            
            UpdateWorkspace(Name);          
        }

        public void DeleteWorkspaceRgbMatrix(string Id, string Name)
        {
            XNamespace ns = "http://www.qlcplus.org/Workspace";
            WorkspaceDictionary[Name].Content.Root
                .Element(ns + "Engine")
                .Elements(ns + "Function")
                //.Where(item => (string)item.Attribute("Type") == "Script")
                .Where(item => (string)item.Attribute("ID") == Id && (string)item.Attribute("Type") == "RGBMatrix")
                .FirstOrDefault()
                .Remove();

            UpdateWorkspace(Name);
        }

        public RgbMatrix GetWorkspaceRgbMatrix(string Id, string WorkspaceName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Workspaces", WorkspaceName);
            XDocument doc = XDocument.Load(path);
            XNamespace ns = "http://www.qlcplus.org/Workspace";

            var value = doc.Root
                .Element(ns + "Engine")
                .Elements(ns + "Function")
                .Where(item => (string)item.Attribute("Type") == "RGBMatrix" && (string)item.Attribute("ID") == Id)
                .FirstOrDefault();
            List<RgbMatrix> rgbMatrices = new List<RgbMatrix>();
            var rgbMatrixPath = value.Attribute("Path").Value;

            var scriptId = doc.Root
                .Element(ns + "Engine")
                .Elements(ns + "Function")
                .Where(item => (string)item.Attribute("Type") == "Script" && (string)item.Attribute("Name") == rgbMatrixPath)
                .Select(x => x.Attribute("ID"))
                .FirstOrDefault();

            RgbMatrix rgbMatrix = new RgbMatrix
            {
                Name = value.Attribute("Name").Value,
                Id = value.Attribute("ID").Value,
                Type = value.Attribute("Type").Value,
                SpeedFadeInAttribute = value.Element(ns + "Speed").Attribute("FadeIn").Value,
                SpeedFadeOutAttribute = value.Element(ns + "Speed").Attribute("FadeOut").Value,
                SpeedDurationAttribute = value.Element(ns + "Speed").Attribute("Duration").Value,
                Direction = value.Element(ns + "Direction").Value,
                RunOrder = value.Element(ns + "RunOrder").Value,
                Path = GetWorkspaceScript(scriptId.Value, WorkspaceName),
                AlgorithmName = value.Element(ns + "Algorithm").Value,
                MonoColor = value.Element(ns + "MonoColor").Value,
                FixtureGroup = value.Element(ns + "FixtureGroup").Value
            };


            return rgbMatrix;

        }     

        public Workspace AddWorkspace(string name)
        {
            if (WorkspaceDictionary.ContainsKey(name))
            {
                throw new NotImplementedException();
            }
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Workspaces", name);
            XDocument doc = XDocument.Load(path);
            
            if (doc.DocumentType.Name == null || !doc.DocumentType.Name.Equals("Workspace"))
            {
                Console.WriteLine("Wrong Doctype! Doctype = " + doc.DocumentType.Name);
                //TODO: Handle error/wrong input
            }
            Workspace ws = new Workspace {
                Name = name,
                Content = doc
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



        public async Task<WorkspaceItemListViewModel> GetWorkspaceScripts(string Name, CancellationToken cancellationToken, LoadOptions loadOptions = LoadOptions.PreserveWhitespace)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Workspaces", Name);
            using(StreamReader st = new StreamReader(path))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Async = true;
                settings.DtdProcessing = DtdProcessing.Parse;
                using (XmlReader reader = XmlReader.Create(st, settings))
                {
                    WorkspaceDictionary[Name].Content = await XDocument.LoadAsync(reader, loadOptions, cancellationToken);
                    XNamespace ns = "http://www.qlcplus.org/Workspace";
                    var values = WorkspaceDictionary[Name].Content.Root
                        .Element(ns + "Engine")
                        .Elements(ns + "Function")
                        .Where(item => (string)item.Attribute("Type") == "Script")
                        .ToList();
                    WorkspaceItemListViewModel model = new WorkspaceItemListViewModel();
                    foreach (var item in values)
                    {
                        model.WorkspaceItems.Add(new Script
                        {
                            Name = item.Attribute("Name").Value,
                            Id = item.Attribute("ID").Value,
                            Type = item.Attribute("Type").Value
                        });
                    }
                    reader.Dispose();
                    
                    return model;
                }
            }

        }

        public async Task<WorkspaceItemListViewModel> GetWorkspaceRgbMatrices(string Name, CancellationToken cancellationToken, LoadOptions loadOptions = LoadOptions.PreserveWhitespace)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Workspaces", Name);
            using (StreamReader st = new StreamReader(path))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Async = true;
                settings.DtdProcessing = DtdProcessing.Parse;
                using(XmlReader reader = XmlReader.Create(st, settings))
                {
                    WorkspaceDictionary[Name].Content = await XDocument.LoadAsync(reader, loadOptions, cancellationToken);
                    XNamespace ns = "http://www.qlcplus.org/Workspace";
                    var values = WorkspaceDictionary[Name].Content.Root
                        .Element(ns + "Engine")
                        .Elements(ns + "Function")
                        .Where(item => (string)item.Attribute("Type") == "RGBMatrix")
                        .ToList();
                    WorkspaceItemListViewModel model = new WorkspaceItemListViewModel();
                    foreach (var item in values)
                    {
                        model.WorkspaceItems.Add(new RgbMatrix
                        {
                            Name = item.Attribute("Name").Value,
                            Id = item.Attribute("ID").Value,
                            Type = item.Attribute("Type").Value,
                            Path = new Script { Name = item.Attribute("Path").Value }
                        });
                    }

                    return model;
                }
            }

        }

        public async Task<WorkspaceItemListViewModel> GetWorkspaceAllItems(string Name, CancellationToken cancellationToken, LoadOptions loadOptions = LoadOptions.PreserveWhitespace)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Workspaces", Name);
            using (StreamReader st = new StreamReader(path))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Async = true;
                settings.DtdProcessing = DtdProcessing.Parse;
                using (XmlReader reader = XmlReader.Create(st, settings))
                {
                    WorkspaceDictionary[Name].Content = await XDocument.LoadAsync(reader, loadOptions, cancellationToken);
                    XNamespace ns = "http://www.qlcplus.org/Workspace";
                    var values = WorkspaceDictionary[Name].Content.Root
                        .Element(ns + "Engine")
                        .Elements(ns + "Function")
                        .Where(item => (string)item.Attribute("Type") == "RGBMatrix" || (string)item.Attribute("Type") == "Script")
                        .ToList();
                    WorkspaceItemListViewModel model = new WorkspaceItemListViewModel();
                    foreach (var item in values)
                    {
                        var attributeValue = item.Attribute("Type");
                        if (attributeValue.Value == ("RGBMatrix"))
                        {
                            model.WorkspaceItems.Add(new RgbMatrix
                            {
                                Name = item.Attribute("Name").Value,
                                Id = item.Attribute("ID").Value,
                                Type = item.Attribute("Type").Value
                            });
                        }
                        else if (attributeValue.Value == ("Script"))
                        {
                            model.WorkspaceItems.Add(new Script {
                                Name = item.Attribute("Name").Value,
                                Id = item.Attribute("ID").Value,
                                Type = item.Attribute("Type").Value
                            });
                        }
                    }                  
                    return model;
                }
            }


        }



        public void UpdateWorkspace(string WorkspaceName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Workspaces", WorkspaceName);
            //save to file(overwrite)
            WorkspaceDictionary[WorkspaceName].Content.Save(path, SaveOptions.None);
            //Clean up memory
            GC.Collect();
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

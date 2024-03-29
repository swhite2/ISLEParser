﻿using System;
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
using System.Text.RegularExpressions;
using System.Text;

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

        private XElement CreateRgbMatrixNode(RgbMatrix matrix)
        {
            XNamespace ns = "http://www.qlcplus.org/Workspace";
            XElement root = new XElement(ns + "Function");
            root.Add(
                new XAttribute("ID", matrix.Id),
                new XAttribute("Type", matrix.Type),
                new XAttribute("Name", matrix.Name),
                new XAttribute("Path", matrix.Path.Name),
                new XElement(ns + "Speed", 
                    new XAttribute("FadeIn", matrix.SpeedFadeInAttribute),
                    new XAttribute("FadeOut", matrix.SpeedFadeOutAttribute),
                    new XAttribute("Duration", matrix.SpeedDurationAttribute)),
                new XElement(ns + "Direction", matrix.Direction),
                new XElement(ns + "RunOrder", matrix.RunOrder),
                new XElement(ns + "Algorithm",
                    new XAttribute("Type", matrix.AlgorithmTypeAttribute), matrix.AlgorithmName),
                new XElement(ns + "DimmerControl", matrix.DimmerControl),
                new XElement(ns + "MonoColor", ""),
                new XElement(ns + "FixtureGroup", matrix.FixtureGroup)
                );
            return root;
        }

        private XElement CreateScriptNode(Script script)
        {
            XNamespace ns = "http://www.qlcplus.org/Workspace";
            XElement root = new XElement(ns + "Function");
            root.Add(
                new XAttribute("ID", script.Id),
                new XAttribute("Type", script.Type),
                new XAttribute("Name", script.Name),
                new XElement(ns + "Speed",
                    new XAttribute("FadeIn", script.SpeedFadeInAttribute),
                    new XAttribute("FadeOut", script.SpeedFadeOutAttribute),
                    new XAttribute("Duration", script.SpeedDurationAttribute)),
                new XElement(ns + "Direction", script.Direction),
                new XElement(ns + "RunOrder", script.RunOrder)               
                );
            foreach(var item in script.Commands)
            {
                root.Add(new XElement(ns + "Command", item));
            }
            return root;
        }

        public Script GenerateNewScript(List<string> fileNames, string WorkspaceName)
        {
            string scriptName = (Regex.Replace(fileNames[0], "\\wuniverse[1-8]", String.Empty));
            List<string> commands = new List<string>();
            List<RgbMatrix> rgbMatrices = new List<RgbMatrix>();
            for (int i = 1; i < 9; i++ ){
                rgbMatrices.Add(GenerateNewRgbMatrix(WorkspaceName, scriptName, (i - 1).ToString(), i));
                //name of the rgb matrix should have its whitespace removed, then replaced with %20
                commands.Add("startfunction%3A" + rgbMatrices[i - 1].Id + "%20%2F%2F%20" + rgbMatrices[i - 1].Name.Replace(" ", "%20").Replace("-", "%2D").Replace("_", "%5F"));
            }
            //%3A = :
            //%20 SPACE
            //%2F = /
            //%28 = (
            //%29 = ) 
            commands.Add("wait%3A300s");
            Script script = new Script
            {
                Name = scriptName,
                Type = "Script",
                RgbMatrices = rgbMatrices,
                Id = GetNewId(WorkspaceName, 9),
                Commands = commands
            };

            return script;
        }
        
        public RgbMatrix GenerateNewRgbMatrix(string WorkspaceName, string ScriptName, string fixtureGroup, int i)
        {
            string newId = GetNewId(WorkspaceName, i);
            RgbMatrix rm = new RgbMatrix
            {
                Id = newId,
                Name = ScriptName + "_" + newId,
                Type = "RGBMatrix",
                AlgorithmName = ScriptName + "_U" + i.ToString(),
                FixtureGroup = fixtureGroup,
                Path = new Script { Name = ScriptName}
            };
            return rm;
        }

        private string GetNewId(string WorkspaceName, int offset)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Workspaces", WorkspaceName);
            using (StreamReader st = new StreamReader(path))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Async = true;
                settings.DtdProcessing = DtdProcessing.Parse;
                using (XmlReader reader = XmlReader.Create(st, settings))
                {
                    if (WorkspaceDictionary[WorkspaceName].Content == null)
                        WorkspaceDictionary[WorkspaceName].Content = XDocument.Load(reader);
                    XNamespace ns = "http://www.qlcplus.org/Workspace";
                    var values = WorkspaceDictionary[WorkspaceName].Content.Root
                        .Element(ns + "Engine")
                        .Elements(ns + "Function")
                        .Select(x => x.Attribute("ID"))
                        .ToList();
                    List<int> idList = new List<int>();
                    foreach (var item in values)
                    {
                        idList.Add(Int32.Parse(item.Value));
                    }
                    int last = idList.Last();
                    last += offset;
                    st.Dispose();
                    st.Close();
                    reader.Dispose();
                    reader.Close();
                    return last.ToString();
                }
            }
        }

        public void AddScript(string WorkspaceName, Script script)
        {
            XNamespace ns = "http://www.qlcplus.org/Workspace";
            foreach (var item in script.RgbMatrices)
            {
                WorkspaceDictionary[WorkspaceName].Content.Root
                    .Element(ns + "Engine")
                    .Add(CreateRgbMatrixNode(item));
            }
            WorkspaceDictionary[WorkspaceName].Content.Root
                .Element(ns + "Engine")
                .Add(CreateScriptNode(script));
            UpdateWorkspace(WorkspaceName);
        }

        public Script GetWorkspaceScript(string Id, string WorkspaceName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Workspaces", WorkspaceName);
            XDocument doc = WorkspaceDictionary[WorkspaceName].Content;
            if(doc == null)
                doc = XDocument.Load(path);
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
                    MonoColor = (string)item.Element(ns + "MonoColor").Value ?? "",
                    FixtureGroup = item.Element(ns + "FixtureGroup").Value,
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
            var scriptName = WorkspaceDictionary[Name].Content.Root
                .Element(ns + "Engine")
                .Elements(ns + "Function")
                .Where(x => (string)x.Attribute("ID") == Id && (string)x.Attribute("Type") == "Script")
                .FirstOrDefault()
                .Attribute("Name");

            var rgbMatrices = WorkspaceDictionary[Name].Content.Root
                .Element(ns + "Engine")
                .Elements(ns + "Function")
                .Where(item => (string)item.Attribute("Type") == "RGBMatrix" && (string)item.Attribute("Path") == scriptName.Value)
                .ToList();
            List<string> fileNames = new List<string>();
            foreach(var item in rgbMatrices)
            {
                fileNames.Add(item.Element(ns + "Algorithm").Value);
                item.Remove();
            }

            WorkspaceDictionary[Name].Content.Root
                .Element(ns + "Engine")
                .Elements(ns + "Function")
                .Where(item => (string)item.Attribute("ID") == Id && (string)item.Attribute("Type") == "Script")
                .FirstOrDefault()
                .Remove();

            // string scriptName = (Regex.Replace(fileNames[0], "\\wuniverse[1-8]", String.Empty));
            string rootFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ScriptJs");
            //string fileExample = rgbMatrices[0].Element("Algorithm").Value;
            List<string> newFileNames = new List<string>();
            foreach(var item in fileNames)
            {
                newFileNames.Add(item.Replace(item, Regex.Replace(item, @".(?=.$)", "universe")) + ".js");
            }
            //string filesToDelete = (Regex.Replace(fileExample, "/.(?=.$)/gim", "universe"));
            foreach (var fileName in newFileNames)               
                File.Delete(rootFolderPath + $"/{fileName}");
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
            XDocument doc = WorkspaceDictionary[WorkspaceName].Content;
            if (doc == null)
                doc = XDocument.Load(path);
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
                //Path = GetWorkspaceScript(scriptId?.Attribute("ID")?.Value, WorkspaceName) ?? new Script { },
                AlgorithmName = value.Element(ns + "Algorithm").Value,
                MonoColor = value.Element(ns + "MonoColor").Value,
                FixtureGroup = value.Element(ns + "FixtureGroup").Value
            };
            if (scriptId == null)
            {
                return rgbMatrix;
            }
            else
            {
                rgbMatrix.Path = GetWorkspaceScript(scriptId.Value, WorkspaceName);
                return rgbMatrix;
            }

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
                    if (WorkspaceDictionary[Name].Content == null)                    
                        WorkspaceDictionary[Name].Content = await XDocument.LoadAsync(reader, loadOptions, cancellationToken);
                    XNamespace ns = "http://www.qlcplus.org/Workspace";
                    WorkspaceItemListViewModel model = new WorkspaceItemListViewModel();
                    var values = WorkspaceDictionary[Name].Content.Root
                        .Element(ns + "Engine")
                        .Elements(ns + "Function")
                        .Where(item => (string)item.Attribute("Type") == "Script")
                        .ToList();
                    foreach (var item in values)
                    {
                        //model.WorkspaceItems.Add(new Script
                        //{
                        //    Name = item.Attribute("Name").Value,
                        //    Id = item.Attribute("ID").Value,
                        //    Type = item.Attribute("Type").Value
                        //});
                        model.WorkspaceItems.Add(GetWorkspaceScript(item.Attribute("ID").Value, Name));
                    }
                    st.Dispose();
                    st.Close();
                    reader.Dispose();
                    reader.Close();
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
                    if (WorkspaceDictionary[Name].Content == null)
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
                    st.Dispose();
                    st.Close();
                    reader.Dispose();
                    reader.Close();
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
                    if (WorkspaceDictionary[Name].Content == null)
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
                    st.Dispose();
                    st.Close();
                    reader.Dispose();
                    reader.Close();
                    return model;
                }
            }


        }



        public void UpdateWorkspace(string WorkspaceName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Workspaces", WorkspaceName);
            //save to file(overwrite)
            //Apply indentation
            XmlWriterSettings xws = new XmlWriterSettings
            {
                Indent = true,
                NewLineHandling = NewLineHandling.Entitize

            };
            using (XmlWriter writer = XmlWriter.Create(path, xws))
            {
                WorkspaceDictionary[WorkspaceName].Content.Save(writer);
            }

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

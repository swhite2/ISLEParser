using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ISLEParser.Models;
using System.IO;
using ISLEParser.Models.Home;
using ISLEParser.Models.Workspace;
using Microsoft.Extensions.FileProviders;
using System.Xml;
using ISLEParser.Models.WorkspaceItems;
using ISLEParser.Models.RgbMatrices;
using ISLEParser.Models.Scripts;
using System.Threading;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace ISLEParser.Controllers
{
    public class WorkspaceController : Controller
    {
        private IWorkspaceRepository workspaceRepository;
        public WorkspaceController(IWorkspaceRepository repo)
        {
            workspaceRepository = repo;
            
        }

        public IActionResult DeleteWorkspace(string name)
        {
            workspaceRepository.DeleteWorkspace(name);
            TempData["deleteMessage"] = $"{name} has been deleted";
            return RedirectToAction("Index", "Home");
        }

        //these functions should include caching, as all objects are stacked upon the large object heap
        public async Task<IActionResult> GetWorkspaceRgbMatrices(string Name)
        {
            WorkspaceItemListViewModel model = await workspaceRepository.GetWorkspaceRgbMatrices(Name, new CancellationToken(), new LoadOptions());
            model.WorkspaceName = Name;
            return View("ViewWorkspace", model);
        }

        public async Task<IActionResult> GetWorkspaceScripts(string Name)
        {
            WorkspaceItemListViewModel model = await workspaceRepository.GetWorkspaceScripts(Name, new CancellationToken(), new LoadOptions());
            model.WorkspaceName = Name;
            //List<string> scriptNames = new List<string>();
            //Dictionary<string, string> _scriptNames = new Dictionary<string, string>();
            //foreach(var item in model.WorkspaceItems)
            //{
            //    foreach(var rgbMatrix in item.Script?.RgbMatrices/* ?? Enumerable.Empty<RgbMatrix>()*/)
            //    {
            //        string rgbMatrixScriptFileName = rgbMatrix.AlgorithmName.Replace(rgbMatrix.AlgorithmName, Regex.Replace(rgbMatrix.AlgorithmName, @".(?=.$)", "universe")) + ".js";
            //        // newFileNames.Add(item.Replace(item, Regex.Replace(item, @".(?=.$)", "universe")) + ".js");
            //        _scriptNames.Add(item.Name, rgbMatrixScriptFileName);
            //    }
            //}
            //model.scriptNames = _scriptNames;
            return View("ViewWorkspace", model);

        }

        public async Task<IActionResult> GetWorkspaceAllItems(string Name)
        {
            WorkspaceItemListViewModel model = await workspaceRepository.GetWorkspaceAllItems(Name, new CancellationToken(), new LoadOptions());
            model.WorkspaceName = Name;
            return View("ViewWorkspace", model);
        }



    }
}

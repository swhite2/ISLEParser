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
            return Content("File deleted");
        }

        
        public IActionResult GetWorkspaceRgbMatrices(string Name)
        {
            WorkspaceItemListViewModel model = workspaceRepository.GetWorkspaceRgbMatrices(Name);
            model.WorkspaceName = Name;
            return View("ViewWorkspace", model);
        }

        public IActionResult GetWorkspaceScripts(string Name)
        {
            WorkspaceItemListViewModel model = workspaceRepository.GetWorkspaceScripts(Name);
            model.WorkspaceName = Name;
            return View("ViewWorkspace", model);

        }

        public IActionResult GetWorkspaceAllItems(string Name)
        {
            WorkspaceItemListViewModel model = workspaceRepository.GetWorkspaceAllItems(Name);
            model.WorkspaceName = Name;
            return View("ViewWorkspace", model);
        }



    }
}

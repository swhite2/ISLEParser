﻿using ISLEParser.Models.WorkspaceItems;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISLEParser.Models.RgbMatrices;
using ISLEParser.Models.Workspace;
using Microsoft.AspNetCore.Http;
using System.IO;
using ISLEParser.Models.Home;
using ISLEParser.Models.Scripts;
using System.Text.RegularExpressions;

namespace ISLEParser.Controllers
{
    public class WorkspaceItemsController : Controller
    {
        private IWorkspaceRepository repository;
        public WorkspaceItemsController(IWorkspaceRepository repository)
        {
            this.repository = repository;
        }

        public ViewResult ViewScript(string Id, string Name, string Type)
        {
            WorkspaceItemViewModel model = new WorkspaceItemViewModel();
            model.Script = repository.GetWorkspaceScript(Id, Name);
            List<string> _scriptNames = new List<string>();
            foreach(var item in model.Script.RgbMatrices)
            {
                _scriptNames.Add(item.AlgorithmName.Replace(item.AlgorithmName, Regex.Replace(item.AlgorithmName, @".(?=.$)", "universe")).Replace(" ", "") + ".js");
            }
            model.scriptNames = _scriptNames;
            return View("ViewScript", model);
        }

        public IActionResult GetWorkspaceItem(string Id, string Name, string Type)
        {
            WorkspaceItemViewModel model = new WorkspaceItemViewModel();
            if(Type == "RGBMatrix")
            {
                model.RgbMatrix = repository.GetWorkspaceRgbMatrix(Id, Name);
            }
            else
            {
                model.Script = repository.GetWorkspaceScript(Id, Name);
                return View("EditScript", model);
            }


            return View("WorkspaceItem", model);
        }

        public IActionResult DeleteWorkspaceItem(string Id, string Name, string Type)
        {
            if (Type.Equals("RGBMatrix"))
            {
                repository.DeleteWorkspaceRgbMatrix(Id, Name);
                TempData["workspaceItemDeleted"] = $"RGB Matrix with ID {Id} has been deleted";
                return RedirectToAction("GetWorkspaceRgbMatrices", "Workspace", new { name = Name });
            }
            else
            {
                repository.DeleteWorkspaceScript(Id, Name);
                //TODO: Return partial view that confirms deletion
                TempData["workspaceItemDeleted"] = $"Script with ID {Id} has been deleted, including all linked RGB Matrices";
                return RedirectToAction("GetWorkspaceScripts", "Workspace", new { name = Name });
            }
            throw new NotImplementedException();
        }

        //[HttpPost("ProcessScript")]
        public ViewResult AddScript(string Name)
        {
            WorkspaceItemViewModel model = new WorkspaceItemViewModel
            {
                filesViewModel = new FilesViewModel(),
                WorkspaceName = Name
            };
            return View("AddScript", model);
        }

        [HttpPost]
        public IActionResult AddScriptInFile(WorkspaceItemViewModel model, string Name)
        {
            repository.AddScript(Name, model.Script);
            TempData["scriptAdded"] = $"Added {model.Script.Name} with {model.Script.RgbMatrices.Count} new RGB Matrices";
            return RedirectToAction("GetWorkspaceScripts", "Workspace", new { name = Name});
        }

        [HttpPost("UploadFiles")]
        public async Task<IActionResult> Post(List<IFormFile> files, string Name)
        {
            long size = files.Sum(f => f.Length);

            List<string> fileNames = new List<string>();
            FilesViewModel model = new FilesViewModel();
            WorkspaceItemViewModel itemModel = new WorkspaceItemViewModel();
            foreach (var formFile in files)
            {
                fileNames.Add(Path.GetFileNameWithoutExtension(formFile.FileName));
                var filePath = Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot/ScriptJs",
                    formFile.FileName.Replace(" ", ""));
                if (formFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }
            foreach(var item in fileNames)
            {
                model.Files.Add(new FileDetails
                {
                    Name = item
                });
            }
            if(fileNames.Count != 8)
            {
                TempData["fileNumberError"] = "Please upload exactly 8 scripts";
                return RedirectToAction("AddScript");
            }
            itemModel.filesViewModel = model;
            itemModel.Script = repository.GenerateNewScript(fileNames, Name);
            itemModel.WorkspaceName = Name;

            return View("AddScript", itemModel);

        }


        public IActionResult EditSkill(int Id, string WorkspaceName)
        {
            throw new NotImplementedException();
        }

        //Next ActionMethod: Add a Script. This will add 8 new RGB Matrices as well, linked to this script. The form will also ask for 8 files, of which only the names
        //will be stored and added in the new xml node(no purpose for the actual file data). The script name will default to the file name (-U1-8), but can be edited in the form
        //Also the UpdateWorkspace method in the repository will need to be implemented to make this work.
        //public IActionResult EditScript(int Id, string WorkspaceName)
        //{
                //Something like WorkspaceItemViewlModel model = new WorkspaceItemViewModel();
                //and then model.RgbMatrix = repository.GetWorkspaceRgbScript(Id, WorkspaceName);
                //model.Script = repository.GetWorkspaceScript()Id, WorkspaceName);
                //null checking
        //}
    }
}

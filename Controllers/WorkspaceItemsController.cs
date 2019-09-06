using ISLEParser.Models.WorkspaceItems;
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
                return RedirectToAction("GetWorkspaceRgbMatrices", "Workspace", new { name = Name });
            }
            else
            {
                repository.DeleteWorkspaceScript(Id, Name);
                //TODO: Return partial view that confirms deletion
                return RedirectToAction("GetWorkspaceScripts", "Workspace", new { name = Name });
            }
            throw new NotImplementedException();
        }

        //[HttpPost("ProcessScript")]
        public ViewResult AddScript(WorkspaceItemViewModel model, string Name)
        {
            return View("AddScript", new WorkspaceItemViewModel {
                filesViewModel = new FilesViewModel(),
                WorkspaceName = Name
            });
        }

        //public IActionResult ProcessScript(WorkspaceItemViewModel model)
        //{
        //    return View("AddScript", model);
        //}

        [HttpPost("UploadFiles")]
        public IActionResult Post(List<IFormFile> files, string Name)
        {
            long size = files.Sum(f => f.Length);


            List<string> fileNames = new List<string>();
            FilesViewModel model = new FilesViewModel();
            WorkspaceItemViewModel itemModel = new WorkspaceItemViewModel();
            foreach (var formFile in files)
            {
                fileNames.Add(Path.GetFileNameWithoutExtension(formFile.FileName));
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
                return Content("Please select 8 files");
            }
            itemModel.filesViewModel = model;
            //Generate new script here?
            itemModel.Script = repository.GenerateNewScript(fileNames, Name);

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

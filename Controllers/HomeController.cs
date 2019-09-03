using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using ISLEParser.Models.Home;
using Microsoft.Extensions.FileProviders;
using System.Xml;
using ISLEParser.Models.Workspace;

namespace ISLEParser.Controllers
{
    public class HomeController : Controller
    {
        private IWorkspaceRepository workspaceRepository;
        

        public HomeController(IWorkspaceRepository repo)
        {           
            workspaceRepository = repo;
        }
        public ViewResult Index()
        {
            var model = new WorkspaceListViewModel();
            foreach(var item in workspaceRepository.GetAllWorkspaces())
            {
                model.Workspaces.Add(item);
            }
            return View("Index", model);
        }


        [HttpPost]
        public async Task<IActionResult> UploadFileViaModel(FileInputModel model)
        {
            if (model == null || model.FileToUpload == null || model.FileToUpload.Length == 0)
                return Content("file not selected");

            var path = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot/Workspaces",
                model.FileToUpload.GetFilename());

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await model.FileToUpload.CopyToAsync(stream);

            }
            workspaceRepository.AddWorkspace(model.FileToUpload.GetFilename());
            return RedirectToAction("GetWorkspaceScripts", "Workspace", new { name = model.FileToUpload.GetFilename() }) ;
        }

        public async Task<IActionResult> Download(string filename)
        {
            if (filename == null)
                return Content("filename not present");

            var path = Path.Combine(
                           Directory.GetCurrentDirectory(),
                           "wwwroot/Workspaces", filename);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(path), Path.GetFileName(path));
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"},
                {".qxw", "text/xml" }
            };
        }
    }
}

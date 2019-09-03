using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using ISLEParser.Models.WorkspaceItems;
using ISLEParser.Models.Workspace;

namespace ISLEParser.Controllers
{
    public class RgbMatrixController : Controller
    { 
        public RgbMatrixController()
        {

        }

        

        public ActionResult ModalAction(int Id)
        {
            ViewBag.Id = Id;
            return PartialView("ModalContent");
        }

    }
}

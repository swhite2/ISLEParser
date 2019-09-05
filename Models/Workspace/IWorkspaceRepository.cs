using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISLEParser.Models.Home;
using System.Xml;
using ISLEParser.Models.WorkspaceItems;
using ISLEParser.Models.Scripts;
using ISLEParser.Models.RgbMatrices;
using System.Threading;
using System.Xml.Linq;

namespace ISLEParser.Models.Workspace
{
    public interface IWorkspaceRepository
    {
        //Interface for basic CRUD operations
        //Goes to WorkspaceRepository
        Workspace AddWorkspace(string name);
        void DeleteWorkspace(string Name);
        void UpdateWorkspace(string Name);
        void DeleteWorkspaceScript(string Id, string Name);
        void DeleteWorkspaceRgbMatrix(string Id, string Name);
        IEnumerable<Workspace> GetAllWorkspaces();
        Task<WorkspaceItemListViewModel> GetWorkspaceScripts(string Name, CancellationToken cancellationToken, LoadOptions loadOptions);
        Task<WorkspaceItemListViewModel> GetWorkspaceRgbMatrices(string Name, CancellationToken cancellationToken, LoadOptions loadOptions);
        Task<WorkspaceItemListViewModel> GetWorkspaceAllItems(string Name, CancellationToken cancellationToken, LoadOptions loadOptions);
        //WorkspaceItemViewModel GetWorkspaceItem(int Id, string Name);
        RgbMatrix GetWorkspaceRgbMatrix(string Id, string WorkspaceName);
        Script GetWorkspaceScript(string Id, string WorkspaceName);

    }
}

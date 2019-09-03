using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISLEParser.Models.WorkspaceItems
{
    public class WorkspaceItemListViewModel
    {
        public List<WorkspaceItem> WorkspaceItems { get; set; } = new List<WorkspaceItem>();
        public string WorkspaceName { get; set; }
    }

    
}

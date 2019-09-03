using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISLEParser.Models.WorkspaceItems

{
    public interface IRgbMatrixRepository
    {
        IQueryable<WorkspaceItem> RgbMatrices { get; }
    }
}

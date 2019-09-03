using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISLEParser.Models.WorkspaceItems
{
    public class RgbMatrixRepository : IRgbMatrixRepository
    {
        public IQueryable<WorkspaceItem> RgbMatrices => throw new NotImplementedException();
    }
}

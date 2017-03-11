using System.Collections.Generic;

namespace RDerP.Models
{
    public class ApplicationState
    {
        public IEnumerable<string> ExpandedPaths { get; set; }

        public ApplicationState()
        {
            ExpandedPaths = new List<string>();
        }
    }
}

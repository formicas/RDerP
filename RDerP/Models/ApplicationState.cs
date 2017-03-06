using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

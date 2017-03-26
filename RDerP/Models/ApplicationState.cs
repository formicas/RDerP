using System.Collections.Generic;

namespace RDerP.Models
{
    public class ApplicationState
    {
        public IEnumerable<string> ExpandedPaths { get; set; }
        public double? Left { get; set; }
        public double? Top { get; set; }
        public double? Height { get; set; }
        public double? Width { get; set; }

        public ApplicationState()
        {
            ExpandedPaths = new List<string>();
        }
    }
}

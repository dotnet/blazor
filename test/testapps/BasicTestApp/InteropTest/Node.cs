using System.Collections.Generic;

namespace BasicTestApp.InteropTest
{
    public class Node
    {
        public string Id { get; set; }
        public int Value { get; set; }
        public IList<Node> Children { get; set; } = new List<Node>();
    }
}

using System.Collections.Generic;

namespace Reloaded.Mod.Loader.IO.Structs.Sorting
{
    public class Node<T>
    {
        public Mark Visited = Mark.NotVisited;
        public T Element;
        public List<Node<T>> Edges; // Stores the individual list of dependencies of the current node. i.e. Dependent Mods.

        public Node(T element)
        {
            this.Element = element;
        }
    }
}

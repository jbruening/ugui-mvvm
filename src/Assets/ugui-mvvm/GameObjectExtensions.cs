
using System.Collections.Generic;
using UnityEngine;

namespace uguimvvm
{
    /// <summary>
    /// Helper methods for performing additional operations on <see cref="GameObject"/> instances.
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Builds a string that represents a <see cref="GameObject"/>'s position in the scene hierarchy.
        /// </summary>
        /// <param name="gObj">The <see cref="GameObject"/> to evaluate.</param>
        /// <returns>A string showing the parent/child chain from root-most <see cref="GameObject"/> to the given <see cref="GameObject"/>.</returns>
        public static string GetParentNameHierarchy(this GameObject gObj)
        {
            Transform parentObj = gObj.transform;
            Stack<string> stack = new Stack<string>();

            while (parentObj != null)
            {
                stack.Push(parentObj.name);
                parentObj = parentObj.parent;
            }

            string nameHierarchy = "";
            while (stack.Count > 0)
            {
                nameHierarchy += stack.Pop() + "->";
            }

            return nameHierarchy;
        }
    }
}


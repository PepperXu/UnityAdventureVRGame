#if UNITY_EDITOR

using System;
using System.IO;
using UnityEngine;

namespace Eliot.Repository
{
    /// <summary>
    /// Helps with finding files, folders etc. in the filesystem.
    /// </summary>
    public static class PathManager
    {
        /// Cache for Eliot root path
        private static string _root;
        /// Cache for Eliot styles path
        private static string _styles;
        /// Cache for Eliot images path
        private static string _images;
        
        /// <summary>
        /// Find Eliot root folder in the project
        /// </summary>
        /// <returns></returns>
        public static string EliotRoot()
        {
            if (_root != null)
                return _root;
            
            var root = Directory.GetDirectories(Application.dataPath, 
                "Eliot", SearchOption.AllDirectories)[0].Replace("\\", "/");
            var index = root.IndexOf("Assets", StringComparison.Ordinal);
            var eliotRoot = "";
            for (var i = index; i < root.Length; i++)
                eliotRoot += root[i];
            _root = eliotRoot;
            return eliotRoot;
        }
        
        /// <summary>
        /// Find Eliot Styles folder in the project
        /// </summary>
        /// <returns></returns>
        public static string EliotStyles()
        {
            if (_styles != null)
                return _styles;
            _styles = EliotRoot() + "/BehaviourEditor/GUIStyles/";
            return _styles;
        }
        
        /// <summary>
        /// Find Eliot Images folder in the project
        /// </summary>
        /// <returns></returns>
        public static string EliotImages()
        {
            if (_images != null)
                return _images;
            _images = EliotStyles() + "Textures/";
            return _images;
        }
    }
}
#endif
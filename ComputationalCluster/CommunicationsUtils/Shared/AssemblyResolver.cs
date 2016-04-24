﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UCCTaskSolver;

namespace CommunicationsUtils.Shared
{
    /// <summary>
    /// Dynamically resolves classes from assemblies located in "External libraries" folder under solution directory.
    /// </summary>
    public class AssemblyResolver : IAssemblyResolver
    {
        /// <summary>
        /// Returns absolute path to current working directory.
        /// </summary>
        private static string solutionFolderPath
        {
            get
            {
                var currentFolderPath = Environment.CurrentDirectory;
                var projectFolderPath = currentFolderPath.Substring(0, currentFolderPath.IndexOf("bin", StringComparison.Ordinal));
                var directoryInfo = new DirectoryInfo(projectFolderPath).Parent;
                if (directoryInfo == null) throw new DirectoryNotFoundException();
                return directoryInfo.FullName;
            }
        }

        /// <summary>
        /// Returns absolute path to external libraries directory.
        /// </summary>
        private static string librariesDirectory => Path.Combine(solutionFolderPath, "External libraries");

        /// <summary>
        /// Returns collection of resolved libraries assemblies.
        /// </summary>
        public IEnumerable<Assembly> ResolveLibrariesAssemblies => Directory.GetFiles(librariesDirectory, "*.dll").Select(Assembly.LoadFile);

        /// <summary>
        /// Returns specified instance of task solver.
        /// It is required to pass substring of class name and constuctor parameters.
        /// </summary>
        /// <param name="startsWith">Substring of class name</param>
        /// <param name="data">Array of bytes transferred to constructor</param>
        /// <returns></returns>
        public TaskSolver GetInstanceByBaseTypeName(string startsWith, byte[] data)
        {
            foreach (var searchType in ResolveLibrariesAssemblies.SelectMany(assembly => assembly.GetExportedTypes().Where(searchType => searchType.IsSubclassOf(typeof(TaskSolver)) && searchType.ToString().Contains(startsWith))))
            {
                return (TaskSolver)Activator.CreateInstance(searchType, data);
            }
            throw new MissingMemberException();
        }
    }

    /// <summary>
    /// Assembly resolver interface.
    /// </summary>
    internal interface IAssemblyResolver
    {
        IEnumerable<Assembly> ResolveLibrariesAssemblies { get; }
        TaskSolver GetInstanceByBaseTypeName(string startsWith, byte[] data);
    }
}
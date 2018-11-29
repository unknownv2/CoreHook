using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.BinaryInjection.ProcessUtils
{
    public class ProcessCreationConfiguration
    {
        /// <summary>
        /// Filepath to an executable program on the system that is to be launched.
        /// </summary>
        public string ExecutablePath { get; set; }
        /// <summary>
        /// Arguments to be passed to the executable program when it is started
        /// </summary>
        public string CommandLine { get; set; }
        /// <summary>
        /// Attributes and options passed to the process creation function call.
        /// </summary>
        public uint ProcessCreationFlags { get; set; }
    }
}

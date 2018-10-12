﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CoreHook.Unmanaged
{
    public interface IProcessManager
    {
        void InjectBinary(string modulePath);
        IntPtr Execute(string module, string function, byte[] args, bool canWait = true);
        void OpenHandle(Process process);
    }
}
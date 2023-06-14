using CoreHook.EntryPoint;
using CoreHook.HookDefinition;
using CoreHook.IPC.NamedPipes;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CoreHook.Uwp.FileMonitor.Hook;

public class EntryPoint : IEntryPoint
{
    private readonly Queue<string> _queue = new Queue<string>();

    private LocalHook _createFileHook;

    // The number of arguments in the constructor and Run method
    // must be equal to the number passed during injection
    // in the FileMonitor application.
    public EntryPoint(string _) { }

    public void Run(string pipeName)
    {
        try
        {
            _ = RunClientAsync(pipeName);
        }
        catch (Exception e)
        {
            ClientWriteLine(e.ToString());
        }
    }

    private static void ClientWriteLine(string msg) => Debug.WriteLine(msg);


    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    delegate IntPtr CreateFile2Delegate(string fileName, uint desiredAccess, uint shareMode, uint creationDisposition, IntPtr pCreateExParams);

    [DllImport("kernelbase.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    static extern IntPtr CreateFile2(string fileName, uint desiredAccess, uint shareMode, uint creationDisposition, IntPtr pCreateExParams);

    // this is where we are intercepting all file accesses!
    private static IntPtr CreateFile2_Hooked(string fileName, uint desiredAccess, uint shareMode, uint creationDisposition, IntPtr pCreateExParams)
    { 
        ClientWriteLine($"Creating file: '{fileName}'...");

        try
        {
            EntryPoint This = (EntryPoint)HookRuntimeInfo.Callback;
            if (This is not null)
            {
                lock (This._queue)
                {
                    This._queue.Enqueue(fileName);
                }
            }
        }
        catch
        {

        }

        // call original API...
        return CreateFile2(fileName, desiredAccess, shareMode, creationDisposition, pCreateExParams);
    }

    private void CreateHooks()
    {
        string[] functionName = { "kernelbase.dll", "CreateFile2" };
        ClientWriteLine($"Adding hook to {functionName[0]}!{functionName[1]}");

        _createFileHook = LocalHook.Create(LocalHook.GetProcAddress(functionName[0], functionName[1]), new CreateFile2Delegate(CreateFile2_Hooked), this);

        _createFileHook.ThreadACL.SetExclusiveACL(new int[] { 0 });
    }

    private async Task RunClientAsync(string pipename)// Stream clientStream)
    {
        await Task.Yield(); // We want this task to run on another thread.

        var client = new NamedPipeClient(pipename, true);

        CreateHooks();

        try
        {
            while (true)
            {
                Thread.Sleep(500);

                if (_queue.Count > 0)
                {
                    CreateFileMessage message;
                    lock (_queue)
                    {
                        message = new CreateFileMessage() { Queue = _queue.ToArray() };
                        _queue.Clear();
                    }

                    await client.TryWrite(message);
                }
            }
        }
        catch (Exception e)
        {
            ClientWriteLine(e.ToString());
        }
    }
}

using CoreHook.EntryPoint;
using CoreHook.HookDefinition;
using CoreHook.IPC.NamedPipes;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CoreHook.FileMonitor.Hook;

public partial class EntryPoint : IEntryPoint
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

    private static void ClientWriteLine(string msg) => Console.WriteLine(msg);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    delegate IntPtr CreateFileDelegate(string fileName, uint desiredAccess, uint shareMode, IntPtr securityAttributes, uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateFile(string fileName, uint desiredAccess, uint shareMode, IntPtr securityAttributes, uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);


    // Intercepts all file accesses and stores the requested filenames to a Queue.
    private static IntPtr CreateFile_Hooked(string fileName, uint desiredAccess, uint shareMode, IntPtr securityAttributes, uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile)
    {
        ClientWriteLine($"Creating file: '{fileName}'...");

        try
        {
            EntryPoint This = (EntryPoint)HookRuntimeInfo.Callback;
            if (This is not null)
            {
                lock (This._queue)
                {
                    This._queue.Enqueue($"{DateTime.Now:'dd/mm'} - Called CreateFile on {fileName}");
                }
            }
        }
        catch
        {

        }

        // Call original API function.
        return CreateFile(fileName, desiredAccess, shareMode, securityAttributes, creationDisposition, flagsAndAttributes, templateFile);
    }

    private void CreateHooks()
    {
        string[] functionName = { "kernel32.dll", "CreateFileW" };

        ClientWriteLine($"Adding hook to {functionName[0]}!{functionName[1]}");

        _createFileHook = LocalHook.Create(LocalHook.GetProcAddress(functionName[0], functionName[1]), new CreateFileDelegate(CreateFile_Hooked), this);

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

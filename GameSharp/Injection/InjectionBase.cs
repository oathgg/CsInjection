﻿using GameSharp.Extensions;
using GameSharp.Native;
using GameSharp.Diagnostics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace GameSharp.Injection
{
    public abstract class InjectionBase : IInjection
    {
        protected Process _process;

        public InjectionBase(Process process)
        {
            _process = process ?? throw new NullReferenceException();
        }

        public void InjectAndExecute(string pathToDll, string entryPoint, bool attach)
        {
            UpdateFiles(pathToDll);

            // Possible anti-cheat detterence.
            SuspendThreads(true);

            Inject(pathToDll);

            // In case we want to attach then we have to do so BEFORE we execute to give full debugging capabilities.
            if (attach && Debugger.IsAttached)
                _process.Attach();

            Logger.Info($"Creating a console for output from our injected DLL.");
            AllocConsole();

            Execute(pathToDll, entryPoint);

            SuspendThreads(false);
        }

        private void SuspendThreads(bool suspend)
        {
            foreach (ProcessThread pT in _process.Threads)
            {
                IntPtr tHandle = Kernel32.OpenThread(Enums.ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (tHandle != IntPtr.Zero)
                {
                    if (suspend)
                    {
                        Kernel32.SuspendThread(tHandle);
                    }
                    else
                    {
                        Kernel32.ResumeThread(tHandle);
                    }

                    // Close the handle; https://docs.microsoft.com/nl-nl/windows/desktop/api/processthreadsapi/nf-processthreadsapi-openthread
                    Kernel32.CloseHandle(tHandle);
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }

        /// <summary>
        ///     Updates all the DLLs which we require for the injection to succeed.
        /// </summary>
        /// <param name="pathToDll"></param>
        private void UpdateFiles(string pathToDll)
        {
            // Full path to the process
            string processPath = Path.GetDirectoryName(_process.MainModule.FileName);

            // Copy all DLLs which our injecting DLL might use which are in the same folder.
            CopyFile(pathToDll, "GameSharp.dll", processPath);
        }

        private void CopyFile(string pathToDll, string dllName, string destination)
        {
            string source = Path.Combine(Path.GetDirectoryName(pathToDll), dllName);
            string destinationFullName = Path.Combine(destination, dllName);

            File.Copy(source, destinationFullName, true);
        }

        /// <summary>
        ///     Allocates a console window by using the native APIs.
        /// </summary>
        private void AllocConsole()
        {
            IntPtr kernel32Module = Kernel32.GetModuleHandle("kernel32.dll");
            IntPtr allocConsoleAddress = Kernel32.GetProcAddress(kernel32Module, "AllocConsole");
            Kernel32.CreateRemoteThread(_process.Handle, IntPtr.Zero, 0, allocConsoleAddress, IntPtr.Zero, 0, IntPtr.Zero);
        }

        /// <summary>
        ///     DLL needs to be the same platform as the target process (e.g. x64 or x86).
        /// </summary>
        /// <param name="pathToDll"></param>
        /// <param name="entryPoint"></param>
        protected abstract void Inject(string pathToDll);

        /// <summary>
        ///     Execution after the DLL has been injected.
        /// </summary>
        /// <param name="pathToDll"></param>
        /// <param name="entryPoint"></param>
        protected abstract void Execute(string pathToDll, string entryPoint);
    }
}

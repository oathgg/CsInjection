﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GameSharp.Native;

namespace GameSharp.Extensions
{
    public static class ProcessThreadExtension
    {
        public static Structs.Context GetThreadContext(this ProcessThread thread)
        {
            Structs.Context Context = new Structs.Context();

            Context.ContextFlags = (uint)Enums.Context.CONTEXT_CONTROL;

            IntPtr hThread = Kernel32.OpenThread(Enums.ThreadAccess.GET_CONTEXT, false, (uint)thread.Id);

            if (hThread == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            Kernel32.SuspendThread(hThread);

            if (!Kernel32.GetThreadContext(hThread, ref Context))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            if (!Kernel32.CloseHandle(hThread))
                throw new Win32Exception("Cannot close thread handle.");

            Kernel32.ResumeThread(hThread);

            return Context;
        }
    }
}
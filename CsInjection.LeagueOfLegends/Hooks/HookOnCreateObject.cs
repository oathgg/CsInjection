﻿using CsInjection.Core.Hooks;
using CsInjection.LeagueOfLegends.Helpers;
using System;
using System.Runtime.InteropServices;

namespace CsInjection.LeagueOfLegends.Hooks
{
    class HookOnCreateObject : HookBase
    {
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        public delegate void OnCreateDelegate(IntPtr thisPtr, IntPtr playerObject);

        public override Delegate GetToHookDelegate()
        {
            return Marshal.GetDelegateForFunctionPointer<OnCreateDelegate>(Offsets.OnCreateObject);
        }

        public override Delegate GetToDetourDelegate()
        {
            return new OnCreateDelegate(DetourMethod);
        }

        private void DetourMethod(IntPtr thisPtr, IntPtr playerObject)
        {
            Console.WriteLine($"Created object {playerObject.ToString("X")}");
            Detour.CallOriginal(thisPtr, playerObject);
        }
    }
}

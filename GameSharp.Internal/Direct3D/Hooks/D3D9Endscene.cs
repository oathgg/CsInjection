﻿using GameSharp.Core.Memory;
using GameSharp.Internal.Extensions;
using GameSharp.Internal.Memory;
using System;
using System.Runtime.InteropServices;

namespace GameSharp.Internal.Direct3D.Hooks
{
    public abstract class D3D9Endscene : Hook
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate int D3D9EndSceneDelegate(IntPtr device);

        private int DetourMethod(IntPtr device)
        {
            DoWork(device);
            return CallOriginal<int>(device);
        }

        public override Delegate GetDetourDelegate()
        {
            return new D3D9EndSceneDelegate(DetourMethod);
        }

        public override Delegate GetHookDelegate()
        {
            InternalMemoryPointer ptr = D3DHelper.GetD3D9Endscene();
            return ptr.ToDelegate<D3D9EndSceneDelegate>();
        }

        public abstract void DoWork(IntPtr device);
    }
}

// dllmain.cpp : Defines the entry point for the DLL application.
#include "stdafx.h"
#include <fstream>
#include <metahost.h>
#pragma comment(lib, "mscoree.lib")
#include <windows.h>

DWORD WINAPI CreateDotNetRunTime(LPVOID lpParam)
{
	ICLRMetaHost    *pMetaHost = nullptr;
	ICLRRuntimeHost *pRuntimeHost = nullptr;
	ICLRRuntimeInfo *pRuntimeInfo = nullptr;
	DWORD dwRet = 0;

	CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID*)&pMetaHost);
	pMetaHost->GetRuntime(L"v4.0.30319", IID_PPV_ARGS(&pRuntimeInfo));
	pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_PPV_ARGS(&pRuntimeHost));

	pRuntimeHost->Start();
	pRuntimeHost->ExecuteInDefaultAppDomain(L"Sandbox.Injectable.dll", L"Sandbox.Injectable.EntryPoint", L"Main", L"", &dwRet);

	return 0;
}

BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		CreateDotNetRunTime(NULL);
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
	default:
		break;
	}
	return true;
}


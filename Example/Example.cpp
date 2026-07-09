#include "Example.h"
#include "resource.h"
#include "CpuUsage.h"
#include "r77def.h"
#include <Shlwapi.h>

extern "C"
{
#include "r77win.h"
}

int CALLBACK WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow)
{
	WCHAR executablePath[MAX_PATH + 1];
	if (FAILED(GetModuleFileNameW(NULL, executablePath, MAX_PATH))) return 0;

	LPWSTR fileName = PathFindFileNameW(executablePath);

	WCHAR processId[100];
	Int32ToStrW(GetCurrentProcessId(), processId);

	StrCpyW(Title, fileName);
	StrCatW(Title, L" (PID: ");
	StrCatW(Title, processId);
	StrCatW(Title, L")");

	if (!StrCmpNIW(fileName, HIDE_PREFIX, HIDE_PREFIX_LENGTH))
	{
		WindowWidth = 460;
		WindowHeight = 200;
		ShowWarningIcon = false;

		StrCpyW(Text, L"This executable's filename starts with \"");
		StrCatW(Text, HIDE_PREFIX);
		StrCatW(Text, L"\"\n\n");
		StrCatW(Text, L"  - A task manager that is injected with r77 will not display this process.\n");
		StrCatW(Text, L"  - File Explorer (or any other file browser) will not display this file.\n");
		StrCatW(Text, L"  - etc... (see documentation)");
	}
	else
	{
		WindowWidth = 400;
		WindowHeight = 150;
		ShowWarningIcon = true;

		StrCpyW(Text, L"Rename this executable's file to start with \"");
		StrCatW(Text, HIDE_PREFIX);
		StrCatW(Text, L"\".\n");
		StrCatW(Text, L"It will be hidden by r77.");
	}



	ULONG_PTR gdiToken;
	GdiplusStartupInput gdiInput;
	GdiplusStartup(&gdiToken, &gdiInput, NULL);

	WNDCLASSW wc = { };
	wc.hInstance = hInstance;
	wc.lpszClassName = L"R77EXAMPLE";
	wc.lpfnWndProc = WindowProc;
	RegisterClassW(&wc);

	Window = CreateWindowExW(
		WS_EX_DLGMODALFRAME,
		wc.lpszClassName,
		L"r77 Rootkit Example File",
		WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX,
		CW_USEDEFAULT,
		CW_USEDEFAULT,
		WindowWidth,
		WindowHeight,
		NULL,
		NULL,
		hInstance,
		NULL
	);
	if (Window == NULL) return 0;

	TextFont = CreateFontW(16, 0, 0, 0, FW_DONTCARE, FALSE, FALSE, FALSE, ANSI_CHARSET, OUT_TT_PRECIS, CLIP_DEFAULT_PRECIS, DEFAULT_QUALITY, DEFAULT_PITCH | FF_DONTCARE, L"Segoe UI");
	TitleFont = CreateFontW(26, 0, 0, 0, FW_DONTCARE, FALSE, FALSE, FALSE, ANSI_CHARSET, OUT_TT_PRECIS, CLIP_DEFAULT_PRECIS, DEFAULT_QUALITY, DEFAULT_PITCH | FF_DONTCARE, L"Segoe UI");
	WhiteBrush = CreateSolidBrush(RGB(255, 255, 255));
	LightGrayBrush = CreateSolidBrush(RGB(240, 240, 240));
	LogoImage = GetImageResource(IDB_EXAMPLE32, "PNG");
	HelpImage = GetImageResource(IDB_HELP16, "PNG");
	WarningImage = GetImageResource(IDB_WARNING16, "PNG");

	RECT windowRect = { 0, 0, WindowWidth, WindowHeight };
	AdjustWindowRect(&windowRect, GetWindowLongW(Window, GWL_STYLE), FALSE);
	SetWindowPos(Window, NULL, 0, 0, windowRect.right - windowRect.left, windowRect.bottom - windowRect.top, SWP_NOMOVE);
	SetClassLongPtrW(Window, GCLP_HBRBACKGROUND, (LONG_PTR)WhiteBrush);
	ShowWindow(Window, nCmdShow);

	CpuUsageComboBox = CreateWindowW(WC_COMBOBOXW, NULL, WS_TABSTOP | WS_CHILD | WS_VISIBLE | CBS_DROPDOWNLIST, 15, WindowHeight - 35, 120, 23, Window, NULL, hInstance, NULL);
	SendMessageW(CpuUsageComboBox, WM_SETFONT, (WPARAM)TextFont, TRUE);
	SendMessageW(CpuUsageComboBox, CB_ADDSTRING, 0, (LPARAM)L"CPU Usage: 0%");
	SendMessageW(CpuUsageComboBox, CB_ADDSTRING, 0, (LPARAM)L"CPU Usage: 25%");
	SendMessageW(CpuUsageComboBox, CB_ADDSTRING, 0, (LPARAM)L"CPU Usage: 50%");
	SendMessageW(CpuUsageComboBox, CB_ADDSTRING, 0, (LPARAM)L"CPU Usage: 75%");
	SendMessageW(CpuUsageComboBox, CB_ADDSTRING, 0, (LPARAM)L"CPU Usage: 100%");
	SendMessageW(CpuUsageComboBox, CB_SETCURSEL, 0, 0);

	HelpButton = CreateWindowW(WC_STATICW, NULL, WS_CHILD | WS_VISIBLE | SS_BITMAP | SS_NOTIFY | SS_CENTERIMAGE, 140, WindowHeight - 35, 23, 23, Window, (HMENU)1, hInstance, NULL);
	SendMessageW(HelpButton, STM_SETIMAGE, IMAGE_BITMAP, (LPARAM)HelpImage);

	CloseButton = CreateWindowW(WC_BUTTONW, L"Close", WS_TABSTOP | WS_CHILD | WS_VISIBLE | BS_DEFPUSHBUTTON, WindowWidth - 90, WindowHeight - 35, 75, 23, Window, NULL, hInstance, NULL);
	SendMessageW(CloseButton, WM_SETFONT, (WPARAM)TextFont, TRUE);

	InitializeCpuUsage();

	MSG msg;
	while (GetMessageW(&msg, NULL, 0, 0))
	{
		if (!IsDialogMessageW(Window, &msg))
		{
			TranslateMessage(&msg);
			DispatchMessageW(&msg);
		}
	}

	UninitializeCpuUsage();
	GdiplusShutdown(gdiToken);
	return 0;
}
LRESULT CALLBACK WindowProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
		case WM_PAINT:
		{
			PAINTSTRUCT ps;
			HDC hdc = BeginPaint(hwnd, &ps);
			SetBkMode(hdc, TRANSPARENT);
			HDC hdcMem = CreateCompatibleDC(hdc);
			BITMAP bitmap;

			// Window logo
			HBITMAP oldBitmap = (HBITMAP)SelectObject(hdcMem, LogoImage);
			GetObject(LogoImage, sizeof(bitmap), &bitmap);
			BitBlt(hdc, 20, 20, bitmap.bmWidth, bitmap.bmHeight, hdcMem, 0, 0, SRCCOPY);

			// Title
			HFONT oldFont = (HFONT)SelectObject(hdc, TitleFont);
			SetTextColor(hdc, RGB(0, 102, 204));
			TextOutW(hdc, 60, 20, Title, lstrlenW(Title));

			// Warning icon
			if (ShowWarningIcon)
			{
				SelectObject(hdcMem, WarningImage);
				GetObject(WarningImage, sizeof(bitmap), &bitmap);
				BitBlt(hdc, 60, 60, bitmap.bmWidth, bitmap.bmHeight, hdcMem, 0, 0, SRCCOPY);
			}

			// Text
			RECT rect;

			if (ShowWarningIcon)
			{
				rect = { 85, 60, WindowWidth - 45, WindowHeight - 50 };
			}
			else
			{
				rect = { 60, 60, WindowWidth - 20, WindowHeight - 50 };
			}

			SelectObject(hdc, TextFont);
			SetTextColor(hdc, RGB(0, 0, 0));
			DrawTextW(hdc, Text, -1, &rect, DT_LEFT | DT_TOP | DT_WORDBREAK);

			// Button row background
			rect = { 0, WindowHeight - 45, WindowWidth, WindowHeight };
			FillRect(hdc, &rect, LightGrayBrush);

			SelectObject(hdc, oldFont);
			SelectObject(hdcMem, oldBitmap);
			DeleteDC(hdcMem);
			EndPaint(hwnd, &ps);
		}
		break;
		case WM_COMMAND:
			if (lParam == (LPARAM)CloseButton)
			{
				DestroyWindow(hwnd);
			}
			else if (lParam == (LPARAM)HelpButton)
			{
				MessageBoxW(hwnd, L"CPU usage is hidden by r77. To see the effect, increase CPU usage of this process.\n\nThis process is running with idle priority to not interrupt other tasks.", L"Help", MB_OK | MB_ICONINFORMATION);
			}
			else if (lParam == (LPARAM)CpuUsageComboBox && HIWORD(wParam) == CBN_SELCHANGE)
			{
				switch ((int)SendMessageW(CpuUsageComboBox, CB_GETCURSEL, 0, 0))
				{
					case 0: SetCpuUsage(0); break;
					case 1: SetCpuUsage(25); break;
					case 2: SetCpuUsage(50); break;
					case 3: SetCpuUsage(75); break;
					case 4: SetCpuUsage(100); break;
					default: throw;
				}
			}
			break;
		case WM_SETCURSOR:
			if ((HWND)wParam == HelpButton)
			{
				SetCursor(LoadCursorW(NULL, IDC_HAND));
				return TRUE;
			}
			else
			{
				SetCursor(LoadCursorW(NULL, IDC_ARROW));
				return TRUE;
			}
		case WM_DESTROY:
			PostQuitMessage(0);
			break;
		default:
			return DefWindowProcW(hwnd, msg, wParam, lParam);
	}

	return 0;
}

HBITMAP GetImageResource(DWORD resourceID, LPCSTR type)
{
	LPBYTE resourceData;
	DWORD resourceSize;

	if (GetResource(resourceID, type, &resourceData, &resourceSize))
	{
		return CreateImage(resourceData, resourceSize);
	}

	return NULL;
}
HBITMAP CreateImage(LPCBYTE data, DWORD size)
{
	HBITMAP result = NULL;

	IStream *imageStream = SHCreateMemStream(data, size);
	if (imageStream)
	{
		Bitmap *bitmap = new Bitmap(imageStream);

		if (bitmap->GetHBITMAP(Color::Transparent, &result) != Ok)
		{
			result = NULL;
		}

		delete bitmap;
		imageStream->Release();
	}

	return result;
}
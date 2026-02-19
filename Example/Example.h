#include "r77mindef.h"
#include <Windows.h>
#include <atlimage.h>
#include <Commctrl.h>
using namespace Gdiplus;

int WindowWidth;
int WindowHeight;
bool ShowWarningIcon;
WCHAR Title[500];
WCHAR Text[1000];

HWND Window;
HWND CpuUsageComboBox;
HWND HelpButton;
HWND CloseButton;
HFONT TextFont;
HFONT TitleFont;
HBRUSH WhiteBrush;
HBRUSH LightGrayBrush;
HBITMAP LogoImage;
HBITMAP HelpImage;
HBITMAP WarningImage;

LRESULT CALLBACK WindowProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);
HBITMAP LoadImageFromResource(DWORD resourceID, LPCWSTR type);
Bitmap* LoadImageFromBinary(LPBYTE data, DWORD size);
BOOL GetResource(DWORD resourceID, LPCWSTR type, LPBYTE *data, LPDWORD size);
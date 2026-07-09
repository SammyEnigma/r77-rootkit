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

HBITMAP GetImageResource(DWORD resourceID, LPCSTR type);
HBITMAP CreateImage(LPCBYTE data, DWORD size);
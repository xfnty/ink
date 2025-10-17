#pragma comment(lib, "gdi32.lib")
#pragma comment(lib, "user32.lib")
#pragma comment(lib, "comctl32.lib")
#pragma comment(lib, "kernel32.lib")

#pragma warning(disable:5105)

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <windowsx.h>
#include <commctrl.h>
#include <stdbool.h>
#include <stdio.h>

static LRESULT window_event_handler(HWND hwnd, UINT msg, WPARAM wp, LPARAM lp);
static void set_status(const char *format, ...);

static HWND s_hwnd;
static HANDLE s_bar;
static bool s_running = true;

int main(int argc, char const *argv[]) {
    INITCOMMONCONTROLSEX inf = {
        .dwSize = sizeof(inf),
        .dwICC = ICC_STANDARD_CLASSES,
    };
    InitCommonControlsEx(&inf);

    HMODULE instance = GetModuleHandle(0);

    WNDCLASSA window_class = {
        .style = CS_OWNDC,
        .hInstance = instance,
        .lpszClassName = "tester",
        .lpfnWndProc = window_event_handler,
        .hCursor = LoadCursorA(0, IDC_ARROW),
        .hbrBackground = GetStockObject(WHITE_BRUSH),
    };
    RegisterClassA(&window_class);

    s_hwnd = CreateWindowExA(
        0,
        "tester",
        "WM_POINTER Plugin Tester",
        WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT,
        CW_USEDEFAULT, CW_USEDEFAULT,
        0,
        0,
        window_class.hInstance,
        0
    );

    s_bar = CreateWindowExA(
        0,
        STATUSCLASSNAME,
        0,
        WS_CHILD | WS_VISIBLE,
        0, 0, 0, 0,
        s_hwnd,
        0,
        instance,
        0
    );
    SendMessageA(s_bar, SB_SETPARTS, 1, (LPARAM)(int[]){ -1 });
    set_status("No WM_POINTER messages were recieved yet.");

    // EnableMouseInPointer(TRUE);

    ShowWindow(s_hwnd, SW_SHOW);

    while (s_running) {
        for (MSG msg; PeekMessageA(&msg, s_hwnd, 0, 0, PM_REMOVE) != 0;) {
            TranslateMessage(&msg);
            DispatchMessageA(&msg);
        }
    }

    return 0;
}

LRESULT window_event_handler(HWND hwnd, UINT msg, WPARAM wp, LPARAM lp) {
    if (hwnd != s_hwnd)
        return DefWindowProcA(hwnd, msg, wp, lp);

    switch (msg) {
    case WM_CLOSE:
        s_running = false;
        return 1;

    case WM_SIZE:
        SendMessageA(s_bar, WM_SIZE, 0, 0);
        break;

    case WM_POINTERENTER:
    case WM_POINTERLEAVE:
    case WM_POINTERUP:
    case WM_POINTERDOWN:
    case WM_POINTERUPDATE:
    case WM_POINTERCAPTURECHANGED:
    case WM_POINTERDEVICECHANGE:
    case WM_POINTERACTIVATE:
        {
            POINTER_INFO inf = {0};
            UINT32 id = GET_POINTERID_WPARAM(wp);
            if (GetPointerInfo(id, &inf)) {
                POINTER_PEN_INFO pinf = {0};
                if (GetPointerPenInfo(id, &pinf)) {
                    set_status(
                        "Pointer x=%d y=%d down=%d pressure=%d B1=%d B2=%d B3=%d B4=%d B5=%d (%u)\n",
                        inf.ptPixelLocation.x,
                        inf.ptPixelLocation.y,
                        (bool)(inf.pointerFlags & POINTER_FLAG_INCONTACT),
                        pinf.pressure,
                        (bool)((inf.pointerFlags & POINTER_FLAG_FIRSTBUTTON) && (inf.ButtonChangeType & POINTER_CHANGE_FIRSTBUTTON_DOWN)),
                        (bool)((inf.pointerFlags & POINTER_FLAG_SECONDBUTTON) && (inf.ButtonChangeType & POINTER_CHANGE_SECONDBUTTON_DOWN)),
                        (bool)((inf.pointerFlags & POINTER_FLAG_THIRDBUTTON) && (inf.ButtonChangeType & POINTER_CHANGE_THIRDBUTTON_DOWN)),
                        (bool)((inf.pointerFlags & POINTER_FLAG_FOURTHBUTTON) && (inf.ButtonChangeType & POINTER_CHANGE_FOURTHBUTTON_DOWN)),
                        (bool)((inf.pointerFlags & POINTER_FLAG_FIFTHBUTTON) && (inf.ButtonChangeType & POINTER_CHANGE_FIFTHBUTTON_DOWN)),
                        msg
                    );
                } else {
                    set_status("GetPointerPenInfo() failed with code %d (inf.type=%d)\n", GetLastError(), inf.pointerType);
                }
            } else {
                set_status("GetPointerInfo() failed with code %d\n", GetLastError());
            }
        }
        break;
    }

    return DefWindowProcA(hwnd, msg, wp, lp);
}

static void set_status(const char *format, ...) {
    char buffer[1024];

    va_list args;
    va_start(args, format);
    vsnprintf(buffer, sizeof(buffer), format, args);
    va_end(args);

    SendMessageA(s_bar, SB_SETTEXTA, 0, (LPARAM)buffer);
}

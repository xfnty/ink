#pragma comment(lib, "gdi32.lib")
#pragma comment(lib, "user32.lib")
#pragma comment(lib, "comctl32.lib")
#pragma comment(lib, "kernel32.lib")

#pragma warning(disable:5105)

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
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
        set_status("Message: WM_POINTER (%u)\n", msg);
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

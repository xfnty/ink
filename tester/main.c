#pragma comment(lib, "gdi32.lib")
#pragma comment(lib, "user32.lib")
#pragma comment(lib, "kernel32.lib")

#pragma warning(disable:5105)

#define POINTER_RADIUS_MAX 30

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <windowsx.h>
#include <stdbool.h>
#include <stdio.h>
#include <assert.h>

static LRESULT window_event_handler(HWND hwnd, UINT msg, WPARAM wp, LPARAM lp);

static WNDCLASSA s_wndclass;
static HWND s_hwnd;
static HBRUSH s_brush;
static RECT s_clrect;
static POINTER_PEN_INFO s_pen;

int main(int argc, char const *argv[]) {
    s_brush = CreateSolidBrush(RGB(0, 0, 0));

    WNDCLASSA s_wndclass = {
        .style = CS_HREDRAW | CS_VREDRAW,
        .hInstance = GetModuleHandle(0),
        .lpszClassName = "tester",
        .lpfnWndProc = window_event_handler,
        .hCursor = LoadCursorA(0, IDC_ARROW),
        .hbrBackground = GetStockObject(WHITE_BRUSH),
    };
    RegisterClassA(&s_wndclass);
    s_hwnd = CreateWindowExA(
        0,
        "tester",
        "Plugin Tester",
        WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT,
        CW_USEDEFAULT, CW_USEDEFAULT,
        0,
        0,
        s_wndclass.hInstance,
        0
    );
    ShowWindow(s_hwnd, SW_SHOW);

    for (MSG m; GetMessageA(&m, 0, 0, 0) > 0;) {
        TranslateMessage(&m);
        DispatchMessageA(&m);
    }

    return 1;
}

LRESULT window_event_handler(HWND hwnd, UINT msg, WPARAM wp, LPARAM lp) {
    if (hwnd != s_hwnd)
        return DefWindowProcA(hwnd, msg, wp, lp);

    PAINTSTRUCT ps;
    HDC hdc;
    POINT p;

    switch (msg) {
    case WM_CLOSE:
        PostQuitMessage(0);
        return 1;

    case WM_SIZE:
        GetClientRect(hwnd, &s_clrect);
        InvalidateRect(hwnd, 0, 1);
        break;

    case WM_PAINT:
        hdc = BeginPaint(hwnd, &ps);

        FillRect(hdc, &s_clrect, s_wndclass.hbrBackground);

        p = s_pen.pointerInfo.ptPixelLocation;
        ScreenToClient(hwnd, &p);
        SelectObject(hdc, s_brush);
        Ellipse(
            hdc,
            p.x - (POINTER_RADIUS_MAX * (s_pen.pressure / 1024.0f)),
            p.y - (POINTER_RADIUS_MAX * (s_pen.pressure / 1024.0f)),
            p.x + (POINTER_RADIUS_MAX * (s_pen.pressure / 1024.0f)),
            p.y + (POINTER_RADIUS_MAX * (s_pen.pressure / 1024.0f))
        );

        EndPaint(hwnd, &ps);
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
            UINT32 id = GET_POINTERID_WPARAM(wp);
            GetPointerInfo(id, &s_pen.pointerInfo) && GetPointerPenInfo(id, &s_pen);
        }
        InvalidateRect(hwnd, 0, 1);
        break;
    }

    return DefWindowProcA(hwnd, msg, wp, lp);
}

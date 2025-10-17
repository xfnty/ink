using System;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace XfntyPlugins
{
    using HANDLE = IntPtr;
    using HWND = IntPtr;
    using WORD = UInt16;
    using DWORD = UInt32;
    using UINT = UInt32;
    using WPARAM = UIntPtr;
    using LPARAM = Int64;

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public enum MSG : UINT
    {
        WM_POINTERUPDATE = 0x0245,
    }

    public enum POINTER_INPUT_TYPE
    {
        PT_POINTER,
        PT_TOUCH,
        PT_PEN,
        PT_MOUSE,
        PT_TOUCHPAD
    }

    public enum POINTER_FEEDBACK_MODE
    {
        DEFAULT = 1,
        INDIRECT = 2,
        NONE = 3
    }

    public enum POINTER_BUTTON_CHANGE_TYPE
    {
        NONE,
        FIRSTBUTTON_DOWN,
        FIRSTBUTTON_UP,
        SECONDBUTTON_DOWN,
        SECONDBUTTON_UP,
        THIRDBUTTON_DOWN,
        THIRDBUTTON_UP,
        FOURTHBUTTON_DOWN,
        FOURTHBUTTON_UP,
        FIFTHBUTTON_DOWN,
        FIFTHBUTTON_UP
    }

    public enum POINTER_FLAGS
    {
        NONE = 0x00000000,
        NEW = 0x00000001,
        INRANGE = 0x00000002,
        INCONTACT = 0x00000004,
        FIRSTBUTTON = 0x00000010,
        SECONDBUTTON = 0x00000020,
        THIRDBUTTON = 0x00000040,
        FOURTHBUTTON = 0x00000080,
        FIFTHBUTTON = 0x00000100,
        PRIMARY = 0x00002000,
        CONFIDENCE = 0x000004000,
        CANCELED = 0x000008000,
        DOWN = 0x00010000,
        UPDATE = 0x00020000,
        UP = 0x00040000,
        WHEEL = 0x00080000,
        HWHEEL = 0x00100000,
        CAPTURECHANGED = 0x00200000,
        HASTRANSFORM = 0x00400000,
    }

    public enum PEN_FLAGS
    {
        NONE = 0x00000000,
        BARREL = 0x00000001,
        INVERTED = 0x00000002,
        ERASER = 0x00000004
    }

    public enum PEN_MASK
    {
        NONE = 0x00000000,
        PRESSURE = 0x00000001,
        ROTATION = 0x00000002,
        TILT_X = 0x00000004,
        TILT_Y = 0x00000008
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINTER_INFO
    {
        public POINTER_INPUT_TYPE pointerType;
        public uint pointerId;
        public uint frameId;
        public POINTER_FLAGS pointerFlags;
        public HANDLE sourceDevice;
        public HWND hwndTarget;
        public POINT ptPixelLocation;
        public POINT ptHimetricLocation;
        public POINT ptPixelLocationRaw;
        public POINT ptHimetricLocationRaw;
        public DWORD dwTime;
        public uint historyCount;
        public int InputData;
        public DWORD dwKeyStates;
        public ulong PerformanceCount;
        public POINTER_BUTTON_CHANGE_TYPE ButtonChangeType;
    }

    public struct POINTER_PEN_INFO
    {
        public POINTER_INFO pointerInfo;
        public PEN_FLAGS pointerFlags;
        public PEN_MASK penMask;
        public uint pressure;
        public uint rotation;
        public int tiltX;
        public int tiltY;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINTER_TYPE_INFO
    {
        public POINTER_INPUT_TYPE type;
        public POINTER_PEN_INFO penInfo;
    }

    public static partial class Win32
    {
        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern HANDLE CreateSyntheticPointerDevice(POINTER_INPUT_TYPE pointerType, ulong maxCount, POINTER_FEEDBACK_MODE mode);

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool InjectSyntheticPointerInput(HANDLE device, [In, MarshalAs(UnmanagedType.LPArray)] POINTER_TYPE_INFO[] pointerInfo, uint count);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern HANDLE GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessageA(HWND hwnd, UINT msg, WPARAM wp, LPARAM lp);
    }

    public class MyPointer : IAbsolutePointer, IMouseButtonHandler, IPressureHandler, ITiltHandler, IEraserHandler, IHoverDistanceHandler
    {
        private TabletReference? _tablet;
        private IVirtualScreen? _screen;
        private HANDLE _pen;
        private readonly POINTER_TYPE_INFO[]? _pointer;

        public MyPointer(TabletReference tablet, IVirtualScreen screen)
        {
            _tablet = tablet;
            _screen = screen;

            _pointer = new POINTER_TYPE_INFO[]
            {
                new POINTER_TYPE_INFO
                {
                    type = POINTER_INPUT_TYPE.PT_PEN,
                    penInfo = new POINTER_PEN_INFO
                    {
                        pointerInfo = new POINTER_INFO
                        {
                            pointerFlags = POINTER_FLAGS.PRIMARY,
                            pointerType = POINTER_INPUT_TYPE.PT_PEN,
                            pointerId = 1,
                            ptPixelLocation = new POINT(),
                            ptPixelLocationRaw = new POINT(),
                            dwTime = 0,
                            PerformanceCount = 0,
                        },
                        penMask = PEN_MASK.PRESSURE,
                    }
                }
            };

            _pen = Win32.CreateSyntheticPointerDevice(POINTER_INPUT_TYPE.PT_PEN, 1, POINTER_FEEDBACK_MODE.NONE);
            if (_pen == IntPtr.Zero)
            {
                throw new Exception($"CreateSyntheticPointerDevice() failed with code {Marshal.GetLastWin32Error()}");
            }
            Log.Write("WM_POINTER", $"Created synthetic pointer device {_pen}");
        }

        public unsafe void SendInput()
        {
            HWND hwnd = Win32.GetForegroundWindow();
            _pointer![0].penInfo.pointerInfo.hwndTarget = hwnd;
            if (!Win32.InjectSyntheticPointerInput(_pen, _pointer!, 1))
            {
                throw new Exception($"InjectSyntheticPointerInput() failed with code {Marshal.GetLastWin32Error()}");
            }

            Log.Write("WM_POINTER", $"send input to {_pointer![0].penInfo.pointerInfo.hwndTarget}");
        }

        public void MouseDown(MouseButton button)
        {
        }

        public void MouseUp(MouseButton button)
        {
        }

        public void SetPressure(float percentage)
        {
            _pointer![0].penInfo.pressure = (uint)(1024 * percentage);
            if (percentage > 0)
            {
                _pointer![0].penInfo.pointerInfo.pointerFlags = POINTER_FLAGS.INRANGE | POINTER_FLAGS.DOWN | POINTER_FLAGS.INCONTACT | POINTER_FLAGS.FIRSTBUTTON;
            }
            else
            {
                _pointer![0].penInfo.pointerInfo.pointerFlags = POINTER_FLAGS.INRANGE | POINTER_FLAGS.UP;
            }
            SendInput();
        }

        public void SetTilt(Vector2 tilt)
        {
        }

        public void SetEraser(bool isEraser)
        {
        }

        public void SetHoverDistance(uint distance)
        {
        }

        public void SetPosition(Vector2 pos)
        {
            POINT p = new POINT((int)pos.X, (int)pos.Y);
            _pointer![0].penInfo.pointerInfo.pointerFlags = POINTER_FLAGS.INRANGE;
            _pointer![0].penInfo.pointerInfo.ptPixelLocation = p;
            _pointer[0].penInfo.pointerInfo.ptPixelLocationRaw = p;
            SendInput();
        }
    }

    [PluginName("WM_POINTER Absolute Mode")]
    public class WMPointerAbsoluteMode : AbsoluteOutputMode
    {
        private MyPointer? _pointer;
        private IVirtualScreen? _virtualScreen;

        [Resolved]
        public IServiceProvider ServiceProvider
        {
            set => _virtualScreen = (IVirtualScreen)value.GetService(typeof(IVirtualScreen))!;
        }

        public override TabletReference Tablet
        {
            get => base.Tablet;
            set
            {
                base.Tablet = value;
                _pointer = new MyPointer(value, _virtualScreen!);
            }
        }

        public override IAbsolutePointer Pointer
        {
            get => _pointer!;
            set { }
        }
    }
}

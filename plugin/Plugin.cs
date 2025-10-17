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
    using HWND   = IntPtr;
    using WORD   = UInt16;
    using DWORD  = UInt32;
    using UINT   = UInt32;
    using WPARAM = UIntPtr;
    using LPARAM = Int64;

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    public enum POINTER_INPUT_TYPE
    {
        PT_POINTER  = 1,
        PT_TOUCH    = 2,
        PT_PEN      = 3,
        PT_MOUSE    = 4,
        PT_TOUCHPAD = 5
    }

    public enum POINTER_FEEDBACK_MODE
    {
        DEFAULT  = 1,
        INDIRECT = 2,
        NONE     = 3
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
        public static extern void DestroySyntheticPointerDevice(HANDLE pen);

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool InjectSyntheticPointerInput(HANDLE device, [In, MarshalAs(UnmanagedType.LPArray)] POINTER_TYPE_INFO[] pointerInfo, uint count);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern HANDLE GetForegroundWindow();
    }

    public class MyPointer : IAbsolutePointer, ISynchronousPointer, IMouseButtonHandler, IPressureHandler, ITiltHandler, IEraserHandler, IHoverDistanceHandler
    {
        private HANDLE _pen;
        private readonly POINTER_TYPE_INFO[]? _pointer;

        private uint  _pressure;
        private POINT _position;
        private bool  _changed;

        public MyPointer()
        {
            _pointer = new POINTER_TYPE_INFO[]
            {
                new POINTER_TYPE_INFO
                {
                    type = POINTER_INPUT_TYPE.PT_PEN,
                    penInfo = new POINTER_PEN_INFO
                    {
                        pointerInfo = new POINTER_INFO
                        {
                            pointerType = POINTER_INPUT_TYPE.PT_PEN,
                            ptPixelLocation = new POINT{},
                            ptPixelLocationRaw = new POINT{},
                        },
                        penMask = PEN_MASK.PRESSURE,
                    }
                }
            };

            _pen = Win32.CreateSyntheticPointerDevice(
                POINTER_INPUT_TYPE.PT_PEN,
                1,
                POINTER_FEEDBACK_MODE.DEFAULT
            );
            if (_pen == IntPtr.Zero)
            {
                throw new Exception($"CreateSyntheticPointerDevice() failed with code {Marshal.GetLastWin32Error()}");
            }
        }

        ~MyPointer()
        {
            Win32.DestroySyntheticPointerDevice(_pen);
        }

        public void Flush()
        {
            if (!_changed)
                return;
            
            POINTER_FLAGS flags = POINTER_FLAGS.INRANGE;
            if (_pressure > 0) flags |= POINTER_FLAGS.INCONTACT | POINTER_FLAGS.DOWN;
            else               flags |= POINTER_FLAGS.UP;

            _pointer![0].penInfo.pointerInfo.hwndTarget = Win32.GetForegroundWindow();
            _pointer[0].penInfo.pointerInfo.pointerFlags = flags;
            _pointer[0].penInfo.pressure = _pressure;
            _pointer[0].penInfo.pointerInfo.ptPixelLocation = _position;
            _pointer[0].penInfo.pointerInfo.ptPixelLocationRaw = _position;
            Win32.InjectSyntheticPointerInput(_pen, _pointer!, 1);

            _changed = false;
        }

        public void Reset()
        {
        }

        public void MouseDown(MouseButton button)
        {
        }

        public void MouseUp(MouseButton button)
        {
        }

        public void SetPressure(float percentage)
        {
            _pressure = (uint)(1024 * percentage);
            _changed = true;
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
            _position.X = (int)pos.X;
            _position.Y = (int)pos.Y;
            _changed = true;
        }
    }

    [PluginName("Windows Ink")]
    public class WMPointerAbsoluteMode : AbsoluteOutputMode
    {
        public override IAbsolutePointer Pointer { get; set; } = new MyPointer();
    }
}

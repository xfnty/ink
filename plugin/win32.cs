using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace XfntyPlugins
{
    public static partial class Win32
    {
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

        public enum EVENT_CONSTANT
        {
            EVENT_SYSTEM_FOREGROUND = 0x0003,
            WINEVENT_OUTOFCONTEXT = 0x0000,
            WINEVENT_SKIPOWNPROCESS = 0x0002,
        }

        public enum INPUT_TYPE
        {
            MOUSE = 0,
            KEYBOARD = 1,
        }

        public enum MOUSEEVENTF
        {
            MOVE = 0x0001,
            LEFTDOWN = 0x0002,
            LEFTUP = 0x0004,
            RIGHTDOWN = 0x0008,
            RIGHTUP = 0x0010,
            ABSOLUTE = 0x8000,
        }

        public struct POINT
        {
            public Int32 X;
            public Int32 Y;
        }

        public struct POINTER_INFO
        {
            public POINTER_INPUT_TYPE pointerType;
            public UInt32 pointerId;
            public UInt32 frameId;
            public POINTER_FLAGS pointerFlags;
            public IntPtr sourceDevice;
            public IntPtr hwndTarget;
            public POINT ptPixelLocation;
            public POINT ptHimetricLocation;
            public POINT ptPixelLocationRaw;
            public POINT ptHimetricLocationRaw;
            public UInt32 dwTime;
            public UInt32 historyCount;
            public Int32 InputData;
            public UInt32 dwKeyStates;
            public UInt64 PerformanceCount;
            public POINTER_BUTTON_CHANGE_TYPE ButtonChangeType;
        }

        public struct POINTER_PEN_INFO
        {
            public POINTER_INFO pointerInfo;
            public PEN_FLAGS pointerFlags;
            public PEN_MASK penMask;
            public UInt32 pressure;
            public UInt32 rotation;
            public Int32 tiltX;
            public Int32 tiltY;
        }

        public struct POINTER_TYPE_INFO
        {
            public POINTER_INPUT_TYPE type;
            public POINTER_PEN_INFO penInfo;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUT
        {
            [FieldOffset(0)]
            public UInt32 type;
            [FieldOffset(4)]
            public MOUSEINPUT mi;
            [FieldOffset(4)]
            public KEYBDINPUT ki;
        }

        public struct MOUSEINPUT
        {
            public Int32  dx;
            public Int32  dy;
            public UInt32 mouseData;
            public UInt32 dwFlags;
            public UInt32 time;
            public UInt64 dwExtraInfo;
        }

        public struct KEYBDINPUT
        {
            public UInt16 wVk;
            public UInt16 wScan;
            public UInt32 dwFlags;
            public UInt32 time;
            public UInt64 dwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr CreateSyntheticPointerDevice(POINTER_INPUT_TYPE pointerType, UInt64 maxCount, POINTER_FEEDBACK_MODE mode);

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern void DestroySyntheticPointerDevice(IntPtr pen);

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool InjectSyntheticPointerInput(IntPtr device, ref POINTER_TYPE_INFO pointerInfo, UInt32 count);

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 SendInput(UInt32 cInputs, ref INPUT input, Int32 cbSize);
    }
}

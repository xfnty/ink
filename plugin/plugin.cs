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
    public class InkPointer : IAbsolutePointer, ISynchronousPointer, IMouseButtonHandler, IPressureHandler, ITiltHandler, IEraserHandler, IHoverDistanceHandler
    {
        private IntPtr _pen;
        private Win32.POINTER_TYPE_INFO _penInput;
        private bool _changed;

        public InkPointer()
        {
            _penInput.type = Win32.POINTER_INPUT_TYPE.PT_PEN;
            _penInput.penInfo.pointerInfo.pointerType = Win32.POINTER_INPUT_TYPE.PT_PEN;
            _penInput.penInfo.penMask = Win32.PEN_MASK.PRESSURE;

            _pen = Win32.CreateSyntheticPointerDevice(
                Win32.POINTER_INPUT_TYPE.PT_PEN,
                1,
                Win32.POINTER_FEEDBACK_MODE.DEFAULT
            );
            if (_pen == IntPtr.Zero)
            {
                throw new Exception($"CreateSyntheticPointerDevice() failed with code {Marshal.GetLastWin32Error()}");
            }
        }

        ~InkPointer()
        {
            Win32.DestroySyntheticPointerDevice(_pen);
        }

        public void Flush()
        {
            if (!_changed)
                return;

            Win32.POINTER_FLAGS flags = Win32.POINTER_FLAGS.INRANGE;
            flags |= (_penInput.penInfo.pressure > 0)
                ? (Win32.POINTER_FLAGS.INCONTACT | Win32.POINTER_FLAGS.DOWN)
                : (Win32.POINTER_FLAGS.UP);

            _penInput.penInfo.pointerInfo.hwndTarget = Win32.GetForegroundWindow();
            _penInput.penInfo.pointerInfo.pointerFlags = flags;
            _penInput.penInfo.pointerInfo.ptPixelLocationRaw = _penInput.penInfo.pointerInfo.ptPixelLocation;
            Win32.InjectSyntheticPointerInput(_pen, ref _penInput, 1);

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
            _penInput.penInfo.pressure = (UInt32)(1024 * percentage);
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
            _penInput.penInfo.pointerInfo.ptPixelLocation = new Win32.POINT { X = (int)pos.X, Y = (int)pos.Y };
            _changed = true;
        }
    }

    [PluginName("Windows Ink")]
    public class WMPointerAbsoluteMode : AbsoluteOutputMode
    {
        public override IAbsolutePointer Pointer { get; set; } = new InkPointer();
    }
}

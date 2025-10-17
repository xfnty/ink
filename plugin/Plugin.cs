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
    public class MyPointer : IAbsolutePointer, IMouseButtonHandler, IPressureHandler, ITiltHandler, IEraserHandler, IHoverDistanceHandler
    {
        private TabletReference? _tablet;
        private IVirtualScreen? _screen;

        public MyPointer(TabletReference tablet, IVirtualScreen screen)
        {
            _tablet = tablet;
            _screen = screen;
            Log.Write("WM_POINTER", $"MyPointer.Ctor()", LogLevel.Info);
        }

        public void MouseDown(MouseButton button)
        {
            Log.Write("WM_POINTER", $"MyPointer.MouseDown()", LogLevel.Info);
        }

        public void MouseUp(MouseButton button)
        {
            Log.Write("WM_POINTER", $"MyPointer.MouseUp()", LogLevel.Info);
        }

        public void SetPressure(float percentage)
        {
            Log.Write("WM_POINTER", $"MyPointer.SetPressure({percentage})", LogLevel.Info);
        }
        
        public void SetTilt(Vector2 tilt)
        {
            Log.Write("WM_POINTER", $"MyPointer.SetTilt({{{tilt.X}, {tilt.Y}}})", LogLevel.Info);
        }

        public void SetEraser(bool isEraser)
        {
            Log.Write("WM_POINTER", $"MyPointer.SetEraser({isEraser})", LogLevel.Info);
        }

        public void SetHoverDistance(uint distance)
        {
            Log.Write("WM_POINTER", $"MyPointer.SetHoverDistance({distance})", LogLevel.Info);
        }

        public void SetPosition(Vector2 pos)
        {
            Log.Write("WM_POINTER", $"MyPointer.SetPosition({{{pos.X}, {pos.Y}}})", LogLevel.Info);
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

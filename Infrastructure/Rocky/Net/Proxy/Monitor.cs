﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Rocky.Net
{
    public class Monitor : MarshalByRefObject
    {
        #region Fields
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001; //一个扩展键
        private const uint KEYEVENTF_KEYUP = 0x0002; //模拟松开一个键
        private const uint INPUT_MOUSE = 0;      //模拟鼠标事件
        private const uint INPUT_KEYBOARD = 1;      //模拟键盘事件

        private static byte[] PreviousBitmapBytes;
        #endregion

        #region Win32API方法包装
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt
        (
            IntPtr hdcDest, //指向目标设备环境的句柄
            int nXDest, //指定目标矩形区域克上角的X轴逻辑坐标
            int nYDest, //指定目标矩形区域左上角的Y轴逻辑坐标
            int nWidth, //指定源和目标矩形区域的逻辑宽度
            int nHeight, //指定源和目标矩形区域的逻辑高度
            IntPtr hdcSrc, //指向源设备环境句柄
            int nXSrc, //指定源矩形区域左上角的X轴逻辑坐标
            int nYSrc, //指定源矩形区域左上角的Y轴逻辑坐标
            Int32 dwRop //指定光栅操作代码。这些代码将定义源矩形区域的颜色数据，如何与目标矩形区域的颜色数据组合以完成最后的颜色
        );

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, ref INPUT input, int cbSize);

        [DllImport("user32.dll")]
        private static extern void mouse_event(MouseEventFlag flags, int dx, int dy, uint data, UIntPtr extraInfo);
        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int nVirtKey);
        #endregion

        #region Win32结构包装
        [StructLayout(LayoutKind.Explicit)]
        private struct INPUT
        {
            [FieldOffset(0)]
            public uint type;

            [FieldOffset(4)]
            public MOUSE_INPUT mi;

            [FieldOffset(4)]
            public KEYBD_INPUT ki;
        }

        private struct MOUSE_INPUT
        {
            public uint dx;
            public uint dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public uint dwExtraInfo;
        }

        private struct KEYBD_INPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public uint dwExtraInfo;
        }

        [Flags]
        private enum MouseEventFlag : uint
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            VirtualDesk = 0x4000,
            /// <summary>
            /// 鼠标坐标系统中的一个绝对位置
            /// </summary>
            Absolute = 0x8000
        }
        #endregion

        #region 获得屏幕
        public byte[] GetDesktopBitmapBytes()
        {
            var stream = new MemoryStream();
            Bitmap currentBitmap = GetDesktopBitmap();
            currentBitmap.Save(stream, ImageFormat.Jpeg);//将图片写入流
            currentBitmap.Dispose();
            stream.Seek(0, SeekOrigin.Begin);
            byte[] currentBitmapBytes = stream.ToArray();
            byte[] result = new byte[0];
            if (PreviousBitmapBytes == null || !currentBitmapBytes.SequenceEqual(PreviousBitmapBytes))
            {
                PreviousBitmapBytes = currentBitmapBytes;
                result = currentBitmapBytes;
            }
            return result;
        }

        private Bitmap GetDesktopBitmap()
        {
            Size desktopSize = this.GetDesktopSize();

            //Graphics graphic = Graphics.FromHwnd(GetDesktopWindow());//从窗口的指定句柄创建新的 Graphics 对象
            //Bitmap bmp = new Bitmap(desktopSize.Width, desktopSize.Height, graphic);//生成图像
            //using (Graphics bmpGraph = Graphics.FromImage(bmp))
            //{
            //    IntPtr dc1 = graphic.GetHdc();//获取与此 Graphics 对象关联的设备上下文的句柄
            //    IntPtr dc2 = bmpGraph.GetHdc();
            //    BitBlt(dc2, 0, 0, desktopSize.Width, desktopSize.Height, dc1, 0, 0, 0xCC0020);
            //    graphic.ReleaseHdc(dc1);//释放通过以前对此 Graphics 对象的 GetHdc 方法的调用获得的设备上下文句柄
            //    bmpGraph.ReleaseHdc(dc2);
            //}
            //graphic.Dispose();
            //return bmp;

            Bitmap bmp = new Bitmap(desktopSize.Width, desktopSize.Height);
            using (Graphics bmpGraph = Graphics.FromImage(bmp))
            {
                bmpGraph.CopyFromScreen(0, 0, 0, 0, desktopSize);
            }
            return bmp;
        }

        public Size GetDesktopSize()
        {
            //Size size = Screen.PrimaryScreen.Bounds.Size;
            return new Size(GetSystemMetrics(0), GetSystemMetrics(1));
        }
        #endregion

        #region 模拟鼠标、键盘操作
        public void MoveMouse(int x, int y)
        {
            SetCursorPos(x, y);
        }

        public static void ClickMouse()
        {
            mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
            mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
        }

        public static void DoubleClickMouse()
        {
            ClickMouse();
            Thread.Sleep(50);
            ClickMouse();
        }

        public static void RightClickMouse()
        {
            mouse_event(MouseEventFlag.RightDown, 0, 0, 0, UIntPtr.Zero);
            mouse_event(MouseEventFlag.RightUp, 0, 0, 0, UIntPtr.Zero);
        }

        /// <summary>
        /// 拖动鼠标到新位置（相对值）
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public static void MouseDrag(int dx, int dy)
        {
            mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
            mouse_event(MouseEventFlag.Move, dx, dy, 0, UIntPtr.Zero);
            mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
        }

        /// <summary>
        /// 从当前鼠标位置拖动到新位置（绝对值）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void MouseDragTo(int x, int y)
        {
            mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
            SetCursorPos(x, y);
            mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
        }

        /// <summary>
        /// 从某点拖动鼠标到新的地点（绝对值）
        /// </summary>
        /// <param name="beginX"></param>
        /// <param name="beginY"></param>
        /// <param name="endX"></param>
        /// <param name="endY"></param>
        public static void MouseDrag(int beginX, int beginY, int endX, int endY)
        {
            SetCursorPos(beginX, beginY);
            MouseDragTo(endX, endY);
        }

        public void PressOrReleaseMouse(bool press, bool left, int x, int y)
        {
            INPUT input = new INPUT();
            input.type = INPUT_MOUSE;
            input.mi.dx = (uint)x;
            input.mi.dy = (uint)y;
            input.mi.mouseData = 0;
            input.mi.dwFlags = 0;
            input.mi.time = 0;
            input.mi.dwExtraInfo = 0;

            if (left)
            {
                input.mi.dwFlags = press ? (uint)MouseEventFlag.LeftDown : (uint)MouseEventFlag.LeftUp;
            }
            else
            {
                input.mi.dwFlags = press ? (uint)MouseEventFlag.RightDown : (uint)MouseEventFlag.RightUp;
            }

            SendInput(1, ref input, Marshal.SizeOf(input));
        }

        /// <summary>
        /// 发送键盘事件
        /// </summary>
        /// <param name="virtualKeyCode"></param>
        /// <param name="scanCode"></param>
        /// <param name="keyDown"></param>
        /// <param name="extendedKey"></param>
        public void SendKeystroke(byte virtualKeyCode, byte scanCode, bool keyDown, bool extendedKey)
        {
            INPUT input = new INPUT();
            input.type = INPUT_KEYBOARD;
            input.ki.wVk = virtualKeyCode;
            input.ki.wScan = scanCode;
            input.ki.dwExtraInfo = 0;
            input.ki.time = 0;

            if (!keyDown)
            {
                input.ki.dwFlags |= KEYEVENTF_KEYUP;
            }
            if (extendedKey)
            {
                input.ki.dwFlags |= KEYEVENTF_EXTENDEDKEY;
            }

            SendInput(1, ref input, Marshal.SizeOf(input));
        }

        public static bool IsLButtonDown()
        {
            return GetKeyState(1) == 1;
        }
        public static bool IsMButtonDown()
        {
            return GetKeyState(4) == 1;
        }
        public static bool IsRButtonDown()
        {
            return GetKeyState(2) == 1;
        }
        public static bool IsLButtonAndRButtonDown()
        {
            return IsLButtonDown() && IsRButtonDown();
        }
        public static bool IsKeyDown(int key)
        {
            return GetKeyState(key) == 1;
        }
        #endregion
    }
}
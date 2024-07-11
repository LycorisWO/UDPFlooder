using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace UDPFlooder;
public class HotKey : IDisposable
{
    private static int hotKeyIdCounter = 0;
    private int hotKeyId;
    private Action<HotKey> action;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public HotKey(Key key, KeyModifier keyModifiers, Action<HotKey> action)
    {
        this.hotKeyId = hotKeyIdCounter++;
        this.action = action;

        IntPtr hWnd = new WindowInteropHelper(Application.Current.MainWindow).Handle;
        if (!RegisterHotKey(hWnd, hotKeyId, (uint)keyModifiers, (uint)KeyInterop.VirtualKeyFromKey(key)))
        {
            throw new InvalidOperationException("Hotkey registration failed.");
        }

        ComponentDispatcher.ThreadFilterMessage += ComponentDispatcher_ThreadFilterMessage;
    }

    private void ComponentDispatcher_ThreadFilterMessage(ref MSG msg, ref bool handled)
    {
        if (handled) return;

        if (msg.message != 0x0312) return;

        if ((int)msg.wParam != hotKeyId) return;

        action?.Invoke(this);
        handled = true;
    }

    public void Dispose()
    {
        IntPtr hWnd = new WindowInteropHelper(Application.Current.MainWindow).Handle;
        UnregisterHotKey(hWnd, hotKeyId);
    }
}

[Flags]
public enum KeyModifier
{
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Windows = 8
}

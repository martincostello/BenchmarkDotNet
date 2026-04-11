using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace BenchmarkDotNet.Helpers
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal static class MediaPlayerHelper
    {
        private const int HWND_BROADCAST = 0xFFFF;
        private const int WM_APPCOMMAND = 0x0319;
        private const int APPCOMMAND_MEDIA_PLAY = 46;
        private const int APPCOMMAND_MEDIA_PAUSE = 47;

        internal static bool IsMediaPlaying()
        {
            try
            {
                var deviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                int hr = deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out var device);
                if (hr != 0 || device == null)
                    return false;

                var audioSessionManager2Guid = AudioSessionManager2Guid;
                hr = device.Activate(ref audioSessionManager2Guid, CLSCTX_ALL, IntPtr.Zero, out var sessionManagerObj);
                if (hr != 0 || sessionManagerObj == null)
                    return false;

                var sessionManager = (IAudioSessionManager2)sessionManagerObj;
                hr = sessionManager.GetSessionEnumerator(out var sessionEnumerator);
                if (hr != 0 || sessionEnumerator == null)
                    return false;

                hr = sessionEnumerator.GetCount(out int count);
                if (hr != 0)
                    return false;

                for (int i = 0; i < count; i++)
                {
                    hr = sessionEnumerator.GetSession(i, out var session);
                    if (hr != 0 || session == null)
                        continue;

                    hr = session.GetState(out var state);
                    if (hr == 0 && state == AudioSessionState.AudioSessionStateActive)
                        return true;
                }
            }
            catch
            {
                // Ignore any errors - if we can't detect media state, assume nothing is playing
            }

            return false;
        }

        internal static void PauseMedia()
        {
            PostMessage((IntPtr)HWND_BROADCAST, WM_APPCOMMAND, IntPtr.Zero, (IntPtr)(APPCOMMAND_MEDIA_PAUSE << 16));
        }

        internal static void ResumeMedia()
        {
            PostMessage((IntPtr)HWND_BROADCAST, WM_APPCOMMAND, IntPtr.Zero, (IntPtr)(APPCOMMAND_MEDIA_PLAY << 16));
        }

        [DllImport("user32.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private static readonly Guid AudioSessionManager2Guid = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");

        private const uint CLSCTX_ALL = 0x17;

        private enum EDataFlow
        {
            eRender = 0,
            eCapture = 1,
            eAll = 2
        }

        private enum ERole
        {
            eConsole = 0,
            eMultimedia = 1,
            eCommunications = 2
        }

        private enum AudioSessionState
        {
            AudioSessionStateInactive = 0,
            AudioSessionStateActive = 1,
            AudioSessionStateExpired = 2
        }

        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class MMDeviceEnumerator { }

        [ComImport]
        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            int EnumAudioEndpoints(EDataFlow dataFlow, uint dwStateMask, out IntPtr ppDevices);

            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppEndpoint);

            int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string pwstrId, out IMMDevice ppDevice);

            int RegisterEndpointNotificationCallback(IntPtr pClient);

            int UnregisterEndpointNotificationCallback(IntPtr pClient);
        }

        [ComImport]
        [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            int Activate(ref Guid iid, uint dwClsCtx, IntPtr pActivationParams,
                [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

            int OpenPropertyStore(uint stgmAccess, out IntPtr ppProperties);

            int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);

            int GetState(out uint pdwState);
        }

        [ComImport]
        [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionManager2
        {
            int GetAudioSessionControl(ref Guid AudioSessionGuid, uint StreamFlags,
                out IAudioSessionControl SessionControl);

            int GetSimpleAudioVolume(ref Guid AudioSessionGuid, uint StreamFlags,
                out IntPtr AudioVolume);

            int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);

            int RegisterSessionNotification(IntPtr SessionNotification);

            int UnregisterSessionNotification(IntPtr SessionNotification);

            int RegisterDuckNotification([MarshalAs(UnmanagedType.LPWStr)] string sessionID,
                IntPtr duckNotification);

            int UnregisterDuckNotification(IntPtr duckNotification);
        }

        [ComImport]
        [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionEnumerator
        {
            int GetCount(out int SessionCount);

            int GetSession(int SessionCount, out IAudioSessionControl Session);
        }

        [ComImport]
        [Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionControl
        {
            int GetState(out AudioSessionState pRetVal);

            int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

            int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string Value, ref Guid EventContext);

            int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

            int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, ref Guid EventContext);

            int GetGroupingParam(out Guid pRetVal);

            int SetGroupingParam(ref Guid Override, ref Guid EventContext);

            int RegisterAudioSessionNotification(IntPtr NewNotifications);

            int UnregisterAudioSessionNotification(IntPtr NewNotifications);
        }
    }
}

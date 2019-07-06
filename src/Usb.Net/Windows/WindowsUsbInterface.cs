﻿using Device.Net;
using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Threading.Tasks;

namespace Usb.Net.Windows
{
    public class WindowsUsbInterface : UsbInterfaceBase, IDisposable, IUsbInterface
    {
        #region Private Properties
        private  bool _IsDisposed;
        /// <summary>
        /// TODO: Make private?
        /// </summary>
        internal SafeFileHandle Handle { get; set; }
        #endregion

        #region Constructor
        public WindowsUsbInterface(ILogger logger, ITracer tracer) : base(logger, tracer)
        {
        }
        #endregion


        #region Public Methods
        public async Task<byte[]> ReadAsync(uint bufferLength)
        {
            return await Task.Run(() =>
            {
                var bytes = new byte[bufferLength];
                var isSuccess = WinUsbApiCalls.WinUsb_ReadPipe(Handle, ReadEndpoint.PipeId, bytes, bufferLength, out var bytesRead, IntPtr.Zero);
                WindowsDeviceBase.HandleError(isSuccess, "Couldn't read data");
                Tracer?.Trace(false, bytes);
                return bytes;
            });
        }

        public async Task WriteAsync(byte[] data)
        {
            await Task.Run(() =>
            {
                var isSuccess = WinUsbApiCalls.WinUsb_WritePipe(Handle, WriteEndpoint.PipeId, data, (uint)data.Length, out var bytesWritten, IntPtr.Zero);
                WindowsDeviceBase.HandleError(isSuccess, "Couldn't write data");
                Tracer?.Trace(true, data);
            });
        }

        public void Dispose()
        {
            if (_IsDisposed) return;
            _IsDisposed = true;

            //This is a native resource, so the IDisposable pattern should probably be implemented...
            var isSuccess = WinUsbApiCalls.WinUsb_Free(Handle);
            WindowsDeviceBase.HandleError(isSuccess, "Interface could not be disposed");

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

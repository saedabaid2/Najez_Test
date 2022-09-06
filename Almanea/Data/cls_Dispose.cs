using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Almanea.Data
{
    /// <summary>
    /// <see cref="IDisposable" /> interface example.
    /// </summary>
    public class cls_Dispose : IDisposable
    {
        /// <summary>
        /// Flag stating if the current instance is already disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The <see cref="IDisposable" /> implementation.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        /// <summary>
        /// Dispose method, releasing all managed resources.
        /// </summary>
        /// <param name="disposing">States if the resources should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose all managed resources here.
            }

            _disposed = true;
        }
    }
}
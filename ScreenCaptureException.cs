using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeteBrown.ScreenCapture
{
    class ScreenCaptureException : Exception
    {
        public ScreenCaptureException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public ScreenCaptureException(string message)
            : base(message)
        { }
    
    }
}

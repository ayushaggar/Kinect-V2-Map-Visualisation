using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace PeteBrown.ScreenCapture.Win32Api
{
    class DwmApi
    {
        // http://msdn.microsoft.com/en-us/library/aa969530(VS.85).aspx
        // http://msdn.microsoft.com/en-us/library/aa969515(VS.85).aspx
        // http://msdn.microsoft.com/en-us/library/aa970874.aspx (for signature)
        [DllImport("DwmApi.dll")]
        public static extern int DwmGetWindowAttribute(
            IntPtr hwnd,
            uint dwAttributeToGet, //DWMWA_* values
            IntPtr pvAttributeValue,
            uint cbAttribute);

        public const int DWMNCRP_USEWINDOWSTYLE = 0;           // Enable/disable non-client rendering based on window style
        public const int DWMNCRP_DISABLED = 1;                 // Disabled non-client rendering; window style is ignored
        public const int DWMNCRP_ENABLED = 2;                  // Enabled non-client rendering; window style is ignored

        public const int DWMWA_NCRENDERING_ENABLED = 1;        // Enable/disable non-client rendering Use DWMNCRP_* values
        public const int DWMWA_NCRENDERING_POLICY = 2;         // Non-client rendering policy
        public const int DWMWA_TRANSITIONS_FORCEDISABLED = 3;  // Potentially enable/forcibly disable transitions 0 or 1

    }
}

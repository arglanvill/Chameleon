using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Chameleon.Emulator.Core.Video;
using Chameleon.Host.Display;
using Chameleon.Emulator.Systems.Computers;
using System.Data;
using System.Reflection;
using System.IO;
using Chameleon.Host.IO;
using Chameleon.Emulator.Core.IO;
using Chameleon.Emulator.Core;

namespace Chameleon.Host
{
    public class Host
    {
        public Host()
        {
            Resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            Display = new Win2DDisplay();
        }

        public IStream GetResourceStream(string resource)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            if (stream == null)
                return null;
            return new StreamWrapper(stream);
        }

        public void UpdateFrame()
        {
            if (EmulatedSystem != null)
            {
                if (EmulatedSystem.Debugger != null)
                    EmulatedSystem.Debugger.UpdateFrame();
                else
                    EmulatedSystem.UpdateFrame();
            }
        }

        public void DrawFrame(params Object[] drawParams)
        {
            if (EmulatedSystem != null)
                EmulatedSystem.DrawFrame(drawParams);
        }

        public IDisplay GetDisplay()
        {
            return Display;
        }

        public ISystem EmulatedSystem;
        Win2DDisplay Display;
        string[] Resources;
    }
}

using System;
using System.ComponentModel.Composition;
using MemoryVisualizer;
using MemoryVisualizer.UI;
using RTCV.Common;
using RTCV.NetCore;
using RTCV.PluginHost;
using RTCV.UI;

namespace MemoryVisualizer
{
    [Export(typeof(IPlugin))]
    public class PluginCore : IPlugin
    {
        public static RTCSide CurrentSide = RTCSide.Both;
        //public static PluginForm PluginForm = (PluginForm)null;
        internal static MemVisConnectorEMU ConnectorEmu;
        internal static MemVisConnectorRTC ConnectorRtc;

        public string Name => "Memory Visualizer";
        public string Description => "Allows you to view arbitrary memory as an image";

        public string Author => "NullShock78";

        public Version Version => Ver;
        public static Version Ver => new Version(1, 2, 0);

        public RTCSide SupportedSide => RTCSide.Both;

        public void Dispose()
        {
        }

        public bool Start(RTCSide side)
        {
            Logging.GlobalLogger.Info(string.Format("{0} v{1} initializing.", (object)this.Name, (object)this.Version));
            if (side == RTCSide.Client)
            {
                ConnectorEmu = new MemVisConnectorEMU();
                //S.SET<PluginForm>(new PluginForm());
            }
            else if (side == RTCSide.Server)
            {
                if (S.ISNULL<OpenToolsForm>())
                {
                    Logging.GlobalLogger.Error(
                        $"{(object)this.Name} v{(object)this.Version} failed to start: Singleton RTC_OpenTools_Form was null.");
                    return false;
                }
                if (S.ISNULL<CoreForm>())
                {
                    Logging.GlobalLogger.Error(
                        $"{(object)this.Name} v{(object)this.Version} failed to start: Singleton UI_CoreForm was null.");
                    return false;
                }
                S.GET<OpenToolsForm>().RegisterTool("Memory Visualizer", "Open Memory Visualizer", () => { LocalNetCoreRouter.Route(Ep.EMU_SIDE, Commands.SHOW_WINDOW, true); });
            }
            Logging.GlobalLogger.Info($"{(object)this.Name} v{(object)this.Version} initialized.");
            CurrentSide = side;
            return true;
        }

        public bool Stop()
        {
            if (CurrentSide == RTCSide.Client && !S.ISNULL<PluginForm>() && !S.GET<PluginForm>().IsDisposed)
            {
                S.GET<PluginForm>().HideOnClose = false;
                S.GET<PluginForm>().Close();
            }
            return true;
        }

        public bool StopPlugin()
        {
            if (!S.ISNULL<PluginForm>() && !S.GET<PluginForm>().IsDisposed)
            {
                S.GET<PluginForm>().HideOnClose = false;
                S.GET<PluginForm>().Close();
            }

            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class RunningContextBridge
        : EmulateClasses.IRunningContext
    {
        public bool isTopLevel { get; set; }

        readonly ILogger logger;

        public RunningContextBridge(
            ILogger logger
        )
        {
            this.logger = logger;
            Reset();
        }

        public void Reset()
        {
            isTopLevel = false;
        }

        public bool CheckTopLevel(string method)
        {
            if (isTopLevel) logger.Warning(String.Format("{0}はトップレベルに記述しないでください。", method));
            return isTopLevel;
        }
    }
}

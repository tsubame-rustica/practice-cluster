using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Preview
{
    public class DebugLogFactory
        : Engine.ILoggerFactory
    {
        readonly EmulatorOptions options;

        public DebugLogFactory(
            EmulatorOptions options
        )
        {
            this.options = options;
        }

        public ILogger Create(UnityEngine.GameObject gameObject, IProgramStatus programStatus)
        {
            return new DebugLogger(
                gameObject,
                programStatus,
                options               
            );
        }
    }
}

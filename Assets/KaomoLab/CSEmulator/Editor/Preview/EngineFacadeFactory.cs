using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Preview
{
    public class EngineFacadeFactory
    {
        readonly OptionBridge options;
        readonly CSEmulatorOscServer oscServer;

        public EngineFacadeFactory(
            OptionBridge options,
            CSEmulatorOscServer oscServer
        )
        {
            this.options = options;
            this.oscServer = oscServer;
        }

        public EngineFacade CreateDefault(
        )
        {
            return new EngineFacade(
                options,
                oscServer,
                ClusterVR.CreatorKit.Editor.Preview.Bootstrap.ItemCreator,
                ClusterVR.CreatorKit.Editor.Preview.Bootstrap.ItemDestroyer,
                ClusterVR.CreatorKit.Editor.Preview.Bootstrap.SpawnPointManager,
                ClusterVR.CreatorKit.Editor.Preview.Bootstrap.CommentScreenPresenter
            );
        }

    }
}

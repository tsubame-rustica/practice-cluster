using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator
{
    //EmulateClassesにあるのが良いと思っているけど、
    //あまり時間もなく、どうにも良い解釈が出来ず致し方なくここに置いている。
    //CSETODO V3の時に考えたい。
    public enum EventRole
    {
        Audience = 3,
        Guest = 2,
        Staff = 1
    }

    public enum PlayerDevice : int
    {
        Desktop, VR, Mobile
    }
    public enum PlayerOperatingSystem : int
    {
        Windows, macOS, iOS, Android
    }
}

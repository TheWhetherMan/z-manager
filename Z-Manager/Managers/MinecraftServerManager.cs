using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System;

namespace Z_Manager.Managers
{
    public class MinecraftServerManager
    {
        public static MinecraftServerManager _instance;
        public static MinecraftServerManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MinecraftServerManager();

                return _instance;
            }
        }

        private MinecraftServerManager() { }

        /// <summary> Check if there are running instance(s) of a Minecraft server </summary>
        public void CheckServerStatus()
        {

        }

        /// <summary> Start the Minecraft server </summary>
        public void StartServer()
        {

        }
    }
}

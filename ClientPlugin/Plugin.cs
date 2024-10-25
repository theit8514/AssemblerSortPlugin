using ClientPlugin.Logging;
using HarmonyLib;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using VRage.Plugins;

namespace ClientPlugin
{
    // ReSharper disable once UnusedType.Global
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Plugin : IPlugin
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public const string Name = "AssemblerSortPlugin";

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static Plugin Instance { get; private set; }

        public IPluginLogger Log => Logger;
        private static readonly IPluginLogger Logger = new PluginLogger(Name);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Init(object gameInstance)
        {
            Instance = this;

            Log.Info("Starting AssemblerSortPlugin");

            var harmony = new Harmony(Name);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void Dispose()
        {
            Instance = null;
        }

        public void Update()
        {
        }
    }
}
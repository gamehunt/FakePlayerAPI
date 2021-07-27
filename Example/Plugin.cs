using Exiled.API.Features;
using System;
using Handlers = Exiled.Events.Handlers;

namespace FakePlayers.Example
{
    public class Plugin : Plugin<Config>
    {
        public EventHandlers EventHandlers;

        public static Plugin Instance { get; private set; }
        public static HarmonyLib.Harmony Harmony { get; private set; }

        public override string Author { get; } = "gamehunt";
        public override string Name { get; } = "FakePlayers.Example";
        public override string Prefix { get; } = "FakePlayers.Example";
        public override Version Version { get; } = new Version(1, 0, 0);
        public override Version RequiredExiledVersion { get; } = new Version(2, 11, 1);

        public override void OnEnabled()
        {
            try
            {
                Instance = this;

                EventHandlers = new EventHandlers();

                Handlers.Server.RoundStarted += EventHandlers.OnRoundStart;

                Log.Info($"{Name} plugin loaded. @gamehunt");
            }
            catch (Exception e)
            {
                Log.Error($"There was an error loading the plugin: {e}");
            }
        }

        public override void OnDisabled()
        {
            Handlers.Server.RoundStarted -= EventHandlers.OnRoundStart;

            Instance = null;
            Harmony = null;
            EventHandlers = null;
        }

        public override void OnReloaded()
        {
        }
    }
}
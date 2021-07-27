using Exiled.API.Features;
using System;
using System.Reflection;
using Evs = Exiled.Events;
using Handlers = Exiled.Events.Handlers;

namespace FakePlayers.Runtime
{
    public class Plugin : Plugin<Config>
    {
        public EventHandlers EventHandlers;

        public static Plugin Instance { get; private set; }
        public static HarmonyLib.Harmony Harmony { get; private set; }

        public override string Author { get; } = "gamehunt";
        public override string Name { get; } = "FakePlayers.Runtime";
        public override string Prefix { get; } = "FakePlayers.Runtime";
        public override Version Version { get; } = new Version(1, 0, 0);
        public override Version RequiredExiledVersion { get; } = new Version(2, 11, 1);

        public override void OnEnabled()
        {
            try
            {
                Instance = this;

                foreach (MethodBase bas in Evs.Events.Instance.Harmony.GetPatchedMethods())
                {
                    if (bas.Name.Equals("TransmitData"))
                    {
                        Evs.Events.DisabledPatchesHashSet.Add(bas);
                    }
                    else if (bas.DeclaringType.Name.Equals("RoundSummary") && bas.Name.Equals("Start"))
                    {
                        Evs.Events.DisabledPatchesHashSet.Add(bas);
                    }
                    else if (bas.Name.Equals("PlayEntranceAnnouncement"))
                    {
                        Evs.Events.DisabledPatchesHashSet.Add(bas);
                    }
                }

                Evs.Events.Instance.ReloadDisabledPatches();

                Harmony = new HarmonyLib.Harmony("fakeplayersapi.instance");

                Harmony.PatchAll();

                EventHandlers = new EventHandlers();

                Handlers.Server.RoundStarted += EventHandlers.OnRoundStart;
                Handlers.Server.RoundEnded += EventHandlers.OnRoundEnd;
                Handlers.Server.WaitingForPlayers += EventHandlers.OnWaitingForPlayers;

                Handlers.Player.Died += EventHandlers.OnDeath;

                Log.Info($"{Name} plugin loaded. @gamehunt");
            }
            catch (Exception e)
            {
                Log.Error($"There was an error loading the plugin: {e}");
            }
        }

        public override void OnDisabled()
        {
            foreach (API.FakePlayer npc in API.FakePlayer.List)
            {
                npc.Kill();
            }

            Harmony.UnpatchAll();

            Handlers.Server.RoundStarted -= EventHandlers.OnRoundStart;
            Handlers.Server.RoundEnded -= EventHandlers.OnRoundEnd;
            Handlers.Server.WaitingForPlayers -= EventHandlers.OnWaitingForPlayers;

            Handlers.Player.Died -= EventHandlers.OnDeath;

            Instance = null;
            Harmony = null;
            EventHandlers = null;
        }

        public override void OnReloaded()
        {
        }
    }
}
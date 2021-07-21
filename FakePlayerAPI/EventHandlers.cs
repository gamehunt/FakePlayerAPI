using Exiled.API.Features;
using Exiled.Events.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FakePlayerAPI
{
    public class EventHandlers
    {

        public void OnWaitingForPlayers()
        {
            ServerConsole.singleton.NameFormatter.Commands["player_count"] = delegate (List<string> args)
            {
                return (PlayerManager.players.Count - FakePlayer.Dictionary.Count).ToString();
            };
            ServerConsole.singleton.NameFormatter.Commands["full_player_count"] = delegate (List<string> args)
            {
                int count = PlayerManager.players.Count - FakePlayer.Dictionary.Count;
                if (count != CustomNetworkManager.TypedSingleton.ReservedMaxPlayers)
                {
                    return string.Format("{0}/{1}", count, CustomNetworkManager.TypedSingleton.ReservedMaxPlayers);
                }
                int count2 = args.Count;
                if (count2 == 1)
                {
                    return "FULL";
                }
                if (count2 != 2)
                {
                    throw new ArgumentOutOfRangeException("args", args, "Invalid arguments. Use: full_player_count OR full_player_count,[full]");
                }
                return ServerConsole.singleton.NameFormatter.ProcessExpression(args[1]);
            };
        }

        public void OnRoundStart()
        {
#if true
            MEC.Timing.CallDelayed(0.5f, () =>
            {
                try
                {
                    FakePlayer.Create<ExampleFakePlayer>(Player.List.FirstOrDefault()?.Position ?? Vector3.zero, RoleType.Tutorial);
                }catch(Exception e)
                {
                    Log.Error($"Exception caught: {e}");
                }
            });
#endif       
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            foreach (FakePlayer npc in FakePlayer.List)
            {
                npc.Kill();
            }
        }

        public void OnDeath(DiedEventArgs ev)
        {
            if (ev.Target.IsFakePlayer())
            {
                ev.Target.AsFakePlayer().Kill();
            }
        }
    }
}
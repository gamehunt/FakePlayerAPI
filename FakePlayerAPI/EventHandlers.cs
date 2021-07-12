using Exiled.API.Features;
using Exiled.Events.EventArgs;
using System;
using System.Linq;
using UnityEngine;

namespace FakePlayerAPI
{
    public class EventHandlers
    {
        public void OnRoundStart()
        {
            MEC.Timing.CallDelayed(0.5f, () =>
            {
                try
                {
                    FakePlayer.Create<TestFakePlayer>(Player.List.FirstOrDefault()?.Position ?? Vector3.zero, RoleType.Tutorial);
                }catch(Exception e)
                {
                    Log.Error($"Exception caught: {e}");
                }
            });
            
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            foreach (FakePlayer npc in FakePlayer.List)
            {
                npc.Kill(false);
            }
        }
    }
}
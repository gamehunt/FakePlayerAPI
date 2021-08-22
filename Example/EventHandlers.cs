using Exiled.API.Features;
using FakePlayers.API;
using System.Linq;
using UnityEngine;
using MEC;
using System;

namespace FakePlayers.Example
{
        public class EventHandlers
        {
            public void OnRoundStart()
            {
                Timing.CallDelayed(0.5f, () =>
                {
                    try
                    {
                        //When processEvents = false, EXILED can throw NRE on some events, because Player.Get() returns null for this instance
                        //So use false processEvents flag when u are at least 90% sure thar ur instance dont need any events and wont trigger ones too often
                        FakePlayer.Create<ExampleFakePlayer>(Player.List.FirstOrDefault()?.Position ?? Vector3.zero, Vector3.one, RoleType.Tutorial, true);
                    }catch(Exception e)
                    {
                        Log.Error($"Error occured: {e}");
                    }
                });
            }
        }
}
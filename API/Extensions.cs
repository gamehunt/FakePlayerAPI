using Exiled.API.Features;
using UnityEngine;

namespace FakePlayers.Extensions
{
    public static class Extensions
    {
        public static bool IsFakePlayer(this GameObject p)
        {
            return API.FakePlayer.Dictionary.ContainsKey(p);
        }

        public static bool IsFakePlayer(this Player p)
        {
            return p.GameObject.IsFakePlayer();
        }

        public static API.FakePlayer AsFakePlayer(this Player p)
        {
            return p.IsFakePlayer() ? API.FakePlayer.Dictionary[p.GameObject] : null;
        }

        public static API.FakePlayer AsFakePlayer(this GameObject p)
        {
            return p.IsFakePlayer() ? API.FakePlayer.Dictionary[p] : null;
        }
    }
}
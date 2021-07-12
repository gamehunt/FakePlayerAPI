using Exiled.API.Features;
using UnityEngine;

namespace FakePlayerAPI
{
	public static class Extensions
	{
        public static bool IsFakePlayer(this GameObject p)
        {
            return FakePlayer.Dictionary.ContainsKey(p);
        }

        public static bool IsFakePlayer(this Player p)
        {
            return p.GameObject.IsFakePlayer();
        }

        public static FakePlayer AsFakePlayer(this Player p)
        {
            return p.IsFakePlayer() ? FakePlayer.Dictionary[p.GameObject] : null;
        }

        public static FakePlayer AsFakePlayer(this GameObject p)
        {
            return p.IsFakePlayer() ? FakePlayer.Dictionary[p] : null;
        }
    }
}
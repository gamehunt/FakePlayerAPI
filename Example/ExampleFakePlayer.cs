using Exiled.API.Features;
using System;
namespace FakePlayers.Example
{
    class ExampleFakePlayer : FakePlayers.API.FakePlayer
    {
        private static int __counter = 0;

        public override bool AffectEndConditions { get; set; } = false;
        public override bool DisplayInRA { get; set; } = true;

        public override string GetIdentifier()
        {
            return $"ExampleFakePlayer_{__counter}";
        }

        public override bool IsVisibleFor(Player ply)
        {
            return true;
        }

        public override void OnDestroying()
        {
            
        }

        public override void OnPostInitialization()
        {
            
        }

        public override void OnPreInitialization()
        {
            __counter++;
        }
    }
}

using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakePlayerAPI
{
    class TestFakePlayer : FakePlayer
    {

        public override bool DisplayInRA { get; set; } = true; 

        public override Plugin PluginInstance => Plugin.Instance;

        public override string GetIdentifier()
        {
            return "TestFakePlayer";
        }

        public override bool IsVisibleFor(Player ply)
        {
            return true;
        }
    }
}

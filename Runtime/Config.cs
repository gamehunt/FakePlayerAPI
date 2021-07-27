using Exiled.API.Interfaces;
using System.ComponentModel;

namespace FakePlayers.Runtime
{
        public class Config : IConfig
        {
            public bool IsEnabled { get; set; } = true;

            [Description("Enables debug output (Spams trash in console)")]
            public bool VerboseOutput { get; set; } = false;

            [Description("Assign it if fake players floats or under floor")]
            public float FakePlayerSizePositionMultiplier = 1.3f;
        }
}
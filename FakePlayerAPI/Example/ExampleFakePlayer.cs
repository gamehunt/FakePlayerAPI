using Exiled.API.Features;

namespace FakePlayerAPI
{
    class ExampleFakePlayer : FakePlayer
    {
        private static int __counter = 0;
        public override bool DisplayInRA { get; set; } = true; 

        public override Plugin PluginInstance => Plugin.Instance;

        public override string GetIdentifier()
        {
            return $"{PluginInstance.Name}_ExampleFakePlayer_{__counter}";
        }

        public override bool IsVisibleFor(Player ply)
        {
            return true;
        }

        public override void OnPostInitialization()
        {
            __counter++;
        }

        public override void OnPreInitialization()
        {
            
        }
    }
}

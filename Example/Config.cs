using Exiled.API.Interfaces;

namespace FakePlayers.Example
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
    }
}
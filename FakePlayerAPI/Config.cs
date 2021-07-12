using Exiled.API.Interfaces;
using System.ComponentModel;

namespace FakePlayerAPI
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        [Description("Enables debug output (Spams trash in console)")]
        public bool VerboseOutput { get; set; } = false;
    }
}
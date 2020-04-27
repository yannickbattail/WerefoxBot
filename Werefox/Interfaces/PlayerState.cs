using System.ComponentModel;

namespace Werefox.Interfaces
{
    public enum PlayerState
    {
        [Description("alive :star_struck:")]
        Alive,
        [Description("dead :skull:")]
        Dead,
        [Description("Schrödinger's cat :scream_cat:")]
        SchrödingersCat,
    }
}
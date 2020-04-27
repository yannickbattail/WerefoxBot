using System.ComponentModel;

namespace Werefox.Interfaces
{
    public enum GameStep
    {
        [Description("thief step :supervillain:")]
        ThiefStep,
        [Description("cupid/lover Step :angel:")]
        CupidStep,

        [Description("seer :crystal_ball:")]
        SeerStep,
        [Description("night :crescent_moon:")]
        Night,
        [Description("witch step :woman_mage:")]
        WitchStep,
        [Description("day :sunny:")]
        Day,
    }
}
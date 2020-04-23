using System.Collections.Generic;
using System.Linq;

namespace WerefoxBot.Model
{
    internal static class Utils {
        public static string CardToS(Card card)
        {
            return card switch
            {
                Card.Werefox => "werefox :fox:",
                Card.VillagePeople => "village people :man_farmer:",
                Card.LittleGirl => "littleGirl :girl:",
                Card.Seer => "seer :crystal_ball:",
                Card.Thief => "thief :supervillain:",
                Card.Hunter => "hunter :gun:",
                Card.Cupid => "Cupid :angel:",
                Card.Witch => "witch :woman_mage:",
                _ => ":x: UNKNOWN Card"
            };
        }

        public static string AliveToS(PlayerState playerState)
        {
            return playerState switch
            {
                PlayerState.Alive => "alive :star_struck:",
                PlayerState.Dead => "dead :skull:",
                PlayerState.SchrödingersCat => "Schrödinger's cat :scream_cat:",
                _ => ":x: UNKNOWN PlayerStep"
            };
        }

        public static string StepToS(GameStep step)
        {
            return step switch
            {
                GameStep.ThiefStep => "thief step :supervillain:",
                GameStep.CupidStep => "cupid/lover Step :angel:",
                
                GameStep.SeerStep => "seer :crystal_ball:",
                GameStep.Night => "night :crescent_moon:",
                GameStep.WitchStep => "witch step :woman_mage:",
                GameStep.Day => "day :sunny:",
                _ => ":x: UNKNOWN GameStep"
            };
        }
        
        public static string DisplayPlayerList(IEnumerable<Player> players)
        {
            return string.Join(", ", players.Select(p => p.User.Mention));
        }
    }
}
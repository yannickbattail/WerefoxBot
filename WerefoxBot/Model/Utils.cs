using System.Collections.Generic;
using System.Linq;

namespace WerefoxBot.Model
{
    internal class Utils {
        public static string CardToS(Card card)
        {
            switch (card)
            {
                case Card.Werefox:
                    return "werefox :werefox:";
                case Card.VillagePeople:
                    return "village people :man_farmer:";
                case Card.LittleGirl:
                    return "LittleGirl :girl:";
                default:
                    return ":x: UNKNOWN Card";
            }
        }

        public static string AliveToS(PlayerState playerState)
        {
            switch (playerState)
            {
                case PlayerState.Alive:
                    return "alive :star_struck:";
                case PlayerState.Dead:
                    return "dead :skull:";
                default:
                    return ":x: UNKNOWN PlayerStep";
            }
        }

        public static string StepToS(GameStep step)
        {
            switch (step)
            {
                case GameStep.Day:
                    return "day :sunny:";
                case GameStep.Night:
                    return "night :crescent_moon:";
                default:
                    return ":x: UNKNOWN GameStep";
            }
        }
        
        public static string DisplayPlayerList(IEnumerable<Player> players)
        {
            return string.Join(", ", players.Select(p => p.User.Mention));
        }
    }
}
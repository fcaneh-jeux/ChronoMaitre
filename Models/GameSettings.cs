using System;
using System.Collections.Generic;
using System.Text;

namespace HurryUpDavid.Models
{
    public class GameSettings
    {
        public int PlayerCount { get; set; }
        public int TurnDuration { get; set; }
        public string SoundTheme { get; set; } = "Aucune";
        public int BankDuration { get; set; }
        public GameMode GameMode { get; set; }

    }
}

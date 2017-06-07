using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EarnYourStripes
{
    class StripeSettings : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "EarnYourStripes Options"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "EarnYourStripes"; } }
        public override string DisplaySection { get { return Section; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }
        public bool autoPersistance = true;
        public bool newGameOnly = false;
        [GameParameters.CustomIntParameterUI("Number of Flights", toolTip = "How many flights must a Kerbal log to be considered a veteran?")]
        public int numberOfFlightsRequired = 5;
        [GameParameters.CustomIntParameterUI("Flight Hours", toolTip = "How many hours must a Kerbal have logged to be considered a veteran?")]
        public int flightHoursRequired = 12;
        [GameParameters.CustomParameterUI("Remove Existing Honours?", toolTip = "Remove veteran status from kerbals who haven't earned it? (The Big 4)")]
        public bool removeExistingHonours = false;
    }
}

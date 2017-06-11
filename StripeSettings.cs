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
        [GameParameters.CustomParameterUI("Do Something Amazing?", toolTip = "Does the kerbal have to do perform a 'World First'?")]
        public bool worldFirsts = false;
    }
    class StripeSettingsClassRestrictions : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "Class Restrictions"; } }
        public override string Section { get { return "EarnYourStripes"; } }
        public override string DisplaySection { get { return Section; } }
        public override int SectionOrder { get { return 2; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override bool HasPresets { get { return false; } }
        [GameParameters.CustomParameterUI("Remove Existing Honours?", toolTip = "Remove Honours from the Big 4 if they haven't earned it")]
        public bool removeExistingHonours = false;
        [GameParameters.CustomParameterUI("Allow Pilot Veterans")]
        public bool pilotsAllowed = true;
        [GameParameters.CustomParameterUI("Allow Scientist Veterans")]
        public bool scientistsAllowed = true;
        [GameParameters.CustomParameterUI("Allow Engineer Veterans")]
        public bool engineersAllowed = true;
    }
}

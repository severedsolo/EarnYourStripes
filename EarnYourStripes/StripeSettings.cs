using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EarnYourStripes
{
    internal class StripeSettings : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "EarnYourStripes Options"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "EarnYourStripes"; } }
        public override string DisplaySection { get { return Section; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }
        public bool AutoPersistance = true;
        public bool NewGameOnly = false;
        [GameParameters.CustomParameterUI("Enable Debug Mode")]
        public bool Debug = false;
        [GameParameters.CustomIntParameterUI("Number of Flights", toolTip = "How many flights must a Kerbal log to be considered a veteran?")]
        public int NumberOfFlightsRequired = 5;
        [GameParameters.CustomIntParameterUI("Flight Hours", toolTip = "How many hours must a Kerbal have logged to be considered a veteran?", minValue = 0, maxValue = 10000)]
        public int FlightHoursRequired = 12;
        [GameParameters.CustomParameterUI("Do Something Amazing?", toolTip = "Does the kerbal have to do perform a 'World First'?")]
        public bool WorldFirsts = false;
        [GameParameters.CustomParameterUI("Replace Starting Crew", toolTip = "Replace the 'Big 4' with a different starting crew?")]
        public bool GenerateCrew = false;
        [GameParameters.CustomParameterUI("Use Vintage Suits by default (requires Making History)?")]
        public bool BasicSuit = false;
        [GameParameters.CustomParameterUI("Give Veterans their own suits (requires Breaking Ground)?")]
        public bool BgSuits = true;
    }

    internal class StripeSettingsClassRestrictions : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "Class Restrictions"; } }
        public override string Section { get { return "EarnYourStripes"; } }
        public override string DisplaySection { get { return Section; } }
        public override int SectionOrder { get { return 2; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override bool HasPresets { get { return false; } }
        [GameParameters.CustomParameterUI("Remove Existing Honours?", toolTip = "Remove Honours from the Big 4 if they haven't earned it")]
        public bool RemoveExistingHonours = false;
        [GameParameters.CustomParameterUI("Allow Pilot Veterans")]
        public bool PilotsAllowed = true;
        [GameParameters.CustomParameterUI("Allow Scientist Veterans")]
        public bool ScientistsAllowed = true;
        [GameParameters.CustomParameterUI("Allow Engineer Veterans")]
        public bool EngineersAllowed = true;
    }
}

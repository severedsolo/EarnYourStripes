using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ConvertToConstant.Global

// ReSharper disable ArrangeAccessorOwnerBody

namespace EarnYourStripes
{
    internal class StripeSettings : GameParameters.CustomParameterNode
    {
        public override string Title
        {
            get { return "EarnYourStripes Options"; }
        }

        public override GameParameters.GameMode GameMode
        {
            get { return GameParameters.GameMode.ANY; }
        }

        public override string Section
        {
            get { return "EarnYourStripes"; }
        }

        public override string DisplaySection
        {
            get { return Section; }
        }

        public override int SectionOrder
        {
            get { return 1; }
        }

        public override bool HasPresets
        {
            get { return false; }
        }
        [UsedImplicitly]
        public bool AutoPersistance = true;
        [UsedImplicitly]
        // ReSharper disable once RedundantDefaultMemberInitializer
        public bool NewGameOnly = false;
        [GameParameters.CustomParameterUI("Enable Debug Mode")]
#if DEBUG
        [UsedImplicitly]
        public bool Debug = true;
#endif
#if !DEBUG
        public bool Debug = false;
#endif
        [GameParameters.CustomIntParameterUI("Number of Flights", toolTip = "How many flights must a Kerbal log to be considered a veteran?")]
        public int NumberOfFlightsRequired = 5;

        [GameParameters.CustomIntParameterUI("Flight Hours", toolTip = "How many hours must a Kerbal have logged to be considered a veteran?", minValue = 0, maxValue = 10000)]
        public int FlightHoursRequired = 12;

        [GameParameters.CustomIntParameterUI("Number of World Firsts", toolTip = "How many World Firsts must a kerbal accomplish before being promoted?")]
        public int WorldFirsts = 1;
        
        [GameParameters.CustomParameterUI("Replace Starting Crew", toolTip = "Replace the 'Big 4' with a different starting crew?")]
        public bool GenerateCrew = true;
        
        [GameParameters.CustomIntParameterUI("Default Suit", toolTip = "0 = Basic, 1 = Vintage, 2 = SciFi", minValue = 0, maxValue = 2, stepSize = 1)]
        public int DefaultSuit = 0;
        
        [GameParameters.CustomIntParameterUI("Veteran Suit", toolTip = "0 = Basic, 1 = Vintage, 2 = SciFi", minValue = 0, maxValue = 2, stepSize = 1)]
        public int VeteranSuit = 2;
    }

    [SuppressMessage("ReSharper", "ArrangeAccessorOwnerBody")]
    [UsedImplicitly]
    internal class StripeSettingsClassRestrictions : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "Class Restrictions"; } }
        public override string Section { get { return "EarnYourStripes"; } }
        public override string DisplaySection { get { return Section; } }
        public override int SectionOrder { get { return 2; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override bool HasPresets { get { return false; } }
        [GameParameters.CustomParameterUI("Strip Honours (Big 4)", toolTip = "Remove Honours from the Big 4 if they haven't earned it")]
        public bool RemoveExistingHonoursBig4 = true;
        [GameParameters.CustomParameterUI("Strip Honours (Other)", toolTip = "Remove Honours from any veterans who haven't earned it (rescue missions etc)")]
        public bool RemoveExistingHonoursOthers = true;
        [GameParameters.CustomParameterUI("Allow Pilot Veterans")]
        public bool PilotsAllowed = true;
        [GameParameters.CustomParameterUI("Allow Scientist Veterans")]
        public bool ScientistsAllowed = true;
        [GameParameters.CustomParameterUI("Allow Engineer Veterans")]
        public bool EngineersAllowed = true;
    }
}

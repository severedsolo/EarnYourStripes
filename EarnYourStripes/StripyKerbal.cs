using FlightTracker;

namespace EarnYourStripes
{
    public class StripyKerbal
    {
        public bool Promoted = false;
        public bool SuitSet = false;
        private string kerbalName;
        private string kerbalClass;

        public StripyKerbal(ProtoCrewMember p)
        {
            kerbalName = p.name;
            kerbalClass = p.trait;
        }


        public bool EligibleForPromotion()
        {
            if (ClassExcluded(kerbalClass)) return false;
            if (FlightTrackerApi.Instance.GetNumberOfFlights(kerbalName) < HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().NumberOfFlightsRequired) return false;
            if (FlightTrackerApi.Instance.GetRecordedMissionTimeHours(kerbalName) < HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().FlightHoursRequired) return false;
            if (FlightTrackerApi.Instance.GetNumberOfWorldFirsts(kerbalName) < HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().WorldFirsts) return false;
            return true;
        }

        private bool ClassExcluded(string trait)
        {
            switch (trait)
            {
                case "Pilot":
                    return !HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().PilotsAllowed;
                case "Scientist":
                    return !HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().ScientistsAllowed;
                case "Engineer":
                    return !HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().EngineersAllowed;
                default:
                    return false;
            }
        }

        public void OnSave(ConfigNode saveNode)
        {
            ConfigNode cn = new ConfigNode("STRIPYKERBAL");
            cn.SetValue("Name", kerbalName, true);
            cn.SetValue("SuitSet", SuitSet, true);
            cn.SetValue("Promoted", Promoted, true);
        }

        public void Promote(ProtoCrewMember p)
        {
            if (!Promoted) SuitSet = false;
            p.veteran = true;
            if(!SuitSet) p.suit = EarnYourStripes.Instance.GetSuit(p);
            Promoted = true;
        }

        public void OnLoad(ConfigNode cn)
        {
            cn.TryGetValue("SuitSet", ref SuitSet);
            cn.TryGetValue("Promoted", ref Promoted);
        }
    }
}
using FlightTracker;
using UnityEngine;

namespace EarnYourStripes
{
    public class StripyKerbal
    {
        public bool Promoted;
        public bool SuitSet;
        private readonly string kerbalName;
        private readonly string kerbalClass;

        public StripyKerbal(ProtoCrewMember p)
        {
            kerbalName = p.name;
            kerbalClass = p.trait;
        }


        public bool EligibleForPromotion()
        {
            if (Promoted) return true;
            Debug.Log("[EarnYourStripes]: Checking "+kerbalName+" eligibility for promotion");
            if (ClassExcluded(kerbalClass))
            {
                Debug.Log("[EarnYourStripes] "+kerbalClass+" not allowed to be a veteran. Promotion Denied");
                return false;
            }

            if (FlightTrackerApi.Instance.GetNumberOfFlights(kerbalName) < HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().NumberOfFlightsRequired)
            {
                Debug.Log("[EarnYourStripes]: "+kerbalName+" hasn't recorded enough flights ("+FlightTrackerApi.Instance.GetNumberOfFlights(kerbalName)+"/"+HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().NumberOfFlightsRequired+")");
                return false;
            }

            if (FlightTrackerApi.Instance.GetRecordedMissionTimeHours(kerbalName) < HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().FlightHoursRequired)
            {
                Debug.Log("[EarnYourStripes]: "+kerbalName+" hasn't recorded enough hours ("+FlightTrackerApi.Instance.GetRecordedMissionTimeHours(kerbalName)+"/"+HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().FlightHoursRequired+")");
                return false;
            }

            if (FlightTrackerApi.Instance.GetNumberOfWorldFirsts(kerbalName) < HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().WorldFirsts)
            {
                Debug.Log("[EarnYourStripes]: "+kerbalName+" hasn't recorded enough World Firsts ("+FlightTrackerApi.Instance.GetNumberOfWorldFirsts(kerbalName)+"/"+HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().WorldFirsts+")");
                return false;
            }
            Debug.Log("[EarnYourStripes]: Promoting "+kerbalName);
            return true;
        }

        private static bool ClassExcluded(string trait)
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
            saveNode.AddNode(cn);
        }

        public void Promote(ProtoCrewMember p)
        {
            if (!Promoted) SuitSet = false;
            p.veteran = true;
            if (!SuitSet)
            {
                p.suit = EarnYourStripes.Instance.GetSuit(p);
                SuitSet = true;
            }
            Promoted = true;
        }

        public void OnLoad(ConfigNode cn)
        {
            cn.TryGetValue("SuitSet", ref SuitSet);
            cn.TryGetValue("Promoted", ref Promoted);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EarnYourStripes
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class FlightTracker : MonoBehaviour
    {
        public static FlightTracker instance;
        public Dictionary<string, int> flights = new Dictionary<string, int>();
        public Dictionary<string, double> MET = new Dictionary<string, double>();
        public List<string> promotedKerbals = new List<string>();

        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(this);
            Debug.Log("[EarnYourStripes]: Flight Tracker is Awake");
        }
        private void Start()
        {
            GameEvents.onVesselRecovered.Add(onVesselRecovered);
            GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);
            Debug.Log("[EarnYourStripes]: Registered Event Handlers");
            StripHonours();
        }
        void StripHonours()
        {
            IEnumerable<ProtoCrewMember> crew = HighLogic.CurrentGame.CrewRoster.Crew;
            if (crew.Count() == 0) return;
            for (int i = 0; i < crew.Count(); i++)
            {
                string p = crew.ElementAt(i).name;
                if (!promotedKerbals.Contains(p) && crew.ElementAt(i).veteran && HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().removeExistingHonours)
                {
                    crew.ElementAt(i).veteran = false;
                    Debug.Log("[EarnYourStripes]: Removed " + p + "'s veteran status as they haven't earned it");
                }
                if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().pilotsAllowed && HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().scientistsAllowed && HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().engineersAllowed) continue;
                switch(crew.ElementAt(i).trait)
                {
                    case "Pilot":
                        if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().pilotsAllowed) crew.ElementAt(i).veteran = false;
                        Debug.Log("[EarnYourStripes]: Removed " + p + "'s veteran status due to settings");
                        break;
                    case "Scientist":
                        if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().scientistsAllowed) crew.ElementAt(i).veteran = false;
                        Debug.Log("[EarnYourStripes]: Removed " + p + "'s veteran status due to settings");
                        break;
                    case "Engineer":
                        if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().engineersAllowed) crew.ElementAt(i).veteran = false;
                        Debug.Log("[EarnYourStripes]: Removed " + p + "'s veteran status due to settings");
                        break;
                    default:
                        Debug.Log("[EarnYourStripes]: Attempted to remove " + p + "'s veteran status but couldn't figure out what class they were");
                        break;
                }
            }
        }
        private void OnGameSettingsApplied()
        {
            Debug.Log("[EarnYourStripes]: Game Settings Updated");
            StripHonours();
        }

        private void onVesselRecovered(ProtoVessel v, bool data1)
        {
            Debug.Log("[EarnYourStripes]: Vessel Recovery Attempt Detected");
            List<ProtoCrewMember> crew = v.GetVesselCrew();
            Debug.Log("[EarnYourStripes]: Processing " + crew.Count() + " Kerbals");
            if (crew.Count == 0) return;
            for (int i = 0; i < crew.Count; i++)
            {
                string p = crew.ElementAt(i).name;
                int recovered = 0;
                if (flights.TryGetValue(p, out recovered)) flights.Remove(p);
                recovered = recovered + 1;
                double d;
                if (MET.TryGetValue(p, out d)) MET.Remove(p);
                d = d + v.missionTime;
                if (!crew.ElementAt(i).veteran && recovered >= HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().numberOfFlightsRequired && d > HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().flightHoursRequired*60*60)
                {
                    crew.ElementAt(i).veteran = true;
                    promotedKerbals.Add(p);
                    Debug.Log("[EarnYourStripes]: "+p+" has earned a promotion");
                    StripHonours();
                }
                flights.Add(p, recovered);
                MET.Add(p, d);
                Debug.Log("[EarnYourStripes]: Processed Recovery of " + p);
                Debug.Log("[EarnYourStripes]: "+p+" - Flights: " + recovered+"/"+ HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().numberOfFlightsRequired);
                Debug.Log("[EarnYourStripes]: " + p + " - Time Logged: " + d + "/" + HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().flightHoursRequired * 60 * 60);
                Debug.Log("[EarnYourStripes]: "+p+" - Veteran Status: "+crew.ElementAt(i).veteran);
            }
        }

        private void OnDestroy()
        {
            GameEvents.onVesselRecovered.Remove(onVesselRecovered);
            GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);
            Debug.Log("[EarnYourStripes]: Unregistered Event Handlers");
        }
    }
}

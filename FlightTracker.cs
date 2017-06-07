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
            if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().removeExistingHonours) return;
            StripHonours();
        }
        void StripHonours()
        {
            IEnumerable<ProtoCrewMember> crew = HighLogic.CurrentGame.CrewRoster.Crew;
            if (crew.Count() == 0) return;
            for (int i = 0; i < crew.Count(); i++)
            {
                string p = crew.ElementAt(i).name;
                if (!promotedKerbals.Contains(p) && crew.ElementAt(i).veteran)
                {
                    crew.ElementAt(i).veteran = false;
                    Debug.Log("[EarnYourStripes]: Removed " + p + "'s veteran status as they haven't earned it");
                }
            }
        }
        private void OnGameSettingsApplied()
        {
            Debug.Log("[EarnYourStripes]: Game Settings Updated");
            if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().removeExistingHonours) return;
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
                if (!crew.ElementAt(i).veteran && recovered >= HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().numberOfFlightsRequired && d > HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().flightHoursRequired*60)
                {
                    crew.ElementAt(i).veteran = true;
                    promotedKerbals.Add(p);
                    Debug.Log("[EarnYourStripes]: "+p+" has earned a promotion");
                }
                flights.Add(p, recovered);
                MET.Add(p, d);
                Debug.Log("[EarnYourStripes]: Processed Recovery of " + p + " - Flights: " + recovered + " Time Logged: " + d + " (" + HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().flightHoursRequired * 60 + ") required");
                Debug.Log("[EarnYourStripes]: "+p+" Veteran Status: "+crew.ElementAt(i).veteran);
            }
        }

        private void OnDestroy()
        {
            GameEvents.onVesselRecovered.Remove(onVesselRecovered);
        }
    }
}

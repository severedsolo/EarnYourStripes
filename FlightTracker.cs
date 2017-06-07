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
        }
        private void Start()
        {
            GameEvents.onVesselRecovered.Add(onVesselRecovered);
            GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);
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
                if (!promotedKerbals.Contains(p)) crew.ElementAt(i).veteran = false;
            }
        }
        private void OnGameSettingsApplied()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().removeExistingHonours) return;
            StripHonours();
        }

        private void onVesselRecovered(ProtoVessel v, bool data1)
        {
            Debug.Log("[EarnYourStripes]: Vessel Recovery Attempt Detected");
            List<ProtoCrewMember> crew = v.GetVesselCrew();
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
                    Debug.Log("[EarnYourStripes]: Promoted " + crew.ElementAt(i).name);
                }
                flights.Add(p, recovered);
                MET.Add(p, d);
            }
        }

        private void OnDestroy()
        {
            GameEvents.onVesselRecovered.Remove(onVesselRecovered);
        }
    }
}

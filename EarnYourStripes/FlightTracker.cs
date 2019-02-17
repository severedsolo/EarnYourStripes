using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using KSP.UI;

namespace EarnYourStripes
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class FlightTracker : MonoBehaviour
    {
        public static FlightTracker instance;
        public Dictionary<string, int> flights = new Dictionary<string, int>();
        public Dictionary<string, double> MET = new Dictionary<string, double>();
        public Dictionary<string, double> LaunchTime = new Dictionary<string, double>();
        public List<string> promotedKerbals = new List<string>();
        public List<string> eligibleForPromotion = new List<string>();
        bool astronautComplexSpawned = false;
        private int UPDATE_CNT = 5;
        private bool updateLabelOnce = true;
        private int updateCnt = 5;

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
            GameEvents.OnProgressComplete.Add(OnProgressComplete);
            GameEvents.OnVesselRollout.Add(OnVesselRollout);
            GameEvents.onKerbalAddComplete.Add(OnKerbalAdded);
            GameEvents.onGUIAstronautComplexSpawn.Add(AstronautComplexSpawned);
            GameEvents.onGUIAstronautComplexDespawn.Add(AstronautComplexDespawned);
            Debug.Log("[EarnYourStripes]: Registered Event Handlers");
            StripHonours();
        }

        private void AstronautComplexDespawned()
        {
            astronautComplexSpawned = false;
            updateCnt = UPDATE_CNT;
            updateLabelOnce = true;
        }

        private void AstronautComplexSpawned()
        {
            astronautComplexSpawned = true;
        }

        private void OnKerbalAdded(ProtoCrewMember kerbal)
        {
            if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().basicSuit) kerbal.suit = ProtoCrewMember.KerbalSuit.Vintage;
        }

        private void OnVesselRollout(ShipConstruct data)
        {
            Debug.Log("[EarnYourStripes]: OnVesselRollout fired");
            if (FlightGlobals.ActiveVessel.GetCrewCount() == 0) return;
            for(int i = 0; i<FlightGlobals.ActiveVessel.GetCrewCount();i++)
            {
                ProtoCrewMember p = FlightGlobals.ActiveVessel.GetVesselCrew().ElementAt(i);
                if (p == null) return;
                if (p.type == ProtoCrewMember.KerbalType.Tourist)
                {
                    Debug.Log("[EarnYourStipes]: " + p.name + " is a tourist and not eligible for veteranhood");
                    continue;
                }
                LaunchTime.Remove(p.name);
                LaunchTime.Add(p.name, Planetarium.GetUniversalTime());
                if(!flights.TryGetValue(p.name, out int flightCount))flights.Add(p.name, 0);
                if(!MET.TryGetValue(p.name, out double d))MET.Add(p.name, 0);
                Debug.Log("[EarnYourStripes]: "+p.name+" launched at "+Planetarium.GetUniversalTime());
            }
        }
        void LateUpdate()
        {
            if (astronautComplexSpawned && updateLabelOnce)
            {
                if (updateCnt-- <= 0) updateLabelOnce = false;
                Debug.Log("[EarnYourStripes]: Attempting to override AstronautComplex");
                IEnumerable<CrewListItem> crewItemContainers = FindObjectsOfType<CrewListItem>().Where(x => x.GetCrewRef().rosterStatus == ProtoCrewMember.RosterStatus.Available);
                CrewListItem crewContainer;
                for (int i = 0; i < crewItemContainers.Count(); i++)
                {
                    crewContainer = crewItemContainers.ElementAt(i);
                    if(crewContainer.GetCrewRef().type == ProtoCrewMember.KerbalType.Applicant) continue;
                    string kerbalName = crewContainer.GetName();
                    flights.TryGetValue(kerbalName, out int numberOfFlights);
                    MET.TryGetValue(kerbalName, out double flightTime);
                    string timeString = ConvertUtToString(flightTime);
                    if (!updateLabelOnce) crewContainer.SetName(kerbalName + " (" + timeString +" hrs)");
                }
            }
        }

        private string ConvertUtToString(double time)
        {
            time = time / 60 / 60;
            time = (int)Math.Floor(time);
            string timeString = time.ToString();
            int stringLength = timeString.Count() - 3;
            if (time.ToString().Count() > 4) timeString = timeString.Substring(0, stringLength)+"k";
            else timeString = time.ToString();
            return timeString;
        }

        private void OnProgressComplete(ProgressNode data)
        {
            Debug.Log("[EarnYourStripes]: OnProgressComplete fired");
            if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().worldFirsts) return;
            if (FlightGlobals.ActiveVessel == null) return;
            List<ProtoCrewMember> crew = FlightGlobals.ActiveVessel.GetVesselCrew();
            Debug.Log("[EarnYourStripes]: Found " + crew.Count() + " potential candiates for World First promotion");
            if (crew.Count == 0) return;
            for(int i = 0; i<crew.Count; i++)
            {
                if (eligibleForPromotion.Contains(crew.ElementAt(i).name)) continue;
                if (!crew.ElementAt(i).flightLog.HasEntry(FlightLog.EntryType.Orbit))
                {
                    Debug.Log("[EarnYourStripes]: " + crew.ElementAt(i).name + " has not reached orbit yet and won't be given credit for this world first");
                    continue;
                }
                eligibleForPromotion.Add(crew.ElementAt(i).name);
                Debug.Log("[EarnYourStripes]: " + crew.ElementAt(i).name + " has achieved a World First and is now eligible for promotion");
            }
        }

        void StripHonours()
        {
            IEnumerable<ProtoCrewMember> crew = HighLogic.CurrentGame.CrewRoster.Crew;
            if (crew.Count() == 0) return;
            for (int i = 0; i < crew.Count(); i++)
            {
                string p = crew.ElementAt(i).name;
                if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().debug)
                {
                    Debug.Log("[EarnYourStripes]: Attempting to process StripHonours for " + p);
                    Debug.Log("[EarnYourStripes]: promotedKerbals.Contains(p):" + promotedKerbals.Contains(p));
                    Debug.Log("[EarnYourStripes]: Veteran Status:" + crew.ElementAt(i).veteran);
                    Debug.Log("[EarnYourStipes]: StripHonours On: " + HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().removeExistingHonours);
                }
                if (!promotedKerbals.Contains(p) && crew.ElementAt(i).veteran && HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().removeExistingHonours)
                {
                    crew.ElementAt(i).veteran = false;
                    Debug.Log("[EarnYourStripes]: Removed " + p + "'s veteran status as they haven't earned it");
                }
                else if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().debug)
                {
                    Debug.Log("[EarnYourStripes]: Failed to remove honours of " + p);
                }

                if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().basicSuit) crew.ElementAt(i).suit = ProtoCrewMember.KerbalSuit.Vintage;

                if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().pilotsAllowed && HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().scientistsAllowed && HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().engineersAllowed)
                {
                    if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().debug) Debug.Log("[EarnYourStripes]: All classes allowed");
                    continue;
                }
                switch(crew.ElementAt(i).trait)
                {
                    case "Pilot":
                        if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().pilotsAllowed)
                        {
                            crew.ElementAt(i).veteran = false;
                            Debug.Log("[EarnYourStripes]: Removed " + p + "'s veteran status due to settings");
                        }
                        break;
                    case "Scientist":
                        if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().scientistsAllowed)
                        {
                            crew.ElementAt(i).veteran = false;
                            Debug.Log("[EarnYourStripes]: Removed " + p + "'s veteran status due to settings");
                        }
                        break;
                    case "Engineer":
                        if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().engineersAllowed)
                        {
                            crew.ElementAt(i).veteran = false;
                            Debug.Log("[EarnYourStripes]: Removed " + p + "'s veteran status due to settings");
                        }
                        break;
                    default:
                        Debug.Log("[EarnYourStripes]: Attempted to remove " + p + "'s veteran status but couldn't figure out what class they were");
                        break;
                }
            }
        }
        private void OnGameSettingsApplied()
        {
            Debug.Log("[EarnYourStripes]: OnGameSettingsApplied fired");
            StripHonours();
        }

        private void onVesselRecovered(ProtoVessel v, bool data1)
        {
            Debug.Log("[EarnYourStripes]: onVesselRecovered Fired");
            if (v.missionTime == 0)
            {
                Debug.Log("[EarnYourStripes]: " + v.vesselName + " hasn't gone anywhere. No credit will be awarded for this flight");
                return;
            }
            List<ProtoCrewMember> crew = v.GetVesselCrew();
            Debug.Log("[EarnYourStripes]: Processing " + crew.Count() + " Kerbals");
            if (crew.Count == 0) return;
            for (int i = 0; i < crew.Count; i++)
            {
                if (crew.ElementAt(i).type == ProtoCrewMember.KerbalType.Tourist)
                {
                    Debug.Log("[EarnYourStipes]: " + crew.ElementAt(i).name + " is a tourist and not eligible for veteranhood");
                    continue;
                }
                string p = crew.ElementAt(i).name;
                int recovered = 0;
                if (flights.TryGetValue(p, out recovered)) flights.Remove(p);
                recovered = recovered + 1;
                double d = 0;
                if (MET.TryGetValue(p, out d)) MET.Remove(p);
                double missionTime = 0;
                if (LaunchTime.TryGetValue(p, out double launchTime)) missionTime = Planetarium.GetUniversalTime() - launchTime;
                else missionTime = v.missionTime;
                d = d + missionTime;
                flights.Add(p, recovered);
                MET.Add(p, d);
                Debug.Log("[EarnYourStripes]: Processed Recovery of " + p);
                Debug.Log("[EarnYourStripes]: " + p + " - Flights: " + recovered + "/" + HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().numberOfFlightsRequired);
                Debug.Log("[EarnYourStripes]: " + p + " - Time Logged: " + (int)d + "/" + HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().flightHoursRequired * 60 * 60);
                Debug.Log("[EarnYourStripes]: " + p + " - World First Achieved: " + eligibleForPromotion.Contains(p));
                Debug.Log("[EarnYourStripes]: " + p + " - Veteran Status: " + crew.ElementAt(i).veteran);
                if (!eligibleForPromotion.Contains(p) && HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().worldFirsts) continue;
                if (!crew.ElementAt(i).veteran && recovered >= HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().numberOfFlightsRequired && d > HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().flightHoursRequired * 60 * 60)
                {
                    crew.ElementAt(i).veteran = true;
                    promotedKerbals.Add(p);
                    Debug.Log("[EarnYourStripes]: " + p + " has earned a promotion");
                    StripHonours();
                }
            }
        }

        private void OnDestroy()
        {
            GameEvents.onVesselRecovered.Remove(onVesselRecovered);
            GameEvents.OnGameSettingsApplied.Remove(OnGameSettingsApplied);
            GameEvents.OnProgressReached.Remove(OnProgressComplete);
            GameEvents.OnVesselRollout.Remove(OnVesselRollout);
            Debug.Log("[EarnYourStripes]: Unregistered Event Handlers");
        }
    }
}

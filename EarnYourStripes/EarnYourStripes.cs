using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FlightTracker;
using System;
using Expansions;

namespace EarnYourStripes
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class EarnYourStripes : MonoBehaviour
    {
        public static EarnYourStripes Instance;
        public List<string> promotedKerbals = new List<string>();
        public bool firstRun = true;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);
            Debug.Log("[EarnYourStripes]: Earn Your Stripes is Awake");
        }
        private void Start()
        {
            ActiveFlightTracker.onFlightTrackerUpdated.Add(FlightTrackerUpdated);
            GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);
            GameEvents.onKerbalAddComplete.Add(OnKerbalAdded);
            Debug.Log("[EarnYourStripes]: Registered Event Handlers");
            StripHonours();
        }

        private void OnKerbalAdded(ProtoCrewMember kerbal)
        {
            if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().BasicSuit) kerbal.suit = ProtoCrewMember.KerbalSuit.Vintage;
        }

        private void StripHonours()
        {
            List<ProtoCrewMember> crew = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
            if (crew.Count == 0) return;
            for (int i = 0; i < crew.Count(); i++)
            {
                string p = crew.ElementAt(i).name;
                if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().Debug)
                {
                    Debug.Log("[EarnYourStripes]: Attempting to process StripHonours for " + p);
                    Debug.Log("[EarnYourStripes]: promotedKerbals.Contains(p):" + promotedKerbals.Contains(p));
                    Debug.Log("[EarnYourStripes]: Veteran Status:" + crew.ElementAt(i).veteran);
                    Debug.Log("[EarnYourStripes]: StripHonours On: " + HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().RemoveExistingHonours);
                }
                if (!promotedKerbals.Contains(p) && crew.ElementAt(i).veteran && HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().RemoveExistingHonours)
                {
                    crew.ElementAt(i).veteran = false;
                    Debug.Log("[EarnYourStripes]: Removed " + p + "'s veteran status as they haven't earned it");
                }
                else if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().Debug)
                {
                    Debug.Log("[EarnYourStripes]: Failed to remove honours of " + p);
                }

                if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().BasicSuit) crew.ElementAt(i).suit = ProtoCrewMember.KerbalSuit.Vintage;
                if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().PilotsAllowed && HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().ScientistsAllowed && HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().EngineersAllowed)
                {
                    if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().Debug) Debug.Log("[EarnYourStripes]: All classes allowed");
                    continue;
                }
                switch(crew.ElementAt(i).trait)
                {
                    case "Pilot":
                        if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().PilotsAllowed)
                        {
                            crew.ElementAt(i).veteran = false;
                            Debug.Log("[EarnYourStripes]: Removed " + p + "'s veteran status due to settings");
                        }
                        break;
                    case "Scientist":
                        if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().ScientistsAllowed)
                        {
                            crew.ElementAt(i).veteran = false;
                            Debug.Log("[EarnYourStripes]: Removed " + p + "'s veteran status due to settings");
                        }
                        break;
                    case "Engineer":
                        if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().EngineersAllowed)
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
            UpdateSuits();
        }

        private void UpdateSuits()
        {
            List<ProtoCrewMember> crew = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
            if (crew.Count == 0) return;
            for (int i = 0; i < crew.Count; i++)
            {
                if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().BgSuits && crew.ElementAt(i).veteran) crew.ElementAt(i).suit = ProtoCrewMember.KerbalSuit.Future;
            }
        }
        private void OnGameSettingsApplied()
        {
            Debug.Log("[EarnYourStripes]: OnGameSettingsApplied fired");
            StripHonours();
        }

        internal void FlightTrackerUpdated (ProtoCrewMember p)
        {
            Debug.Log("[EarnYourStripes]: FlightTrackerUpdated");
            string s = p.name;
            int flights = ActiveFlightTracker.instance.GetNumberOfFlights(p.name);
            double met = ActiveFlightTracker.instance.GetRecordedMissionTimeHours(p.name);
            int worldFirsts = ActiveFlightTracker.instance.GetNumberOfWorldFirsts(p.name);
            Debug.Log("[EarnYourStripes]: Evaluating "+p.name+"'s chances of promotion");
            if (!promotedKerbals.Contains(p.name) && EligibleForPromotion(flights, met, worldFirsts))
            {
                promotedKerbals.Add(p.name);
                p.veteran = true;
                if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().BgSuits && p.veteran) p.suit = ProtoCrewMember.KerbalSuit.Future;
                Debug.Log("[EarnYourStripes]: Promoting " + p.name);
            }
            UpdateSuits();
        }

        private bool EligibleForPromotion(int flights, double met, int worldFirsts)
        {
            if (flights < HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().NumberOfFlightsRequired)
            {
                Debug.Log("[EarnYourStripes]: Promotion Rejected: Insufficient Flights "+flights+"/"+ HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().NumberOfFlightsRequired);
                return false;
            }

            if (met < HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().FlightHoursRequired)
            {
                Debug.Log("[EarnYourStripes]: Promotion Rejected: Insufficient Hours "+met+"/"+ HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().FlightHoursRequired);
                return false;
            }

            if (worldFirsts < 1 && HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().WorldFirsts)
            {
                Debug.Log("[EarnYourStripes]: Promotion Rejected: No World Firsts");
                return false;
            }
            return true;
        }

        private void OnDestroy()
        {
            GameEvents.OnGameSettingsApplied.Remove(OnGameSettingsApplied);
            GameEvents.onKerbalAddComplete.Remove(OnKerbalAdded);
            if (ActiveFlightTracker.onFlightTrackerUpdated != null) ActiveFlightTracker.onFlightTrackerUpdated.Remove(FlightTrackerUpdated);
            Debug.Log("[EarnYourStripes]: Unregistered Event Handlers");
        }
    }
}

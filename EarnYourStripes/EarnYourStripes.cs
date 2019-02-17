using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FlightTracker;
using System;

namespace EarnYourStripes
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class EarnYourStripes : MonoBehaviour
    {
        public static EarnYourStripes instance;
        public List<string> promotedKerbals = new List<string>();
        StripeSettings settings;

        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(this);
            settings = HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>();
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
            if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().basicSuit) kerbal.suit = ProtoCrewMember.KerbalSuit.Vintage;
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

        internal void FlightTrackerUpdated (ProtoCrewMember p)
        {
            Debug.Log("[EarnYourStripes]: FlightTrackerUpdated");
            string s = p.name;
            int flights = ActiveFlightTracker.instance.GetNumberOfFlights(p.name);
            double met = ActiveFlightTracker.instance.GetRecordedMissionTimeHours(p.name);
            int worldFirsts = ActiveFlightTracker.instance.GetNumberOfWorldFirsts(p.name);
            if (!promotedKerbals.Contains(p.name) && EligibleForPromotion(flights, met, worldFirsts))
            {
                promotedKerbals.Add(p.name);
                p.veteran = true;
            }
        }

        private bool EligibleForPromotion(int flights, double met, int worldFirsts)
        {
            if (flights < settings.numberOfFlightsRequired) return false;
            if (met < settings.flightHoursRequired) return false;
            if (worldFirsts < 1 && settings.worldFirsts) return false;
            return true;
        }

        private void OnDestroy()
        {
            GameEvents.OnGameSettingsApplied.Remove(OnGameSettingsApplied);
            if (ActiveFlightTracker.onFlightTrackerUpdated != null) ActiveFlightTracker.onFlightTrackerUpdated.Remove(FlightTrackerUpdated);
            Debug.Log("[EarnYourStripes]: Unregistered Event Handlers");
        }
    }
}

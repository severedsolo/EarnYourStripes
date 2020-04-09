using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FlightTracker;

namespace EarnYourStripes
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class EarnYourStripes : MonoBehaviour
    {
        public static EarnYourStripes Instance;
        public bool firstRun = true;
        private List<ProtoCrewMember> crewMembers;
        public string defaultSuit = "Basic";
        public string veteranSuit = "Future";
        public Dictionary<string, StripyKerbal> trackedCrew = new Dictionary<string, StripyKerbal>();

        private void Awake()
        {
            Instance = this;
            FlightTrackerApi.OnFlightTrackerUpdated.Add(OnFlightTrackerUpdated);
            GameEvents.OnCrewmemberHired.Add(OnCrewHire);
            crewMembers = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
            for (int i = 0; i < crewMembers.Count; i++)
            {
                ProtoCrewMember p = crewMembers.ElementAt(i);
                trackedCrew[p.name] = new StripyKerbal(p);
            }
            
        }

        private void OnCrewHire(ProtoCrewMember p, int numberOfEmployees)
        {
            p.suit = GetSuit(p);
        }

        public ProtoCrewMember.KerbalSuit GetSuit(ProtoCrewMember p)
        {
            string suitToFind;
            if (p.veteran) suitToFind = veteranSuit;
            else suitToFind = defaultSuit;
            switch (suitToFind)
            {
                case "Basic": return ProtoCrewMember.KerbalSuit.Default; 
                case "Vintage": return ProtoCrewMember.KerbalSuit.Vintage;
                case "Future": return ProtoCrewMember.KerbalSuit.Future;
                default: return ProtoCrewMember.KerbalSuit.Default;
            }
        }

      private void Start()
        {
            //Remove veteranhood from Kerbals who don't deserve it.
            if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().RemoveExistingHonours) return;
            for (int i = 0; i < crewMembers.Count; i++)
            {
                ProtoCrewMember p = crewMembers.ElementAt(i);
                StripyKerbal sk = trackedCrew[p.name];
                p.veteran = sk.Promoted;
                if (!sk.SuitSet)
                {
                    p.suit = GetSuit(p);
                    sk.SuitSet = true;
                }
            }
        }

        private void OnFlightTrackerUpdated(ProtoCrewMember p)
        {
            if (trackedCrew[p.name].EligibleForPromotion()) trackedCrew[p.name].Promote(p);
        }

        public void OnSave(ConfigNode saveNode)
        {
            for (int i = 0; i < trackedCrew.Count; i++)
            {
                StripyKerbal s = trackedCrew.ElementAt(i).Value;
                s.OnSave(saveNode);
            }
        }

        public void OnLoad(ConfigNode loadNode)
        {
            ConfigNode[] kerbalNodes = loadNode.GetNodes("STRIPYKERBAL");
            for (int i = 0; i < kerbalNodes.Length; i++)
            {
                ConfigNode cn = kerbalNodes.ElementAt(i);
                string kerbalName = cn.GetValue("Name");
                //If Kerbal isn't in Tracked Crew they are no longer crew, so don't need to load them.
                if (!trackedCrew.TryGetValue(kerbalName, out StripyKerbal s)) continue;
                s.OnLoad(cn);
            }
        }
        
        private void OnDisable()
        {
            FlightTrackerApi.OnFlightTrackerUpdated.Remove(OnFlightTrackerUpdated);
        }
    }
}

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
        public string defaultSuit = "Basic";
        public string veteranSuit = "Future";
        public Dictionary<string, StripyKerbal> trackedCrew = new Dictionary<string, StripyKerbal>();

        private void Awake()
        {
            Instance = this;
            FlightTrackerApi.OnFlightTrackerUpdated.Add(OnFlightTrackerUpdated);
            GameEvents.OnCrewmemberHired.Add(OnCrewHire);
            List<ProtoCrewMember> crewMembers = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
            for (int i = 0; i < crewMembers.Count; i++)
            {
                ProtoCrewMember p = crewMembers.ElementAt(i);
                trackedCrew[p.name] = new StripyKerbal(p);
            }
        }

        private void OnCrewHire(ProtoCrewMember p, int numberOfEmployees)
        {
            p.suit = GetSuit(p);
            StripyKerbal sk = new StripyKerbal(p);
            trackedCrew.Add(p.name, sk);
            sk.SuitSet = true;
        }

        public ProtoCrewMember.KerbalSuit GetSuit(ProtoCrewMember p)
        {
            string suitToFind = p.veteran ? veteranSuit : defaultSuit;
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
            //Update suits
            defaultSuit = SetDefaultSuit(HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().DefaultSuit);
            veteranSuit = SetDefaultSuit(HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().VeteranSuit);
            Debug.Log("[EarnYourStripes] Set suits: Default: "+defaultSuit+" Veteran: "+veteranSuit);
            //Remove veteranhood from Kerbals who don't deserve it.
            List<ProtoCrewMember> crewMembers = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
            for (int i = 0; i < crewMembers.Count; i++)
            {
                ProtoCrewMember p = crewMembers.ElementAt(i);
                StripyKerbal sk = trackedCrew[p.name];
                if (StripHonours(p))
                {
                    p.veteran = sk.Promoted;
                    Debug.Log("[EarnYourStripes]: StripHonours for "+p.name+" Veteran: "+p.veteran);
                }

                if (sk.SuitSet) continue;
                p.suit = GetSuit(p);
                Debug.Log("[EarnYourStripes]: Set Default Suit for "+p.name+" - "+p.suit);
                sk.SuitSet = true;
            }
        }

      private bool StripHonours(ProtoCrewMember p)
      {
          if (p.isHero && HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().RemoveExistingHonoursBig4) return true;
          return HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().RemoveExistingHonoursOthers;
      }

      private static string SetDefaultSuit(int selection)
      {
          switch (selection)
          {
              case 0: return "Basic";
              case 1: return "Vintage";
              case 2: return "Future";
              default: return "Basic";
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

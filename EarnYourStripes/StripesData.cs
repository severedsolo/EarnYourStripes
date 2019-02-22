using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FlightTracker;

namespace EarnYourStripes
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.FLIGHT, GameScenes.TRACKSTATION)]
    class StripesData : ScenarioModule
    {
        public override void OnSave(ConfigNode node)
        {
            int counter = 0;
            ConfigNode cn = node.GetNode("EarnYourStripes");
            if (cn == null)
            {
                node.AddNode("EarnYourStripes");
                cn = node.GetNode("EarnYourStripes");
            }
            cn.SetValue("firstRun", FirstKerbaliser.instance.firstRun, true);
            cn.RemoveNodes("PROMOTED_KERBAL");
            for(int i = 0; i<EarnYourStripes.instance.promotedKerbals.Count(); i++)
            {
                ConfigNode temp = new ConfigNode("PROMOTED_KERBAL");
                temp.SetValue("Name", EarnYourStripes.instance.promotedKerbals.ElementAt(i), true);
                cn.AddNode(temp);
                counter++;
            }
            Debug.Log("[EarnYourStripes]: Saved " + counter + " kerbals flight data");
        }
        public override void OnLoad(ConfigNode node)
        {
            int counter = 0;
            ConfigNode cn = node.GetNode("EarnYourStripes");
            if (cn == null) return;
            cn.TryGetValue("firstRun", ref FirstKerbaliser.instance.firstRun);
            ConfigNode[] loaded = cn.GetNodes("PROMOTED_KERBAL");
            //Upgrade path from StripesData to FlightTrackerScenario
            if (loaded.Count() == 0)
            {
                loaded = cn.GetNodes("KERBAL");
                if (loaded.Count() == 0) return;
                else ActiveFlightTracker.instance.EarnYourStripesUpgradePath(cn);
                foreach (ProtoCrewMember p in HighLogic.CurrentGame.CrewRoster.Crew)
                {
                    EarnYourStripes.instance.FlightTrackerUpdated(p);
                }
                return;
            }
            EarnYourStripes.instance.promotedKerbals.Clear();
            for(int i = 0; i<loaded.Count();i++)
            {
                ConfigNode temp = loaded.ElementAt(i);
                string s = temp.GetValue("Name");
                EarnYourStripes.instance.promotedKerbals.Add(s);
                counter++;
            }
            Debug.Log("[EarnYourStripes]: Loaded " + counter + " kerbals flight data");
        }
    }
}

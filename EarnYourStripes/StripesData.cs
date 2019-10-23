using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FlightTracker;

namespace EarnYourStripes
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.FLIGHT, GameScenes.TRACKSTATION)]
    internal class StripesData : ScenarioModule
    {
        public override void OnSave(ConfigNode node)
        {
            int counter = 0;
            node.AddValue("firstRun", EarnYourStripes.Instance.firstRun);
            for(int i = 0; i<EarnYourStripes.Instance.promotedKerbals.Count(); i++)
            {
                ConfigNode temp = new ConfigNode("PROMOTED_KERBAL");
                temp.SetValue("Name", EarnYourStripes.Instance.promotedKerbals.ElementAt(i), true);
                node.AddNode(temp);
                counter++;
            }
            Debug.Log("[EarnYourStripes]: Saved " + counter + " kerbals flight data");
        }
        public override void OnLoad(ConfigNode node)
        {
            int counter = 0;
            node.TryGetValue("firstRun", ref EarnYourStripes.Instance.firstRun);
            ConfigNode[] loaded = node.GetNodes("PROMOTED_KERBAL");
            EarnYourStripes.Instance.promotedKerbals.Clear();
            for(int i = 0; i<loaded.Count();i++)
            {
                ConfigNode temp = loaded.ElementAt(i);
                string s = temp.GetValue("Name");
                EarnYourStripes.Instance.promotedKerbals.Add(s);
                counter++;
            }
            Debug.Log("[EarnYourStripes]: Loaded " + counter + " kerbals flight data");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EarnYourStripes
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER)]
    class StripesData : ScenarioModule
    {
        public override void OnSave(ConfigNode node)
        {
            ConfigNode cn = node.GetNode("EarnYourStripes");
            if (cn == null)
            {
                node.AddNode("EarnYourStripes");
                cn = node.GetNode("EarnYourStripes");
            }
            if (FlightTracker.instance.flights.Count() == 0) return;
            cn.RemoveNodes("KERBAL");
            foreach (var v in FlightTracker.instance.flights)
            {
                ConfigNode temp = new ConfigNode("KERBAL");
                temp.SetValue("Name", v.Key, true);
                temp.SetValue("Flights", v.Value, true);
                double d;
                if (FlightTracker.instance.MET.TryGetValue(v.Key, out d)) temp.SetValue("TimeLogged", d, true);
                if (FlightTracker.instance.promotedKerbals.Contains(v.Key)) temp.SetValue("Promoted", true, true);
                else temp.SetValue("Promoted", false, true);
                cn.AddNode(temp);
            }
        }
        public override void OnLoad(ConfigNode node)
        {
            ConfigNode cn = node.GetNode("EarnYourStripes");
            if (cn == null) return;
            ConfigNode[] loaded = cn.GetNodes("KERBAL");
            if (loaded.Count() == 0) return;
            FlightTracker.instance.flights.Clear();
            FlightTracker.instance.MET.Clear();
            FlightTracker.instance.promotedKerbals.Clear();
            for(int i = 0; i<loaded.Count();i++)
            {
                ConfigNode temp = loaded.ElementAt(i);
                string s = temp.GetValue("Name");
                if (s == null) continue;
                int t;
                if (Int32.TryParse(temp.GetValue("Flights"), out t)) FlightTracker.instance.flights.Add(s, t);
                double d;
                if (Double.TryParse(temp.GetValue("TimeLogged"), out d)) FlightTracker.instance.MET.Add(s, d);
                bool promoted;
                if (Boolean.TryParse(temp.GetValue("Promoted"), out promoted))
                {
                    if (promoted) FlightTracker.instance.promotedKerbals.Add(s);
                }
            }
        }
    }
}

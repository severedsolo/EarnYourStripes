﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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
            if (FlightTracker.instance.flights.Count() == 0) return;
            cn.RemoveNodes("KERBAL");
            foreach (var v in FlightTracker.instance.flights)
            {
                ConfigNode temp = new ConfigNode("KERBAL");
                temp.SetValue("Name", v.Key, true);
                temp.SetValue("Flights", v.Value, true);
                double d = 0;
                FlightTracker.instance.LaunchTime.TryGetValue(v.Key, out d);
                temp.SetValue("LaunchTime", d, true);
                if (FlightTracker.instance.MET.TryGetValue(v.Key, out d)) temp.SetValue("TimeLogged", d, true);
                if (FlightTracker.instance.promotedKerbals.Contains(v.Key)) temp.SetValue("Promoted", true, true);
                else temp.SetValue("Promoted", false, true);
                if (FlightTracker.instance.eligibleForPromotion.Contains(v.Key)) temp.SetValue("WorldFirst", true, true);
                else temp.SetValue("WorldFirst", false, true);
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
            ConfigNode[] loaded = cn.GetNodes("KERBAL");
            if (loaded.Count() == 0) return;
            FlightTracker.instance.flights.Clear();
            FlightTracker.instance.MET.Clear();
            FlightTracker.instance.promotedKerbals.Clear();
            FlightTracker.instance.eligibleForPromotion.Clear();
            FlightTracker.instance.LaunchTime.Clear();
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
                Double.TryParse(temp.GetValue("LaunchTime"), out d);
                if (d != 0) FlightTracker.instance.LaunchTime.Add(s, d);
                if (Boolean.TryParse(temp.GetValue("Promoted"), out promoted))
                {
                    if (promoted) FlightTracker.instance.promotedKerbals.Add(s);
                }
                if (Boolean.TryParse(temp.GetValue("WorldFirst"), out promoted))
                {
                    if (promoted) FlightTracker.instance.eligibleForPromotion.Add(s);
                }
                counter++;
            }
            Debug.Log("[EarnYourStripes]: Loaded " + counter + " kerbals flight data");
        }
    }
}

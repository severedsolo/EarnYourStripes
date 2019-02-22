using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI;
using FlightTracker;

namespace EarnYourStripes
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class AstronautComplexHandler : MonoBehaviour
    {
        bool astronautComplexSpawned = false;
        bool updateDone = false;

        void Start()
        {
            GameEvents.onGUIAstronautComplexSpawn.Add(AstronautComplexSpawned);
            GameEvents.onGUIAstronautComplexDespawn.Add(AstronautComplexDespawned);
            Debug.Log("[EarnYourStripes]: AstronautComplexHandler has registered events");
        }

        private void AstronautComplexDespawned()
        {
            astronautComplexSpawned = false;
            updateDone = false;
            Debug.Log("[EarnYourStripes]: Astronaut Complex despawned");
        }

        private void AstronautComplexSpawned()
        {
            Debug.Log("[EarnYourStripes]: Astronaut Complex spawned");
            astronautComplexSpawned = true;
        }

        private string ConvertUtToString(double time)
        {
            time = time / 60 / 60;
            time = (int)Math.Floor(time);
            string timeString = time.ToString();
            int stringLength = timeString.Count() - 3;
            if (time.ToString().Count() > 4) timeString = timeString.Substring(0, stringLength) + "k";
            return timeString;
        }

        private void LateUpdate()
        {
            if (astronautComplexSpawned && !updateDone)
            {
                Debug.Log("[EarnYourStripes]: Attempting to override AstronautComplex UI");
                IEnumerable<CrewListItem> crewItemContainers = FindObjectsOfType<CrewListItem>();
                CrewListItem crewContainer;
                for (int i = 0; i < crewItemContainers.Count(); i++)
                {
                    crewContainer = crewItemContainers.ElementAt(i);
                    ProtoCrewMember p = crewContainer.GetCrewRef();
                    if (p.type == ProtoCrewMember.KerbalType.Applicant) continue;
                    string kerbalName = p.name;
                    double flightTime = ActiveFlightTracker.instance.GetRecordedMissionTimeSeconds(kerbalName);
                    kerbalName = p.name + " (" + ConvertUtToString(flightTime)+" hrs)";
                    if (crewContainer.GetName() == kerbalName) updateDone = true;
                    if (p.rosterStatus == ProtoCrewMember.RosterStatus.Available) crewContainer.SetName(kerbalName);
                }
            }
        }

        void OnDisable()
        {
            GameEvents.onGUIAstronautComplexSpawn.Remove(AstronautComplexSpawned);
            GameEvents.onGUIAstronautComplexDespawn.Remove(AstronautComplexDespawned);
            Debug.Log("[EarnYourStripes]: AstronautComplexHandler has unregistered events");
        }
    }
}

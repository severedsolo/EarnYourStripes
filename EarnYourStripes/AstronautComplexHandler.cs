using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using KSP.UI;
using FlightTracker;

namespace EarnYourStripes
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    internal class AstronautComplexHandler : MonoBehaviour
    {
        private bool astronautComplexSpawned;
        private bool updateDone;

        private void Start()
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
            string timeString = time.ToString(CultureInfo.CurrentCulture);
            int stringLength = timeString.Length - 3;
            if (time.ToString(CultureInfo.CurrentCulture).Length > 4) timeString = timeString.Substring(0, stringLength) + "k";
            return timeString;
        }

        private void LateUpdate()
        {
            if (!astronautComplexSpawned || updateDone) return;
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation;
            List<CrewListItem> crewToOverwrite = GetCrewToOverwrite(FindObjectsOfType<CrewListItem>().ToList());
            if (crewToOverwrite.Count == 0) updateDone = true;
            Debug.Log("[EarnYourStripes]: Attempting to override AstronautComplex UI");
            for (int i = 0; i < crewToOverwrite.Count; i++)
            {
                CrewListItem crewContainer = crewToOverwrite.ElementAt(i);
                ProtoCrewMember p = crewContainer.GetCrewRef();
                string kerbalName = p.name;
                double flightTime = FlightTrackerApi.Instance.GetRecordedMissionTimeSeconds(kerbalName);
                kerbalName = p.name + " (" + ConvertUtToString(flightTime)+" hrs)";
                if (crewContainer.GetName() == kerbalName) updateDone = true;
                if (p.rosterStatus == ProtoCrewMember.RosterStatus.Available) crewContainer.SetName(kerbalName);
            }
        }

        private List<CrewListItem> GetCrewToOverwrite(List<CrewListItem> crewItemContainers)
        {
            List<CrewListItem> crewToOverwrite = new List<CrewListItem>();
            for (int i = 0; i < crewItemContainers.Count(); i++)
            {
                CrewListItem crewContainer = crewItemContainers.ElementAt(i);
                ProtoCrewMember p = crewContainer.GetCrewRef();
                if (p.type == ProtoCrewMember.KerbalType.Applicant || p.type == ProtoCrewMember.KerbalType.Tourist) continue;
                crewToOverwrite.Add(crewContainer);
            }
            return crewToOverwrite;
        }

        private void OnDisable()
        {
            GameEvents.onGUIAstronautComplexSpawn.Remove(AstronautComplexSpawned);
            GameEvents.onGUIAstronautComplexDespawn.Remove(AstronautComplexDespawned);
            Debug.Log("[EarnYourStripes]: AstronautComplexHandler has unregistered events");
        }
    }
}

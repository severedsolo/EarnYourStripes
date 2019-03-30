using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Experience;
using UnityEngine;

namespace EarnYourStripes
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class FirstRunSpaceCentreLoader : FirstKerbaliser
    {

    }
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class FirstRunFlightLoader : FirstKerbaliser
    {

    }
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    class FirstRunTrackingStationLoader : FirstKerbaliser
    {

    }

    class FirstKerbaliser : MonoBehaviour
    {
        public static FirstKerbaliser instance;
        public bool firstRun = true;
        int indent = 20;
        int numberOfKerbalsToGenerate = 4;
        bool showGUI = false;
        bool randomise = true;
        bool confirmation;
        bool nameChange;
        string nameChanging = "";
        string trait;
        bool allowPilots = true;
        bool allowScientists = true;
        bool allowEngineers = true;
        bool allowMales = true;
        bool allowFemales = true;
        ProtoCrewMember selected;
        List<string> availableTraits = new List<string>();
        List<string> disallowedTraits = new List<string>();

        List<ProtoCrewMember> generatedKerbals = new List<ProtoCrewMember>();

        Rect Window = new Rect(20, 100, 240, 50);


        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().generateCrew) return;
            if (!firstRun) return;
            IEnumerable<ProtoCrewMember> crew = HighLogic.CurrentGame.CrewRoster.Crew;
            for (int i = 0; i < crew.Count(); i++)
            {
                ProtoCrewMember p = crew.ElementAt(i);
                if (p.rosterStatus != ProtoCrewMember.RosterStatus.Available) continue;
                generatedKerbals.Add(p);
            }
            ConfigNode[] experienceTraits = GameDatabase.Instance.GetConfigNodes("EXPERIENCE_TRAIT");
            for (int i = 0; i < experienceTraits.Count(); i++)
            {
                ConfigNode cn = experienceTraits.ElementAt(i);
                availableTraits.Add(cn.GetValue("title"));
            }
            showGUI = true;
            Debug.Log("[EarnYourStripes]: FirstKerbaliser: Start");
        }

        public void OnGUI()
        {
            if (showGUI) Window = GUILayout.Window(42736959, Window, GUIDisplay, "Earn Your Stripes", GUILayout.Width(200));
        }

        private void GUIDisplay(int id)
        {
            if (confirmation)
            {
                GUILayout.Label("Are you sure? You won't be able to make any further changes");
                if (GUILayout.Button("Yes"))
                {
                    showGUI = false;
                    UpdateAllKerbals();
                    firstRun = false;
                    Debug.Log("[EarnYourStripes]: Initial Crew Changes committed");
                }
                if (GUILayout.Button("No"))
                {
                    confirmation = false;
                }
                return;
            }
            randomise = GUILayout.Toggle(randomise, "Generate Starting Crew Randomly");
            if (!randomise)
            {
                GUILayout.BeginVertical();
                for (int i = 0; i < generatedKerbals.Count(); i++)
                {
                    ProtoCrewMember p = generatedKerbals.ElementAt(i);
                    if (GUILayout.Button(p.name))
                    {
                        if (selected != p) selected = p;
                        else selected = null;
                        Debug.Log("[EarnYourStripes]: " + p.name + " selected for editing");
                    }
                    if (selected == p)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(indent);
                        if (GUILayout.Button("Change Name"))
                        {
                            nameChange = true;
                            nameChanging = p.name;
                            Debug.Log("[EarnYourStripes]: Changing name of " + p.name);
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(indent);
                        if (nameChange && nameChanging == p.name)
                        {
                            nameChanging = GUILayout.TextField(nameChanging);
                            p.ChangeName(nameChanging);
                            if (GUILayout.Button("Save"))
                            {
                                nameChange = false;
                                Debug.Log("[EarnYourStripes]: Commited name change to " + p.name);
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(indent);
                        if (GUILayout.Button("Gender: " + p.gender))
                        {
                            if (p.gender == ProtoCrewMember.Gender.Male) p.gender = ProtoCrewMember.Gender.Female;
                            else p.gender = ProtoCrewMember.Gender.Male;
                            Debug.Log("[EarnYourStripes]: " + p.name + " gender changed to " + p.gender);
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(indent);
                        if (GUILayout.Button("Class: " + p.GetLocalizedTrait()))
                        {
                            int index = FindIndex(p.trait);
                            if (index > availableTraits.Count - 1) index = 0;
                            trait = availableTraits.ElementAt(index);
                            KerbalRoster.SetExperienceTrait(p, trait);
                            Debug.Log("[EarnYourStripes]: " + p.name + " class changed to " + p.trait);
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(indent);
                        GUILayout.Label("Courage");
                        float.TryParse(GUILayout.TextField(p.courage.ToString()), out p.courage);
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(indent);
                        GUILayout.Label("Stupidity");
                        float.TryParse(GUILayout.TextField(p.stupidity.ToString()), out p.stupidity);
                        GUILayout.EndHorizontal();
                        if (p.courage > 0.99) p.courage = 0.99f;
                        if (p.stupidity > 0.99) p.stupidity = 0.99f;
                        if (p.courage < 0.01) p.courage = 0.01f;
                        if (p.stupidity < 0.01) p.stupidity = 0.01f;
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(indent);
                        p.isBadass = GUILayout.Toggle(p.isBadass, "BadS?");
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(indent);
                        if (GUILayout.Button("Remove Kerbal"))
                        {
                            Debug.Log("[EarnYourStripes]: Deleting " + p.name);
                            generatedKerbals.Remove(p);
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.Space(indent);
                if (GUILayout.Button("Add new Kerbal"))
                {
                    ProtoCrewMember p = HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Crew);
                    if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().removeExistingHonours) p.veteran = true;
                    if (!HighLogic.CurrentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().KerbalExperienceEnabled(HighLogic.CurrentGame.Mode)) KerbalRoster.SetExperienceLevel(p, 5);
                    HighLogic.CurrentGame.CrewRoster.AddCrewMember(p);
                    generatedKerbals.Add(p);
                    Debug.Log("[EarnYourStripes]: New Kerbal added: " + p.name);
                }
                GUILayout.Space(indent);
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.Label("How many Kerbals do you want to start with?");
                string buttonString = string.Empty;
                int.TryParse(GUILayout.TextField(numberOfKerbalsToGenerate.ToString()), out numberOfKerbalsToGenerate);
                if (disallowedTraits.Contains("Male")) buttonString = "Males: Not Allowed";
                else buttonString = "Males: Allowed";
                if (GUILayout.Button(buttonString))
                {
                    if (disallowedTraits.Contains("Male")) disallowedTraits.Remove("Male");
                    else disallowedTraits.Add("Male");
                }
                if (disallowedTraits.Contains("Female")) buttonString = "Females: Not Allowed";
                else buttonString = "Males: Allowed";
                if (GUILayout.Button(buttonString))
                {
                    if (disallowedTraits.Contains("Female")) disallowedTraits.Remove("Female");
                    else disallowedTraits.Add("Female");
                }
                for (int i = 0; i < availableTraits.Count(); i++)
                {
                    string selectedTrait = availableTraits.ElementAt(i);
                    if (selectedTrait == "Tourist") continue;
                    if (disallowedTraits.Contains(selectedTrait))
                    {
                        buttonString = selectedTrait + ": Not Allowed";
                    }
                    else
                    {
                        buttonString = selectedTrait + ": Allowed";
                    }
                    if (GUILayout.Button(buttonString))
                    {
                        if (disallowedTraits.Contains(selectedTrait)) disallowedTraits.Remove(selectedTrait);
                        else disallowedTraits.Add(selectedTrait);
                    }
                }
            }
            if (GUILayout.Button("Done")) confirmation = true;
            GUI.DragWindow();
        }

        private int FindIndex(string kerbalTrait)
        {
            for (int i = 0; i < availableTraits.Count(); i++)
            {
                string s = availableTraits.ElementAt(i);
                if (s == kerbalTrait) return i+1;
            }
            return -1;
        }

        void UpdateAllKerbals()
        {
            if (randomise) generatedKerbals.Clear();
            List<ProtoCrewMember> kerbals = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
            for (int i = 0; i < kerbals.Count(); i++)
            {
                ProtoCrewMember p = kerbals.ElementAt(i);
                if (p.rosterStatus != ProtoCrewMember.RosterStatus.Available) continue;
                bool found = false;
                for (int g = 0; g < generatedKerbals.Count(); g++)
                {
                    ProtoCrewMember generated = generatedKerbals.ElementAt(g);
                    if (p.name == generated.name)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    Debug.Log("[EarnYourStripes]: " + p.name + " not selected with initial crew. Removing");
                    HighLogic.CurrentGame.CrewRoster.Remove(p);
                }
            }
            if (randomise)
            {
                int i = 0;
                Debug.Log("[EarnYourStripes]: Running Random Crew Generator");
                Debug.Log("[EarnYourStripes]: allowMales: " + allowMales + " allowFemales: " + allowFemales + " allowPilots: " + allowPilots + " allowScientists: " + allowScientists + " allowEngineers: " + allowEngineers);
                while (i < numberOfKerbalsToGenerate)
                {
                    ProtoCrewMember p = HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Crew);
                    if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().removeExistingHonours) p.veteran = true;
                    if (!HighLogic.CurrentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().KerbalExperienceEnabled(HighLogic.CurrentGame.Mode)) KerbalRoster.SetExperienceLevel(p, 5);
                    if (disallowedTraits.Contains("Male") && p.gender == ProtoCrewMember.Gender.Male) HighLogic.CurrentGame.CrewRoster.Remove(p);
                    else if (disallowedTraits.Contains("Female") && p.gender == ProtoCrewMember.Gender.Female) HighLogic.CurrentGame.CrewRoster.Remove(p);
                    else if (disallowedTraits.Contains(p.trait)) HighLogic.CurrentGame.CrewRoster.Remove(p);
                    else
                    {
                        Debug.Log("[EarnYourStripes]: " + p.name + " selected randomly for initial crew");
                        Debug.Log("[EarnYourStripes]: " + p.trait + ", " + p.gender);
                        i++;
                    }
                }
            }
        }
    }
}

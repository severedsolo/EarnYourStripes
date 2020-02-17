using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// ReSharper disable ImplicitlyCapturedClosure

namespace EarnYourStripes
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    internal class FirstKerbaliser : MonoBehaviour
    {
        private PopupDialog uiDialog;
        private bool allowFemales = true;
        private bool allowMales = true;
        private bool randomKerbals = true;
        private bool invokingUi;
        private Dictionary<string, bool> traits;
        private ProtoCrewMember kerbalToEdit;
        private const float SwitchWindowTime = 0.1f;


        private void Start()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().GenerateCrew) return;
            if (!EarnYourStripes.Instance.firstRun) return;
            if (HighLogic.CurrentGame.CrewRoster.GetAssignedCrewCount() > 0) return;
            uiDialog = RandomKerbalDialog();
            Debug.Log("[FirstKerbaliser].Start");
        }

        private void LockCamera()
        {
            InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS, "EYSCameraLock");
        }

        private PopupDialog RandomKerbalDialog()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            int numberOfRandomKerbals = 4;
            dialogElements.Add(new DialogGUIToggle(() => randomKerbals, "Generate Random Kerbals", b => { SwitchMode(); }));
            DialogGUIBase[] verticalArray = new DialogGUIBase[3];
            verticalArray[0] = new DialogGUILabel(() => "Number of Kerbals: " + numberOfRandomKerbals);
            verticalArray[1] = new DialogGUISpace(30.0f);
            verticalArray[2] = new DialogGUISlider(() => numberOfRandomKerbals, 1, 10, true, 90.0f, 30.0f, newValue => { numberOfRandomKerbals = (int) newValue; });
            dialogElements.Add(new DialogGUIHorizontalLayout(verticalArray));
            verticalArray = new DialogGUIBase[2];
            DialogGUIBase[] splitBox = new DialogGUIBase[2];
            verticalArray[0] = new DialogGUIToggle(allowFemales, "Allow Female Kerbals", b => allowFemales = b);
            verticalArray[1] = new DialogGUIToggle(allowMales, "Allow Male Kerbals", b => allowMales = b);
            splitBox[0] = new DialogGUIVerticalLayout(verticalArray);
            splitBox[1] = new DialogGUIVerticalLayout(TraitDialogOptions());
            dialogElements.Add(new DialogGUIHorizontalLayout(splitBox));
            dialogElements.Add(new DialogGUISpace(5.0f));
            dialogElements.Add(new DialogGUIButton("I'm Done", () => GenerateRandomKerbals(numberOfRandomKerbals), true));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("FirstKerbaliserRandomKerbals", "", "Earn Your Stripes", UISkinManager.defaultSkin,
                    new Rect(0.5f, 0.5f, 280.0f, 210), dialogElements.ToArray()), false, UISkinManager.defaultSkin);
        }

        private DialogGUIVerticalLayout TraitDialogOptions()
        {
            traits = GetAvailableTraits();
            List<DialogGUIBase> horizontal = new List<DialogGUIBase>();
            if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().Debug) Debug.Log("[FirstKerbaliser]: UI: Processing " + traits.Count + " elements");
            for (int i = 0; i < traits.Count; i++)
            {
                string s = traits.ElementAt(i).Key;
                horizontal.Add(new DialogGUIToggle(traits.ElementAt(i).Value, () => "Allow: " + s, b => { traits[s] = b; }));
                if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().Debug) Debug.Log("[FirstKerbaliser]: UI: Added " + traits.ElementAt(i) + " to UI");
            }

            return new DialogGUIVerticalLayout(horizontal.ToArray());
        }

        private DialogGUIHorizontalLayout TraitDialogOptions(ProtoCrewMember pcm)
        {
            traits = GetAvailableTraits();
            List<DialogGUIBase> horizontal = new List<DialogGUIBase>();
            Debug.Log("[FirstKerbaliser]: UI: Processing " + traits.Count + " elements");
            for (int i = 0; i < traits.Count; i++)
            {
                string s = traits.ElementAt(i).Key;
                horizontal.Add(new DialogGUIButton(s, () => KerbalRoster.SetExperienceTrait(pcm, s), s.Length * 10, 20.0f, false));
                if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().Debug) Debug.Log("[FirstKerbaliser]: UI: Added " + traits.ElementAt(i) + " to UI");
            }

            return new DialogGUIHorizontalLayout(horizontal.ToArray());
        }

        private void SwitchMode()
        {
            if (uiDialog != null) uiDialog.Dismiss();
            if (!invokingUi)
            {
                randomKerbals = !randomKerbals;
                invokingUi = true;
                Invoke(nameof(SwitchMode), SwitchWindowTime);
                return;
            }

            if (randomKerbals) uiDialog = RandomKerbalDialog();
            else uiDialog = CustomKerbalDialog();
            invokingUi = false;
        }

        private PopupDialog CustomKerbalDialog()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            dialogElements.Add(GenerateListOfKerbals());
            dialogElements.Add(new DialogGUIButton("Add New Kerbal", NewKerbal));
            dialogElements.Add(new DialogGUIButton("Done", () => ClearControlLock(false), true));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("FirstKerbaliserCustomKerbalDialog", "", "Earn Your Stripes", UISkinManager.defaultSkin,
                    new Rect(0.5f, 0.5f, 135.0f, 90.0f), dialogElements.ToArray()), false, UISkinManager.defaultSkin);
        }

        private void NewKerbal()
        {
            ProtoCrewMember p = HighLogic.CurrentGame.CrewRoster.GetNewKerbal();
            kerbalToEdit = p;
            Debug.Log("[EarnYourStripes]: Generating New Kerbal " + p.name);
            EditKerbal();
        }

        private DialogGUIVerticalLayout GenerateListOfKerbals()
        {
            List<ProtoCrewMember> crew = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
            List<DialogGUIBase> verticalElements = new List<DialogGUIBase>();
            for (int i = 0; i < crew.Count; i++)
            {
                ProtoCrewMember pcm = crew.ElementAt(i);
                if (pcm.rosterStatus != ProtoCrewMember.RosterStatus.Available) continue;
                verticalElements.Add(new DialogGUIButton(() => pcm.displayName, () => SetKerbalToEdit(pcm), 100.0f, 30.0f, false));
            }

            return new DialogGUIVerticalLayout(verticalElements.ToArray());
        }

        private void SetKerbalToEdit(ProtoCrewMember pcm)
        {
            kerbalToEdit = pcm;
            Invoke(nameof(EditKerbal), SwitchWindowTime);
        }

        private void EditKerbal()
        {
            Debug.Log("[EarnYourStripes]: Editing " + kerbalToEdit.name);
            uiDialog = EditKerbalDialog();
        }

        private PopupDialog EditKerbalDialog()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            LockCamera();
            dialogElements.Add(new DialogGUITextInput(kerbalToEdit.name, false, 100, SetName, 30));
            DialogGUIBase[] horizontal = new DialogGUIBase[2];
            horizontal[0] = new DialogGUILabel(() => "Gender: " + kerbalToEdit.gender);
            horizontal[1] = new DialogGUIButton("Change", SwitchGender, 50.0f, 20.0f, false);
            dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
            dialogElements.Add(new DialogGUILabel(() => "Class: " + kerbalToEdit.trait));
            dialogElements.Add(TraitDialogOptions(kerbalToEdit));
            horizontal[0] = new DialogGUILabel(() => "Stupidity: " + kerbalToEdit.stupidity);
            horizontal[1] = new DialogGUISlider(() => kerbalToEdit.stupidity, 0.0f, 1.0f, false, 100.0f, 30.0f, newValue => { kerbalToEdit.stupidity = newValue; });
            dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
            horizontal[0] = new DialogGUILabel(() => "Courage: " + kerbalToEdit.courage);
            horizontal[1] = new DialogGUISlider(() => kerbalToEdit.courage, 0.0f, 1.0f, false, 100.0f, 30.0f, newValue => { kerbalToEdit.courage = newValue; });
            dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
            dialogElements.Add(new DialogGUIButton("Done", () => ClearControlLock(true)));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("FirstKerbaliserEditKerbalDialog", "", "Earn Your Stripes", UISkinManager.defaultSkin,
                    new Rect(0.5f, 0.5f, 240.0f, 90.0f), dialogElements.ToArray()), false, UISkinManager.defaultSkin);

        }

        private void SwitchGender()
        {
            if (kerbalToEdit.gender == ProtoCrewMember.Gender.Female) kerbalToEdit.gender = ProtoCrewMember.Gender.Male;
            else kerbalToEdit.gender = ProtoCrewMember.Gender.Female;
            Debug.Log("[EarnYourStripes]: " + kerbalToEdit.name + " Gender Change");
        }

        private string SetName(string nameToSet)
        {
            Debug.Log("[EarnYourStripes]: Changing " + kerbalToEdit.name + " to " + nameToSet);
            kerbalToEdit.ChangeName(nameToSet);
            return nameToSet;
        }

        private PopupDialog BrokenDialog()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            dialogElements.Add(new DialogGUILabel("Your choices were invalid. Try again"));
            randomKerbals = !randomKerbals;
            allowMales = true;
            allowFemales = true;
            dialogElements.Add(new DialogGUIButton("OK", SwitchMode));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("FirstKerbaliserErrorDialog", "", "Earn Your Stripes", UISkinManager.defaultSkin,
                    new Rect(0.5f, 0.5f, 200.0f, 90.0f), dialogElements.ToArray()), false, UISkinManager.defaultSkin);

        }


        private void GenerateRandomKerbals(int numberOfRandomKerbals)
        {
            if (!ValidSelection())
            {
                Invoke(nameof(GenerateBrokenDialog), SwitchWindowTime);
                return;
            }

            int i = 0;
            Debug.Log("[EarnYourStripes]: Running Random Crew Generator. Generating " + numberOfRandomKerbals + " Kerbals");
            List<ProtoCrewMember> kerbals = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
            ProtoCrewMember pcm;
            for (int crewCount = 0; crewCount < kerbals.Count; crewCount++)
            {
                pcm = kerbals.ElementAt(crewCount);
                HighLogic.CurrentGame.CrewRoster.Remove(pcm);
            }

            while (i < numberOfRandomKerbals)
            {
                pcm = HighLogic.CurrentGame.CrewRoster.GetNewKerbal();
                if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettingsClassRestrictions>().RemoveExistingHonours) pcm.veteran = true;
                if (!HighLogic.CurrentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().KerbalExperienceEnabled(HighLogic.CurrentGame.Mode)) KerbalRoster.SetExperienceLevel(pcm, 5);
                if (!ValidKerbal(pcm.gender, pcm.trait))
                {
                    HighLogic.CurrentGame.CrewRoster.Remove(pcm);
                    continue;
                }

                Debug.Log("[EarnYourStripes]: Generated " + pcm.name + " (" + pcm.trait + ")");
                i++;
            }

            EarnYourStripes.Instance.firstRun = false;
        }

        private void ClearControlLock(bool switchMode)
        {
            InputLockManager.RemoveControlLock("EYSCameraLock");
            randomKerbals = true;
            if (uiDialog != null) uiDialog.Dismiss();
            EarnYourStripes.Instance.firstRun = false;
            if (switchMode) Invoke(nameof(SwitchMode), SwitchWindowTime);
        }

        private bool ValidKerbal(ProtoCrewMember.Gender gender, string trait)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (gender)
            {
                case ProtoCrewMember.Gender.Female when !allowFemales:
                case ProtoCrewMember.Gender.Male when !allowMales:
                    return false;
            }

            if (!traits[trait]) return false;
            return true;
        }

        private void GenerateBrokenDialog()
        {
            uiDialog = BrokenDialog();
        }

        private bool ValidSelection()
        {
            if (!allowFemales && !allowMales) return false;
            for (int i = 0; i < traits.Count; i++)
            {
                if (traits.ElementAt(i).Value) return true;
            }

            return false;
        }

        private Dictionary<string, bool> GetAvailableTraits()
        {
            ConfigNode[] experienceTraits = GameDatabase.Instance.GetConfigNodes("EXPERIENCE_TRAIT");
            Dictionary<string, bool> traitTitles = new Dictionary<string, bool>();
            for (int i = 0; i < experienceTraits.Length; i++)
            {
                if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().Debug) Debug.Log("[FirstKerbaliser]: Trying to Add Trait " + i);
                ConfigNode cn = experienceTraits.ElementAt(i);
                string trait = cn.GetValue("title");
                if (traitTitles.ContainsKey(trait)) continue;
                if (trait == "Tourist") continue;
                traitTitles.Add(cn.GetValue("title"), true);
                if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().Debug) Debug.Log("[FirstKerbaliser]: Added Trait " + trait);
            }

            Debug.Log("[EarnYourStripes]: Loaded " + traitTitles.Count + " traits");
            return traitTitles;
        }
    }
}

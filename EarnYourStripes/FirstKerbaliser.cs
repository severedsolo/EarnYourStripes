using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Experience;
using UnityEngine;

namespace EarnYourStripes
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    internal class FirstKerbaliser : MonoBehaviour
    {
        private PopupDialog _uidialog;
        private bool _allowFemales = true;
        private bool _allowMales = true;
        private bool _randomKerbals = true;
        private bool _invokingUi;
        Dictionary<string, bool> _traits;
        private ProtoCrewMember _kerbalToEdit;
        private float _switchWindowTime = 0.1f;


        private void Start()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().GenerateCrew) return;
            if (!EarnYourStripes.Instance.firstRun) return;
            _uidialog = RandomKerbalDialog();
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
            dialogElements.Add(new DialogGUIToggle(() => _randomKerbals, "Generate Random Kerbals", b => { SwitchMode(); }));
            DialogGUIBase[] verticalArray = new DialogGUIBase[3];
            verticalArray[0] = new DialogGUILabel(() => "Number of Kerbals: " + numberOfRandomKerbals);
            verticalArray[1] = new DialogGUISpace(30.0f);
            verticalArray[2] = new DialogGUISlider(() => numberOfRandomKerbals, 1, 10, true, 90.0f, 30.0f, newValue => { numberOfRandomKerbals = (int) newValue; });
            dialogElements.Add(new DialogGUIHorizontalLayout(verticalArray));
            verticalArray = new DialogGUIBase[2];
            DialogGUIBase[] splitBox = new DialogGUIBase[2];
            verticalArray[0] = new DialogGUIToggle(_allowFemales, "Allow Female Kerbals", b => _allowFemales = b);
            verticalArray[1] = new DialogGUIToggle(_allowMales, "Allow Male Kerbals", b => _allowMales = b);
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
            _traits = GetAvailableTraits();
            List<DialogGUIBase> horizontal = new List<DialogGUIBase>();
            if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().Debug) Debug.Log("[FirstKerbaliser]: UI: Processing " + _traits.Count + " elements");
            for (int i = 0; i < _traits.Count; i++)
            {
                string s = _traits.ElementAt(i).Key;
                horizontal.Add(new DialogGUIToggle(_traits.ElementAt(i).Value, () => "Allow: " + s, b => { _traits[s] = b; }));
                if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().Debug) Debug.Log("[FirstKerbaliser]: UI: Added " + _traits.ElementAt(i) + " to UI");
            }

            return new DialogGUIVerticalLayout(horizontal.ToArray());
        }

        private DialogGUIHorizontalLayout TraitDialogOptions(ProtoCrewMember pcm)
        {
            _traits = GetAvailableTraits();
            List<DialogGUIBase> horizontal = new List<DialogGUIBase>();
            Debug.Log("[FirstKerbaliser]: UI: Processing " + _traits.Count + " elements");
            for (int i = 0; i < _traits.Count; i++)
            {
                string s = _traits.ElementAt(i).Key;
                horizontal.Add(new DialogGUIButton(s, () => KerbalRoster.SetExperienceTrait(pcm, s), s.Length * 10, 20.0f, false));
                if (HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().Debug) Debug.Log("[FirstKerbaliser]: UI: Added " + _traits.ElementAt(i) + " to UI");
            }

            return new DialogGUIHorizontalLayout(horizontal.ToArray());
        }

        private void SwitchMode()
        {
            if (_uidialog != null) _uidialog.Dismiss();
            if (!_invokingUi)
            {
                _randomKerbals = !_randomKerbals;
                _invokingUi = true;
                Invoke(nameof(SwitchMode), _switchWindowTime);
                return;
            }

            if (_randomKerbals) _uidialog = RandomKerbalDialog();
            else _uidialog = CustomKerbalDialog();
            _invokingUi = false;
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
            _kerbalToEdit = p;
            Debug.Log("[EarnYourStripes]: Generating New Kerbal " + p.name);
            EditKerbal();
        }

        DialogGUIVerticalLayout GenerateListOfKerbals()
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
            _kerbalToEdit = pcm;
            Invoke(nameof(EditKerbal), _switchWindowTime);
        }

        private void EditKerbal()
        {
            Debug.Log("[EarnYourStripes]: Editing " + _kerbalToEdit.name);
            _uidialog = EditKerbalDialog();
        }

        private PopupDialog EditKerbalDialog()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            LockCamera();
            dialogElements.Add(new DialogGUITextInput(_kerbalToEdit.name, false, 100, SetName, 30));
            DialogGUIBase[] horizontal = new DialogGUIBase[2];
            horizontal[0] = new DialogGUILabel(() => "Gender: " + _kerbalToEdit.gender);
            horizontal[1] = new DialogGUIButton("Change", SwitchGender, 50.0f, 20.0f, false);
            dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
            dialogElements.Add(new DialogGUILabel(() => "Class: " + _kerbalToEdit.trait));
            dialogElements.Add(TraitDialogOptions(_kerbalToEdit));
            horizontal[0] = new DialogGUILabel(() => "Stupidity: " + _kerbalToEdit.stupidity);
            horizontal[1] = new DialogGUISlider(() => _kerbalToEdit.stupidity, 0.0f, 1.0f, false, 100.0f, 30.0f, newValue => { _kerbalToEdit.stupidity = newValue; });
            dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
            horizontal[0] = new DialogGUILabel(() => "Courage: " + _kerbalToEdit.courage);
            horizontal[1] = new DialogGUISlider(() => _kerbalToEdit.courage, 0.0f, 1.0f, false, 100.0f, 30.0f, newValue => { _kerbalToEdit.courage = newValue; });
            dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
            dialogElements.Add(new DialogGUIButton("Done", () => ClearControlLock(true)));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("FirstKerbaliserEditKerbalDialog", "", "Earn Your Stripes", UISkinManager.defaultSkin,
                    new Rect(0.5f, 0.5f, 240.0f, 90.0f), dialogElements.ToArray()), false, UISkinManager.defaultSkin);

        }

        private void SwitchGender()
        {
            if (_kerbalToEdit.gender == ProtoCrewMember.Gender.Female) _kerbalToEdit.gender = ProtoCrewMember.Gender.Male;
            else _kerbalToEdit.gender = ProtoCrewMember.Gender.Female;
            Debug.Log("[EarnYourStripes]: " + _kerbalToEdit.name + " Gender Change");
        }

        private string SetName(string nameToSet)
        {
            Debug.Log("[EarnYourStripes]: Changing " + _kerbalToEdit.name + " to " + nameToSet);
            _kerbalToEdit.ChangeName(nameToSet);
            return nameToSet;
        }

        private PopupDialog BrokenDialog()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            dialogElements.Add(new DialogGUILabel("Your choices were invalid. Try again"));
            _randomKerbals = !_randomKerbals;
            _allowMales = true;
            _allowFemales = true;
            dialogElements.Add(new DialogGUIButton("OK", SwitchMode));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("FirstKerbaliserErrorDialog", "", "Earn Your Stripes", UISkinManager.defaultSkin,
                    new Rect(0.5f, 0.5f, 200.0f, 90.0f), dialogElements.ToArray()), false, UISkinManager.defaultSkin);

        }


        private void GenerateRandomKerbals(int numberOfRandomKerbals)
        {
            if (!ValidSelection())
            {
                Invoke(nameof(GenerateBrokenDialog), _switchWindowTime);
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
            _randomKerbals = true;
            if (_uidialog != null) _uidialog.Dismiss();
            EarnYourStripes.Instance.firstRun = false;
            if (switchMode) Invoke(nameof(SwitchMode), _switchWindowTime);
        }

        private bool ValidKerbal(ProtoCrewMember.Gender gender, string trait)
        {
            if (gender == ProtoCrewMember.Gender.Female && !_allowFemales) return false;
            if (gender == ProtoCrewMember.Gender.Male && !_allowMales) return false;
            if (!_traits[trait]) return false;
            return true;
        }

        private void GenerateBrokenDialog()
        {
            _uidialog = BrokenDialog();
        }

        private bool ValidSelection()
        {
            if (!_allowFemales && !_allowMales) return false;
            for (int i = 0; i < _traits.Count; i++)
            {
                if (_traits.ElementAt(i).Value) return true;
            }

            return false;
        }

        private Dictionary<string, bool> GetAvailableTraits()
        {
            ConfigNode[] experienceTraits = GameDatabase.Instance.GetConfigNodes("EXPERIENCE_TRAIT");
            Dictionary<string, bool> traitTitles = new Dictionary<string, bool>();
            for (int i = 0; i < experienceTraits.Count(); i++)
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

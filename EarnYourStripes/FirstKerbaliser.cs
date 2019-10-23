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
        private float _sizeOfWindow = 0;
        Dictionary<string, bool> _traits;
        private ProtoCrewMember _kerbalToEdit;
        

        private void Start()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().GenerateCrew) return;
            if (!EarnYourStripes.Instance.firstRun) return;
            _uidialog = RandomKerbalDialog();
            Debug.Log("[FirstKerbaliser].Start");
        }
        
        

        private PopupDialog RandomKerbalDialog()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            int numberOfRandomKerbals = 4;
            dialogElements.Add(new DialogGUIToggle(() => _randomKerbals, "Generate Random Kerbals", b => { SwitchMode(); }));
            DialogGUIBase[] horizontal = new DialogGUIBase[3];
            horizontal[0] = new DialogGUILabel(() => "Number of Kerbals: " + numberOfRandomKerbals );
            horizontal[1] = new DialogGUISpace(30.0f);
            horizontal[2] = new DialogGUISlider(() => numberOfRandomKerbals, 1, 10, true, 180.0f, 30.0f, newValue => { numberOfRandomKerbals = (int) newValue; });
            dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
            horizontal = new DialogGUIBase[2];
            horizontal[0] = new DialogGUIToggle(_allowFemales, "Allow Female Kerbals", b => _allowFemales = b);
            horizontal[1] = new DialogGUIToggle(_allowMales, "Allow Male Kerbals", b => _allowMales = b);
            dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
            dialogElements.Add(TraitDialogOptions());
            dialogElements.Add(new DialogGUISpace(5.0f));
            dialogElements.Add(new DialogGUIButton("I'm Done", () => GenerateRandomKerbals(numberOfRandomKerbals), true));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("FirstKerbaliserRandomKerbals", "", "Earn Your Stripes", UISkinManager.defaultSkin,
                    new Rect(0.5f, 0.5f, _sizeOfWindow, 210), dialogElements.ToArray()), false, UISkinManager.defaultSkin);
        }

        private DialogGUIHorizontalLayout TraitDialogOptions()
        {
            _traits = GetAvailableTraits();
            List<DialogGUIBase> horizontal = new List<DialogGUIBase>();
            Debug.Log("[FirstKerbaliser]: UI: Processing "+_traits.Count + " elements");
            for (int i = 0; i < _traits.Count; i++)
            {
                string s = _traits.ElementAt(i).Key;
                horizontal.Add(new DialogGUIToggle(_traits.ElementAt(i).Value, "Allow: " + s, b => { _traits[s] = b; }));
                Debug.Log("[FirstKerbaliser]: UI: Added "+_traits.ElementAt(i)+" to UI");
            }
            return new DialogGUIHorizontalLayout(horizontal.ToArray());
        }
        
        private DialogGUIHorizontalLayout TraitDialogOptions(ProtoCrewMember pcm)
        {
            _traits = GetAvailableTraits();
            List<DialogGUIBase> horizontal = new List<DialogGUIBase>();
            Debug.Log("[FirstKerbaliser]: UI: Processing "+_traits.Count + " elements");
            for (int i = 0; i < _traits.Count; i++)
            {
                string s = _traits.ElementAt(i).Key;
                horizontal.Add(new DialogGUIButton("Make " + s, () => KerbalRoster.SetExperienceTrait(pcm, s)));
                Debug.Log("[FirstKerbaliser]: UI: Added "+_traits.ElementAt(i)+" to UI");
            }
            return new DialogGUIHorizontalLayout(horizontal.ToArray());
        }
        
        private void SwitchMode()
        {
            if(_uidialog!= null) _uidialog.Dismiss();
            if (!_invokingUi)
            {
                _randomKerbals = !_randomKerbals;
                _invokingUi = true;
                Invoke(nameof(SwitchMode), 0.1f);
                return;
            }
            if (_randomKerbals) _uidialog = RandomKerbalDialog();
            else _uidialog = CustomKerbalDialog();
            _invokingUi = false;
        }

        private PopupDialog CustomKerbalDialog()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            dialogElements.Add(new DialogGUILabel("Select a Kerbal To Edit"));
            dialogElements.Add(GenerateListOfKerbals());
        }

        DialogGUIVerticalLayout GenerateListOfKerbals()
        {
            List<ProtoCrewMember> crew = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
            List<DialogGUIBase> verticalElements = new List<DialogGUIBase>();
            for (int i = 0; i < crew.Count; i++)
            {
                ProtoCrewMember pcm = crew.ElementAt(i);
                if(pcm.rosterStatus != ProtoCrewMember.RosterStatus.Available) continue;
                verticalElements.Add(new DialogGUIButton(pcm.displayName, () => SetKerbalToEdit(pcm), true));
            }
            return new DialogGUIVerticalLayout(verticalElements.ToArray());
        }

        private void SetKerbalToEdit(ProtoCrewMember pcm)
        {
            _kerbalToEdit = pcm;
            Invoke(nameof(EditKerbal), 0.5f);
        }

        private void EditKerbal()
        {
            _uidialog = EditKerbalDialog();
        }

        private PopupDialog EditKerbalDialog()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            dialogElements.Add(new DialogGUITextInput(_kerbalToEdit.name, false, 100, SetName, 30 ));
            DialogGUIBase[] horizontal = new DialogGUIBase[2];
            horizontal[0] = new DialogGUILabel(() => "Gender: "+_kerbalToEdit.gender);
            horizontal[1] = new DialogGUIButton("Change", SwitchGender);
            dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
            dialogElements.Add(new DialogGUILabel( () => "Class: "+_kerbalToEdit.trait));
            dialogElements.Add(TraitDialogOptions(_kerbalToEdit));
            horizontal[0] = new DialogGUILabel(() => "Stupidity: "+_kerbalToEdit.stupidity);
            horizontal[1] = new DialogGUISlider(() => _kerbalToEdit.stupidity, 0.0f, 1.0f, false, 100.0f, 30.0f, newValue => { _kerbalToEdit.stupidity = newValue; });
            dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
            horizontal[0] = new DialogGUILabel(() => "Courage: "+_kerbalToEdit.courage);
            horizontal[1] = new DialogGUISlider(() => _kerbalToEdit.stupidity, 0.0f, 1.0f, false, 100.0f, 30.0f, newValue => { _kerbalToEdit.stupidity = newValue; });
            dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
        }

        private void SwitchGender()
        {
            if (_kerbalToEdit.gender == ProtoCrewMember.Gender.Female) _kerbalToEdit.gender = ProtoCrewMember.Gender.Male;
            else _kerbalToEdit.gender = ProtoCrewMember.Gender.Female;
        }

        private string SetName(string nameToSet)
        {
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
                Invoke(nameof(GenerateBrokenDialog), 0.5f);
                return;
            }
            int i = 0;
            Debug.Log("[EarnYourStripes]: Running Random Crew Generator");
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
                i++;
            }
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
                Debug.Log("[FirstKerbaliser]: Trying to Add Trait "+i);
                ConfigNode cn = experienceTraits.ElementAt(i);
                string trait = cn.GetValue("title");
                if (traitTitles.ContainsKey(trait)) continue;
                if (trait == "Tourist") continue;
                traitTitles.Add(cn.GetValue("title"), true);
                Debug.Log("[FirstKerbaliser]: Added Trait "+trait);
            }
            Debug.Log("[FirstKerbaliser]: Loaded "+traitTitles.Count+" traits");
            _sizeOfWindow = traitTitles.Count * 130.0f;
            return traitTitles;
        }
    }
}

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
        private bool _allowFemales;
        private bool _allowMales;
        private bool _randomKerbals = true;
        private bool _invokingUi;
        

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
            DialogGUIBase[] horizontal = new DialogGUIBase[2];
            horizontal[0] = new DialogGUILabel("Number of Kerbals: "+numberOfRandomKerbals, true);
            horizontal[1] = new DialogGUISlider(() => numberOfRandomKerbals, 1, 10, true, 100.0f, 30.0f, newValue => { numberOfRandomKerbals = (int) newValue; });
            dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
            horizontal = new DialogGUIBase[2];
            horizontal[0] = new DialogGUIToggle(_allowFemales, "Allow Female Kerbals", b => _allowFemales = b);
            horizontal[1] = new DialogGUIToggle(_allowMales, "Allow Male Kerbals", b => _allowMales = b);
            dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
            dialogElements.Add(TraitDialogOptions());
            dialogElements.Add(new DialogGUIButton("I'm Done", () => GenerateRandomKerbals(numberOfRandomKerbals)));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("FirstKerbaliserRandomKerbals", "", "Earn Your Stripes", UISkinManager.defaultSkin,
                    new Rect(0.5f, 0.5f, 1000, 500), dialogElements.ToArray()), false, UISkinManager.defaultSkin);
        }

        private DialogGUIHorizontalLayout TraitDialogOptions()
        {
            Dictionary<string, bool> traits = GetAvailableTraits();
            DialogGUIBase[] horizontal = new DialogGUIBase[traits.Count];
            Debug.Log("[FirstKerbaliser]: UI: Processing "+traits.Count + "elements");
            for (int i = 0; i < traits.Count; i++)
            {
                string s = traits.ElementAt(i).Key;
                horizontal[i] = new DialogGUIToggle(traits.ElementAt(i).Value, "Allow: " + s, b => { traits[s] = b; });
                Debug.Log("[FirstKerbaliser]: UI: Added "+traits.ElementAt(i)+" to UI");
            }
            return new DialogGUIHorizontalLayout(horizontal);
        }
        
        private void SwitchMode()
        {
            _uidialog.Dismiss();
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
            throw new NotImplementedException();
        }

        private PopupDialog BrokenDialog()
        {
            throw new NotImplementedException();
        }


        private void GenerateRandomKerbals(int numberOfRandomKerbals)
        {
            throw new NotImplementedException();
        }
        private static Dictionary<string, bool> GetAvailableTraits()
        {
            ConfigNode[] experienceTraits = GameDatabase.Instance.GetConfigNodes("EXPERIENCE_TRAIT");
            Dictionary<string, bool> traitTitles = new Dictionary<string, bool>();
            for (int i = 0; i < experienceTraits.Count(); i++)
            {
                Debug.Log("[FirstKerbaliser]: Trying to Add Trait "+i);
                ConfigNode cn = experienceTraits.ElementAt(i);
                string trait = cn.GetValue("title");
                if (traitTitles.ContainsKey(trait)) continue;
                traitTitles.Add(cn.GetValue("title"), true);
                Debug.Log("[FirstKerbaliser]: Added Trait "+trait);
            }
            Debug.Log("[FirstKerbaliser]: Loaded "+traitTitles.Count+" traits");
            return traitTitles;
        }
    }
}

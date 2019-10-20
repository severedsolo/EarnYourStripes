using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Experience;
using UnityEngine;

namespace EarnYourStripes
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    internal class FirstKerbaliser : MonoBehaviour
    {
        private PopupDialog _uidialog;
        private bool _uiWaitEnforced = false;
        private UiUtilities _ui;
        private Dictionary<string, bool> _availableAllowedTraits = new Dictionary<string, bool>();
        private bool _allowFemales;
        private bool _allowMales;

        private void Start()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().GenerateCrew) return;
            if (!EarnYourStripes.Instance.firstRun) return;
            _ui = new UiUtilities();
            _uidialog = FirstDialog();
        }

        private PopupDialog FirstDialog()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            dialogElements.Add(new DialogGUILabel("Looks like you are starting a new game. Want to customise your Kerbals?", _ui.Header()));
            DialogGUIBase[] horizontal = new DialogGUIBase[2];
            horizontal[0] = new DialogGUIButton("Yes", () => SwitchDialog("randomQuestion"), true);
            horizontal[1] = new DialogGUIButton("No", () => _uidialog.Dismiss(), false);
            dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("FirstKerbaliserFirst", "", "Earn Your Stripes", UISkinManager.defaultSkin,
                    new Rect(0.5f, 0.5f, 200, 100), dialogElements.ToArray()), false, UISkinManager.defaultSkin);
        }

        private PopupDialog RandomQuestionDialog()
        {
            SwitchUiWait();
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            dialogElements.Add(new DialogGUILabel("Great! Do you want Random Kerbals or to customise them yourself?", _ui.Header()));
            DialogGUIBase[] horizontal = new DialogGUIBase[3];
            horizontal[0] = new DialogGUIButton("Random Kerbals", () => SwitchDialog("randomKerbals"), true);
            horizontal[1] = new DialogGUIButton("Customise Them Myself", () => SwitchDialog("customKerbals"), true);
            horizontal[2] = new DialogGUIButton("Actually I changed my mind", () => _uidialog.Dismiss(), false);
            dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("FirstKerbaliserRandomQuestion", "", "Earn Your Stripes", UISkinManager.defaultSkin,
                    new Rect(0.5f, 0.5f, 200, 100), dialogElements.ToArray()), false, UISkinManager.defaultSkin);
        }

        private PopupDialog RandomKerbalDialog()
        {
            SwitchUiWait();
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            int numberOfRandomKerbals = 4;
            dialogElements.Add(new DialogGUITextInput("Number of Kerbals", false, 2, s =>
            {
                numberOfRandomKerbals = Convert.ToInt32(s);
                return Convert.ToString(numberOfRandomKerbals);
            }));
            _availableAllowedTraits = GetAvailableTraits();
            DialogGUIBase[] horizontal = new DialogGUIBase[2];
            horizontal[0] = new DialogGUIToggle(_allowFemales, "Allow Female Kerbals", b => _allowFemales = b);
            horizontal[1] = new DialogGUIToggle(_allowMales, "Allow Male Kerbals", b => _allowMales = b);
            dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
            horizontal = new DialogGUIBase[_availableAllowedTraits.Count];
            for (int i = 0; i < _availableAllowedTraits.Count; i++)
            {
                string s = _availableAllowedTraits.ElementAt(i).Key;
                horizontal[i] = new DialogGUIToggle(_availableAllowedTraits.ElementAt(i).Value, "Allow: " + s, b => { _availableAllowedTraits[s] = b; });
            }
            dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
            horizontal = new DialogGUIBase[2];
            horizontal[0] = new DialogGUIButton("I'm Done", () => GenerateRandomKerbals(numberOfRandomKerbals)); 
            horizontal[1] = new DialogGUIButton("I changed my mind. Customise my Kerbals", () => SwitchDialog("customKerbals"), true);
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("FirstKerbaliserRandomKerbals", "", "Earn Your Stripes", UISkinManager.defaultSkin,
                    new Rect(0.5f, 0.5f, 200, 100), dialogElements.ToArray()), false, UISkinManager.defaultSkin);
        }

        private PopupDialog CustomKerbalDialog()
        {
            throw new NotImplementedException();
        }

        private PopupDialog BrokenDialog()
        {
            throw new NotImplementedException();
        }

        private void SwitchDialog(string reason)
        {
            if (!_uiWaitEnforced)
            {
                Invoke(nameof(SwitchDialog), 0.5f);
                SwitchUiWait();
                return;
            }

            switch (reason)
            {
                case "randomQuestion":
                    _uidialog = RandomQuestionDialog();
                    break;
                case "randomKerbals":
                    _uidialog = RandomKerbalDialog();
                    break;
                case "customKerbals":
                    _uidialog = CustomKerbalDialog();
                    break;
                default:
                    _uidialog = BrokenDialog();
                    break;
            }
        }

        private void SwitchUiWait()
        {
            _uiWaitEnforced = !_uiWaitEnforced;
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
                ConfigNode cn = experienceTraits.ElementAt(i);
                traitTitles.Add(cn.GetValue("title"), true);
            }
            return traitTitles;
        }
    }
}

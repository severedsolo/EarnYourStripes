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
        private List<DialogGUIBase> _dialogElements;

        private void Start()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().GenerateCrew) return;
            if (!EarnYourStripes.Instance.firstRun) return;
            _uidialog = GenerateFirstDialog();
        }

        private PopupDialog GenerateFirstDialog()
        {
            _dialogElements = new List<DialogGUIBase>();
            _dialogElements.Add(new DialogGUILabel("Looks like you are starting a new game. Want to customise your Kerbals?"));
            DialogGUIBase[] horizontal = new DialogGUIBase[2];
            horizontal[0] = new DialogGUIButton("Yes", () => SwitchDialog("random"), true);
            horizontal[1] = new DialogGUIButton("No", () => _uidialog.Dismiss(), false);
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("FirstKerbaliserFirst", "", "Earn Your Stripes", UISkinManager.defaultSkin,
                    new Rect(0.5f, 0.5f, 200, 100), _dialogElements.ToArray()), false, UISkinManager.defaultSkin);
        }

        private void SwitchDialog(string reason)
        {
            if (!_uiWaitEnforced)
            {
                Invoke(nameof(SwitchDialog), 0.5f);
                SwitchUiWait();
                return;
            }
            _uidialog = GenerateRandomDialog();
        }

        private void SwitchUiWait()
        {
            _uiWaitEnforced = !_uiWaitEnforced;
        }

        private PopupDialog GenerateRandomDialog()
        {
            SwitchUiWait();
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
        }
    }

using System;
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

        private void Start()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<StripeSettings>().GenerateCrew) return;
            if (!EarnYourStripes.Instance.firstRun) return;
            _uidialog = GenerateFirstDialog();
        }

        private PopupDialog GenerateFirstDialog()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            dialogElements.Add(new DialogGUILabel("Looks like you are starting a new game. Want to customise your Kerbals?"));
            DialogGUIBase[] horizontal = new DialogGUIBase[2];
            horizontal[0] = new DialogGUIButton("Yes", delegate {  });
        }
    }
}

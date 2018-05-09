using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EarnYourStripes
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    class FirstKerbaliser : MonoBehaviour
    {
        FirstKerbaliser instance;
        bool firstRun;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {

        }

    }
}

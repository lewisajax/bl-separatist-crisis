﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Entities
{
    public class AbilityAgent
    {
        public Agent Agent { get; private set; }

        public AbilityAgent(Agent agent)
        {
            this.Agent = agent;
        }
    }
}

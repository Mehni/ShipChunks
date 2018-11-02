using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Harmony;
using System.Reflection.Emit;
using System.Reflection;

namespace ShipChunkDrop_Transpiler
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("mehni.rimworld.shipchunks.main");

            harmony.Patch(AccessTools.Method(typeof(IncidentWorker_ShipChunkDrop), "SpawnChunk"), null, null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(IncidentWorker_ShipChunkDrop_SpawnChunk_Transpiler)));
        }

        public static IEnumerable<CodeInstruction> IncidentWorker_ShipChunkDrop_SpawnChunk_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo chunkSelector = AccessTools.Method(typeof(HarmonyPatches), nameof(SelectChunkFromAvailableOptions));

            List<CodeInstruction> instructionList = instructions.ToList();
            int i = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldsfld)
                {
                    if (i == 1)
                    {
                        instruction.opcode = OpCodes.Call;
                        instruction.operand = chunkSelector;
                    }
                    i++;
                }
                yield return instruction;
            }
        }

        public static ThingDef SelectChunkFromAvailableOptions()
        {
            IEnumerable<ThingDef> chunk = from defs in DefDatabase<ThingDef>.AllDefs
                                            where defs.defName.StartsWith("ShipChunk") && !defs.defName.Contains("Incoming")
                                            select defs;

            return chunk.RandomElement();
        }
    }
}
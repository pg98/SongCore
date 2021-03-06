﻿using HarmonyLib;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
namespace SongCore.HarmonyPatches
{
    
    [HarmonyPatch(typeof(BeatmapData))]
    [HarmonyPatch("AddBeatmapObjectData", MethodType.Normal)]
    
    class BeatmapDataLoaderGetBeatmapDataFromBeatmapSaveData
    {
        static readonly MethodInfo clampMethod = SymbolExtensions.GetMethodInfo(() => Clamp(0, 0, 0));
        static readonly CodeInstruction[] clampInstructions = new CodeInstruction[] { new CodeInstruction(OpCodes.Ldc_I4_0),
            new CodeInstruction(OpCodes.Ldc_I4_3), new CodeInstruction(OpCodes.Call, clampMethod) };

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (instructionList[i].opcode == OpCodes.Ldelem_Ref)
                {
                    if (instructionList[i + 2].opcode != OpCodes.Callvirt || instructionList[i + 1].opcode != OpCodes.Ldarg_1)
                    {
                        continue;
                    }
                //    Type varType = ((LocalVariableInfo)(instructionList[i - 2].operand)).LocalType;
                //    if (varType == typeof(BeatmapObjectData))
                //    {
                        
                        Utilities.Logging.logger.Debug($"{i}Inserting Clamp Instruction for SaveData Reading");
                        instructionList.InsertRange(i, clampInstructions);
                        i += clampInstructions.Count();
                //    }

                }
            }

            return instructionList.AsEnumerable();
        }

        static int Clamp(int input, int min, int max)
        {
            return Math.Min(Math.Max(input, min), max);
        }
    }

    [HarmonyPatch(typeof(NotesInTimeRowProcessor))]
    [HarmonyPatch("ProcessAllNotesInTimeRow", MethodType.Normal)]

    class NoteProcessorClampPatch
    {
        static readonly MethodInfo clampMethod = SymbolExtensions.GetMethodInfo(() => Clamp(0, 0, 0));
        static readonly CodeInstruction[] clampInstructions = new CodeInstruction[] { new CodeInstruction(OpCodes.Ldc_I4_0),
            new CodeInstruction(OpCodes.Ldc_I4_3), new CodeInstruction(OpCodes.Call, clampMethod) };

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (instructionList[i].opcode == OpCodes.Ldelem_Ref)
                {
                    if (instructionList[i - 1].opcode != OpCodes.Callvirt || instructionList[i - 2].opcode != OpCodes.Ldloc_2)
                    {
                        continue;
                    }
                    //    Type varType = ((LocalVariableInfo)(instructionList[i - 2].operand)).LocalType;
                    //    if (varType == typeof(BeatmapObjectData))
                    //    {

                    Utilities.Logging.logger.Debug($"{i}Inserting Clamp Instruction for Note Processor");
                    instructionList.InsertRange(i, clampInstructions);
                    i += clampInstructions.Count();
                    //    }

                }
            }

            return instructionList.AsEnumerable();
        }

        static int Clamp(int input, int min, int max)
        {
            return Math.Min(Math.Max(input, min), max);
        }
    }


    [HarmonyPatch(typeof(BeatmapData))]
    [HarmonyPatch("beatmapObjectsData", MethodType.Getter)]

    class BeatmapObjectsDataClampPatch
    {
        public static bool Prefix(BeatmapLineData[] ____beatmapLinesData, BeatmapData __instance, ref IEnumerable<BeatmapObjectData> __result)
        {
            IEnumerable<BeatmapObjectData> getObjects(BeatmapLineData[] _beatmapLinesData)
            {
                BeatmapLineData[] beatmapLinesData = _beatmapLinesData;
                int[] idxs = new int[beatmapLinesData.Length];
                for (; ; )
                {
                    BeatmapObjectData minBeatmapObjectData = null;
                    float num = float.MaxValue;
                    for (int i = 0; i < beatmapLinesData.Length; i++)
                    {
                        if (idxs[i] < beatmapLinesData[i].beatmapObjectsData.Count)
                        {
                            BeatmapObjectData beatmapObjectData = beatmapLinesData[i].beatmapObjectsData[idxs[i]];
                            float time = beatmapObjectData.time;
                            if (time < num)
                            {
                                num = time;
                                minBeatmapObjectData = beatmapObjectData;
                            }
                        }
                    }
                    if (minBeatmapObjectData == null)
                    {
                        break;
                    }
                    yield return minBeatmapObjectData;
                    idxs[minBeatmapObjectData.lineIndex > 3 ? 3 : minBeatmapObjectData.lineIndex < 0 ? 0 : minBeatmapObjectData.lineIndex]++;
                    minBeatmapObjectData = null;
                }
                yield break;
                yield break;
            }
            __result = getObjects(____beatmapLinesData);
            return false;
        }
        /*
        static readonly MethodInfo clampMethod = SymbolExtensions.GetMethodInfo(() => Clamp(0, 0, 0));
        static readonly CodeInstruction[] clampInstructions = new CodeInstruction[] { new CodeInstruction(OpCodes.Ldc_I4_0),
            new CodeInstruction(OpCodes.Ldc_I4_3), new CodeInstruction(OpCodes.Call, clampMethod) };

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                Utilities.Logging.logger.Debug($"{instructionList[i].opcode}");
                if (instructionList[i].opcode == OpCodes.Ldelema)
                {
                    if (instructionList[i - 1].opcode != OpCodes.Callvirt || instructionList[i - 2].opcode != OpCodes.Ldfld)
                    {
                        continue;
                    }
                    //    Type varType = ((LocalVariableInfo)(instructionList[i - 2].operand)).LocalType;
                    //    if (varType == typeof(BeatmapObjectData))
                    //    {

                    Utilities.Logging.logger.Debug($"{i}Inserting Clamp Instruction for BeatmapObjectsData Getter");
                    instructionList.InsertRange(i, clampInstructions);
                    i += clampInstructions.Count();
                    //    }

                }
            }

            return instructionList.AsEnumerable();
        }

        static int Clamp(int input, int min, int max)
        {
            return Math.Min(Math.Max(input, min), max);
        }
        */
    }

}
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Localization;
using Sandbox.Graphics.GUI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VRage;

#pragma warning disable IDE0060
// ReSharper disable UnusedParameter.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
namespace ClientPlugin.Patches
{
    [HarmonyPatch(ClassType, "Init")]
    public class GridProductionControllerPatch
    {
        private const string ClassType = "Sandbox.Game.Gui.MyTerminalProductionController";
        private static readonly FieldInfo ComboBoxFieldInfo;
        private static readonly FieldInfo AssemblersByKeyFieldInfo;

        static GridProductionControllerPatch()
        {
            ComboBoxFieldInfo = AccessTools.Field(AccessTools.TypeByName(ClassType), "m_comboboxAssemblers");
            AssemblersByKeyFieldInfo = AccessTools.Field(AccessTools.TypeByName(ClassType), "m_assemblersByKey");
        }

        // We want to patch this after BetterInventorySearch, as that just calls SortItemsByValueText
        // ReSharper disable once StringLiteralTypo
        [HarmonyAfter("avaness.BetterInventorySearch")]
        public static void Postfix(object __instance,
            IMyGuiControlsParent controlsParent,
            MyCubeGrid grid,
            MyCubeBlock currentBlock)
        {
            if (grid == null)
                return;

            if (!(ComboBoxFieldInfo.GetValue(__instance) is MyGuiControlCombobox comboboxAssemblers))
                return;

            // Just sorts by name
            //comboboxAssemblers.SortItemsByValueText();

            if (!(AssemblersByKeyFieldInfo.GetValue(__instance) is Dictionary<int, MyAssembler> assemblersByKey))
                return;

            // For whatever reason this only works if performed twice.
            //comboboxAssemblers.CustomSortItems((item1, item2) =>
            //{
            //    var assembler1 = assemblersByKey[(int) item1.Key];
            //    var assembler2 = assemblersByKey[(int) item2.Key];
            //    if (assembler1 == currentBlock)
            //        return -1;
            //    if (!assembler1.IsFunctional && assembler2.IsFunctional)
            //        return 1;
            //    return string.Compare(item1.Value.ToString(), item2.Value.ToString(), StringComparison.CurrentCulture);
            //});

            // This is the best method I've found so far:
            // Rebuild the combobox by using m_assemblersByKey
            var incompleteAssemblerName = new StringBuilder();
            comboboxAssemblers.ClearItems();
            foreach (var (key, myAssembler) in assemblersByKey.OrderBy(x =>
                     {
                         var myAssembler = x.Value;
                         return myAssembler == currentBlock
                             ? -1
                             : myAssembler.IsFunctional ? 0 : 10000;
                     }).ThenBy(x => x.Value.CustomName.ToString()))
            {
                if (!myAssembler.IsFunctional)
                {
                    incompleteAssemblerName.Clear();
                    incompleteAssemblerName.AppendStringBuilder(myAssembler.CustomName);
                    incompleteAssemblerName.AppendStringBuilder(
                        MyTexts.Get(MySpaceTexts.Terminal_BlockIncomplete));
                    comboboxAssemblers.AddItem(key, incompleteAssemblerName);
                }
                else
                    comboboxAssemblers.AddItem(key, myAssembler.CustomName);
            }

            // Reset the selected index.
            comboboxAssemblers.SelectItemByIndex(0);
            Plugin.Instance.Log.Debug("Sorted assemblers!");
        }
    }
}
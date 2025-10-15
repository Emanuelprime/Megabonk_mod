using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Inventory__Items__Pickups.Chests;
using Assets.Scripts.Inventory__Items__Pickups.Items;
using Assets.Scripts.Inventory__Items__Pickups.Interactables;
using Il2CppInterop.Runtime.Injection;

namespace Command_Artifact
{
    public class CommandUI : MonoBehaviour
    {
        public void OnGUI()
        {
            if (!Plugin.IsChoosingItem) return;

            GUI.Box(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 200, 400, 400), "Artefato do Comando: Escolha um Item");

            for (int i = 0; i < Plugin.itemsToShow.Count; i++)
            {
                if (i >= 10) break;

                try
                {
                    if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 150 + (i * 30), 300, 25), Plugin.itemsToShow[i].GetName()))
                    {
                        Plugin.Instance.Log.LogInfo($"Jogador escolheu: {Plugin.itemsToShow[i].GetName()}");
                        Plugin.Instance.GiveItemAndPay(Plugin.itemsToShow[i]);
                        Plugin.CloseCommandUI();
                    }
                }
                catch (System.Exception ex)
                {
                    Plugin.Instance.Log.LogError($"Erro ao desenhar o botão para o item nº {i}: {ex.Message}");
                    Plugin.CloseCommandUI();
                }
            }
        }
    }

    [BepInPlugin("Prime_Purpura.Command_Artifact", "Command_Artifact", "1.3.0")]
    public class Plugin : BasePlugin
    {
        private readonly Harmony harmony = new Harmony("Prime_Purpura.Command_Artifact");
        public static Plugin Instance;

        public static List<ItemData> AllItemsMasterList = new List<ItemData>();
        public static InteractableChest currentChest;
        public static bool IsChoosingItem = false;
        public static List<ItemData> itemsToShow = new List<ItemData>();

        public override void Load()
        {
            Instance = this;
            ClassInjector.RegisterTypeInIl2Cpp<CommandUI>();

            GameObject uiHost = new GameObject("CommandUI_Host");
            uiHost.AddComponent<CommandUI>();
            GameObject.DontDestroyOnLoad(uiHost);

            // A chamada para BuildMasterItemList() foi REMOVIDA daqui.

            Log.LogInfo("--- MOD ATUALIZADO (v1.3) COM CONSTRUÇÃO JUST-IN-TIME ---");
            harmony.PatchAll();
        }

        public static void OpenCommandUI()
        {
            IsChoosingItem = true;
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public static void CloseCommandUI()
        {
            IsChoosingItem = false;
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        // A função para construir a lista continua aqui, mas só será chamada quando necessário.
        public void BuildMasterItemList()
        {
            Log.LogInfo("Construindo lista mestra de itens (Just-in-Time)...");
            foreach (EItem itemEnum in System.Enum.GetValues(typeof(EItem)))
            {
                try
                {
                    ItemData item = DataManager.Instance.GetItem(itemEnum);
                    if (item != null && item.inItemPool)
                    {
                        AllItemsMasterList.Add(item);
                        //Log.LogInfo($"Adicionado: {item.GetName()} (Raridade: {item.rarity})"); // Descomente para depurar raridades
                    }
                }
                catch (System.Exception) { /* Ignora itens fantasma */ }
            }
            Log.LogInfo($"Lista mestra construída com {AllItemsMasterList.Count} itens.");
        }

        public void GiveItemAndPay(ItemData chosenItem)
        {
            // TODO: Preencher com a lógica de pagar e dar o item
            Log.LogInfo("Item dado e pagamento efetuado (lógica a ser implementada)!");
        }
    }

    [HarmonyPatch(typeof(InteractableChest), "Interact")]
    public static class InteractPatch
    {
        [HarmonyPrefix]
        public static bool InterceptInteraction(InteractableChest __instance, ref bool __result)
        {
            if (Plugin.IsChoosingItem)
            {
                __result = false;
                return false;
            }

            if (!__instance.CanAfford())
            {
                return true;
            }

            // --- AQUI ESTÁ A CORREÇÃO "JUST-IN-TIME" ---
            // Verificamos se a lista mestra já foi construída.
            if (Plugin.AllItemsMasterList.Count == 0)
            {
                // Se não foi, nós a construímos AGORA, na primeira vez que um baú é aberto.
                Plugin.Instance.BuildMasterItemList();
            }
            // ---------------------------------------------

            Plugin.currentChest = __instance;
            var chestType = __instance.chestType;
            EItemRarity sorteada = RollRarityForChest(chestType);

            Plugin.itemsToShow = Plugin.AllItemsMasterList.Where(item => item.rarity == sorteada).ToList();

            Plugin.Instance.Log.LogInfo($"Sorteio: {sorteada}. Encontrado(s) {Plugin.itemsToShow.Count} item(ns) para escolher.");

            Plugin.OpenCommandUI();

            __result = true;
            return false;
        }

        private static EItemRarity RollRarityForChest(EChest chestType)
        {
            float roll = UnityEngine.Random.Range(0f, 100f);

            if (chestType == EChest.Normal || chestType == EChest.Free)
            {
                if (roll < 75f) return EItemRarity.Common;
                if (roll < 95f) return EItemRarity.Rare;
                return EItemRarity.Epic;
            }

            if (chestType == EChest.Corrupt)
            {
                return EItemRarity.Corrupted;
            }

            return EItemRarity.Common;
        }
    }
}
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

                        // Desliga a UI e restaura o jogo
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

    [BepInPlugin("Prime_Purpura.Command_Artifact", "Command_Artifact", "1.1.0")]
    public class Plugin : BasePlugin
    {
        private readonly Harmony harmony = new Harmony("Prime_Purpura.Command_Artifact");
        public static Plugin Instance;

        // --- MUDANÇA 1: OTIMIZAÇÃO ---
        // Guardamos a lista mestra de itens aqui, para construir apenas uma vez.
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

            // --- MUDANÇA 1 (CONTINUAÇÃO): Construímos a lista mestra na inicialização ---
            BuildMasterItemList();

            Log.LogInfo("--- MOD ATUALIZADO COM OTIMIZAÇÃO E CONTROLE DE ESTADO ---");
            harmony.PatchAll();
        }

        // --- NOVAS FUNÇÕES DE CONTROLE ---
        public static void OpenCommandUI()
        {
            IsChoosingItem = true;
            Time.timeScale = 0f; // Pausa o jogo
            Cursor.visible = true; // Mostra o cursor
            Cursor.lockState = CursorLockMode.None; // Destrava o cursor
        }

        public static void CloseCommandUI()
        {
            IsChoosingItem = false;
            Time.timeScale = 1f; // Retoma o tempo do jogo
            Cursor.visible = false; // Esconde o cursor
            Cursor.lockState = CursorLockMode.Locked; // Trava o cursor no centro (padrão de jogos)
        }

        private void BuildMasterItemList()
        {
            Log.LogInfo("Construindo lista mestra de itens...");
            foreach (EItem itemEnum in System.Enum.GetValues(typeof(EItem)))
            {
                try
                {
                    ItemData item = DataManager.Instance.GetItem(itemEnum);
                    if (item != null && item.inItemPool)
                    {
                        AllItemsMasterList.Add(item);
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

            Plugin.currentChest = __instance;
            var chestType = __instance.chestType;
            EItemRarity sorteada = RollRarityForChest(chestType);

            // --- MUDANÇA 1 (FINAL): Usamos a lista mestra já pronta, que é muito mais rápido ---
            Plugin.itemsToShow = Plugin.AllItemsMasterList.Where(item => item.rarity == sorteada).ToList();

            Plugin.Instance.Log.LogInfo($"Sorteio: {sorteada}. Encontrado(s) {Plugin.itemsToShow.Count} item(ns) para escolher.");

            // --- MUDANÇA 2: Usamos nossa função de controle para abrir a UI ---
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
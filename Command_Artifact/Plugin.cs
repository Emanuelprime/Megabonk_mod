using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Collections.Generic;
using System.Linq; //filtrar lista
using Assets.Scripts.Inventory__Items__Pickups.Chests; // InteractableChest
using Assets.Scripts.Inventory__Items__Pickups.Items; // EItem e EItemRarity
using Assets.Scripts.Inventory__Items__Pickups.Interactables; // EChest
using Il2CppInterop.Runtime.Injection; // Essencial para registrar componentes

namespace Command_Artifact
{
    // --- NOSSO "ADAPTADOR" PARA O MOTOR DO JOGO ---
    public class CommandUI : MonoBehaviour
    {
        // A classe MonoBehaviour não precisa de um construtor customizado.
        // Removendo o construtor problemático, o C# usará o padrão, que é o correto.

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
                        Plugin.IsChoosingItem = false;
                    }
                }
                catch (System.Exception ex)
                {
                    Plugin.Instance.Log.LogError($"Erro ao desenhar o botão para o item nº {i} ({Plugin.itemsToShow[i].GetName()}). Erro: {ex.Message}");
                    Plugin.IsChoosingItem = false;
                }
            }
        }
    }

    // --- SUA CLASSE PLUGIN PRINCIPAL ---
    [BepInPlugin("Prime_Purpura.Command_Artifact", "Command_Artifact", "1.0.0")]
    public class Plugin : BasePlugin
    {
        private readonly Harmony harmony = new Harmony("Prime_Purpura.Command_Artifact");
        public static Plugin Instance;
        public static InteractableChest currentChest;
        public static bool IsChoosingItem = false;
        public static List<ItemData> itemsToShow = new List<ItemData>();

        public override void Load()
        {
            Instance = this;

            // 1. REGISTRAMOS nosso componente customizado
            ClassInjector.RegisterTypeInIl2Cpp<CommandUI>();

            // 2. Criamos o objeto e adicionamos o componente
            GameObject uiHost = new GameObject("CommandUI_Host");
            uiHost.AddComponent<CommandUI>();
            GameObject.DontDestroyOnLoad(uiHost);

            Log.LogInfo("--- MOD ATUALIZADO E PRONTO ---");
            harmony.PatchAll();
        }

        public void GiveItemAndPay(ItemData chosenItem)
        {
            // TODO: (Próximo Passo) Implementar a lógica de pagamento e entrega
            Log.LogInfo("Item dado e pagamento efetuado (lógica a ser implementada)!");
        }
    }

    // --- O PATCH ---
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
                Plugin.Instance.Log.LogInfo("Não pode pagar. Deixando o jogo original lidar com isso.");
                return true;
            }

            Plugin.Instance.Log.LogInfo("Pode pagar! Interceptando interação...");
            Plugin.currentChest = __instance;
            var chestType = __instance.chestType;
            Plugin.Instance.Log.LogInfo($"Tipo do baú: {chestType}");

            EItemRarity sorteada = RollRarityForChest(chestType);
            Plugin.Instance.Log.LogInfo($"Sorteio de raridade do baú {chestType}: {sorteada}!");

            List<ItemData> allItemsInGame = new List<ItemData>();
            foreach (EItem itemEnum in System.Enum.GetValues(typeof(EItem)))
            {
                try
                {
                    ItemData item = DataManager.Instance.GetItem(itemEnum);
                    if (item != null && item.inItemPool)
                    {
                        allItemsInGame.Add(item);
                    }
                }
                catch (System.Exception) { /* Ignora itens fantasma */ }
            }

            Plugin.itemsToShow = allItemsInGame.Where(item => item.rarity == sorteada).ToList();

            Plugin.Instance.Log.LogInfo($"Filtrando para a raridade {sorteada}. Encontrado(s) {Plugin.itemsToShow.Count} item(ns) correspondente(s).");

            Plugin.IsChoosingItem = true;

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
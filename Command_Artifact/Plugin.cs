using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Collections.Generic;
using System.Linq; //filtrar lista
using Assets.Scripts.Inventory__Items__Pickups.Chests; //  InteractableChest
using Assets.Scripts.Inventory__Items__Pickups.Items; //  EItem e EItemRarity
using Assets.Scripts.Inventory__Items__Pickups.Interactables; // EChest

namespace Command_Artifact
{
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
            Log.LogInfo("--- MOD ATUALIZADO COM SUCESSO! VERSÃO COM TRY-CATCH UNIVERSAL ---");
            harmony.PatchAll();
        }

        public void OnGUI()
        {
            if (!IsChoosingItem) return;

            GUI.Box(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 200, 400, 400), "Artefato do Comando: Escolha um Item");

            for (int i = 0; i < itemsToShow.Count; i++)
            {
                if (i >= 10) break;

                if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 150 + (i * 30), 300, 25), itemsToShow[i].GetName()))
                {
                    Log.LogInfo($"Jogador escolheu: {itemsToShow[i].GetName()}");
                    GiveItemAndPay(itemsToShow[i]);
                    IsChoosingItem = false;
                }
            }
        }

        private void GiveItemAndPay(ItemData chosenItem)
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

            // --- LÓGICA DE SORTEIO E FILTRAGEM ATUALIZADA ---

            // 1. SORTEAMOS a raridade do item, em vez de traduzir diretamente
            EItemRarity sorteada = RollRarityForChest(chestType);
            Plugin.Instance.Log.LogInfo($"Sorteio de raridade do baú {chestType}: {sorteada}!");

            // 2. Construímos nossa lista mestra de todos os itens do jogo
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

            // 3. Filtramos a lista pela raridade que o nosso sorteio acabou de decidir
            Plugin.itemsToShow = allItemsInGame.Where(item => item.rarity == sorteada).ToList();

            Plugin.Instance.Log.LogInfo($"Filtrando para a raridade {sorteada}. Encontrado(s) {Plugin.itemsToShow.Count} item(ns) correspondente(s).");

            // ----------------------------------------------------

            // Ativamos nossa UI
            Plugin.IsChoosingItem = true;

            // Cancelamos o método original do jogo
            __result = true;
            return false;
        }

        // --- NOSSA NOVA FUNÇÃO "SORTEADORA" ---
        private static EItemRarity RollRarityForChest(EChest chestType)
        {
            // TODO: (Avançado) No futuro, poderíamos pegar a estatística de 'sorte' do jogador aqui
            // float playerLuck = PlayerStats.Instance.luck;

            // Rola um dado de 100 lados
            float roll = UnityEngine.Random.Range(0f, 100f);

            // Estas porcentagens são suposições e podem ser ajustadas para balanceamento!
            // Exemplo para um baú Normal:
            if (chestType == EChest.Normal || chestType == EChest.Free)
            {
                if (roll < 75f) // 75% de chance
                {
                    return EItemRarity.Common;
                }
                if (roll < 95f) // 20% de chance (de 75 a 95)
                {
                    return EItemRarity.Rare;
                }
                // 5% de chance restantes (de 95 a 100)
                return EItemRarity.Epic;
            }

            if (chestType == EChest.Corrupt)
            {
                return EItemRarity.Corrupted;
            }

            // Padrão seguro caso encontremos um baú desconhecido
            return EItemRarity.Common;
        }
    }
}
using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Collections.Generic;
using System.Linq; // Filtra lista
using Assets.Scripts.Inventory__Items__Pickups.Chests;
using Assets.Scripts.Inventory__Items__Pickups.Items;

// NOTA: O DataManager pode não precisar de um 'using' se estiver no namespace global.
// Se o VS Code sublinhar 'DataManager' de vermelho, teremos que encontrar o namespace dele.

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
            Log.LogInfo("Artefato do Comando Ativado!");
            harmony.PatchAll();
        }

        public void OnGUI()
        {
            if (!IsChoosingItem) return;

            // --- Lógica da UI (Interface do Usuário) ---
            GUI.Box(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 200, 400, 400), "Artefato do Comando: Escolha um Item");

            // Desenha um botão para cada item na nossa lista
            for (int i = 0; i < itemsToShow.Count; i++)
            {
                // Limita a quantidade de itens na tela para não sobrecarregar
                if (i >= 10) break;

                // GUI.Button(new Rect(x, y, largura, altura), "Texto do Botão")
                if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 150 + (i * 30), 300, 25), itemsToShow[i].GetName()))
                {
                    Log.LogInfo($"Jogador escolheu: {itemsToShow[i].GetName()}");
                    GiveItemAndPay(itemsToShow[i]);
                    IsChoosingItem = false; // Fecha a UI
                }
            }
        }

        private void GiveItemAndPay(ItemData chosenItem)
        {
            // TODO: (Próximo Passo) Encontre a função do jogo que deduz dinheiro do jogador
            // Ex: PlayerStats.Instance.SpendGold(currentChest.GetPrice());

            // TODO: (Próximo Passo) Encontre a função do jogo que dá um item ao jogador
            // Ex: PlayerInventory.Instance.AddItem(chosenItem.eItem);

            // TODO: (Próximo Passo) Chame a função que abre o baú visualmente sem dar outro item
            // Ex: currentChest.PlayOpeningAnimation();

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

            // --- LÓGICA ATUALIZADA PARA CRIAR A LISTA DE ITENS ---

            // 1. Criamos nossa lista mestra de todos os itens do jogo
            List<ItemData> allItemsInGame = new List<ItemData>();
            foreach (EItem itemEnum in System.Enum.GetValues(typeof(EItem)))
            {
                // --- AQUI ESTÁ A CORREÇÃO ---
                try
                {
                    // TENTAMOS pegar o item
                    ItemData item = DataManager.Instance.GetItem(itemEnum);

                    // Verificamos se o item existe e se ele pode aparecer no jogo
                    if (item != null && item.inItemPool)
                    {
                        allItemsInGame.Add(item);
                    }
                }
                catch (System.Collections.Generic.KeyNotFoundException)
                {
                    // SE DER O ERRO "KeyNotFound", nós o ignoramos e continuamos o loop.
                    // Não fazemos nada aqui dentro, simplesmente evitamos que o jogo quebre.
                }
            }
            Plugin.Instance.Log.LogInfo($"Encontrado(s) {allItemsInGame.Count} item(ns) no total no jogo.");

            // 2. Filtramos a lista para pegar apenas os itens da raridade correta
            // TODO: (Próximo Passo) Precisamos descobrir como comparar 'chestType' com 'item.rarity'
            // Por enquanto, vamos mostrar TODOS os itens para testar a UI.
            Plugin.itemsToShow = allItemsInGame;

            // --------------------------------------------------------

            // Ativamos nossa UI
            Plugin.IsChoosingItem = true;

            // Cancelamos o método original do jogo
            __result = true;
            return false;
        }
    }
}
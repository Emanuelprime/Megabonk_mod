using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Inventory__Items__Pickups.Chests;


namespace Command_Artifact
{
    [BepInPlugin("Prime_Purpura.Command_Artifact", "Command_Artifact", "1.0.0")]
    public class Plugin : BasePlugin
    {
        private readonly Harmony harmony = new Harmony("Prime_Purpura.Command_Artifact");

        public static bool IsChoosingItem = false;
        // TODO: Substitua "ItemData" pelo nome real da classe de itens do jogo
        private static List<ItemData> itemsToShow = new List<ItemData>();
        private static InteractableChest currentChest;

        public override void Load()
        {
            Log.LogInfo("Artefato do Comando Ativado!");
            harmony.PatchAll();
        }

        // --- A Interface Gráfica (UI) ---
        public void OnGUI()
        {
            if (!IsChoosingItem) return;

            GUI.Box(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 150, 400, 300), "Artefato do Comando: Escolha um Item");

            for (int i = 0; i < itemsToShow.Count; i++)
            {
                // TODO: Substitua ".itemName" pelo nome real da propriedade do nome do item
                if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 100 + (i * 30), 300, 25), itemsToShow[i].itemName))
                {
                    Log.LogInfo($"Jogador escolheu: {itemsToShow[i].itemName}");
                    GiveItemAndPay(itemsToShow[i]);
                    IsChoosingItem = false; // Fecha a UI
                }
            }
        }

        private void GiveItemAndPay(ItemData chosenItem)
        {
            // TODO: Encontre a função do jogo que deduz dinheiro do jogador
            // Ex: Player.Instance.money -= currentChest.GetPrice();

            // TODO: Encontre a função do jogo que dá um item ao jogador
            // Ex: Player.Instance.inventory.AddItem(chosenItem);

            // TODO: Chame a função que abre o baú visualmente sem dar outro item
            // Ex: currentChest.PlayOpeningAnimation();

            Log.LogInfo("Item dado e pagamento efetuado!");
        }
    }

    // --- O PATCH ---
    // Este é o endereço que encontramos!
    [HarmonyPatch(typeof(InteractableChest), "Interact")]
    public static class InteractPatch
    {
        // Um PREFIX roda ANTES do método original
        [HarmonyPrefix]
        public static bool InterceptInteraction(InteractableChest __instance, ref bool __result)
        {
            // __instance é uma referência ao baú com o qual interagimos

            // Se a UI já estiver aberta, não faz nada
            if (Plugin.IsChoosingItem)
            {
                __result = false; // Diz ao jogo que a interação falhou
                return false;     // Cancela o método original
            }

            // 1. Verificamos se o jogador pode pagar
            if (!__instance.CanAfford())
            {
                Plugin.Log.LogInfo("Não pode pagar. Deixando o jogo original lidar com isso.");
                return true; // Deixa o método original rodar para mostrar a mensagem de "sem dinheiro"
            }

            Plugin.Log.LogInfo("Pode pagar! Interceptando interação...");
            Plugin.currentChest = __instance;

            // 2. Pegamos a raridade/tipo do baú
            var chestType = __instance.chestType; // Usamos a propriedade que vimos no dnSpy
            Plugin.Log.LogInfo($"Tipo do baú: {chestType}");

            // 3. Pegamos todos os itens daquela raridade
            // TODO: Precisamos encontrar a "lista mestra" de itens do jogo para preencher isso
            // Plugin.itemsToShow = ItemDatabase.GetItemsByRarity(chestType);

            // 4. Ativamos nossa UI
            Plugin.IsChoosingItem = true;

            // 5. CANCELAMOS o método original do jogo
            __result = true; // Dizemos ao jogo que a "interação" foi um sucesso
            return false;    // Retornar 'false' impede que o método original seja executado!
        }
    }
}
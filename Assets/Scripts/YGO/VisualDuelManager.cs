using UnityEngine;
using System.Collections.Generic;

namespace YGO
{
    /// <summary>
    /// Escuta os eventos lógicos do MessageParser e traduz em instâncias visuais (Criação de Prefabs na Tela).
    /// </summary>
    public class VisualDuelManager : MonoBehaviour
    {
        [Header("Referências")]
        [Tooltip("Prefab que representa a Carta na mesa (Pode ser um Quad com SpriteRenderer)")]
        public GameObject cardPrefab;
        
        [Tooltip("Ponto invisível onde as cartas da mão ficarão ancoradas")]
        public Transform player0HandZone;

        [Header("UI Canvas (Mão 2D)")]
        [Tooltip("Prefab da Carta feito com Image de UI")]
        public GameObject uiCardPrefab;
        [Tooltip("Painel que vai organizar as cartas (Horizontal Layout Group)")]
        public Transform uiHandPanel;

        [Header("Configurações Visuais (Apenas 3D)")]
        public float handSpacing = 1.2f;

        // Guarda as instâncias para podermos organizar o leque da mão
        private List<VisualCard> player0Hand = new List<VisualCard>();

        private void Start()
        {
            // Fallback de segurança para o sistema 3D antigo
            if (player0HandZone != null && Camera.main != null && uiHandPanel == null)
            {
                Camera.main.transform.position = new Vector3(0, 7.5f, -8f);
                Camera.main.transform.rotation = Quaternion.Euler(45, 0, 0);

                player0HandZone.SetParent(Camera.main.transform);
                player0HandZone.localPosition = new Vector3(0, -2.5f, 6f);
                player0HandZone.localRotation = Quaternion.Euler(-10, 0, 0);
                player0HandZone.localScale = Vector3.one;
            }
        }

        private void OnEnable()
        {
            MessageParser.OnCardsDrawn += HandleCardsDrawn;
        }

        private void OnDisable()
        {
            MessageParser.OnCardsDrawn -= HandleCardsDrawn;
        }

        private void HandleCardsDrawn(int player, uint[] drawnIds)
        {
            if (player == 0)
            {
                foreach (uint id in drawnIds)
                {
                    GameObject cardObj;
                    
                    // Prioridade 1: UI Canvas. Prioridade 2: 3D Antigo.
                    if (uiCardPrefab != null && uiHandPanel != null) {
                        cardObj = Instantiate(uiCardPrefab, uiHandPanel);
                    } else if (cardPrefab != null && player0HandZone != null) {
                        cardObj = Instantiate(cardPrefab, player0HandZone);
                    } else {
                        break; // Se não tem nenhum configurado, ignora
                    }
                    
                    VisualCard vCard = cardObj.GetComponent<VisualCard>();
                    if (vCard == null) vCard = cardObj.AddComponent<VisualCard>();
                    
                    vCard.Initialize(id);
                    player0Hand.Add(vCard);
                }
                
                // Na UI, o Layout Group do Canvas organiza sozinho! Só reposicionamos na mão se for 3D
                if (uiHandPanel == null) {
                    RepositionHand();
                }
                
                Debug.Log($"<color=lime>[VisualManager]</color> Instanciados {drawnIds.Length} Cartas Visuais na Mão do Jogador 0!");
            }
        }

        /// <summary>
        /// (Sistema 3D Legado) Reorganiza as cartas para formarem um leque / linha perfeita baseada no centro.
        /// </summary>
        private void RepositionHand()
        {
            float totalWidth = (player0Hand.Count - 1) * handSpacing;
            float startX = -totalWidth / 2f;

            for (int i = 0; i < player0Hand.Count; i++)
            {
                player0Hand[i].transform.localPosition = new Vector3(startX + (i * handSpacing), 0, 0);
                player0Hand[i].transform.localScale = new Vector3(0.7f, 1f, 1f);
                player0Hand[i].transform.localRotation = Quaternion.Euler(0, 0, 0); 
            }
        }
    }
}

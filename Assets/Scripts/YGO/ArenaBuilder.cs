using UnityEngine;

namespace YGO
{
    /// <summary>
    /// Gera os pontos 3D da Arena de Duelo baseados nas regras oficiais (Master Rule).
    /// </summary>
    public class ArenaBuilder : MonoBehaviour
    {
        [Header("Configurações de Espaçamento")]
        public float cardSpacingX = 1.5f;
        public float rowSpacingZ = 1.8f;

        [ContextMenu("Construir Arena 3D")]
        public void BuildArena()
        {
            // Limpa as zonas antigas se estiver regerando
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            // Jogador 0 (Base do Tabuleiro / Parte inferior)
            BuildPlayerZones(0, false);

            // Jogador 1 (Topo do Tabuleiro / Oponente)
            BuildPlayerZones(1, true);

            // Extra Monster Zones (Master Rule 4/5 - Ficam no centro)
            CreateZone("ExtraMonsterZone_0", -cardSpacingX, rowSpacingZ);
            CreateZone("ExtraMonsterZone_1", cardSpacingX, rowSpacingZ);

            Debug.Log("<color=green>Arena construída com sucesso!</color> O campo está alinhado.");
        }

        private void BuildPlayerZones(int playerIndex, bool isOpponent)
        {
            // Um objeto vazio para agrupar as zonas deste jogador
            GameObject playerRoot = new GameObject($"Player_{playerIndex}_Zones");
            playerRoot.transform.SetParent(this.transform);
            playerRoot.transform.localPosition = Vector3.zero;

            // Define a inversão do eixo Z e X para o oponente (para que ele fique de frente para nós)
            float zDir = isOpponent ? 1f : -1f;
            // Para o oponente, a zona 0 fica na direita, zona 4 na esquerda sob a ótica dele
            float xDir = isOpponent ? -1f : 1f;

            // A linha "zero" dos monstros do P0 é Z = 0. Do P1 é Z = 2 * rowSpacingZ (aprox 3.6f)
            float baseZ = isOpponent ? rowSpacingZ * 2 : 0f;

            // Zonas de Monstros Principais (5)
            for (int i = 0; i < 5; i++)
            {
                float x = (i - 2) * cardSpacingX * xDir;
                CreateZone($"P{playerIndex}_MonsterZone_{i}", x, baseZ, playerRoot.transform);
            }

            // Zonas de Magias e Armadilhas (5)
            float stZ = baseZ + (rowSpacingZ * zDir);
            for (int i = 0; i < 5; i++)
            {
                float x = (i - 2) * cardSpacingX * xDir;
                CreateZone($"P{playerIndex}_SpellTrapZone_{i}", x, stZ, playerRoot.transform);
            }

            // Zonas Especiais: Field, Graveyard, Deck, Extra, Banished
            // Campo Fica à esquerda (i=-3)
            float fieldX = -3 * cardSpacingX * xDir;
            CreateZone($"P{playerIndex}_FieldZone", fieldX, baseZ, playerRoot.transform);
            CreateZone($"P{playerIndex}_ExtraDeck", fieldX, stZ, playerRoot.transform);

            // Cemitério e Deck ficam à direita (i=3)
            float graveX = 3 * cardSpacingX * xDir;
            CreateZone($"P{playerIndex}_Graveyard", graveX, baseZ, playerRoot.transform);
            CreateZone($"P{playerIndex}_Deck", graveX, stZ, playerRoot.transform);

            // Zonas de Banidas (um pouco mais para fora ou para trás)
            CreateZone($"P{playerIndex}_Banished", graveX, stZ + (rowSpacingZ * zDir), playerRoot.transform);
        }

        private void CreateZone(string name, float x, float z, Transform parent = null)
        {
            // Em vez de objeto vazio, agora criamos um "Quad" real (Um plano 2D nativo da Unity)
            GameObject zone = GameObject.CreatePrimitive(PrimitiveType.Quad);
            zone.name = name;
            
            // Destruímos o colisor padrão pois ele pode atrapalhar os cliques depois
            DestroyImmediate(zone.GetComponent<MeshCollider>());

            zone.transform.SetParent(parent == null ? this.transform : parent);
            
            // Colocamos Y = 0.01f para ele ficar uma casquinha de nada acima da sua Mesa (Plane), evitando que a tela pisque
            zone.transform.localPosition = new Vector3(x, 0.01f, z);
            
            // O Quad nasce em pé. Deitamos ele 90 graus no chão!
            zone.transform.localRotation = Quaternion.Euler(90, 0, 0);
            
            // Escala real de uma carta YGO (1 de largura por 1.4 de altura)
            zone.transform.localScale = new Vector3(1f, 1.4f, 1f);

            // Criando as cores translúcidas estilo Master Duel
            MeshRenderer renderer = zone.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                // O shader "Sprites/Default" funciona perfeitamente para transparência limpa
                Material mat = new Material(Shader.Find("Sprites/Default"));
                
                Color zoneColor = new Color(0.5f, 0.5f, 0.5f, 0.3f); // Cinza/Neutro (Padrão)

                if (name.Contains("Monster")) zoneColor = new Color(0.8f, 0.4f, 0.1f, 0.35f); // Laranja translúcido
                else if (name.Contains("Spell")) zoneColor = new Color(0.1f, 0.6f, 0.5f, 0.35f); // Azul/Verde translúcido
                else if (name.Contains("Field")) zoneColor = new Color(0.2f, 0.8f, 0.2f, 0.35f); // Verde claro translúcido
                
                mat.color = zoneColor;
                renderer.sharedMaterial = mat; // sharedMaterial é ideal para rodar dentro do Editor
            }
        }
    }
}

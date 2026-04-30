using System;
using UnityEngine;

namespace YGO
{
    public static class MessageParser
    {
        // IDs do EDO9300
        public const byte MSG_RETRY = 1;
        public const byte MSG_HINT = 2;
        public const byte MSG_WAITING = 3;
        public const byte MSG_START = 4;
        public const byte MSG_SELECT_IDLECMD = 11;
        public const byte MSG_NEW_TURN = 40;     // 0x28
        public const byte MSG_NEW_PHASE = 41;    // 0x29
        public const byte MSG_DRAW = 90;         // 0x5A
        
        // Eventos visuais para a Unity assinar!
        public static event Action<int, uint[]> OnCardsDrawn;

        public static void Parse(byte[] messageData)
        {
            if (messageData == null || messageData.Length == 0)
                return;

            int offset = 0;

            // O buffer pode conter MÚLTIPLAS mensagens.
            // A API edo9300 envia o formato: [4 bytes de tamanho] + [N bytes da mensagem]
            while (offset < messageData.Length)
            {
                // Lê o tamanho da mensagem atual (4 bytes)
                int msgLen = BitConverter.ToInt32(messageData, offset);
                offset += 4;

                if (msgLen <= 0 || offset + msgLen > messageData.Length)
                    break;

                // O primeiro byte do payload é o Tipo da Mensagem
                byte msgType = messageData[offset];

                switch (msgType)
                {
                    case MSG_NEW_TURN:
                        byte player = messageData[offset + 1];
                        Debug.Log($"<color=yellow>[MSG_NEW_TURN]</color> O Turno passou para o Jogador {player}!");
                        break;
                        
                    case MSG_NEW_PHASE:
                        short phase = BitConverter.ToInt16(messageData, offset + 1);
                        Debug.Log($"<color=orange>[MSG_NEW_PHASE]</color> Fase avançou para o ID: {phase}");
                        break;
                        
                    case MSG_DRAW:
                        byte p = messageData[offset + 1];
                        byte count = (byte)BitConverter.ToUInt32(messageData, offset + 2);
                        string drawnCards = "";
                        
                        // O cabeçalho ocupa 6 bytes (1 do tipo + 1 do player + 4 do count)
                        int readOffset = offset + 6;
                        uint[] drawnIds = new uint[count];

                        bioafor (int i = 0; i < count; i++)
                        {
                            // A nova API usa 8 bytes por carta (4 para ID e 4 para status/posição)
                            uint cardId = BitConverter.ToUInt32(messageData, readOffset);
                            
                            bool isHidden = (cardId & 0x80000000) != 0;
                            uint realId = cardId & 0x7FFFFFFF;
                            
                            drawnIds[i] = realId;

                            if (isHidden) drawnCards += "[Oculta] ";
                            else drawnCards += $"[{realId}] ";
                            
                            readOffset += 8; // Pula os 8 bytes desta carta
                        }
                        Debug.Log($"<color=lightblue>[MSG_DRAW]</color> Jogador {p} sacou {count} cartas: {drawnCards}");
                        
                        // Dispara o evento visual!
                        OnCardsDrawn?.Invoke(p, drawnIds);
                        break;

                    case MSG_SELECT_IDLECMD:
                        Debug.Log($"<color=green>[MSG_SELECT_IDLECMD]</color> O Motor pausou. Ele está aguardando você jogar uma carta ou passar o turno!");
                        break;

                    case MSG_RETRY:
                        // Ignoramos o retry, é só a engine dizendo "Estou esperando sua resposta"
                        break;

                    default:
                        // Logamos os que ainda não conhecemos
                        // Debug.Log($"[MSG DESCONHECIDO] Tipo: {msgType} (Hex: {msgType:X2}) | Tamanho: {msgLen}");
                        break;
                }

                // Avança o ponteiro para a próxima mensagem dentro do mesmo array
                offset += msgLen;
            }
        }
    }
}

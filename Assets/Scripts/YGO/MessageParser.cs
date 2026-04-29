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
                        // O payload tem mais dados, mas vamos só avisar por cima
                        Debug.Log($"<color=lightblue>[MSG_DRAW]</color> Ocorreu um saque de cartas (Draw)!");
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

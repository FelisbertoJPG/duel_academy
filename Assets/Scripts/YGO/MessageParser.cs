using System;
using UnityEngine;

namespace YGO
{
    public static class MessageParser
    {
        // IDs básicos de mensagens (conforme protocolo do EDOPRO)
        public const byte MSG_RETRY = 0;
        public const byte MSG_HINT = 1;
        public const byte MSG_WAITING = 2;
        public const byte MSG_START = 3;
        public const byte MSG_WIN = 4;
        public const byte MSG_UPDATE_DATA = 5;
        public const byte MSG_UPDATE_CARD = 6;
        public const byte MSG_REQUEST_DECK = 7;
        public const byte MSG_SELECT_BATTLECMD = 10;
        public const byte MSG_SELECT_IDLECMD = 11;
        public const byte MSG_SELECT_EFFECTYN = 12;
        public const byte MSG_NEW_TURN = 40;
        public const byte MSG_NEW_PHASE = 41;
        public const byte MSG_DRAW = 50;
        public const byte MSG_DAMAGE = 51;
        public const byte MSG_RECOVER = 52;
        public const byte MSG_EQUIP = 53;
        public const byte MSG_SUMMONING = 60;
        public const byte MSG_SUMMONED = 61;
        public const byte MSG_SPSUMMONING = 62;
        public const byte MSG_SPSUMMONED = 63;
        
        public static void Parse(byte[] messageData)
        {
            if (messageData == null || messageData.Length == 0)
                return;

            byte msgType = messageData[0]; // O primeiro byte é sempre o Tipo da Ação
            
            switch (msgType)
            {
                case MSG_START:
                    Debug.Log("<color=cyan>[MSG_START]</color> O Duelo Começou! (LPs iniciais configurados)");
                    break;
                case MSG_NEW_TURN:
                    if (messageData.Length >= 2)
                        Debug.Log($"<color=yellow>[MSG_NEW_TURN]</color> O Turno passou para o Jogador {messageData[1]}!");
                    break;
                case MSG_NEW_PHASE:
                    if (messageData.Length >= 3) // Bytes adicionais contêm ID da fase
                        Debug.Log($"<color=orange>[MSG_NEW_PHASE]</color> Fase avançou!");
                    break;
                case MSG_DRAW:
                    if (messageData.Length >= 3)
                    {
                        byte player = messageData[1];
                        byte count = messageData[2];
                        Debug.Log($"<color=lightblue>[MSG_DRAW]</color> Jogador {player} sacou {count} carta(s).");
                    }
                    break;
                case MSG_SUMMONING:
                    Debug.Log("<color=magenta>[MSG_SUMMONING]</color> Uma Invocação Normal começou!");
                    break;
                case MSG_SPSUMMONING:
                    Debug.Log("<color=magenta>[MSG_SPSUMMONING]</color> Uma Invocação Especial começou!");
                    break;
                case MSG_DAMAGE:
                    Debug.Log("<color=red>[MSG_DAMAGE]</color> Dano de batalha ou efeito causado!");
                    break;
                case MSG_WIN:
                    Debug.Log("<color=green>[MSG_WIN]</color> O Duelo acabou!");
                    break;
                // Deixamos as demais silenciadas por enquanto para não flodar o console com dados de cartas viradas (UpdateData)
                default:
                    // Debug.Log($"[MSG RAW] Código: {msgType} | Tamanho: {messageData.Length} bytes");
                    break;
            }
        }
    }
}

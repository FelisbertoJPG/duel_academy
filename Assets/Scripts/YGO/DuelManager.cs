using System;
using System.Runtime.InteropServices;
using UnityEngine;
using YGO;

public class DuelManager : MonoBehaviour
{
    private IntPtr duelInstance = IntPtr.Zero;

    // Precisamos manter a referência viva para o Garbage Collector não limpar
    private OCG_DataReader dummyCardReader;
    private OCG_ScriptReader dummyScriptReader;

    void Start()
    {
        Debug.Log("Inicializando ygopro-core (edo9300)...");

        try
        {
            // Instanciar os callbacks
            dummyCardReader = (payload, code, data) => { Debug.Log($"DLL pediu a carta: {code}"); };
            dummyScriptReader = (payload, duel, name) => { Debug.Log($"DLL pediu o script: {name}"); return 0; };

            // O novo padrão exige definir as opções do duelo num struct antes de criar
            OCG_DuelOptions options = new OCG_DuelOptions
            {
                seed0 = 12345, // Seed aleatória
                flags = 0,
                team1 = new OCG_Player { startingLP = 8000, startingDrawCount = 5, drawCountPerTurn = 1 },
                team2 = new OCG_Player { startingLP = 8000, startingDrawCount = 5, drawCountPerTurn = 1 },
                cardReader = Marshal.GetFunctionPointerForDelegate(dummyCardReader),
                scriptReader = Marshal.GetFunctionPointerForDelegate(dummyScriptReader),
                logHandler = IntPtr.Zero,
                cardReaderDone = IntPtr.Zero,
                enableUnsafeLibraries = 0
            };

            // 1. Criar o duelo no backend nativo
            int status = YgoCoreAPI.OCG_CreateDuel(out duelInstance, ref options);

            if (status == 0 && duelInstance != IntPtr.Zero)
            {
                Debug.Log($"<color=green>Sucesso ABSOLUTO!</color> Duelo criado no ponteiro: {duelInstance}");
                
                // 2. Iniciar o duelo
                YgoCoreAPI.OCG_StartDuel(duelInstance);
                Debug.Log("Duelo Iniciado com a nova API.");
            }
            else
            {
                Debug.LogError($"Falha ao criar o duelo. Código de erro: {status}");
            }
        }
        catch (DllNotFoundException e)
        {
            Debug.LogError($"<color=red>DLL NÃO ENCONTRADA!</color>\nErro: {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro genérico ao tentar falar com o core: {e.Message}");
        }
    }

    void OnDestroy()
    {
        if (duelInstance != IntPtr.Zero)
        {
            YgoCoreAPI.OCG_DestroyDuel(duelInstance);
            Debug.Log("Duelo finalizado e memória limpa via API moderna.");
        }
    }
}

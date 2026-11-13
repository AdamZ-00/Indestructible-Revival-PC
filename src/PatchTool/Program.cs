using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: dotnet run -- <managed-dll-directory>");
            return 1;
        }

        string managedDir = Path.GetFullPath(args[0]);
        string inputDll = Path.Combine(managedDir, "Assembly-CSharp.dll");
        string backupDll = Path.Combine(managedDir, "Assembly-CSharp.orig.dll");
        string outputDll = Path.Combine(managedDir, "Assembly-CSharp.patched.dll");

        if (!File.Exists(backupDll))
        {
            File.Copy(inputDll, backupDll, overwrite: false);
        }

        var replacementsByType = new Dictionary<string, Dictionary<string, string>>
        {
            ["PhotonAppIDPrompt"] = new Dictionary<string, string>
            {
                ["APP ID MULTIJOUEUR (Photon)"] = "MULTIPLAYER APP ID (Photon)",
                ["Collez votre AppID Photon (touchez le champ) :"] = "Paste your Photon App ID (tap the box):",
                ["VALIDER"] = "CONFIRM",
                ["ANNULER"] = "CANCEL",
            },
            ["CustomMatchPrompt"] = new Dictionary<string, string>
            {
                ["JOUER AVEC DES AMIS (SALON PRIVÉ)"] = "PLAY WITH FRIENDS (PRIVATE ROOM)",
                ["Code du salon (partagez le même avec votre ami) :"] = "Room code (share the same code with your friend):",
                ["CRÉER LE SALON"] = "CREATE ROOM",
                ["REJOINDRE"] = "JOIN ROOM",
                ["RETOUR"] = "BACK",
                ["SALON PRIVÉ CRÉÉ : "] = "PRIVATE ROOM CREATED: ",
                ["En attente de votre ami sur le salon \""] = "Waiting for your friend in room \"",
                ["Joueurs connectés : "] = "Players connected: ",
                ["ANNULER"] = "CANCEL",
                ["CONNEXION AU SALON : "] = "JOINING ROOM: ",
                ["Connecté au salon \""] = "Connected to room \"",
                ["\" !"] = "\"!",
                ["Recherche du salon \""] = "Searching for room \"",
                ["TOUS LES JOUEURS SONT PRÊTS !"] = "ALL PLAYERS ARE READY!",
                ["Lancement du combat en cours..."] = "Starting match...",
                ["Création du salon en cours..."] = "Creating room...",
                ["Connexion à Photon..."] = "Connecting to Photon...",
            },
            ["DialogPlayerName"] = new Dictionary<string, string>
            {
                ["APP ID MULTIJOUEUR"] = "MULTIPLAYER APP ID",
                ["AppID Photon (36 car.)"] = "Photon App ID (36 chars)",
            },
        };

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managedDir);

        var readerParams = new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false,
            InMemory = true,
        };

        using var module = ModuleDefinition.ReadModule(backupDll, readerParams);

        var hitCounts = new Dictionary<string, int>();
        foreach (var typeEntry in replacementsByType)
        {
            foreach (var key in typeEntry.Value.Keys)
            {
                hitCounts[$"{typeEntry.Key}::{key}"] = 0;
            }
        }

        foreach (var type in module.Types)
        {
            if (!replacementsByType.TryGetValue(type.FullName, out var map))
                continue;

            foreach (var method in type.Methods)
            {
                if (!method.HasBody) continue;
                foreach (var instr in method.Body.Instructions)
                {
                    if (instr.OpCode == OpCodes.Ldstr && instr.Operand is string s && map.TryGetValue(s, out var translated))
                    {
                        instr.Operand = translated;
                        hitCounts[$"{type.FullName}::{s}"]++;
                    }
                }
            }
        }

        foreach (var kv in hitCounts)
        {
            if (kv.Value == 0)
            {
                Console.Error.WriteLine($"ERROR: Expected string not found: {kv.Key}");
                return 2;
            }
        }

        // Align AppID minimum length (>= 11, matching > 10 in Dialogs.EnterPhotonAppID)
        int lengthPatched = 0;
        foreach (var target in new[] { ("PhotonAppIDPrompt", "OnGUI"), ("DialogPlayerName", "OnValidateName") })
        {
            foreach (var type in module.Types)
            {
                if (type.FullName != target.Item1) continue;
                foreach (var method in type.Methods)
                {
                    if (method.Name != target.Item2 || !method.HasBody) continue;
                    var ins = method.Body.Instructions;
                    for (int i = 0; i + 2 < ins.Count; i++)
                    {
                        if (ins[i].Operand is MethodReference mr &&
                            mr.Name == "get_Length" &&
                            ins[i + 1].OpCode == OpCodes.Ldc_I4_5 &&
                            (ins[i + 2].OpCode == OpCodes.Clt || ins[i + 2].OpCode == OpCodes.Blt_S))
                        {
                            ins[i + 1].OpCode = OpCodes.Ldc_I4_S;
                            ins[i + 1].Operand = (sbyte)11;
                            lengthPatched++;
                        }
                    }
                }
            }
        }
        if (lengthPatched != 2)
        {
            Console.Error.WriteLine($"ERROR: Expected 2 length patches, got {lengthPatched}");
            return 3;
        }

        // Remove accidental keyboard shortcut (F1 / C -> CheatsScene) from PanelGarage.Update
        bool cheatRemoved = false;
        foreach (var type in module.Types)
        {
            if (type.FullName != "PanelGarage") continue;
            foreach (var method in type.Methods)
            {
                if (method.Name != "Update" || !method.HasBody) continue;
                var ins = method.Body.Instructions;
                if (ins.Count >= 8 &&
                    ins[0].OpCode == OpCodes.Ldc_I4 && (int)ins[0].Operand == 282 &&
                    ins[3].OpCode == OpCodes.Ldc_I4_S && (sbyte)ins[3].Operand == 99 &&
                    ins[6].OpCode == OpCodes.Ldstr && (string)ins[6].Operand == "CheatsScene" &&
                    ins[7].Operand is MethodReference lm && lm.Name == "LoadLevel")
                {
                    for (int k = 0; k < 8; k++) ins.RemoveAt(0);
                    cheatRemoved = true;
                }
            }
        }
        if (!cheatRemoved)
        {
            Console.Error.WriteLine("ERROR: Could not find F1/C CheatsScene block in PanelGarage.Update");
            return 4;
        }

        module.Write(outputDll);
        Console.WriteLine($"Wrote {outputDll} ({new FileInfo(outputDll).Length} bytes)");
        return 0;
    }
}

﻿using Exiled.API.Features;
using InventorySystem.Items.Pickups;
using MapEditorReborn.API.Features;
using MapEditorReborn.API.Features.Objects;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using SCPSLAudioApi.AudioCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace AutoEvent.API
{
    internal class API
    {
        public static List<ReferenceHub> Dummies;
        /// <summary>Тп игроков после конца игры</summary>
        public static void TeleportEnd()
        {
            foreach (Player player in Player.List)
            {
                player.Role.Set(RoleTypeId.Tutorial, Exiled.API.Enums.SpawnReason.None, RoleSpawnFlags.None);
                player.Position = new Vector3(39.332f, 1014.766f, -31.922f);
            }
        }
        /// <summary>Проиграть аудиофайл</summary>
        public static void PlayAudio(string audioFile, byte volume, bool loop, string eventName)
        {
            try
            {
                Dummies = new List<ReferenceHub>();
                var newPlayer = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
                int id = Dummies.Count;
                var fakeConnection = new FakeConnection(id++);
                var hubPlayer = newPlayer.GetComponent<ReferenceHub>();
                Dummies.Add(hubPlayer);
                NetworkServer.AddPlayerForConnection(fakeConnection, newPlayer);

                hubPlayer.characterClassManager.InstanceMode = ClientInstanceMode.Unverified;

                try
                {
                    hubPlayer.nicknameSync.SetNick(eventName);
                }
                catch (Exception) { }

                var audioPlayer = AudioPlayerBase.Get(hubPlayer);

                var path = Path.Combine(Path.Combine(Paths.Configs, "Music"), audioFile);

                audioPlayer.Enqueue(path, -1);
                audioPlayer.LogDebug = false;
                audioPlayer.BroadcastChannel = VoiceChat.VoiceChatChannel.Intercom;
                audioPlayer.Volume = volume;
                audioPlayer.Loop = loop;
                audioPlayer.Play(0);
                Log.Debug($"Playing sound {path}");
            }
            catch (Exception e)
            {
                Log.Error($"Error on: {e.Data} -- {e.StackTrace}");
            }
        }
        /// <summary>Остановить прогирывание</summary>
        public static void StopAudio()
        {
            foreach (var dummies in Dummies)
            {
                NetworkServer.Destroy(dummies.gameObject);
            }
            Dummies.Clear();
        }
        /// <summary>Загрузить карту</summary>
        public static SchematicObject LoadMap(string nameSchematic , Vector3 pos, Quaternion rot, Vector3 scale)
        {
            return ObjectSpawner.SpawnSchematic(nameSchematic, pos, rot, scale);
        }
        /// <summary>Удалить карту</summary>
        public static void UnLoadMap(SchematicObject scheme)
        {
            scheme.Destroy();
        }
        /// <summary>Очистка мусора после ивента.</summary>
        public static void CleanUpAll()
        {
            foreach (BasicRagdoll ragdoll in Object.FindObjectsOfType<BasicRagdoll>())
            {
                Object.Destroy(ragdoll.gameObject);
            }
            foreach (ItemPickupBase pickupBase in Object.FindObjectsOfType<ItemPickupBase>())
            {
                if (pickupBase != null)
                {
                    Object.Destroy(pickupBase.gameObject);
                }
            }
        }
    }
}
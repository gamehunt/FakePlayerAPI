using Exiled.API.Features;
using MEC;
using Mirror;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FakePlayerAPI
{
    public abstract class FakePlayer: MonoBehaviour
    {
        public static IEnumerable<FakePlayer> List
        {
            get
            {
                return Dictionary.Values;
            }
        }

        public static Dictionary<GameObject, FakePlayer> Dictionary { get; } = new Dictionary<GameObject, FakePlayer>();

        /// <summary>
        /// Gets or sets internal <see cref="Player"/> instance.
        /// </summary>
        public Player PlayerInstance { get; set; }

        /// <summary>
        /// Gets owner <see cref="Plugin"/> instance.
        /// </summary>
        public abstract Plugin PluginInstance { get; }

        /// <summary>
        /// Gets debug identifier for <see cref="FakePlayer"/> instance.
        /// Should be unique!!!
        /// </summary>
        /// <returns>Fake player identifier for logging</returns>
        public abstract string GetIdentifier();

        /// <summary>
        /// Checks if <see cref="FakePlayer"/> instance can bee seen by <see cref="Player"/>.
        /// </summary>
        /// <param name="ply"><see cref="Player"/> for check</param>
        /// <returns>True if instance should be transmitted to player, false otherwise</returns>
        public abstract bool IsVisibleFor(Player ply);

        /// <summary>
        /// Gets or sets value indicating whether <see cref="FakePlayer"/> should be treated as normal <see cref="Player"/> (if false, most of Exiled events won't be called for this instance).
        /// </summary>
        //public bool IsPlayer { get; set; } = false;

        /// <summary>
        /// Gets or sets value indicating whether <see cref="FakePlayer"/> should be counted when checking round end.
        /// </summary>
        public virtual bool AffectEndConditions { get; set; } = false;

        /// <summary>
        /// Gets or sets value indicating whether <see cref="FakePlayer"/> should be displayed in RA.
        /// </summary>
        public virtual bool DisplayInRA { get; set; } = false;

        /// <summary>
        /// Gets list of attached <see cref="CoroutineHandle">coroutines</see>, coroutines from this list will be destroyed with this instance.
        /// </summary>
        public List<CoroutineHandle> AttachedCoroutines { get; } = new List<CoroutineHandle>();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is fully initialized and ready for further usage.
        /// </summary>
        public bool IsValid { get; set; } = false;

        public void Kill(bool spawn_ragdoll)
        {
            if (IsValid)
            {
                IsValid = false;
                Log.Debug($"kill() called in FakePlayer {GetIdentifier()}", Plugin.Instance.Config.VerboseOutput);
                if (spawn_ragdoll)
                {
                    gameObject.GetComponent<RagdollManager>().SpawnRagdoll(gameObject.transform.position, gameObject.transform.rotation, Vector3.zero, (int)PlayerInstance.Role, new PlayerStats.HitInfo(), false, "", GetIdentifier(), 9999);
                }
                UnityEngine.Object.Destroy(PlayerInstance.GameObject);
            }
        }

        private void OnDestroy()
        {
            Dictionary.Remove(gameObject);
            Timing.KillCoroutines(AttachedCoroutines.ToArray());
        }

        public static FakePlayer Get(Player p)
        {
            if (Dictionary.TryGetValue(p.GameObject, out FakePlayer npc))
            {
                return npc;
            }
            else
            {
                return null;
            }
        }

        public static FakePlayer Get(GameObject p)
        {
            if (Dictionary.TryGetValue(p, out FakePlayer npc))
            {
                return npc;
            }
            else
            {
                return null;
            }
        }

        private void Awake()
        {
            Dictionary.Add(gameObject, this);
        }

        public void FinishInitialization()
        {
            try
            {
                PlayerInstance = Player.Get(gameObject);
                Log.Debug($"Constructed FakePlayer {GetIdentifier()}", Plugin.Instance.Config.VerboseOutput);
                IsValid = true;
            }
            catch (Exception e)
            {
                Log.Error($"Exception in init finalizer: {e}");
            }
        }

        public static T Create<T>(Vector3 position, RoleType role) where T : FakePlayer
        {
            GameObject obj = Instantiate(NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
            CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();

            obj.transform.localScale = Vector3.one;
            obj.transform.position = position;

            QueryProcessor processor = obj.GetComponent<QueryProcessor>();

            processor.NetworkPlayerId = QueryProcessor._idIterator++;


            ccm.CurClass = RoleType.Tutorial;
            ccm._privUserId = null;
            ccm.UserId2 = null;
            obj.GetComponent<PlayerStats>().SetHPAmount(ccm.Classes.SafeGet(RoleType.Tutorial).maxHP);

            obj.GetComponent<NicknameSync>().Network_myNickSync = "Fake Player";

            ServerRoles roles = obj.GetComponent<ServerRoles>();

            roles.MyText = "Fake Player";
            roles.MyColor = "red";

            NetworkServer.Spawn(obj);
            PlayerManager.AddPlayer(obj, CustomNetworkManager.slots); //I'm not sure if I need this

            T fake_player = obj.AddComponent<T>();
            processor._ipAddress = fake_player.GetIdentifier();

            // We triggering JoinedEvent in RHub.Awake() -> our object got added to the unverified list 
            // We dont trigger VerifiedEvent so we need to add our fake player to the player dictionary manually
            // And actually finish the NPC initialization after that, otherwise PlayerInstance will be null and all shit is broken

            Timing.CallDelayed(0.1f, () =>
            {
                try
                {
                    Player.UnverifiedPlayers.TryGetValue(ccm._hub, out Player ply_obj);
                    Player.Dictionary.Add(obj, ply_obj);
                    ply_obj.IsVerified = true;

                    fake_player.FinishInitialization();

                    fake_player.PlayerInstance.ReferenceHub.transform.localScale = Vector3.one;
                    fake_player.PlayerInstance.SessionVariables.Add("IsFakePlayer", true);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            });

            fake_player.AttachedCoroutines.Add(Timing.CallDelayed(0.3f, () =>
            {
                fake_player.PlayerInstance.ReferenceHub.playerMovementSync.OverridePosition(position, 0, true);
                fake_player.PlayerInstance.Rotations = Vector2.zero;
            }));

            return fake_player;
        }
    }
}

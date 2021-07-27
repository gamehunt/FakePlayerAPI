using Exiled.API.Features;
using MEC;
using Mirror;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FakePlayers.API
{
    public abstract class FakePlayer : MonoBehaviour
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

        /// <summary>
        /// Gets or a value indicating whether this instance fires exiled events.
        /// IMPORTANT! This value dont tell anyting to game, if event being fired for this instance unpredictable things can happen
        /// </summary>
        public bool IsFiringEvents { get; private set; }

        /// <summary>
        /// Called after FinishInitialization(), when this instance fully initialized
        /// </summary>
        public abstract void OnPostInitialization();

        /// <summary>
        /// Called before PlayerInstance is set
        /// </summary>
        public abstract void OnPreInitialization();

        /// <summary>
        /// Called when kill() called in this instance
        /// </summary>
        public abstract void OnDestroying();

        public void Kill()
        {
            if (IsValid)
            {
                IsValid = false;
                Log.Debug($"kill() called in FakePlayer {GetIdentifier()}");
                OnDestroying();
                Destroy(PlayerInstance.GameObject);
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

        private static IEnumerator<float> FinishInstanceCreationCoroutine(GameObject obj, Vector3 position, Vector3 scale)
        {
            yield return Timing.WaitForSeconds(0.1f);
            CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();
            FakePlayer fake_player = obj.GetComponent<FakePlayer>();
            try
            {
                fake_player.OnPreInitialization();

                Player.UnverifiedPlayers.TryGetValue(ccm._hub, out Player ply_obj);

                if (fake_player.IsFiringEvents)
                {
                    Player.Dictionary.Add(obj, ply_obj);
                }

                ply_obj.IsVerified = true;

                string prefs = "";
                for (int i = 0; i < ply_obj.ReferenceHub.weaponManager.weapons.Length; i++)
                {
                    prefs += i == 0 ? "0:0:0" : "#0:0:0";
                }

                ply_obj.ReferenceHub.weaponManager.CallCmdChangeModPreferences(prefs);

                fake_player.PlayerInstance = ply_obj;
                fake_player.PlayerInstance.ReferenceHub.transform.localScale = scale;
                fake_player.PlayerInstance.SessionVariables.Add("IsFakePlayer", true);
                fake_player.IsValid = true;

                fake_player.OnPostInitialization();

                Log.Debug($"Constructed FakePlayer {fake_player.GetIdentifier()}");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            yield return Timing.WaitForSeconds(0.3f);

            fake_player.PlayerInstance.ReferenceHub.playerMovementSync.OverridePosition(position, 0, true);
            fake_player.PlayerInstance.Rotations = Vector2.zero;
        }

        public static T Create<T>(Vector3 position, Vector3 scale, RoleType role, bool processEvents) where T : FakePlayer
        {
            GameObject obj = Instantiate(NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
            CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();

            obj.transform.localScale = scale;
            obj.transform.position = position;

            QueryProcessor processor = obj.GetComponent<QueryProcessor>();

            processor.NetworkPlayerId = QueryProcessor._idIterator++;

            ccm.CurClass = role;
            ccm._privUserId = null;
            ccm.UserId2 = null;
            obj.GetComponent<PlayerStats>().SetHPAmount(ccm.Classes.SafeGet(role).maxHP);

            obj.GetComponent<NicknameSync>().Network_myNickSync = "Fake Player";

            ServerRoles roles = obj.GetComponent<ServerRoles>();

            roles.MyText = "Fake Player";
            roles.MyColor = "red";

            NetworkServer.Spawn(obj);
            PlayerManager.AddPlayer(obj, CustomNetworkManager.slots);

            T fake_player = obj.AddComponent<T>();
            fake_player.IsFiringEvents = processEvents;
            processor._ipAddress = fake_player.GetIdentifier();

            fake_player.AttachedCoroutines.Add(Timing.RunCoroutine(FinishInstanceCreationCoroutine(obj, position, scale)));

            return fake_player;
        }
    }
}
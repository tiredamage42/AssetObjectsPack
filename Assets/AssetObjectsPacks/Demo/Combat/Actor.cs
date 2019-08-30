using UnityEngine;
using AssetObjectsPacks;

namespace Game.Combat {

    /*
        characters, vehicles, barrels, etc...



        severity:
            0 = nothing, sickness damage, etc...
            1 = weapon / melee
            2 = heavy weapon / far explosion
            3 = near explosion / ragdoll from animation viable
            4 = straight ragdoll
    */

    public class Actor : MonoBehaviour
    {

        public float health = 100;

        public event System.Action onDeath;

        public bool isDead { get { return health <= 0; } }
        

        public int playerLayer = 0;
        public CueBehavior shotCue;
        public event OnDamageReceiveCallback onDamageReceive;

        EventPlayer player;
        DamageAdjusterCallback damageAdjuster;

        public delegate void OnDamageReceiveCallback (Vector3 origin, Transform damagedTransform, float baseDamage, int severity, float newHealth);
        public delegate float DamageAdjusterCallback (Vector3 origin, Transform damagedTransform, float baseDamage, int severity);

        public void SetDamageAdjuster (DamageAdjusterCallback damageAdjuster) {
            this.damageAdjuster = damageAdjuster;
        }

        void InitializeActorElements () {
            ActorElement[] allElements = GetComponentsInChildren<ActorElement>();
            for (int i = 0; i < allElements.Length; i++) {
                allElements[i].onDamageReceive += OnDamageReceive;
            }
        }

        public void OnDamageReceive (Vector3 origin, Transform damagedTransform, float baseDamage, int severity) {
            Debug.Log(name + " was shot at : " + damagedTransform);

            CalculateDamageOriginDirection(origin);

            CalculateFinalDamageAndHealth (origin, damagedTransform, baseDamage, severity);

            BroadcastDamageReceive (origin, damagedTransform, baseDamage, severity);

            player["Health"].SetValue(health);
            player["Severity"].SetValue(severity);

            Playlist.InitializePerformance("on damageReceive", shotCue, player, false, playerLayer, new MiniTransform(Vector3.zero, Quaternion.identity), true);
        }

        void OnDeath () {
            
        }



        void Awake () {
            player = GetComponent<EventPlayer>();
            player.AddParameter(new CustomParameter("DamageAngle", 0.0f));
            player.AddParameter(new CustomParameter("Severity", 0));
            
            // player.AddParameter(new CustomParameter("ToRight", false));
            player.AddParameter(new CustomParameter("Health", 0.0f));
        }
        void Start () {
            InitializeActorElements();
        }


        void CalculateDamageOriginDirection (Vector3 origin) {
            Vector3 rawDir = origin - transform.position;
            rawDir.y = 0;

            player["DamageAngle"].SetValue(Vector3.Angle(transform.forward, rawDir));
            player["ToRight"].SetValue(Vector3.Angle(transform.right, rawDir) <= 90);
        }
        void CalculateFinalDamageAndHealth (Vector3 origin, Transform damagedTransform, float baseDamage, int severity) {
            if (damageAdjuster != null) {
                baseDamage = damageAdjuster(origin, damagedTransform, baseDamage, severity);
            }
            health -= baseDamage;
        }

        void BroadcastDamageReceive (Vector3 origin, Transform damagedTransform, float baseDamage, int severity) {
            if (onDamageReceive != null) {
                onDamageReceive(origin, damagedTransform, baseDamage, severity, health);
            }
        }

        // public void OnShot (Vector3 shotOrigin, Transform hitTransform, float damage) {
        //     Debug.Log(name + " was shot at : " + hitTransform);

        //     CalculateDamageOriginDirection(shotOrigin);

        //     CalculateFinalDamageAndHealth (shotOrigin, hitTransform, damage);

        //     BroadcastShot (shotOrigin, hitTransform, damage);

        //     player["Health"].SetValue(health);

        //     Playlist.InitializePerformance("on shot", shotCue, player, false, playerLayer, new MiniTransform(Vector3.zero, Quaternion.identity), true);
        // }
    }
}

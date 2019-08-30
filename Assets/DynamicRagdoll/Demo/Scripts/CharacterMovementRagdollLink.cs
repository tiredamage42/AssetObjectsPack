using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Movement;
using Player;

namespace DynamicRagdoll.Demo {

    public class CharacterMovementRagdollLink : MonoBehaviour
    {
        /*
            fall speeds to set for the ragdoll controller
        */
		[Header("Fall Decay Speeds")]
		public float idleFallSpeed = 3;
		public float walkFallSpeed = 1;
		public float runFallSpeed = 1;

        RagdollController ragdollController;
        CharacterMovement characterMovement;
        MovementController movementController;

        public FollowTarget cameraScript;

        Transform originalCamScriptTarget;
        VariableUpdateScript.UpdateMode originalUpdateMode;

        

        void Awake () {
            ragdollController = GetComponent<RagdollController>();
            
            characterMovement = GetComponent<CharacterMovement>();
            
            movementController = GetComponent<MovementController>();
            
            humanoidAim = GetComponent<Game.Combat.HumanoidAim>();



            if (cameraScript != null) {
                originalCamScriptTarget = cameraScript.m_Target;
                originalUpdateMode = cameraScript.updateMode;
            }

            SubscribeToEvents();


            ragdollRightHand = ragdollController.ragdoll.GetBone(HumanBodyBones.RightLowerArm).transform.GetChild(0);
        }

        Transform ragdollRightHand;









        bool ragdollEnabled;

        /*
			switch camera to follow ragdoll	or animated hips based on ragdoll state
		*/
		// void CheckCameraTarget () {

        //     //switch camera to follow ragdoll (or animated hips)
        //     if (ragdollController.ragdollRenderersEnabled != ragdollEnabled) {

        //         ragdollEnabled = !ragdollEnabled;

        //         Ragdoll.Element hipBone = ragdollController.ragdoll.RootBone();
        //         cameraScript.m_Target = !ragdollEnabled ? hipBone.followTarget.transform : hipBone.transform;
        //         cameraScript.updateMode = !ragdollEnabled ? originalUpdateMode : VariableUpdateScript.UpdateMode.FixedUpdate;

        //         // camFollow.target = cameraTargetIsAnimatedHips ? hipBone.followTarget.transform : hipBone.transform;
        //         // camFollow.updateMode = cameraTargetIsAnimatedHips ? UpdateMode.Update : UpdateMode.FixedUpdate;
        //     }
		// }

        void SubscribeToEvents () {
            ragdollController.onGoRagdoll += OnGoRagdoll;
            ragdollController.onOrientateMaster += OnTeleportToMaster;
            ragdollController.onBlendToAnim += OnBlendToAnim;
            ragdollController.onEndGetUp += OnGetUpEnd;
        }

        void OnBlendToAnim () {
            humanoidAim.SetRightHandTransform(null);

            Ragdoll.Element hipBone = ragdollController.ragdoll.RootBone();
                cameraScript.m_Target =  hipBone.followTarget.transform ;
                cameraScript.updateMode = originalUpdateMode ;

        }


        void OnGetUpEnd () {
            if (ragdollController.state == RagdollControllerState.Animated) {
                // Debug.Log("ended getup");
                // movementController.scriptedMove = false;

                movementController.EnableScriptedMove(GetInstanceID(), false);

            }
        }

        Game.Combat.HumanoidAim humanoidAim;

        void OnGoRagdoll () {
            humanoidAim.SetRightHandTransform(ragdollRightHand);

            // Debug.Log("went ragdoll");
            
            movementController.EnableScriptedMove(GetInstanceID(), true);

            // if we're falling, dont let us go "up"
            // ragdoll was falling up stairs...

            characterMovement.preventUpwardsMotion = true;



            /* 
                when falling
                
                use normal transform stuff (dont want the character controller collisions messing stuff up)
                for falling /calculating fall ( we need all exterion collisions to reach ragdol bones)
                and teh characer chontroller acts as a 'protective shell' when it's enabled
            */

              
            characterMovement.usePhysicsController = false;



            Ragdoll.Element hipBone = ragdollController.ragdoll.RootBone();
            cameraScript.m_Target =  hipBone.transform;
            cameraScript.updateMode =  VariableUpdateScript.UpdateMode.FixedUpdate;


            
        }

        void OnTeleportToMaster () {


            /* 
				when animated or blending to animation
				use character controller movement 
				
				it has less step offset jitter than the normal transform movement
				especially when getting up 
			*/
            characterMovement.usePhysicsController = true;

            characterMovement.preventUpwardsMotion = false;


            //cehck if we started getting up
			// if (ragdollController.state == RagdollControllerState.BlendToAnimated) {
				//set zero speed
				if (movementController.speed != 0) {

                    movementController.speed = 0;

                    Debug.LogError("set spped 0");
				}
			// }

        }
        // disable scripted movement through event...



        // Update is called once per frame
        void Update()
        {
            UpdateLoop();   

        }

        void UpdateLoop () {

            if (ragdollController.state == RagdollControllerState.Animated) {

                if (characterMovement.freeFall) {
                    ragdollController.GoRagdoll(" free fall ");
                }
            }
            
            // CheckCameraTarget();


            // characterMovement.preventUpwardsMotion = ragdollController.state == RagdollControllerState.Falling;
                      
            /* 
				skip moving the character transform if we're completely ragdolled, 
                or waiting to reorient the main transform through the ragdoll controller
			*/
            characterMovement.enableMovement = ragdollController.state != RagdollControllerState.Ragdolled && ragdollController.state != RagdollControllerState.TeleportMasterToRagdoll;
			if (!characterMovement.enableMovement) {
                characterMovement.SetMoveDelta(Vector3.zero);
                characterMovement.SetRotationDelta(Vector3.zero);
            }

			/* 
				when animated or blending to animation
				use character controller movement 
				
				it has less step offset jitter than the normal transform movement
				especially when getting up 
			*/

            // on start blend to anim:
                // usePhysicsController = true

			// if (ragdollController.state == RagdollControllerState.Animated || ragdollController.state == RagdollControllerState.BlendToAnimated) {
			// 	characterMovement.usePhysicsController = true;
					
			// }
			// else {
				/* 
					when falling
					
					use normal transform stuff (dont want the character controller collisions messing stuff up)
					for falling /calculating fall ( we need all exterion collisions to reach ragdol bones)
					and teh characer chontroller acts as a 'protective shell' when it's enabled
				*/

                // on start fall (on ragdoll)
                    // usePhysicsController = false

            //     characterMovement.usePhysicsController = false;
			// }

            //cehck if we started getting up
			// if (ragdollController.state == RagdollControllerState.BlendToAnimated) {
			// 	//set zero speed
			// 	if (movementController.speed != 0) {

            //         movementController.speed = 0;
			// 	}
			// }

			//set the ragdolls fall speed based on our speed
			ragdollController.SetFallSpeed(movementController.speed == 0 ? idleFallSpeed : (movementController.speed == 1 ? walkFallSpeed : runFallSpeed));
		}







    }
}

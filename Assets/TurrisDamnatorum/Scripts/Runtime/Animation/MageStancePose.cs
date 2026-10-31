using UnityEngine;

namespace Turris
{
    /// <summary>Korekta zwykłej postawy nowego maga po retargetowaniu; stopy i długości nóg pozostają zachowane.</summary>
    public sealed class MageStancePose
    {
        static readonly float[] Shares={.40f,.35f,.25f};
        readonly Transform root,hips,neck,thighL,shinL,footL,thighR,shinR,footR;
        readonly Transform[] spine;
        readonly float restHipHeight;
        float weight;

        public MageStancePose(Animator animator,Transform character)
        {
            root=character;
            hips=animator.GetBoneTransform(HumanBodyBones.Hips);
            neck=animator.GetBoneTransform(HumanBodyBones.Neck);
            spine=new[]{animator.GetBoneTransform(HumanBodyBones.Spine),animator.GetBoneTransform(HumanBodyBones.Chest),animator.GetBoneTransform(HumanBodyBones.UpperChest)};
            thighL=animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);shinL=animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);footL=animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            thighR=animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);shinR=animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);footR=animator.GetBoneTransform(HumanBodyBones.RightFoot);
            if(hips!=null&&footL!=null&&footR!=null)restHipHeight=Vector3.Dot(hips.position-(footL.position+footR.position)*.5f,root.up);
        }

        public void Apply(in CharacterAnimState state,Vector3 velocity,float dt)
        {
            if(hips==null||neck==null||footL==null||footR==null)return;
            // Osobne, krótkie wygaszenie pozwala zachować pełny zamach czaru, uniku i ataku.
            bool ordinary=state.action==AnimAction.None;
            weight=Mathf.MoveTowards(weight,ordinary?1:0,dt*(ordinary?6:12));
            if(weight<=0)return;
            float speed=Vector3.ProjectOnPlane(velocity,root.up).magnitude;
            float rest=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.08f,.85f,speed));
            Vector3 left=footL.position,right=footR.position;
            Quaternion leftRotation=footL.rotation,rightRotation=footR.rotation;
            Vector3 poleL=Vector3.ProjectOnPlane(shinL.position-thighL.position,left-thighL.position);
            Vector3 poleR=Vector3.ProjectOnPlane(shinR.position-thighR.position,right-thighR.position);
            Vector3 support=(left+right)*.5f;
            if(rest>0) {
                float behind=Vector3.Dot(hips.position-support,root.forward);
                float hipHeight=Vector3.Dot(hips.position-support,root.up);
                float advance=Mathf.Clamp(-.035f-behind,0,.10f)*rest*weight;
                float rise=Mathf.Clamp(restHipHeight*.94f-hipHeight,0,.075f)*rest*weight;
                hips.position+=root.forward*advance+root.up*rise;
                // Pelvis pozostaje lekko ruchomy; usuwamy wyłącznie nadmierne przodopochylenie/odchylenie.
                float pelvisPitch=Mathf.Atan2(Vector3.Dot(hips.up,root.forward),Vector3.Dot(hips.up,root.up))*Mathf.Rad2Deg;
                float pelvisFix=(Mathf.Clamp(pelvisPitch,-3,3)-pelvisPitch)*rest*weight;
                hips.rotation=Quaternion.AngleAxis(pelvisFix,root.right)*hips.rotation;
            }
            float target=Mathf.Lerp(11,4,rest);
            // Rozłożenie na kręgi zachowuje krzywiznę, kołysanie barków i oddychanie z nagrania.
            for(int pass=0;pass<2;pass++) {
                Vector3 axis=neck.position-hips.position;
                float pitch=Mathf.Atan2(Vector3.Dot(axis,root.forward),Vector3.Dot(axis,root.up))*Mathf.Rad2Deg;
                float correction=-Mathf.Clamp(pitch-target,0,30)*weight;
                for(int i=0;i<spine.Length;i++)if(spine[i]!=null)
                    spine[i].rotation=Quaternion.AngleAxis(correction*Shares[i],root.right)*spine[i].rotation;
            }
            if(rest>0) {
                HellEquipmentPose.Solve(thighL,shinL,footL,left,poleL,1);
                HellEquipmentPose.Solve(thighR,shinR,footR,right,poleR,1);
                footL.rotation=leftRotation;footR.rotation=rightRotation;
            }
        }
    }
}

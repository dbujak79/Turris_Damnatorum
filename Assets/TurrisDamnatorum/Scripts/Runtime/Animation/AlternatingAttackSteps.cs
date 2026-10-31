using UnityEngine;

namespace Turris
{
    /// <summary>Naprzemienne kroki serii jednoręcznej. Zapamiętuje podporę w świecie, niezależnie od powrotu klipu do gardy.</summary>
    sealed class AlternatingAttackSteps
    {
        const float Advance=.702f,Lead=.702f,Lift=.17f;
        sealed class Leg
        {
            public Transform thigh,shin,foot;
            public Vector3 shinLocal,footLocal,thighDirection,shinDirection;
            public Quaternion thighRotation,shinRotation,footRotation;
            public float height;
        }
        readonly Transform character,hips;
        readonly Leg[] legs=new Leg[2];
        readonly Vector3[] positions=new Vector3[2];
        Vector3 stepFrom,stepTo;
        Vector3 lastPosition;
        float weight,progress,bodyTravel,bodyProgress;
        Quaternion heading;
        bool initialized,attacking;
        int stepping;
        AttackAnim lastAttack;
        public bool OwnsPose => weight>.001f;

        public AlternatingAttackSteps(Animator animator,Transform character)
        {
            this.character=character; lastPosition=character.position; hips=animator.GetBoneTransform(HumanBodyBones.Hips);
            for(int side=0;side<2;side++)
            {
                var leg=new Leg {
                    thigh=animator.GetBoneTransform(side==0?HumanBodyBones.LeftUpperLeg:HumanBodyBones.RightUpperLeg),
                    shin=animator.GetBoneTransform(side==0?HumanBodyBones.LeftLowerLeg:HumanBodyBones.RightLowerLeg),
                    foot=animator.GetBoneTransform(side==0?HumanBodyBones.LeftFoot:HumanBodyBones.RightFoot)
                };
                leg.shinLocal=leg.shin.localPosition; leg.footLocal=leg.foot.localPosition;
                var inverse=Quaternion.Inverse(character.rotation);
                leg.thighRotation=inverse*leg.thigh.rotation; leg.shinRotation=inverse*leg.shin.rotation;
                leg.footRotation=inverse*leg.foot.rotation;
                leg.thighDirection=inverse*(leg.shin.position-leg.thigh.position).normalized;
                leg.shinDirection=inverse*(leg.foot.position-leg.shin.position).normalized;
                leg.height=leg.foot.position.y-character.position.y;
                legs[side]=leg;
            }
        }

        public static bool IsStep(in CharacterAnimState state) => state.action==AnimAction.Attack && !state.offhandAttack
            && state.windupSeconds>0 && state.activeSeconds>0
            && (state.attack==AttackAnim.SlashRight || state.attack==AttackAnim.SlashLeft || state.attack==AttackAnim.Thrust);

        /// <summary>Przesunięcie kontrolera w metrach. Nie sumujemy go z dawnym ruchem korzenia tego samego ciosu.</summary>
        public float Prepare(in CharacterAnimState state,float movement,float dt,bool restart)
        {
            if(dt<=0) return 0;
            if((character.position-lastPosition).sqrMagnitude>.8f*.8f*Scale*Scale)
            {
                initialized=attacking=false; weight=0;
            }
            lastPosition=character.position;
            bool strike=IsStep(state);
            bool standing=(state.action==AnimAction.None || state.action==AnimAction.Block) && movement<.25f;
            bool eligible=strike || standing;
            if(initialized && Quaternion.Angle(heading,character.rotation)>25) eligible=false;
            weight=Mathf.MoveTowards(weight,eligible?1:0,dt/(eligible?.12f:.09f));
            if(!eligible)
            {
                attacking=false;
                if(weight<=0) initialized=false;
                return 0;
            }
            if(!initialized) Initialize();
            if(!strike) { attacking=false; return 0; }
            if(!attacking || lastAttack!=state.attack || restart)
            {
                stepping=state.attack==AttackAnim.SlashLeft?0:1;
                stepFrom=positions[stepping];
                float distance=Vector3.Dot(positions[1-stepping]-stepFrom,character.forward)+Lead*Scale;
                distance=Mathf.Clamp(distance,0,Advance*2.5f*Scale);
                stepTo=stepFrom+character.forward*distance;
                bodyTravel=Advance*Scale*Mathf.Clamp01(distance/(Advance*Scale));
                progress=bodyProgress=0; attacking=true; lastAttack=state.attack;
            }
            // Równomierne tempo kroku w rzeczywistym czasie zamachu i trafienia. Stały podział 55/45
            // gwałtownie przyspieszał nogę w krótkim oknie trafienia, co przy dłuższym kroku wyglądało jak szarpnięcie.
            float windupShare=state.windupSeconds/(state.windupSeconds+state.activeSeconds);
            float phase=state.phase==ActionPhase.Startup?state.phaseProgress*windupShare:
                state.phase==ActionPhase.Active?windupShare+state.phaseProgress*(1-windupShare):1;
            progress=Smooth(Mathf.InverseLerp(.08f,.96f,phase));
            positions[stepping]=Vector3.Lerp(stepFrom,stepTo,progress);
            positions[stepping]+=Vector3.up*(Mathf.Sin(progress*Mathf.PI)*Lift*Scale*Mathf.Clamp01(Vector3.Distance(stepFrom,stepTo)/(Advance*Scale)));
            float advance=bodyTravel*Mathf.Max(0,progress-bodyProgress);
            bodyProgress=progress;
            return advance;
        }

        float Scale => character.lossyScale.y;
        static float Smooth(float value) => value*value*(3-2*value);

        void Initialize()
        {
            initialized=true; heading=character.rotation;
            // Lewa noga prowadzi przed pierwszym zamachem; środek ciężkości pozostaje między stopami.
            for(int side=0;side<2;side++)
                positions[side]=character.position+character.right*((side==0?-.105f:.105f)*Scale)
                    +character.forward*((side==0?.10f:-.10f)*Scale)+Vector3.up*legs[side].height;
        }

        public void Apply()
        {
            if(!OwnsPose || !initialized) return;
            float shift=attacking?Mathf.Sin(progress*Mathf.PI)*.012f*Scale*weight:0;
            // Kompresja przesunięcia miednicy z długiego źródłowego wypadu do krótkiej podstawy podparcia.
            // Zostają skręt, pochylenie i wysokość z klipu, a cele stóp mieszczą się w anatomicznym zasięgu nóg.
            Vector3 center=(positions[0]+positions[1])*.5f;
            Vector3 residual=hips.position-center;
            Vector3 horizontal=character.right*Mathf.Clamp(Vector3.Dot(residual,character.right),-.025f*Scale,.025f*Scale)
                +character.forward*Mathf.Clamp(Vector3.Dot(residual,character.forward),-.025f*Scale,.025f*Scale);
            var pelvis=center+horizontal; pelvis.y=hips.position.y;
            hips.position=Vector3.Lerp(hips.position,pelvis,weight)+character.right*(stepping==1?-shift:shift);
            // Przy dłuższym kroku miednica obniża się tylko o brakujący zasięg. Nie rozciągamy goleni
            // ani nie podnosimy stopy podporowej, gdy szerszy wykrok wymaga lekkiego ugięcia nóg.
            float lowerPelvis=0;
            foreach(var leg in legs)
            {
                int side=leg==legs[0]?0:1;
                float reach=(leg.thigh.TransformVector(leg.shinLocal).magnitude+leg.shin.TransformVector(leg.footLocal).magnitude)*.995f;
                Vector3 horizontalDistance=leg.thigh.position-positions[side]; horizontalDistance.y=0;
                float height=Mathf.Sqrt(Mathf.Max(0,reach*reach-horizontalDistance.sqrMagnitude));
                lowerPelvis=Mathf.Max(lowerPelvis,leg.thigh.position.y-positions[side].y-height);
            }
            hips.position-=Vector3.up*(Mathf.Min(lowerPelvis,.075f*Scale)*weight);
            for(int side=0;side<2;side++)
            {
                var leg=legs[side];
                // Humanoid może rozciągać goleń do celu IK źródła; pancerz zachowuje długości wiązania.
                leg.shin.localPosition=Vector3.Lerp(leg.shin.localPosition,leg.shinLocal,weight);
                leg.foot.localPosition=Vector3.Lerp(leg.foot.localPosition,leg.footLocal,weight);
                Vector3 hip=leg.thigh.position;
                float a=Vector3.Distance(hip,leg.shin.position),b=Vector3.Distance(leg.shin.position,leg.foot.position);
                Vector3 target=Vector3.Lerp(leg.foot.position,positions[side],weight);
                Vector3 axis=(target-hip).normalized;
                float reach=Mathf.Clamp(Vector3.Distance(hip,target),Mathf.Abs(a-b)+.001f,(a+b)*.999f);
                Vector3 hint=Vector3.ProjectOnPlane(character.forward,axis).normalized;
                float along=(a*a+reach*reach-b*b)/(2*reach);
                Vector3 knee=hip+axis*along+hint*Mathf.Sqrt(Mathf.Max(0,a*a-along*along));
                var frame=character.rotation;
                leg.thigh.rotation=Quaternion.FromToRotation(frame*leg.thighDirection,knee-hip)*frame*leg.thighRotation;
                leg.shin.rotation=Quaternion.FromToRotation(frame*leg.shinDirection,hip+axis*reach-leg.shin.position)*frame*leg.shinRotation;
                // Wybicie z palców, uniesienie noska w przeniesieniu i lądowanie na pięcie przed pełnym podparciem.
                float pitch=attacking && side==stepping?
                    14*Mathf.Sin(2*progress*Mathf.PI)-16*Mathf.Sin(progress*Mathf.PI):0;
                leg.foot.rotation=Quaternion.Slerp(leg.foot.rotation,frame*Quaternion.AngleAxis(pitch,Vector3.right)*leg.footRotation,weight);
            }
        }
    }
}

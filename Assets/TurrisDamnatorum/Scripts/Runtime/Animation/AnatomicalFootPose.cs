using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Naprawa retargetu nóg bez stałej poprawki yaw z podporu przenoszonej na wymach.
    /// Tor kostek pochodzi z klipu; zawias kolana i orientację buta odtwarzamy w osiach danego rigu.
    /// Podeszwa jest kalibrowana raz na rzeczywistej siatce, bez BakeMesh w pętli animacji.
    /// </summary>
    public sealed class AnatomicalFootPose
    {
        sealed class Leg
        {
            public Transform upper,lower,foot,toe;
            public Vector3 lowerPosition,footPosition;
            public Vector3 toePosition;
            public Quaternion toeRotation;
            public Quaternion upperOffset,lowerOffset,footOffset;
            public Vector3 flatForwardLocal;
            public float height,yaw;
            public readonly List<Vector3> sole=new List<Vector3>();
        }
        readonly Transform root;
        readonly Leg left,right;
        readonly float scale;
        public AnatomicalFootPose(Animator animator,Transform root)
        {
            this.root=root;scale=Mathf.Max(.1f,animator.transform.lossyScale.y);
            left=Capture(animator,true);right=Capture(animator,false);
            if(left==null||right==null)return;
            // Działa też z siatką bez Read/Write; BakeMesh wykonywany wyłącznie przy budowie postaci.
            var baked=new Mesh();
            foreach(var skin in animator.GetComponentsInChildren<SkinnedMeshRenderer>()) {
                skin.BakeMesh(baked);var vertices=baked.vertices;
                Calibrate(left,right,skin.transform,vertices);Calibrate(right,left,skin.transform,vertices);
            }
            if(Application.isPlaying)Object.Destroy(baked);else Object.DestroyImmediate(baked);
            Fallback(left);Fallback(right);
        }
        Leg Capture(Animator anim,bool isLeft)
        {
            Transform B(HumanBodyBones l,HumanBodyBones r)=>anim.GetBoneTransform(isLeft?l:r);
            var leg=new Leg{upper=B(HumanBodyBones.LeftUpperLeg,HumanBodyBones.RightUpperLeg),lower=B(HumanBodyBones.LeftLowerLeg,HumanBodyBones.RightLowerLeg),foot=B(HumanBodyBones.LeftFoot,HumanBodyBones.RightFoot),toe=B(HumanBodyBones.LeftToes,HumanBodyBones.RightToes)};
            if(leg.upper==null||leg.lower==null||leg.foot==null||leg.toe==null)return null;
            leg.lowerPosition=leg.lower.localPosition;leg.footPosition=leg.foot.localPosition;
            leg.toePosition=leg.toe.localPosition;leg.toeRotation=leg.toe.localRotation;
            var upperUp=(leg.upper.position-leg.lower.position).normalized;var lowerUp=(leg.lower.position-leg.foot.position).normalized;
            Quaternion Frame(Vector3 up)=>Quaternion.LookRotation(Vector3.ProjectOnPlane(root.forward,up).normalized,up);
            leg.upperOffset=Quaternion.Inverse(Frame(upperUp))*leg.upper.rotation;
            leg.lowerOffset=Quaternion.Inverse(Frame(lowerUp))*leg.lower.rotation;
            var forward=Vector3.ProjectOnPlane(leg.toe.position-leg.foot.position,root.up).normalized;
            leg.yaw=Mathf.Clamp(Vector3.SignedAngle(root.forward,forward,root.up),-10f,10f);
            leg.flatForwardLocal=leg.foot.InverseTransformDirection(forward);
            leg.footOffset=Quaternion.Inverse(Quaternion.LookRotation(forward,root.up))*leg.foot.rotation;
            leg.height=Vector3.Dot(leg.foot.position-root.position,root.up);
            return leg;
        }
        void Calibrate(Leg leg,Leg other,Transform skin,Vector3[] vertices)
        {
            Vector3 ankle=leg.foot.position,forward=leg.foot.TransformDirection(leg.flatForwardLocal),rightAxis=Vector3.Cross(root.up,forward);
            var candidates=new List<Vector3>();float low=float.PositiveInfinity;
            float toeLength=Mathf.Clamp(Vector3.ProjectOnPlane(leg.toe.position-ankle,root.up).magnitude+.10f*scale,.16f*scale,.32f*scale);
            foreach(var vertex in vertices) {
                Vector3 p=skin.TransformPoint(vertex),d=p-ankle;
                float h=Vector3.Dot(d,root.up),z=Vector3.Dot(d,forward),x=Vector3.Dot(d,rightAxis);
                if(h>-.015f*scale||h<-.28f*scale||z<-.12f*scale||z>toeLength||Mathf.Abs(x)>.12f*scale)continue;
                if((p-other.foot.position).sqrMagnitude<(p-ankle).sqrMagnitude)continue;
                float y=Vector3.Dot(p-root.position,root.up);low=Mathf.Min(low,y);candidates.Add(p);
            }
            if(candidates.Count==0)return;
            // Ekstrema obwodu podeszwy; pełna siatka i jej wagi pozostają nietknięte.
            for(int direction=0;direction<16;direction++) {
                float angle=direction*Mathf.PI/8;Vector3 axis=forward*Mathf.Cos(angle)+rightAxis*Mathf.Sin(angle),best=Vector3.zero;float extent=float.NegativeInfinity;
                foreach(var p in candidates)if(Vector3.Dot(p-root.position,root.up)<=low+.012f*scale){float e=Vector3.Dot(p-ankle,axis);if(e>extent){extent=e;best=p;}}
                if(float.IsFinite(extent))leg.sole.Add(leg.foot.InverseTransformPoint(best));
            }
        }
        void Fallback(Leg leg)
        {
            if(leg.sole.Count>0)return;
            Vector3 forward=leg.foot.TransformDirection(leg.flatForwardLocal),rightAxis=Vector3.Cross(root.up,forward);
            foreach(float z in new[]{-.07f,.17f})foreach(float x in new[]{-.05f,.05f})leg.sole.Add(leg.foot.InverseTransformPoint(leg.foot.position-root.up*leg.height+(forward*z+rightAxis*x)*scale));
        }
        public void Apply(float weight,float legYaw,float preservedForward)
        {
            if(left==null||right==null||weight<.001f)return;
            Quaternion frame=Quaternion.AngleAxis(legYaw,root.up)*root.rotation;
            Correct(left,weight,frame,preservedForward);Correct(right,weight,frame,preservedForward);
        }
        void Correct(Leg leg,float weight,Quaternion frame,float preservedForward)
        {
            Vector3 up=root.up,forward=frame*Vector3.forward;
            Vector3 hip=leg.upper.position,knee=leg.lower.position,ankle=leg.foot.position;
            Quaternion originalUpper=leg.upper.rotation,originalLower=leg.lower.rotation,originalFoot=leg.foot.rotation;
            // Humanoid przenosił również przesunięcie kości palców, choć długość śródstopia jest stała.
            // Miękkie obuwie zachowuje niewielkie zgięcie palców; stalowy nosek ma już pozę sztywną.
            leg.toe.localPosition=Vector3.Lerp(leg.toe.localPosition,leg.toePosition,weight);
            float toeAngle=Quaternion.Angle(leg.toeRotation,leg.toe.localRotation);
            if(toeAngle>25){var limited=Quaternion.Slerp(leg.toeRotation,leg.toe.localRotation,25/toeAngle);leg.toe.localRotation=Quaternion.Slerp(leg.toe.localRotation,limited,weight);}
            Vector3 axis=(hip-ankle).normalized;
            Vector3 baseHint=Vector3.ProjectOnPlane(forward,axis).normalized;
            if(baseHint.sqrMagnitude<1e-6f)return;
            Vector3 hint=Vector3.ProjectOnPlane(knee-hip,axis);
            float bend=180-Vector3.Angle(hip-knee,ankle-knee);
            float deviation=hint.sqrMagnitude>1e-6f?Vector3.SignedAngle(baseHint,hint,axis):0;
            hint=Quaternion.AngleAxis(Mathf.Clamp(deviation,-18,18)*Mathf.SmoothStep(0,1,Mathf.InverseLerp(8,30,bend)),axis)*baseHint;
            // Kąt zgięcia pozostaje z nagrania; usuwamy skręt wokół goleni, nie prostujemy nogi.
            var shinUp=(knee-ankle).normalized;Vector3 kneeForward=Vector3.ProjectOnPlane(knee-hip,shinUp);
            if(kneeForward.sqrMagnitude<1e-6f)kneeForward=Vector3.ProjectOnPlane(forward,shinUp);
            // Obrót z retargetu potrafi skoczyć o 80° w jednej klatce. Zgięcie buta wyznacza
            // ciągły ruch goleni, tak aby pięta wracała w górę za kolanem bez przejęcia tego skoku.
            float pitch=Vector3.SignedAngle(-up,ankle-knee,frame*Vector3.right)*.65f-5;
            float lift=Mathf.SmoothStep(0,1,Mathf.InverseLerp(leg.height+.015f*scale,leg.height+.13f*scale,Vector3.Dot(ankle-root.position,up)));
            pitch=Mathf.Clamp(pitch,-25,55)*Mathf.Lerp(.25f,1,lift);
            var targetFoot=frame*Quaternion.AngleAxis(leg.yaw,Vector3.up)*Quaternion.AngleAxis(pitch,Vector3.right)*leg.footOffset;
            float rotationWeight=weight*(1-preservedForward);
            leg.foot.rotation=Quaternion.Slerp(originalFoot,targetFoot,rotationWeight);
            float bottom=float.PositiveInfinity;
            foreach(var p in leg.sole)bottom=Mathf.Min(bottom,Vector3.Dot(leg.foot.TransformPoint(p)-root.position,up));
            Vector3 target=ankle+up*Mathf.Clamp(.002f*scale-bottom,0,.22f*scale);
            // Nie zmieniamy rozstawu ani poziomego toru kostek. Stałe długości chronią cholewkę przed rozciąganiem.
            leg.lower.localPosition=Vector3.Lerp(leg.lower.localPosition,leg.lowerPosition,rotationWeight);
            leg.foot.localPosition=Vector3.Lerp(leg.foot.localPosition,leg.footPosition,rotationWeight);
            float a=Vector3.Distance(hip,leg.lower.position),b=Vector3.Distance(leg.lower.position,leg.foot.position);
            var to=target-hip;float reach=Mathf.Clamp(to.magnitude,Mathf.Abs(a-b)+.001f,(a+b)*.9995f);axis=to.normalized;
            hint=Vector3.ProjectOnPlane(hint,axis).normalized;if(hint.sqrMagnitude<1e-6f)return;
            float along=(a*a+reach*reach-b*b)/(2*reach);Vector3 goalKnee=hip+axis*along+hint*Mathf.Sqrt(Mathf.Max(0,a*a-along*along));
            Vector3 normal=Vector3.Cross(goalKnee-hip,hip+axis*reach-goalKnee).normalized;
            Quaternion Frame(Vector3 direction)=>Quaternion.LookRotation(Vector3.Cross(normal,-direction).normalized,-direction);
            var desiredUpper=Frame((goalKnee-hip).normalized)*leg.upperOffset;
            var desiredLower=Frame((hip+axis*reach-goalKnee).normalized)*leg.lowerOffset;
            // Współrzędne ramki w pozie wiązania są światowe; przesunięcie root nie uczestniczy w obrocie.
            Quaternion solvedFoot=leg.foot.rotation;
            if(rotationWeight>.001f){leg.upper.rotation=Quaternion.Slerp(originalUpper,desiredUpper,rotationWeight);leg.lower.rotation=Quaternion.Slerp(originalLower,desiredLower,rotationWeight);}
            // Dodatkowe podniesienie podparcia także dla wcześniej poprawionego paladyna.
            if(target.y>ankle.y+.00001f)MoveAnkle(leg,Vector3.Lerp(ankle,target,weight),hint);
            leg.foot.rotation=solvedFoot;
            if(rotationWeight>.001f) {
                // Ostateczny zawias po rozwiązaniu kolana: osie rigu i palców nie muszą być symetryczne.
                // Nie mnożymy poprawki z innej fazy; usuwamy aktualny skręt wokół aktualnej goleni.
                Vector3 tibia=(leg.lower.position-leg.foot.position).normalized;
                Vector3 plane=Vector3.ProjectOnPlane(leg.lower.position-leg.upper.position,tibia);
                if(bend<25||plane.sqrMagnitude<1e-6f)plane=Vector3.ProjectOnPlane(forward,tibia);
                Vector3 toe=leg.toe.position-leg.foot.position;
                if(plane.sqrMagnitude>1e-6f&&toe.sqrMagnitude>1e-6f) {
                    float flex=Mathf.Clamp(Vector3.Angle(tibia,toe),35,150)*Mathf.Deg2Rad;
                    Vector3 toePlane=Quaternion.AngleAxis(Mathf.Clamp(leg.yaw,-6,6),tibia)*plane.normalized;
                    Vector3 direction=tibia*Mathf.Cos(flex)+toePlane*Mathf.Sin(flex);
                    Quaternion aligned=Quaternion.FromToRotation(toe,direction)*leg.foot.rotation;
                    leg.foot.rotation=Quaternion.Slerp(leg.foot.rotation,aligned,rotationWeight);
                }
                // Obrót zmienia wysokość pięty i noska; podparcie kontrolujemy ponownie po orientacji.
                bottom=float.PositiveInfinity;foreach(var p in leg.sole)bottom=Mathf.Min(bottom,Vector3.Dot(leg.foot.TransformPoint(p)-root.position,up));
                if(bottom<.001f*scale){var rotation=leg.foot.rotation;MoveAnkle(leg,leg.foot.position+up*Mathf.Clamp((.002f*scale-bottom)*weight,0,.16f*scale),hint);leg.foot.rotation=rotation;}
            }
        }
        static void MoveAnkle(Leg leg,Vector3 target,Vector3 hint)
        {
            var hip=leg.upper.position;var knee=leg.lower.position;var ankle=leg.foot.position;
            float a=Vector3.Distance(hip,knee),b=Vector3.Distance(knee,ankle),distance=Mathf.Clamp((target-hip).magnitude,Mathf.Abs(a-b)+.001f,(a+b)*.9995f);
            Vector3 axis=(target-hip).normalized;hint=Vector3.ProjectOnPlane(hint,axis).normalized;
            float along=(a*a+distance*distance-b*b)/(2*distance);var next=hip+axis*along+hint*Mathf.Sqrt(Mathf.Max(0,a*a-along*along));
            leg.upper.rotation=Quaternion.FromToRotation(knee-hip,next-hip)*leg.upper.rotation;
            leg.lower.rotation=Quaternion.FromToRotation(leg.foot.position-leg.lower.position,target-leg.lower.position)*leg.lower.rotation;
        }
    }
}

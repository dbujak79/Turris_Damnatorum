using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Turris.Editor
{
    public static class HeroFeetRepair
    {
        [MenuItem("Turris/Naprawa stóp/Włącz korektę aktywnych bohaterów")]
        public static void Enable()
        {
            int count=0;
            var classes=AssetDatabase.FindAssets("t:ClassDefinition",new[]{"Assets/TurrisDamnatorum/Content/Classes"}).Select(g=>AssetDatabase.LoadAssetAtPath<ClassDefinition>(AssetDatabase.GUIDToAssetPath(g)));
            foreach(var visual in classes.Where(c=>c!=null&&c.id!="class_knight"&&c.Visual!=null).Select(c=>c.Visual).Distinct()) {
                if(visual.demon||visual.name=="visual_hero_warrior")continue;
                Undo.RecordObject(visual,"Naturalne stopy bohatera");visual.anatomicalFeet=true;EditorUtility.SetDirty(visual);count++;
            }
            AssetDatabase.SaveAssets();Debug.Log("HERO_FEET_REPAIR_ENABLED "+count+" wyglądów; wojownik bez zmian.");
        }
    }
}

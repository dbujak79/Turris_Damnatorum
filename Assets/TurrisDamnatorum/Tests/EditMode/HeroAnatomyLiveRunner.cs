using System;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Turris.Tests
{
    [InitializeOnLoad]public static class HeroAnatomyLiveRunner
    {
        const string Key="Turris.AnatomyAudit.Enabled";
        static readonly TestRunnerApi Api;
        static HeroAnatomyLiveRunner(){Api=ScriptableObject.CreateInstance<TestRunnerApi>();Api.RegisterCallbacks(new Results());}
        [MenuItem("Turris/Audyt anatomii/Uruchom pełny audyt")]
        public static void Run() {Start("^Turris\\.Tests\\.HeroAnatomyAuditPlayTests\\.","audit-live.xml");}
        [MenuItem("Turris/Audyt anatomii/Uzupełnij źródła i podeszwy")]
        public static void Followup() {Start("^Turris\\.Tests\\.HeroAnatomyAuditPlayTests\\.D_","followup-live.xml");}
        [MenuItem("Turris/Audyt anatomii/Uzupełnij podparcie i akcje")]
        public static void Contacts() {Start("^Turris\\.Tests\\.HeroAnatomyAuditPlayTests\\.(E_|F_)","contacts-live.xml");}
        [MenuItem("Turris/Audyt anatomii/Powtórz pełną serię berserkera")]
        public static void BerserkerSeries() {Start("^Turris\\.Tests\\.HeroAnatomyAuditPlayTests\\.F_","berserker-series-live.xml",true);}
        static void Start(string pattern,string file,bool berserkerOnly=false) {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Wróć do trybu edycji przed audytem.");
            Directory.CreateDirectory("TestResults/hero-anatomy");SessionState.SetBool(Key,true);
            SessionState.SetBool("Turris.AnatomyAudit.BerserkerOnly",berserkerOnly);
            SessionState.SetString(Key+".File",file);
            File.WriteAllText("TestResults/hero-anatomy/live-status.json","{\"running\":true}");
            Api.Execute(new ExecutionSettings(new Filter{testMode=TestMode.PlayMode,groupNames=new[]{pattern}}));
        }
        class Results:ICallbacks {
            public void RunStarted(ITestAdaptor tests){} public void TestStarted(ITestAdaptor test){} public void TestFinished(ITestResultAdaptor result){}
            public void RunFinished(ITestResultAdaptor result){if(!SessionState.GetBool(Key,false))return;SessionState.SetBool(Key,false);
                TestRunnerApi.SaveResultToFile(result,"TestResults/hero-anatomy/"+SessionState.GetString(Key+".File","audit-live.xml"));
                File.WriteAllText("TestResults/hero-anatomy/live-status.json",JsonUtility.ToJson(new Status{passed=result.PassCount,failed=result.FailCount,skipped=result.SkipCount,result=result.ResultState},true));
                Debug.Log("HERO_ANATOMY_AUDIT "+result.ResultState+" "+result.PassCount+"/"+result.FailCount);
            }
        }
        [Serializable]class Status {public bool running;public int passed,failed,skipped;public string result;}
    }
}

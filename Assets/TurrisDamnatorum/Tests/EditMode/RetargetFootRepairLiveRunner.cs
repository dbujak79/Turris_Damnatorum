using System;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
namespace Turris.Tests
{
    [InitializeOnLoad]public static class RetargetFootRepairLiveRunner
    {
        const string Key="Turris.FeetRepair.Run";static readonly TestRunnerApi Api;
        static RetargetFootRepairLiveRunner(){Api=ScriptableObject.CreateInstance<TestRunnerApi>();Api.RegisterCallbacks(new Results());}
        [MenuItem("Turris/Naprawa stóp/Porównaj IK")]
        public static void Compare(){Run("^Turris\\.Tests\\.RetargetFootRepairPlayTests\\.A_","ik-comparison-live.xml");}
        [MenuItem("Turris/Naprawa stóp/Sprawdź zawiasy wszystkich rigów")]
        public static void Hinges(){Run("^Turris\\.Tests\\.RetargetFootRepairPlayTests\\.B_","hinges-live.xml");}
        [MenuItem("Turris/Naprawa stóp/Sprawdź pełną macierz po naprawie")]
        public static void Audit(){Run("^Turris\\.Tests\\.HeroAnatomyAuditPlayTests\\.(A_|B_|C_|D_|E_|F_)","after-live.xml");}
        [MenuItem("Turris/Naprawa stóp/Sprawdź regresję modeli i postawy")]
        public static void Regression(){Run("^Turris\\.Tests\\.(MagePosturePlayTests|MageAnimePlayTests|BerserkerAnimePlayTests|HeroModelPlayTests)\\.|^Turris\\.Tests\\.RetargetFootRepairPlayTests\\.B_","regression-live.xml");}
        static void Run(string pattern,string file){if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Zakończ Play przed testem.");Directory.CreateDirectory("TestResults/hero-feet-repair");SessionState.SetBool(Key,true);SessionState.SetBool("Turris.AnatomyAudit.BerserkerOnly",false);SessionState.SetString(Key+".File",file);File.WriteAllText("TestResults/hero-feet-repair/status.json","{\"running\":true}");Api.Execute(new ExecutionSettings(new Filter{testMode=TestMode.PlayMode,groupNames=new[]{pattern}}));}
        class Results:ICallbacks{public void RunStarted(ITestAdaptor t){}public void TestStarted(ITestAdaptor t){}public void TestFinished(ITestResultAdaptor r){}public void RunFinished(ITestResultAdaptor r){if(!SessionState.GetBool(Key,false))return;SessionState.SetBool(Key,false);TestRunnerApi.SaveResultToFile(r,"TestResults/hero-feet-repair/"+SessionState.GetString(Key+".File","result.xml"));File.WriteAllText("TestResults/hero-feet-repair/status.json",JsonUtility.ToJson(new Status{passed=r.PassCount,failed=r.FailCount,result=r.ResultState}));}}
        [Serializable]class Status{public int passed,failed;public string result;}
    }
}

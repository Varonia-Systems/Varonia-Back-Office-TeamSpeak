#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public static class TeamspeakDefineEnabler
{
    static TeamspeakDefineEnabler()
    {
        string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        if (!symbols.Contains("VBO_TEAMSPEAK"))
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                symbols + ";VBO_TEAMSPEAK"
            );
        }
    }
}
#endif
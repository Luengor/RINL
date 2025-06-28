using UnityEditor;

public class WebOptimizer
{
    [MenuItem("Example/Optimize")]
    public static void Optimize()
    {
        UnityEditor.WebGL.UserBuildSettings.codeOptimization = UnityEditor.WebGL.WasmCodeOptimization.RuntimeSpeed;
    }
}

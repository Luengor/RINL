using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;
using UnityEngine;

public class CustomBuild : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        // Set IL2CPP code generation to Optimize Size
        PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.WebGL, Il2CppCodeGeneration.OptimizeSpeed);

        // Set the Managed Stripping Level to High
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.High);

        // Enable WebAssembly 2023 features
        PlayerSettings.WebGL.wasm2023 = true;

        // Deactivate exceptions
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;

        // Deactivate debug symbols
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;

        UnityEditor.WebGL.UserBuildSettings.codeOptimization = UnityEditor.WebGL.WasmCodeOptimization.RuntimeSpeedLTO;
    }
}

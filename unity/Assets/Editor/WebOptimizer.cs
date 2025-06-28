using UnityEditor;
using UnityEditor.Build;

public class WebOptimizer
{
    [MenuItem("Example/Optimize")]
    public static void Optimize()
    {
        var namedBuildTarget = NamedBuildTarget.WebGL;

        // Set IL2CPP code generation to Optimize Size
        PlayerSettings.SetIl2CppCodeGeneration(namedBuildTarget,
                                        Il2CppCodeGeneration.OptimizeSpeed);

        // Set the Managed Stripping Level to High
        PlayerSettings.SetManagedStrippingLevel(namedBuildTarget,
                                            ManagedStrippingLevel.High);

        //Enable WebAssembly 2023 features
        PlayerSettings.WebGL.wasm2023 = true;

        // Deactivate exceptions
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;

        // Deactivate debug symbols
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;

        UnityEditor.WebGL.UserBuildSettings.codeOptimization = UnityEditor.WebGL.WasmCodeOptimization.RuntimeSpeed;
    }
}

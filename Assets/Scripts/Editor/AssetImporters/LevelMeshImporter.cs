using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class LevelMeshImporter : AssetPostprocessor
{

    void OnPreprocessModel()
    {
        if (!assetPath.Contains("Mesh_Env_"))
            return;

        Debug.Log($"Processing level mesh: {assetPath}");
        SetupImporterSettings(assetImporter as ModelImporter);
    }

    void OnPostprocessModel(GameObject model)
    {
        CreatePrefab(assetPath, model);
    }


    private void SetupImporterSettings(ModelImporter importer)
    {
        importer.globalScale                            = 100;
        importer.importBlendShapes                      = false;
        importer.importVisibility                       = false;
        importer.importAnimation                        = false;
        importer.importCameras                          = false;
        importer.importLights                           = false;
        importer.isReadable                             = true;
        importer.autoGenerateAvatarMappingIfUnspecified = false;
        importer.avatarSetup                            = ModelImporterAvatarSetup.NoAvatar;
        importer.generateAnimations                     = ModelImporterGenerateAnimations.None;
        importer.materialImportMode                     = ModelImporterMaterialImportMode.None;
        importer.animationType                          = ModelImporterAnimationType.None;
    }


    // Attempts to create a prefab for the asset being imported if it doesn't exist already.
    // Searches for material with its name on the format: Mat_{}_Default
    private void CreatePrefab(string assetPath, GameObject model)
    {
        int      lastIndex       = assetPath.LastIndexOf('/');
        string   path            = assetPath.Substring(0, lastIndex+1);
        string   biome           = assetPath.Split('/').Last().Split('_').Skip(3).First();
        string   materialName    = $"Mat_{biome}_Default.mat";
        string   materialPath    = path + materialName;

        Debug.Log($"Loading model default material at {materialPath}");
        Material defaultMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (defaultMaterial == null)
        {
            Debug.Log($"No default material found for biome: {biome}. Exiting.");
            return;
        }

        bool success = false;

        string prefabPath = assetPath.Replace(".fbx", ".prefab"); 

        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            Debug.Log($"Prefab for model {model.name} already exists. Skipping creation.");
            return;
        }

        // If the editor in currently in the middle of an asset editing batch operation, 
        // as controlled with AssetDatabase.StartAssetEditing and AssetDatabase.StopAssetEditing, 
        // assets are not immediately imported upon being saved. 
        // In this case, SaveAsPrefabAsset will return null even if the save was successful 
        // because the saved Prefab Asset was not yet reimported and thus not yet available.
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(model, prefabPath, out success);
        if (success && prefab == null)
        {
            Debug.Log("Created prefab not available. Quitting.");
            return;
        }

        if (!success)
        {
            Debug.Log($"Failed creating prefab: {prefabPath}");
            return;
        }

        var prefabRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in prefabRenderers)
            renderer.material = defaultMaterial;
    }
}

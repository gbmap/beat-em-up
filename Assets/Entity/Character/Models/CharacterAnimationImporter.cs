using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CharacterAnimationImporter : AssetPostprocessor
{

    void OnPreprocessModel()
    {
        if (!assetPath.Contains("Character@") &&
            !assetPath.Contains("Assets/Entity/Character/Animations/FBX"))
        {
            return;
        }

        ModelImporter modelImporter = assetImporter as ModelImporter;
        modelImporter.importMaterials = false;
        modelImporter.importCameras = false;
        modelImporter.importLights = false;
        modelImporter.animationType = ModelImporterAnimationType.Human;

        foreach (ModelImporterClipAnimation clip in modelImporter.clipAnimations)
        {
            clip.loopTime = assetPath.Contains("_loop");
            clip.keepOriginalOrientation = true;
            clip.keepOriginalPositionXZ = true;
            clip.keepOriginalPositionY = true;
        }
    }
}

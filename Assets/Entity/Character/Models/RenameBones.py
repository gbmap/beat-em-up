
import bpy

boneMapping = {
    "Hips": "Hips",
    "Spine": "Spine_01",
    "Spine1": "Spine_02",
    "Spine2": "Spine_03",
    "Neck": "Neck",
    
    "LeftShoulder": "Clavicle_L",
    "LeftArm": "Shoulder_L",
    "LeftForeArm": "Elbow_L",
    "LeftHand": "Hand_L",
    
    "RightShoulder": "Clavicle_R",
    "RightArm": "Shoulder_R",
    "RightForeArm": "Elbow_R",
    "RightHand": "Hand_R",
    
    "LeftHandIndex1": "IndexFinger_01",
    "LeftHandIndex2": "IndexFinger_02",
    "LeftHandIndex3": "IndexFinger_03",
    "LeftHandIndex4": "IndexFinger_04",
    
    "LeftHandThumb1": "Thumb_01",
    "LeftHandThumb2": "Thumb_02",
    "LeftHandThumb3": "Thumb_03",
    "LeftHandThumb4": "Thumb_04",
    
    "LeftHandMiddle1": "Finger_01",
    "LeftHandMiddle2": "Finger_02",
    "LeftHandMiddle3": "Finger_03",    
    "LeftHandMiddle4": "Finger_04",
    
    "RightHandIndex1": "IndexFinger_01.001",
    "RightHandIndex2": "IndexFinger_02.001",
    "RightHandIndex3": "IndexFinger_03.001",
    "RightHandIndex4": "IndexFinger_04.001",
    
    "RightHandThumb1": "Thumb_01.001",
    "RightHandThumb2": "Thumb_02.001",
    "RightHandThumb3": "Thumb_03.001",
    "RightHandThumb4": "Thumb_04.001",
    
    "RightHandMiddle1": "Finger_01.001",
    "RightHandMiddle2": "Finger_02.001",
    "RightHandMiddle3": "Finger_03.001",    
    "RightHandMiddle4": "Finger_04.001",
    
    "LeftUpLeg": "UpperLeg_L",
    "LeftLeg": "LowerLeg_L",
    "LeftFoot": "Ankle_L",
    "LeftToeBase": "Ball_L",
    "LeftToe_End": "Toes_L",
    
    "RightUpLeg": "UpperLeg_R",
    "RightLeg": "LowerLeg_R",
    "RightFoot": "Ankle_R",
    "RightToeBase": "Ball_R",
    "RightToe_End": "Toes_R"
}

bpy.data.objects['Armature'].name = "Root"
rig = bpy.data.armatures[0]
rig.name = "Root"

# renomear bones
for bone in rig.bones:
    bone.name = bone.name.replace("mixamorig:", "")
    if bone.name in boneMapping:
        bone.name = boneMapping[bone.name]
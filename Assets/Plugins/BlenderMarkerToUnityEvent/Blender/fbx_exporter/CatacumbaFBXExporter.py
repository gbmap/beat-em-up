import bpy
import bpy_extras
import xml.etree.ElementTree as ET

class CatacumbaFBXExporter(bpy.types.Operator):
    bl_idname = 'catacumba.export'
    bl_label = 'Catacumba FBX Exporter'

    #targetGameAssetPath = bpy.props.StringProperty()
    sTargetGameAssetPath: bpy.props.StringProperty(name="Target", subtype='FILE_NAME')
    bExportModel: bpy.props.BoolProperty(name="Model")
    bExportAnimEvents: bpy.props.BoolProperty(name="Anim Events")

 
    def invoke(self, context, event):
        self.sTargetGameAssetPath = bpy.data.filepath.replace('.blend', '.fbx')
        self.bExportModel = True
        self.bExportAnimEvents = True
        return context.window_manager.invoke_props_dialog(self)

    def execute(self, context):
        print(self.sTargetGameAssetPath)
        bpy.ops.export_scene.fbx(filepath=self.sTargetGameAssetPath, object_types={'MESH', 'ARMATURE'})
        self.save_events(context, self.sTargetGameAssetPath.replace('.fbx', '.xml'))
        # Do something here
        return {'FINISHED'}
    

    def save_events(self, context, filepath=""):
        # Build XML structure from actions + markers
        eScene = ET.Element("scene", {
            "version":"%i" % 1,
            "fps":"%i" % context.scene.render.fps
        })
        eTimeline = ET.SubElement(eScene, "timeline")
        eMarkers = ET.SubElement(eTimeline, "markers")
        for marker in context.scene.timeline_markers:
            ET.SubElement(eMarkers, "marker", {"name":marker.name, "frame":"%i" % marker.frame}) 
        
        eActions = ET.SubElement(eScene, "actions")
        for action in bpy.data.actions:
            eAction = ET.SubElement(eActions, "action", {"name":action.name})
            eMarkers = ET.SubElement(eAction, "markers")
            for marker in action.pose_markers:
                ET.SubElement(eMarkers, "marker", {"name":marker.name, "frame":"%i" % marker.frame}) 
    
        if False:
            # wrap it in an ElementTree instance, and save as XML
            doc = ET.ElementTree(eScene)
            doc.write(filepath)
        else:
            import xml.dom.minidom

            # parse it into minidom and pretty print...
            minidom = xml.dom.minidom.parseString(ET.tostring(eScene, encoding='utf8', method='xml'))
            f = open(filepath, "wt")
            f.write(minidom.toprettyxml())
            f.close()
            print(minidom.toprettyxml())
        


def register():
    bpy.utils.register_class(CatacumbaFBXExporter)

def unregister():
    bpy.utils.unregister_class(CatacumbaFBXExporter)

if __name__ == "__main__":
    register()
    bpy.ops.catacumba.export('INVOKE_DEFAULT')

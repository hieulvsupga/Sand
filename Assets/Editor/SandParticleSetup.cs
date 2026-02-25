using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility để tự động thêm SandParticleEffect vào scene Chihien.
/// Sử dụng: Menu -> Sand Tools -> Setup Sand Particle in Chihien Scene
/// </summary>
public class SandParticleSetup : Editor
{
    [MenuItem("Sand Tools/Setup Sand Particle in Chihien Scene")]
    static void SetupSandParticle()
    {
        // Tìm GP_Machine trong scene
        GameObject gpMachine = GameObject.Find("GP_Machine");
        if (gpMachine == null)
        {
            EditorUtility.DisplayDialog("Error", "Không tìm thấy GP_Machine trong scene hiện tại!\nHãy mở scene Chihien trước.", "OK");
            return;
        }

        // Tìm ThreeShape trong scene
        GameObject threeShape = GameObject.Find("ThreeShape");
        if (threeShape == null)
        {
            EditorUtility.DisplayDialog("Error", "Không tìm thấy ThreeShape trong scene hiện tại!", "OK");
            return;
        }

        // Kiểm tra xem đã có SandParticleEffect chưa
        SandParticleEffect existing = Object.FindObjectOfType<SandParticleEffect>();
        if (existing != null)
        {
            EditorUtility.DisplayDialog("Info", "SandParticleEffect đã tồn tại trong scene!", "OK");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        // Tạo GameObject mới
        GameObject sandParticleObj = new GameObject("SandParticle");
        
        // Thêm ParticleSystem trước (SandParticleEffect sẽ cấu hình nó)
        sandParticleObj.AddComponent<ParticleSystem>();
        
        // Thêm SandParticleEffect script
        SandParticleEffect effect = sandParticleObj.AddComponent<SandParticleEffect>();

        // Set references qua SerializedObject
        SerializedObject so = new SerializedObject(effect);
        so.FindProperty("gpMachine").objectReferenceValue = gpMachine.transform;
        so.FindProperty("threeShape").objectReferenceValue = threeShape.transform;
        so.ApplyModifiedProperties();

        // Đặt vị trí ban đầu gần GP_Machine
        sandParticleObj.transform.position = gpMachine.transform.position + new Vector3(-0.5f, -0.3f, 0f);

        // Đánh dấu scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        // Select object mới
        Selection.activeGameObject = sandParticleObj;

        // Undo support
        Undo.RegisterCreatedObjectUndo(sandParticleObj, "Create Sand Particle Effect");

        EditorUtility.DisplayDialog("Success", 
            "Đã tạo SandParticle GameObject!\n\n" +
            "- GP_Machine đã được gán làm nguồn đổ cát\n" +
            "- ThreeShape đã được gán làm nơi nhận cát\n\n" +
            "Nhấn Space khi Play để đổ cát.\n" +
            "Điều chỉnh Spawn Offset trong Inspector nếu cần.", 
            "OK");

        Debug.Log("[SandParticleSetup] Đã tạo SandParticle thành công!");
    }
}

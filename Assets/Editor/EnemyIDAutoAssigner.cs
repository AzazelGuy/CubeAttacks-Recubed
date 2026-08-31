#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class EnemyIDAutoAssigner
{
    [MenuItem("Tools/Inimigos/Gerar IDs Faltantes na Cena")]
    public static void GenerateIDsForCurrentScene()
    {
        // Busca todos os inimigos da cena (incluindo qualquer classe que herde de EnemyBase)
        EnemyBase[] enemies = Object.FindObjectsByType<EnemyBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int generatedCount = 0;

        foreach (var enemy in enemies)
        {
            // Usa SerializedObject para garantir que o Unity salve a alteração corretamente no Prefab Instance
            SerializedObject serializedEnemy = new SerializedObject(enemy);
            SerializedProperty idProperty = serializedEnemy.FindProperty("enemyID");

            if (idProperty != null && string.IsNullOrEmpty(idProperty.stringValue))
            {
                // Cria uma combinação do nome da cena + nome do GameObject + um hash único (GUID)
                string sceneName = enemy.gameObject.scene.name;
                string newID = $"{sceneName}_{enemy.gameObject.name}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";

                idProperty.stringValue = newID;
                serializedEnemy.ApplyModifiedProperties();
                
                generatedCount++;
            }
        }

        if (generatedCount > 0)
        {
            // Marca a cena como alterada para você não esquecer de salvar
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"<color=green>[ID Generator]</color> Sucesso! {generatedCount} IDs de inimigos foram gerados automaticamente.");
        }
        else
        {
            Debug.Log("<color=yellow>[ID Generator]</color> Todos os inimigos na cena já possuem IDs válidos. Ou <color=red> [Nenhum inimigo foi encontrado!] </color>");
        }
    }
}
#endif
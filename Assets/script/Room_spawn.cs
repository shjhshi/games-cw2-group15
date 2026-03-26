using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class Room_spawn : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private string folderPath =  "Scenes";
        public int maxX =100;
        public int maxZ =100;
        public int minX = -100;
        public int minZ = -100;
        public int attempts_spawn = 50;
        public bool Start_spawn = false;
    void Start()
    {
        Start_spawn = false;
    }
// Update is called once per frame
    void Update()
    {
        if (Start_spawn){
            Clean_room();
            Generate_room();
            Start_spawn = false;
        }
    }
    
    void Clean_room()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if(obj.name.EndsWith("(Clone)"))
            {
                Destroy(obj);
            }
        }
    }
    void Generate_room()
    {
        // go through the file
        string fullPath = Path.Combine(Application.dataPath, folderPath);
        if (!Directory.Exists(fullPath))
        {
            Debug.LogError($"not exist: {fullPath}");
            return;
        }
        string[] blenderFiles = Directory.GetFiles(fullPath, "*.blend", SearchOption.TopDirectoryOnly);
        if (blenderFiles.Length == 0)
        {
            Debug.LogError($"No .blender files found in {fullPath}");
            return;
        }

        // store in a list
        List<GameObject> prefabs = new List<GameObject>();
        foreach (string file in blenderFiles)
        {
            string assetPath = file.Replace(Application.dataPath, "Assets");
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (obj != null)
            {
                prefabs.Add(obj);
            }
            else
            {
                Debug.LogWarning($"Cannot load asset: {assetPath}");
            }
        }

        if (prefabs.Count == 0)
        {
            Debug.LogError("No available prefabs");
            return;
        }

        // spawn the prefabs
        foreach (GameObject prefab in prefabs)
        {
            bool spawned = false;
            for (int i = 0; i < attempts_spawn; i++)
            {
                float x = Random.Range(minX, maxX);
                float z = Random.Range(minZ, maxZ);
                int random_rotate = Random.Range(0, 4); 
                Vector3 rotation = new Vector3(0, random_rotate*90, 0);
                Quaternion rotation_quaternion = Quaternion.Euler(rotation);
                Vector3 position = new Vector3(x, 0, z);
                // Collider[] colliders = Physics.OverlapBox(position, prefab.transform.localScale);
                // if (colliders.Length == 0)                {
                //     Instantiate(prefab, position, Quaternion.identity);
                //     spawned = true;
                //     break;
                // }
                if (!Physics.CheckBox(position, prefab.transform.localScale))
                {
                    Instantiate(prefab, position, rotation_quaternion);
                    spawned = true;
                    break;
                }
            }
            if (!spawned)
            {
                Debug.LogWarning($"Failed to spawn {prefab.name} after {attempts_spawn} attempts");
            }
        }
    }
}

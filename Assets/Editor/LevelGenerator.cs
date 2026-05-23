using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [MenuItem("Tools/Generate 30 Levels")]
    public static void GenerateLevels()
    {
        string folderPath = Application.dataPath + "/Resources/Levels";

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        for (int i = 1; i <= 30; i++)
        {
            LevelData data = new LevelData();
            data.levelID = i;
            List<DisabledSlotData> disabled = new List<DisabledSlotData>();

            if (i <= 4)
            {
                if (i <= 3)
                {
                    data.columns = 3;
                    data.rows = 3;
                }
                else
                {
                    data.columns = 4;
                    data.rows = 3;
                    disabled.Add(new DisabledSlotData { x = 0, y = 0 });
                    disabled.Add(new DisabledSlotData { x = 3, y = 0 });
                    disabled.Add(new DisabledSlotData { x = 3, y = 2 });
                }
            }

            else
            {
                if (i == 5)
                {
                    data.columns = 4;
                    data.rows = 4;
                }
                else
                {
                    data.columns = 5;
                    data.rows = 4;

                    disabled.Add(new DisabledSlotData { x = 0, y = 0 });
                    disabled.Add(new DisabledSlotData { x = 4, y = 0 });
                    disabled.Add(new DisabledSlotData { x = 0, y = 3 });
                    disabled.Add(new DisabledSlotData { x = 4, y = 3 });

                    if (i >= 15)
                    {
                        data.columns = 5;
                        data.rows = 5;
                        disabled.Clear();

                        disabled.Add(new DisabledSlotData { x = 0, y = 0 });
                        disabled.Add(new DisabledSlotData { x = 1, y = 0 });
                        disabled.Add(new DisabledSlotData { x = 3, y = 0 });
                        disabled.Add(new DisabledSlotData { x = 4, y = 0 });

                        disabled.Add(new DisabledSlotData { x = 0, y = 4 });
                        disabled.Add(new DisabledSlotData { x = 1, y = 4 });
                        disabled.Add(new DisabledSlotData { x = 3, y = 4 });
                        disabled.Add(new DisabledSlotData { x = 4, y = 4 });

                        disabled.Add(new DisabledSlotData { x = 2, y = 2 });
                    }
                }
            }

            data.disabledSlots = disabled.ToArray();
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(folderPath + $"/Level_{i}.json", json);
        }

        AssetDatabase.Refresh();

    }
}
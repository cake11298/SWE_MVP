using BarSimulator.NPC;
using UnityEngine;

namespace BarSimulator.Data
{
    /// <summary>
    /// 性別枚舉
    /// </summary>
    public enum Gender
    {
        Male,
        Female
    }

    /// <summary>
    /// NPC 資料 ScriptableObject
    /// 參考: NPCManager.js NPC 資料結構
    /// </summary>
    [CreateAssetMenu(fileName = "NPCData", menuName = "Bar/NPC Data")]
    public class NPCData : ScriptableObject
    {
        [Header("基本資訊")]
        [Tooltip("NPC 名稱")]
        public string npcName;

        [Tooltip("角色職位/描述")]
        public string role;

        [Tooltip("性別")]
        public Gender gender;

        [Header("位置設定")]
        [Tooltip("生成位置")]
        public Vector3 position;

        [Tooltip("Y 軸旋轉角度 (弧度)")]
        public float rotation;

        [Header("外觀")]
        [Tooltip("上衣顏色")]
        public Color shirtColor = Color.white;

        [Tooltip("褲子顏色")]
        public Color pantsColor = Color.black;

        [Header("對話")]
        [Tooltip("對話內容列表")]
        public NPCDialogue dialogue;

        /// <summary>
        /// 取得旋轉角度（度數）
        /// </summary>
        public float RotationDegrees => rotation * Mathf.Rad2Deg;

        /// <summary>
        /// 取得 Quaternion 旋轉
        /// </summary>
        public Quaternion RotationQuaternion => Quaternion.Euler(0f, RotationDegrees, 0f);
    }

    /// <summary>
    /// NPC 資料庫 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NPCDatabase", menuName = "Bar/NPC Database")]
    public class NPCDatabase : ScriptableObject
    {
        [Tooltip("所有 NPC 資料")]
        public NPCData[] npcs;

        /// <summary>
        /// 根據名稱取得 NPC 資料
        /// </summary>
        public NPCData GetNPC(string name)
        {
            if (npcs == null) return null;

            foreach (var npc in npcs)
            {
                if (npc.npcName == name)
                    return npc;
            }
            return null;
        }
    }
}

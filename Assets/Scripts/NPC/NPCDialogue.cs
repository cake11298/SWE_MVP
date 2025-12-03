using BarSimulator.Core;
using System.Collections.Generic;
using UnityEngine;

namespace BarSimulator.NPC
{
    /// <summary>
    /// 這個類別用來定義某個NPC的所有對話情景及內容
    /// 以其中的 dialogueEntries 作為儲存在不同情景中的對話內容的容器
    /// </summary>
    [CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "Bar/NPC Dialogue (ScriptableObject)")]
    public class NPCDialogue : ScriptableObject
    {
        [Header("對話內容列表")]
        // 這裡存放所有可能發生的對話
        public List<DialogueEntry> dialogueEntries;

        ///<summary>
        /// 取得 "label" 標記的對話包的對話長度
        /// </summary>
        ///<param name="label">對話包辨識標記</param>
        public int GetDialogueLength(string label)
        {
            return dialogueEntries.Find(entry => entry.label == label).sentences.Length;
        }

        ///<summary>
        /// 取得 "label"標記的對話包中的特定句子
        /// </summary>
        ///<param name="label">對話包辨識標記</param>
        ///<param name="sentenceIndex">選擇的對話包中的句子Index (從0開始)</param>
        public string GetDialogueLine(string label, int sentenceIndex)
        {
            return dialogueEntries?.Find(entry => entry.label == label)?.sentences[sentenceIndex];
        }
    }


    /// <summary>
    /// 這個類別用來定義「單一狀態」下的對話包，
    /// 以 label (string) 為對話包辨識標記<br/> <br/> 
    /// 對話包辨識標記:<br/> 
    /// "init": (通用)NPC的普通對話，通常為自我介紹之類的
    /// </summary>
    [System.Serializable]
    public class DialogueEntry
    {
        public string label; //狀態標記

        [TextArea(3, 10)] // 讓輸入框變大，方便打字
        public string[] sentences; // 對話內容 (一句話一個元素)
    }
}
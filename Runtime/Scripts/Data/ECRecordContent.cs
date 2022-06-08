using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillsVR.EnterpriseCloudSDK.Data
{
    [System.Serializable]
    public class ECRecordContent
    {
        public int id;
        public string parentId;
        public string code;
        public int index;
        public int depth;
        public int scenarioId;
        public string scenario;
        public string name;
        public int type;
        public string passCondition;
        public string requirement;

        public bool gameScoreBool; // runtime user score from game

        public void ResetGameScore()
        {
            gameScoreBool = false;
        }

        public string PrintInLine()
        {
            return string.Join(" ",
                0 == type && gameScoreBool ? "o" : "  ",
                new string(' ', depth * 4),
                id,
                name,
                "\r\n"
                ) ;
        }

        
    }
}

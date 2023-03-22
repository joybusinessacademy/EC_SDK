using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillsVR.EnterpriseCloudSDK.Data
{
    [System.Serializable]
    public class ECRecordContent
    {
        public enum ScoreType
        {
            Unknown = -1,
            Bool = 0,
            Title = 3,
        }
        public string id;
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

        public bool IsScoreType(ScoreType scoreType)
        {
            return (int)scoreType == type;
        }

        public bool isScoreTypeBool => IsScoreType(ScoreType.Bool);
        public string PrintInLine()
        {
            return string.Join(" ",
                isScoreTypeBool && gameScoreBool ? "o" : "  ",
                new string(' ', depth * 4),
                id,
                name,
                "\r\n"
                ) ;
        }

        
    }
}

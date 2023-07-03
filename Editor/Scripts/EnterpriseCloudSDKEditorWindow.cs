using UnityEngine;
using UnityEditor;

using SkillsVR.EnterpriseCloudSDK.Editor.Editors;
using SkillsVR.EnterpriseCloudSDK.Data;
using SkillsVR.EnterpriseCloudSDK.Networking.API;
using SkillsVR.EnterpriseCloudSDK.Networking;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq;
using System;

namespace SkillsVR.EnterpriseCloudSDK.Editor
{
    public class EnterpriseCloudSDKEditorWindow : EditorWindow
    {
        List<ECRecordContentEditorWidget> widgets = new List<ECRecordContentEditorWidget>();

        ECRecordCollectionAsset recordAsset = null;

        Vector2 scrollViewPos;

        SerializedProperty recordAssetSerializedProperty = null;

        protected bool interactable = true;

        protected bool enableEditScope = false;

        [MenuItem("Skills Node/Configure Enterprise Cloud", false, 2)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<EnterpriseCloudSDKEditorWindow>("SkillsVR Enterprise Cloud");
        }

        private void OnEnable()
        {
            recordAsset = ECRecordCollectionAssetEditor.CreateOrLoadAsset();
            widgets.Clear();
            SerializedObject serializedObject = new SerializedObject(this);
            recordAssetSerializedProperty = serializedObject.FindProperty(nameof(recordAsset));
            interactable = true;
        }

        private void OnDisable()
        {
            SaveRecordAsset();
        }

        void OnGUI()
        {
            if (null == recordAsset)
            {
                return;
            }
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical();

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Record Asset: ", recordAsset, recordAsset.GetType(), false);
            GUI.enabled = interactable;

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !EditorApplication.isPlayingOrWillChangePlaymode;

            if (recordAsset.managedConfigs.Count > 1)
            {
                List<string> nameList = new List<string>();
                foreach (var item in recordAsset.managedConfigs)
                {
                    nameList.Add(item.name.Replace("/", "").Replace("\\", ""));
                }
                recordAsset.currentConfigIndex = EditorGUILayout.Popup(new GUIContent("Switch Config"), recordAsset.currentConfigIndex, nameList.ToArray());
            }
            

            GUI.enabled = interactable;
            EditorGUILayout.EndHorizontal();

            recordAsset.currentConfig.loginData = DrawLoginDataGUI(recordAsset.currentConfig.loginData);

            GUI.enabled = interactable && recordAsset.currentConfig.loginData.IsValid();
            if (GUILayout.Button("Login"))
            {
                SendLogin();
            }
            GUI.enabled = interactable;

            recordAsset.currentConfig.scenarioId = EditorGUILayout.TextField("Scenario ID:", recordAsset.currentConfig.scenarioId);

            if (ECAPI.HasLoginToken())
            {
                GUI.enabled = interactable && !string.IsNullOrEmpty(recordAsset.currentConfig.scenarioId);
                if (GUILayout.Button("Get Config"))
                {
                    SendGetConfig();
                }
                GUI.enabled = interactable;
            }
            else
            {
                EditorGUILayout.HelpBox("Display records from local cached . Please login to enable submit and more functions.", MessageType.Warning);
            }

            if (recordAsset.currentConfig.managedRecords.Count > 0)
            {
                if (null == widgets || 0 == widgets.Count)
                {
                    widgets = ECRecordContentEditorWidget.GetEditorRenderingWidgetListFromRecordCollection(recordAsset.currentConfig.manageRecordsMemory);
                }
                scrollViewPos = EditorGUILayout.BeginScrollView(scrollViewPos, GUILayout.ExpandHeight(true));
                foreach(var item in widgets)
                {
                    item?.Draw();
                }
                EditorGUILayout.EndScrollView();

                if (GUILayout.Button("Save Changes"))
                {
                    SaveRecordAsset();
                }

                if (GUILayout.Button("Print Records"))
                {
                    recordAsset.PrintRecords();
                }
                if (GUILayout.Button("Reset User Scores"))
                {
                    recordAsset.ResetUserScores();
                }

                if (ECAPI.HasLoginToken() && GUILayout.Button("Submit"))
                {
                    SendLearningRecord();
                }
            }
            EditorGUILayout.EndVertical();
            GUI.enabled = true;
        }


        private SSOLoginData DrawLoginDataGUI(SSOLoginData loginData)
        {
            if (null == loginData)
            {
                loginData = new SSOLoginData();
            }

            loginData.selectedRegion = EditorGUILayout.Popup("Region", loginData.selectedRegion, SSOLoginData.regions);

            loginData.userName = EditorGUILayout.TextField("Username:", loginData.userName);
            loginData.password = EditorGUILayout.PasswordField("Password:", loginData.password);

            return loginData;
        }

        private string DrawScopeGUI(string scope)
        {
            enableEditScope = GUILayout.Toggle(enableEditScope, "Edit Scope");
            if (!enableEditScope)
            {
                return scope;
            }
            GUILayout.BeginHorizontal();
            GUILayout.Space(40);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reset to Default", GUILayout.ExpandWidth(false)))
            {
                scope = SSOLoginData.GetDefaultScopeString();
            }
            if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
            {
                scope = "";
            }
            GUILayout.EndHorizontal();
            List<string> scopeList = null;
            try
            {
                scopeList = scope.Split(' ').ToList();
            }
            catch {
                scopeList = new List<string>();
            }
            scopeList.RemoveAll(x => string.IsNullOrWhiteSpace(x));
            for (int i = 0; i < scopeList.Count; i++)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
                {
                    scopeList[i] = "";
                    continue;
                }
                scopeList[i] = EditorGUILayout.TextField(scopeList[i]);
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("+", GUILayout.ExpandWidth(true)))
            {
                scopeList.Add("NewScope");
            }
            scopeList.RemoveAll(x => string.IsNullOrWhiteSpace(x));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
            return string.Join(" ", scopeList);
        }

        private void LogError(string error)
        {
            interactable = true;
            Debug.LogError(error);
        }

        private void SendLogin()
        {
            interactable = false;
            // configure my config now
            string targetId = string.Empty;

            switch (SSOLoginData.regions[recordAsset.currentConfig.loginData.selectedRegion])
            {
                case "US":
                    targetId = "prod-us";
                    break;
                case "AU":
                    targetId = "prod-au";
                    break;
                case "US-Test":
                    targetId = "test-us";
                    break;
                case "AU-Test":
                    targetId = "test-au";
                    break;
                case "AU-Dev":
                    targetId = "dev-au";
                    break;
            }

            var config = SkillsVR.EnterpriseCloudSDK.Editor.Networking.ConfigService.Get(targetId);

            recordAsset.currentConfig.loginData.clientId = config.clientId;
            recordAsset.currentConfig.loginData.loginUrl = config.ropcUrl;
            recordAsset.currentConfig.loginData.scope = config.scope;
            recordAsset.currentConfig.domain = config.domain;
            
            PlayerPrefs.SetString("OCAPIM_SUB_KEY", config.subscriptionKey);

            ECAPI.domain = config.domain;
            ECAPI.Login(recordAsset.currentConfig.loginData, OnLoginSuccess, LogError);
            PlayerPrefs.Save();
        }

        private void OnLoginSuccess(SSOLoginResponse response)
        {
            interactable = true;

            // parse token
            JSONNode node = Decode(RESTCore.AccessToken);
            UnityEngine.Debug.Log(node["extension_OrgCode"].ToString().Replace("\"", string.Empty));
            PlayerPrefs.SetString("ORGCODE", node["extension_OrgCode"].ToString().Replace("\"", string.Empty));
            PlayerPrefs.Save();
        }

        private void SendGetConfig()
        {
            interactable = false;
            ECAPI.GetConfig(recordAsset.currentConfig.scenarioId, RecieveConfigResponse, LogError);
        }

        private void RecieveConfigResponse(GetConfig.Response obj)
        {
            widgets.Clear();
            recordAsset.currentConfig.managedRecords.Clear();

            if (null == obj || null == obj.data || 0 == obj.data.Length)
            {
                Debug.Log("No context loaded.");
                SaveRecordAsset();
                interactable = true;
                return;
            }

            recordAsset.AddRange(obj.data);
            SaveRecordAsset();

            widgets = ECRecordContentEditorWidget.GetEditorRenderingWidgetListFromRecordCollection(recordAsset.currentConfig.manageRecordsMemory);
            interactable = true;
        }

        public void SendLearningRecord()
        {
            recordAsset.SubmitUserScore(null, Debug.LogError);
        }

        private void LearningRecordResponse(AbstractAPI.EmptyResponse obj)
        {
        }

        public void SaveRecordAsset()
        {
            EditorUtility.CopySerialized(recordAsset, recordAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // added here internally so it doesn't conflict on other packages
        public static JSONNode Decode(string jwt)
        {
            string[] parts = jwt.Split('.');
            if (parts.Length != 3)
            {
                throw new ArgumentException("Invalid JWT format.");
            }
            string payload = parts[1];
            payload = DecodeBase64(payload);
            return JSON.Parse(payload);
        }

        private static string DecodeBase64(string input)
        {
            string base64 = input.Replace('-', '+').Replace('_', '/');
            while (base64.Length % 4 != 0)
            {
                base64 += '=';
            }
            byte[] bytes = Convert.FromBase64String(base64);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public enum JSONBinaryTag
        {
            Array = 1,
            Class = 2,
            Value = 3,
            IntValue = 4,
            DoubleValue = 5,
            BoolValue = 6,
            FloatValue = 7,
        }

        public class JSONNode
        {
            #region common interface
            public virtual void Add(string aKey, JSONNode aItem) { }
            public virtual JSONNode this[int aIndex] { get { return null; } set { } }
            public virtual JSONNode this[string aKey] { get { return null; } set { } }
            public virtual string Value { get { return ""; } set { } }
            public virtual int Count { get { return 0; } }

            public virtual void Add(JSONNode aItem)
            {
                Add("", aItem);
            }

            public virtual JSONNode Remove(string aKey) { return null; }
            public virtual JSONNode Remove(int aIndex) { return null; }
            public virtual JSONNode Remove(JSONNode aNode) { return aNode; }

            public virtual IEnumerable<JSONNode> Childs { get { yield break; } }
            public IEnumerable<JSONNode> DeepChilds
            {
                get
                {
                    foreach (var C in Childs)
                        foreach (var D in C.DeepChilds)
                            yield return D;
                }
            }

            public override string ToString()
            {
                return "JSONNode";
            }
            public virtual string ToString(string aPrefix)
            {
                return "JSONNode";
            }

            #endregion common interface

            #region typecasting properties
            public virtual int AsInt
            {
                get
                {
                    int v = 0;
                    if (int.TryParse(Value, out v))
                        return v;
                    return 0;
                }
                set
                {
                    Value = value.ToString();
                }
            }
            public virtual float AsFloat
            {
                get
                {
                    float v = 0.0f;
                    if (float.TryParse(Value, out v))
                        return v;
                    return 0.0f;
                }
                set
                {
                    Value = value.ToString();
                }
            }
            public virtual double AsDouble
            {
                get
                {
                    double v = 0.0;
                    if (double.TryParse(Value, out v))
                        return v;
                    return 0.0;
                }
                set
                {
                    Value = value.ToString();
                }
            }
            public virtual bool AsBool
            {
                get
                {
                    bool v = false;
                    if (bool.TryParse(Value, out v))
                        return v;
                    return !string.IsNullOrEmpty(Value);
                }
                set
                {
                    Value = (value) ? "true" : "false";
                }
            }
            public virtual JSONArray AsArray
            {
                get
                {
                    return this as JSONArray;
                }
            }
            public virtual JSONClass AsObject
            {
                get
                {
                    return this as JSONClass;
                }
            }


            #endregion typecasting properties

            #region operators
            public static implicit operator JSONNode(string s)
            {
                return new JSONData(s);
            }
            public static implicit operator string(JSONNode d)
            {
                return (d == null) ? null : d.Value;
            }
            public static bool operator ==(JSONNode a, object b)
            {
                if (b == null && a is JSONLazyCreator)
                    return true;
                return ReferenceEquals(a, b);
            }

            public static bool operator !=(JSONNode a, object b)
            {
                return !(a == b);
            }
            public override bool Equals(object obj)
            {
                return ReferenceEquals(this, obj);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }


            #endregion operators

            internal static string Escape(string aText)
            {
                string result = "";
                foreach (char c in aText)
                {
                    switch (c)
                    {
                        case '\\': result += "\\\\"; break;
                        case '\"': result += "\\\""; break;
                        case '\n': result += "\\n"; break;
                        case '\r': result += "\\r"; break;
                        case '\t': result += "\\t"; break;
                        case '\b': result += "\\b"; break;
                        case '\f': result += "\\f"; break;
                        default: result += c; break;
                    }
                }
                return result;
            }

            public static JSONNode Parse(string aJSON)
            {
                Stack<JSONNode> stack = new Stack<JSONNode>();
                JSONNode ctx = null;
                int i = 0;
                string Token = "";
                string TokenName = "";
                bool QuoteMode = false;
                while (i < aJSON.Length)
                {
                    switch (aJSON[i])
                    {
                        case '{':
                            if (QuoteMode)
                            {
                                Token += aJSON[i];
                                break;
                            }
                            stack.Push(new JSONClass());
                            if (ctx != null)
                            {
                                TokenName = TokenName.Trim();
                                if (ctx is JSONArray)
                                    ctx.Add(stack.Peek());
                                else if (TokenName != "")
                                    ctx.Add(TokenName, stack.Peek());
                            }
                            TokenName = "";
                            Token = "";
                            ctx = stack.Peek();
                            break;

                        case '[':
                            if (QuoteMode)
                            {
                                Token += aJSON[i];
                                break;
                            }

                            stack.Push(new JSONArray());
                            if (ctx != null)
                            {
                                TokenName = TokenName.Trim();
                                if (ctx is JSONArray)
                                    ctx.Add(stack.Peek());
                                else if (TokenName != "")
                                    ctx.Add(TokenName, stack.Peek());
                            }
                            TokenName = "";
                            Token = "";
                            ctx = stack.Peek();
                            break;

                        case '}':
                        case ']':
                            if (QuoteMode)
                            {
                                Token += aJSON[i];
                                break;
                            }
                            if (stack.Count == 0)
                                throw new Exception("JSON Parse: Too many closing brackets");

                            stack.Pop();
                            if (Token != "")
                            {
                                TokenName = TokenName.Trim();
                                if (ctx is JSONArray)
                                    ctx.Add(Token);
                                else if (TokenName != "")
                                    ctx.Add(TokenName, Token);
                            }
                            TokenName = "";
                            Token = "";
                            if (stack.Count > 0)
                                ctx = stack.Peek();
                            break;

                        case ':':
                            if (QuoteMode)
                            {
                                Token += aJSON[i];
                                break;
                            }
                            TokenName = Token;
                            Token = "";
                            break;

                        case '"':
                            QuoteMode ^= true;
                            break;

                        case ',':
                            if (QuoteMode)
                            {
                                Token += aJSON[i];
                                break;
                            }
                            if (Token != "")
                            {
                                if (ctx is JSONArray)
                                    ctx.Add(Token);
                                else if (TokenName != "")
                                    ctx.Add(TokenName, Token);
                            }
                            TokenName = "";
                            Token = "";
                            break;

                        case '\r':
                        case '\n':
                            break;

                        case ' ':
                        case '\t':
                            if (QuoteMode)
                                Token += aJSON[i];
                            break;

                        case '\\':
                            ++i;
                            if (QuoteMode)
                            {
                                char C = aJSON[i];
                                switch (C)
                                {
                                    case 't': Token += '\t'; break;
                                    case 'r': Token += '\r'; break;
                                    case 'n': Token += '\n'; break;
                                    case 'b': Token += '\b'; break;
                                    case 'f': Token += '\f'; break;
                                    case 'u':
                                        {
                                            string s = aJSON.Substring(i + 1, 4);
                                            Token += (char)int.Parse(s, System.Globalization.NumberStyles.AllowHexSpecifier);
                                            i += 4;
                                            break;
                                        }
                                    default: Token += C; break;
                                }
                            }
                            break;

                        default:
                            Token += aJSON[i];
                            break;
                    }
                    ++i;
                }
                if (QuoteMode)
                {
                    throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
                }
                return ctx;
            }

            public virtual void Serialize(System.IO.BinaryWriter aWriter) { }

            public void SaveToStream(System.IO.Stream aData)
            {
                var W = new System.IO.BinaryWriter(aData);
                Serialize(W);
            }

#if USE_SharpZipLib
		public void SaveToCompressedStream(System.IO.Stream aData)
		{
			using (var gzipOut = new ICSharpCode.SharpZipLib.BZip2.BZip2OutputStream(aData))
			{
				gzipOut.IsStreamOwner = false;
				SaveToStream(gzipOut);
				gzipOut.Close();
			}
		}
		
		public void SaveToCompressedFile(string aFileName)
		{
#if USE_FileIO
			System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
			using(var F = System.IO.File.OpenWrite(aFileName))
			{
				SaveToCompressedStream(F);
			}
#else
			throw new Exception("Can't use File IO stuff in webplayer");
#endif
		}
		public string SaveToCompressedBase64()
		{
			using (var stream = new System.IO.MemoryStream())
			{
				SaveToCompressedStream(stream);
				stream.Position = 0;
				return System.Convert.ToBase64String(stream.ToArray());
			}
		}
		
#else
            public void SaveToCompressedStream(System.IO.Stream aData)
            {
                throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
            }
            public void SaveToCompressedFile(string aFileName)
            {
                throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
            }
            public string SaveToCompressedBase64()
            {
                throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
            }
#endif

            public void SaveToFile(string aFileName)
            {
#if USE_FileIO
			System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
			using(var F = System.IO.File.OpenWrite(aFileName))
			{
				SaveToStream(F);
			}
#else
                throw new Exception("Can't use File IO stuff in webplayer");
#endif
            }
            public string SaveToBase64()
            {
                using (var stream = new System.IO.MemoryStream())
                {
                    SaveToStream(stream);
                    stream.Position = 0;
                    return Convert.ToBase64String(stream.ToArray());
                }
            }
            public static JSONNode Deserialize(System.IO.BinaryReader aReader)
            {
                JSONBinaryTag type = (JSONBinaryTag)aReader.ReadByte();
                switch (type)
                {
                    case JSONBinaryTag.Array:
                        {
                            int count = aReader.ReadInt32();
                            JSONArray tmp = new JSONArray();
                            for (int i = 0; i < count; i++)
                                tmp.Add(Deserialize(aReader));
                            return tmp;
                        }
                    case JSONBinaryTag.Class:
                        {
                            int count = aReader.ReadInt32();
                            JSONClass tmp = new JSONClass();
                            for (int i = 0; i < count; i++)
                            {
                                string key = aReader.ReadString();
                                var val = Deserialize(aReader);
                                tmp.Add(key, val);
                            }
                            return tmp;
                        }
                    case JSONBinaryTag.Value:
                        {
                            return new JSONData(aReader.ReadString());
                        }
                    case JSONBinaryTag.IntValue:
                        {
                            return new JSONData(aReader.ReadInt32());
                        }
                    case JSONBinaryTag.DoubleValue:
                        {
                            return new JSONData(aReader.ReadDouble());
                        }
                    case JSONBinaryTag.BoolValue:
                        {
                            return new JSONData(aReader.ReadBoolean());
                        }
                    case JSONBinaryTag.FloatValue:
                        {
                            return new JSONData(aReader.ReadSingle());
                        }

                    default:
                        {
                            throw new Exception("Error deserializing JSON. Unknown tag: " + type);
                        }
                }
            }

#if USE_SharpZipLib
		public static JSONNode LoadFromCompressedStream(System.IO.Stream aData)
		{
			var zin = new ICSharpCode.SharpZipLib.BZip2.BZip2InputStream(aData);
			return LoadFromStream(zin);
		}
		public static JSONNode LoadFromCompressedFile(string aFileName)
		{
#if USE_FileIO
			using(var F = System.IO.File.OpenRead(aFileName))
			{
				return LoadFromCompressedStream(F);
			}
#else
			throw new Exception("Can't use File IO stuff in webplayer");
#endif
		}
		public static JSONNode LoadFromCompressedBase64(string aBase64)
		{
			var tmp = System.Convert.FromBase64String(aBase64);
			var stream = new System.IO.MemoryStream(tmp);
			stream.Position = 0;
			return LoadFromCompressedStream(stream);
		}
#else
            public static JSONNode LoadFromCompressedFile(string aFileName)
            {
                throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
            }
            public static JSONNode LoadFromCompressedStream(System.IO.Stream aData)
            {
                throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
            }
            public static JSONNode LoadFromCompressedBase64(string aBase64)
            {
                throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
            }
#endif

            public static JSONNode LoadFromStream(System.IO.Stream aData)
            {
                using (var R = new System.IO.BinaryReader(aData))
                {
                    return Deserialize(R);
                }
            }
            public static JSONNode LoadFromFile(string aFileName)
            {
#if USE_FileIO
			using(var F = System.IO.File.OpenRead(aFileName))
			{
				return LoadFromStream(F);
			}
#else
                throw new Exception("Can't use File IO stuff in webplayer");
#endif
            }
            public static JSONNode LoadFromBase64(string aBase64)
            {
                var tmp = Convert.FromBase64String(aBase64);
                var stream = new System.IO.MemoryStream(tmp);
                stream.Position = 0;
                return LoadFromStream(stream);
            }
        } // End of JSONNode

        public class JSONArray : JSONNode, IEnumerable
        {
            private List<JSONNode> m_List = new List<JSONNode>();
            public override JSONNode this[int aIndex]
            {
                get
                {
                    if (aIndex < 0 || aIndex >= m_List.Count)
                        return new JSONLazyCreator(this);
                    return m_List[aIndex];
                }
                set
                {
                    if (aIndex < 0 || aIndex >= m_List.Count)
                        m_List.Add(value);
                    else
                        m_List[aIndex] = value;
                }
            }
            public override JSONNode this[string aKey]
            {
                get { return new JSONLazyCreator(this); }
                set { m_List.Add(value); }
            }
            public override int Count
            {
                get { return m_List.Count; }
            }
            public override void Add(string aKey, JSONNode aItem)
            {
                m_List.Add(aItem);
            }
            public override JSONNode Remove(int aIndex)
            {
                if (aIndex < 0 || aIndex >= m_List.Count)
                    return null;
                JSONNode tmp = m_List[aIndex];
                m_List.RemoveAt(aIndex);
                return tmp;
            }
            public override JSONNode Remove(JSONNode aNode)
            {
                m_List.Remove(aNode);
                return aNode;
            }
            public override IEnumerable<JSONNode> Childs
            {
                get
                {
                    foreach (JSONNode N in m_List)
                        yield return N;
                }
            }
            public IEnumerator GetEnumerator()
            {
                foreach (JSONNode N in m_List)
                    yield return N;
            }
            public override string ToString()
            {
                string result = "[ ";
                foreach (JSONNode N in m_List)
                {
                    if (result.Length > 2)
                        result += ", ";
                    result += N.ToString();
                }
                result += " ]";
                return result;
            }
            public override string ToString(string aPrefix)
            {
                string result = "[ ";
                foreach (JSONNode N in m_List)
                {
                    if (result.Length > 3)
                        result += ", ";
                    result += "\n" + aPrefix + "   ";
                    result += N.ToString(aPrefix + "   ");
                }
                result += "\n" + aPrefix + "]";
                return result;
            }
            public override void Serialize(System.IO.BinaryWriter aWriter)
            {
                aWriter.Write((byte)JSONBinaryTag.Array);
                aWriter.Write(m_List.Count);
                for (int i = 0; i < m_List.Count; i++)
                {
                    m_List[i].Serialize(aWriter);
                }
            }
        } // End of JSONArray

        public class JSONClass : JSONNode, IEnumerable
        {
            private Dictionary<string, JSONNode> m_Dict = new Dictionary<string, JSONNode>(StringComparer.Ordinal);
            public override JSONNode this[string aKey]
            {
                get
                {
                    if (m_Dict.ContainsKey(aKey))
                        return m_Dict[aKey];
                    else
                        return new JSONLazyCreator(this, aKey);
                }
                set
                {
                    if (m_Dict.ContainsKey(aKey))
                        m_Dict[aKey] = value;
                    else
                        m_Dict.Add(aKey, value);
                }
            }
            public override JSONNode this[int aIndex]
            {
                get
                {
                    if (aIndex < 0 || aIndex >= m_Dict.Count)
                        return null;

#if DISABLE_LINQ_EXT
				foreach (var kvp in m_Dict)
				{
					if (aIndex==0)
						return kvp.Value;
					aIndex--;
				}
				return null;
#else
                    return m_Dict.ElementAt(aIndex).Value;
#endif
                }
                set
                {
                    if (aIndex < 0 || aIndex >= m_Dict.Count)
                        return;
#if DISABLE_LINQ_EXT
				string[] keys = new string[m_Dict.Keys.Count];
				m_Dict.Keys.CopyTo(keys,0);
				string key = keys[aIndex];
#else
                    string key = m_Dict.ElementAt(aIndex).Key;
#endif
                    m_Dict[key] = value;
                }
            }
            public override int Count
            {
                get { return m_Dict.Count; }
            }


            public override void Add(string aKey, JSONNode aItem)
            {
                if (!string.IsNullOrEmpty(aKey))
                {
                    if (m_Dict.ContainsKey(aKey))
                        m_Dict[aKey] = aItem;
                    else
                        m_Dict.Add(aKey, aItem);
                }
                else
                    m_Dict.Add(Guid.NewGuid().ToString(), aItem);
            }

            public override JSONNode Remove(string aKey)
            {
                if (!m_Dict.ContainsKey(aKey))
                    return null;
                JSONNode tmp = m_Dict[aKey];
                m_Dict.Remove(aKey);
                return tmp;
            }
            public override JSONNode Remove(int aIndex)
            {
                if (aIndex < 0 || aIndex >= m_Dict.Count)
                    return null;

#if DISABLE_LINQ_EXT
			string[] keys = new string[m_Dict.Keys.Count];
			m_Dict.Keys.CopyTo(keys,0);
			string key = keys[aIndex];
			var value = m_Dict[key];
			m_Dict.Remove(key);
			return value;
#else
                var item = m_Dict.ElementAt(aIndex);
                m_Dict.Remove(item.Key);
                return item.Value;
#endif
            }
            public override JSONNode Remove(JSONNode aNode)
            {
                try
                {
#if DISABLE_LINQ_EXT
				foreach (var kvp in m_Dict)
					if (kvp.Value == aNode)
					{
						m_Dict.Remove(kvp.Key);
						break;
					}
				return aNode;
#else
                    var item = m_Dict.Where(k => k.Value == aNode).First();
                    m_Dict.Remove(item.Key);
                    return aNode;
#endif
                }
                catch
                {
                    return null;
                }
            }

            public override IEnumerable<JSONNode> Childs
            {
                get
                {
                    foreach (KeyValuePair<string, JSONNode> N in m_Dict)
                        yield return N.Value;
                }
            }

            public IEnumerator GetEnumerator()
            {
                foreach (KeyValuePair<string, JSONNode> N in m_Dict)
                    yield return N;
            }
            public override string ToString()
            {
                string result = "{";
                foreach (KeyValuePair<string, JSONNode> N in m_Dict)
                {
                    if (result.Length > 2)
                        result += ", ";
                    result += "\"" + Escape(N.Key) + "\":" + N.Value.ToString();
                }
                result += "}";
                return result;
            }
            public override string ToString(string aPrefix)
            {
                string result = "{ ";
                foreach (KeyValuePair<string, JSONNode> N in m_Dict)
                {
                    if (result.Length > 3)
                        result += ", ";
                    result += "\n" + aPrefix + "   ";
                    result += "\"" + Escape(N.Key) + "\" : " + N.Value.ToString(aPrefix + "   ");
                }
                result += "\n" + aPrefix + "}";
                return result;
            }
            public override void Serialize(System.IO.BinaryWriter aWriter)
            {
                aWriter.Write((byte)JSONBinaryTag.Class);
                aWriter.Write(m_Dict.Count);
                foreach (string K in m_Dict.Keys)
                {
                    aWriter.Write(K);
                    m_Dict[K].Serialize(aWriter);
                }
            }
        } // End of JSONClass

        public class JSONData : JSONNode
        {
            private string m_Data;
            public override string Value
            {
                get { return m_Data; }
                set { m_Data = value; }
            }
            public JSONData(string aData)
            {
                m_Data = aData;
            }
            public JSONData(float aData)
            {
                AsFloat = aData;
            }
            public JSONData(double aData)
            {
                AsDouble = aData;
            }
            public JSONData(bool aData)
            {
                AsBool = aData;
            }
            public JSONData(int aData)
            {
                AsInt = aData;
            }

            public override string ToString()
            {
                return "\"" + Escape(m_Data) + "\"";
            }
            public override string ToString(string aPrefix)
            {
                return "\"" + Escape(m_Data) + "\"";
            }
            public override void Serialize(System.IO.BinaryWriter aWriter)
            {
                var tmp = new JSONData("");

                tmp.AsInt = AsInt;
                if (tmp.m_Data == m_Data)
                {
                    aWriter.Write((byte)JSONBinaryTag.IntValue);
                    aWriter.Write(AsInt);
                    return;
                }
                tmp.AsFloat = AsFloat;
                if (tmp.m_Data == m_Data)
                {
                    aWriter.Write((byte)JSONBinaryTag.FloatValue);
                    aWriter.Write(AsFloat);
                    return;
                }
                tmp.AsDouble = AsDouble;
                if (tmp.m_Data == m_Data)
                {
                    aWriter.Write((byte)JSONBinaryTag.DoubleValue);
                    aWriter.Write(AsDouble);
                    return;
                }

                tmp.AsBool = AsBool;
                if (tmp.m_Data == m_Data)
                {
                    aWriter.Write((byte)JSONBinaryTag.BoolValue);
                    aWriter.Write(AsBool);
                    return;
                }
                aWriter.Write((byte)JSONBinaryTag.Value);
                aWriter.Write(m_Data);
            }
        } // End of JSONData

        internal class JSONLazyCreator : JSONNode
        {
            private JSONNode m_Node = null;
            private string m_Key = null;

            public JSONLazyCreator(JSONNode aNode)
            {
                m_Node = aNode;
                m_Key = null;
            }
            public JSONLazyCreator(JSONNode aNode, string aKey)
            {
                m_Node = aNode;
                m_Key = aKey;
            }

            private void Set(JSONNode aVal)
            {
                if (m_Key == null)
                {
                    m_Node.Add(aVal);
                }
                else
                {
                    m_Node.Add(m_Key, aVal);
                }
                m_Node = null; // Be GC friendly.
            }

            public override JSONNode this[int aIndex]
            {
                get
                {
                    return new JSONLazyCreator(this);
                }
                set
                {
                    var tmp = new JSONArray();
                    tmp.Add(value);
                    Set(tmp);
                }
            }

            public override JSONNode this[string aKey]
            {
                get
                {
                    return new JSONLazyCreator(this, aKey);
                }
                set
                {
                    var tmp = new JSONClass();
                    tmp.Add(aKey, value);
                    Set(tmp);
                }
            }
            public override void Add(JSONNode aItem)
            {
                var tmp = new JSONArray();
                tmp.Add(aItem);
                Set(tmp);
            }
            public override void Add(string aKey, JSONNode aItem)
            {
                var tmp = new JSONClass();
                tmp.Add(aKey, aItem);
                Set(tmp);
            }
            public static bool operator ==(JSONLazyCreator a, object b)
            {
                if (b == null)
                    return true;
                return ReferenceEquals(a, b);
            }

            public static bool operator !=(JSONLazyCreator a, object b)
            {
                return !(a == b);
            }
            public override bool Equals(object obj)
            {
                if (obj == null)
                    return true;
                return ReferenceEquals(this, obj);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                return "";
            }
            public override string ToString(string aPrefix)
            {
                return "";
            }

            public override int AsInt
            {
                get
                {
                    JSONData tmp = new JSONData(0);
                    Set(tmp);
                    return 0;
                }
                set
                {
                    JSONData tmp = new JSONData(value);
                    Set(tmp);
                }
            }
            public override float AsFloat
            {
                get
                {
                    JSONData tmp = new JSONData(0.0f);
                    Set(tmp);
                    return 0.0f;
                }
                set
                {
                    JSONData tmp = new JSONData(value);
                    Set(tmp);
                }
            }
            public override double AsDouble
            {
                get
                {
                    JSONData tmp = new JSONData(0.0);
                    Set(tmp);
                    return 0.0;
                }
                set
                {
                    JSONData tmp = new JSONData(value);
                    Set(tmp);
                }
            }
            public override bool AsBool
            {
                get
                {
                    JSONData tmp = new JSONData(false);
                    Set(tmp);
                    return false;
                }
                set
                {
                    JSONData tmp = new JSONData(value);
                    Set(tmp);
                }
            }
            public override JSONArray AsArray
            {
                get
                {
                    JSONArray tmp = new JSONArray();
                    Set(tmp);
                    return tmp;
                }
            }
            public override JSONClass AsObject
            {
                get
                {
                    JSONClass tmp = new JSONClass();
                    Set(tmp);
                    return tmp;
                }
            }
        } // End of JSONLazyCreator

        public static class JSON
        {
            public static JSONNode Parse(string aJSON)
            {
                return JSONNode.Parse(aJSON);
            }
        }
    }
}




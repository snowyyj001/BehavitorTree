using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static TreeNode;

namespace BehaviorTree
{

    public class MainWindow : EditorWindow
    {
        private Material mGridMaterial;
        private float mGraphZoom = 1f;
        private Rect mGraphRect;
        private Vector2 mGraphOffset = Vector2.one;
        private Vector2 mGraphScrollSize = new Vector2(20000f, 20000f);
        private Vector2 mGraphScrollPosition = new Vector2(-1f, -1f);
        private GUIStyle graphBackgroundGUIStyle;
        private Vector2 mCurrentMousePosition = Vector2.zero;
        private int mToggleIndex;
        public static string[] mActions;

        public static TextAsset luaBt = null;
        public static TextAsset luaBt_1 = null;
        private int linenumber = 0;

        [MenuItem("BehaviorTree/行为树")]
        private static void ShowWindow()
        {
            var window = EditorWindow.GetWindow < MainWindow >();//创建窗口
            window.InitData();
            window.Show();
        }
        public void InitData()
        {
            TextAsset actions = Resources.Load("BehaviorTree/actionlist") as TextAsset;
            mActions = actions.text.Split('\n');
            for (int i = 0; i < mActions.Length; i++)
            {
                mActions[i] = mActions[i].TrimEnd(new char[] { '\n', '\r'});
            }
            autoRepaintOnSceneChange = true;
            titleContent = new GUIContent("行为树");
            minSize = new Vector2(600, 600);

            if (this.mGridMaterial == null)
            {
                this.mGridMaterial = new Material(Shader.Find("Hidden/Behavior Designer/Grid"));
                this.mGridMaterial.hideFlags = HideFlags.HideAndDontSave;
                this.mGridMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private void InitGraphBackgroundGUIStyle()
        {
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            if (EditorGUIUtility.isProSkin)
            {
                texture2D.SetPixel(1, 1, new Color(0.1647f, 0.1647f, 0.1647f));
            }
            else
            {
                texture2D.SetPixel(1, 1, new Color(0.3647f, 0.3647f, 0.3647f));
            }
            texture2D.hideFlags = HideFlags.HideAndDontSave;
            texture2D.Apply();
            this.graphBackgroundGUIStyle = new GUIStyle(GUI.skin.box);
            this.graphBackgroundGUIStyle.normal.background = texture2D;
            this.graphBackgroundGUIStyle.active.background = texture2D;
            this.graphBackgroundGUIStyle.hover.background = texture2D;
            this.graphBackgroundGUIStyle.focused.background = texture2D;
            this.graphBackgroundGUIStyle.normal.textColor = Color.white;
            this.graphBackgroundGUIStyle.active.textColor = Color.white;
            this.graphBackgroundGUIStyle.hover.textColor = Color.white;
            this.graphBackgroundGUIStyle.focused.textColor = Color.white;
        }

        private void SetupSizes()
        {
            this.mGraphRect = new Rect(300f, 0f, (float)(Screen.width - 300 - 15), (float)(Screen.height - 36 - 21));
            if (this.mGraphScrollPosition == new Vector2(-1f, -1f))
            {
                this.mGraphScrollPosition = (this.mGraphScrollSize - new Vector2(this.mGraphRect.width, this.mGraphRect.height)) / 2f - 2f * new Vector2(15f, 15f);
            }
        }

        private void HandleEvents()
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                if (this.MouseZoom())
                {
                    Event.current.Use();
                }
            }
        }

        private bool GetMousePositionInGraph(out Vector2 mousePosition)
        {
            mousePosition = this.mCurrentMousePosition;
            if (!this.mGraphRect.Contains(mousePosition))
            {
                return false;
            }
            mousePosition -= new Vector2(this.mGraphRect.xMin, this.mGraphRect.yMin);
            mousePosition /= this.mGraphZoom;
            return true;
        }

        private bool MouseZoom()
        {
            Vector2 vector;
            if (!this.GetMousePositionInGraph(out vector))
            {
                return false;
            }
            float num = -Event.current.delta.y / 150f;
            this.mGraphZoom += num;
            this.mGraphZoom = Mathf.Clamp(this.mGraphZoom, 0.4f, 1f);
            Vector2 vector2;
            this.GetMousePositionInGraph(out vector2);
            this.mGraphOffset += vector2 - vector;
            this.mGraphScrollPosition += vector2 - vector;
            
            return true;
        }

        private void OnGUI()
        {
            if (this.graphBackgroundGUIStyle == null)
            {
                this.InitGraphBackgroundGUIStyle();
            }
            if(mActions == null)
            {
                this.InitData();
            }
            this.mCurrentMousePosition = Event.current.mousePosition;
            this.DrawLeftActions();
            this.SetupSizes();
            this.Draw();
            this.HandleEvents();
        }

        private void DealBTTxt(string pathname)
        {
            var sr = new StreamReader(pathname);
            Dictionary<int, TreeNode> nodes_map = new Dictionary<int, TreeNode>();
            while (sr.Peek() != -1)
            {
                string str = sr.ReadLine();
                if (str.StartsWith("{id="))
                {
                    str = str.Substring(1, str.Length - 3);
                    var arr = str.Split(',');

                    var id = int.Parse(arr[0].Split('=')[1]);
                    var type = (BehaviorType)int.Parse(arr[1].Split('=')[1]);
                    var parent = int.Parse(arr[2].Split('=')[1]);
                    if (parent == 0)        //parent
                    {
                        TreeNodeMgr.Instance.Init();
                        nodes_map.Add(id, TreeNodeMgr.Instance.RootNode);
                    }
                    else
                    {
                        var node = new TreeNode();
                        node.btype = type;
                        nodes_map.TryGetValue(parent, out node.parent);
                        node.parent.childList.Add(node);
                        nodes_map.Add(id, node);
                        if (type == BehaviorType.Leaf)
                        {
                            var file = arr[3].Split('=')[1];
                            file = file.Substring(1, file.Length - 2);
                            var param = arr[4].Split('=')[1];
                            param = param.Substring(1, param.Length - 2);
                            for (int i = 0; i < mActions.Length; i++)
                            {
                                if (mActions[i] == file)
                                {
                                    node.actionIndex = i;
                                    break;
                                }
                            }
                            node.actionParams = param;
                        }
                        switch (type)
                        {
                            case BehaviorType.DecoratorInverter:
                                node.image = Resources.Load("BehaviorTree/EditorRes/decorator-inverter") as Texture;
                                break;
                           case BehaviorType.DecoratorSucceeder:
                                node.image = Resources.Load("BehaviorTree/EditorRes/decorator-success") as Texture;
                                break;
                           case BehaviorType.DecoratorRepeater:
                                node.image = Resources.Load("BehaviorTree/EditorRes/decorator-repeat") as Texture;
                                break;
                           case BehaviorType.DecoratorFail:
                                node.image = Resources.Load("BehaviorTree/EditorRes/decorator-repeat-fail") as Texture;
                                break;
                            case BehaviorType.CompositeParallel:
                                node.image = Resources.Load("BehaviorTree/EditorRes/composite-parallel") as Texture;
                                break;
                            case BehaviorType.CompositeSequence:
                                node.image = Resources.Load("BehaviorTree/EditorRes/composite-sequence") as Texture;
                                break;
                            case BehaviorType.CompositeSelector:
                                node.image = Resources.Load("BehaviorTree/EditorRes/composite-selector") as Texture;
                                break;
                            case BehaviorType.CompositeRandom:
                                node.image = Resources.Load("BehaviorTree/EditorRes/composite-random") as Texture;
                                break;
                            case BehaviorType.Leaf:
                                node.image = Resources.Load("BehaviorTree/EditorRes/leaf") as Texture;
                                break;
                        }
                    }

                }

            }
            sr.Close();
        }

        private void DrawLeftActions()
        {
            if(GUI.Button(new Rect(5, 5, 280, 20), "保存"))
            {
                saveBTLua();
            }
            luaBt = (TextAsset)EditorGUI.ObjectField(new Rect(5, 40, 280, 20), "Lua Behavior Tree", luaBt, typeof(TextAsset), true);
            if (luaBt_1 != luaBt)
            {
                luaBt_1 = luaBt;
                string path = AssetDatabase.GetAssetPath(luaBt_1);
                DealBTTxt(path);
            }

            if (TreeNodeMgr.Instance.SelNode == null || TreeNodeMgr.Instance.SelNode.btype != BehaviorType.Leaf)
            {
                return;
            }
            mToggleIndex = TreeNodeMgr.Instance.SelNode.actionIndex;
            int ox = 5, oy = 70, hright = 20;
            EditorGUI.LabelField(new Rect(ox, oy, position.width, hright), "选择叶节点行为动作");
            oy += hright;
            for (int i = 0; i < mActions.Length; i++)
            {
                bool to = EditorGUI.Toggle(new Rect(ox, oy, 200, hright), mActions[i], i==mToggleIndex);
                if(to)
                {
                    mToggleIndex = i;
                    TreeNodeMgr.Instance.SelNode.actionIndex = mToggleIndex;
                }
                oy += hright;
            }
            EditorGUI.LabelField(new Rect(ox, oy, position.width, hright), "可选动作参数");
            oy += hright;
            TreeNodeMgr.Instance.SelNode.actionParams = EditorGUI.TextField(new Rect(ox, oy, 280, hright), TreeNodeMgr.Instance.SelNode.actionParams);
        }

        private void Draw()
        {
            GUILayout.BeginHorizontal();
            this.DrawGraphArea();
            GUILayout.EndHorizontal();
        }

        private void DrawGraphArea()
        {
            if (Event.current.type != EventType.ScrollWheel)
            {
                Vector2 vector = GUI.BeginScrollView(new Rect(this.mGraphRect.x, this.mGraphRect.y, this.mGraphRect.width + 15f, this.mGraphRect.height + 15f), this.mGraphScrollPosition, new Rect(0f, 0f, this.mGraphScrollSize.x, this.mGraphScrollSize.y), true, true);
                if (vector != this.mGraphScrollPosition && Event.current.type != EventType.DragUpdated && Event.current.type != EventType.Ignore)
                {
                    this.mGraphOffset -= (vector - this.mGraphScrollPosition) / this.mGraphZoom;
                    this.mGraphScrollPosition = vector;
                }
                GUI.EndScrollView();
            }
            GUI.Box(this.mGraphRect, string.Empty, graphBackgroundGUIStyle);
            this.DrawGrid();

            DataTreeEditTool.Begin(this.mGraphRect, this.mGraphZoom);
            TreeNodeMgr.Instance.Show(this.mGraphOffset);
            DataTreeEditTool.End();
        }

        private void DrawGrid()
        {
            this.mGridMaterial.SetPass((!EditorGUIUtility.isProSkin) ? 1 : 0);
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            this.DrawGridLines(10f * this.mGraphZoom, new Vector2(this.mGraphOffset.x % 10f * this.mGraphZoom, this.mGraphOffset.y % 10f * this.mGraphZoom));
            GL.End();
            GL.PopMatrix();
            this.mGridMaterial.SetPass((!EditorGUIUtility.isProSkin) ? 3 : 2);
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            this.DrawGridLines(50f * this.mGraphZoom, new Vector2(this.mGraphOffset.x % 50f * this.mGraphZoom, this.mGraphOffset.y % 50f * this.mGraphZoom));
            GL.End();
            GL.PopMatrix();
        }

        private void DrawGridLines(float gridSize, Vector2 offset)
        {
            float num = mGraphRect.x + offset.x;
            if (offset.x < 0f)
            {
                num += gridSize;
            }
            for (float num2 = num; num2 < mGraphRect.x + mGraphRect.width; num2 += gridSize)
            {
                this.DrawLine(new Vector2(num2, mGraphRect.y), new Vector2(num2, mGraphRect.y + mGraphRect.height));
            }
            float num3 = mGraphRect.y + offset.y;
            if (offset.y < 0f)
            {
                num3 += gridSize;
            }
            for (float num4 = num3; num4 < mGraphRect.y + mGraphRect.height; num4 += gridSize)
            {
                this.DrawLine(new Vector2(mGraphRect.x, num4), new Vector2(mGraphRect.x + mGraphRect.width, num4));
            }
        }

        private void DrawLine(Vector2 p1, Vector2 p2)
        {
            GL.Vertex(p1);
            GL.Vertex(p2);
        }
        private void saveNodeDfs(TreeNode root, StreamWriter st, int parent)
        {
            linenumber++;
            if (root.btype == BehaviorType.Leaf)
            {
                st.WriteLine(String.Format("{{id={0},type={1},parent={2},file=\"{3}\",param=\"{4}\"}},\n", linenumber, (int)root.btype, parent, mActions[root.actionIndex], root.actionParams));
            }
            else
            {
                st.WriteLine(String.Format("{{id={0},type={1},parent={2}}},\n", linenumber, (int)root.btype, parent));
            }
            int tmp = linenumber;
            for (int i = 0; i < root.childList.Count; i++)
            {
                var child = root.childList[i];
                saveNodeDfs(child, st, tmp);
            }
        }

        private void saveBTLua()
        {
            if (luaBt_1 != null)
            {
                string path = Application.dataPath + "/LuaScripts/BTAi/" + luaBt_1.name + ".txt";
                var sr = new StreamWriter(path);
                sr.WriteLine("local node={\n");
                linenumber = 0;
                saveNodeDfs(TreeNodeMgr.Instance.RootNode, sr, 0);
                sr.WriteLine("}\n\nreturn node");
                sr.Close();
                EditorUtility.DisplayDialog("成功", "保存成功，文件路径："+ path, "确定");
            }

        }


    }
}

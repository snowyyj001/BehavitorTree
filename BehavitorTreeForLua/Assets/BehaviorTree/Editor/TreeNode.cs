using BehaviorTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum BehaviorType
{
    DecoratorInverter, DecoratorSucceeder, DecoratorRepeater, DecoratorFail,
    CompositeParallel = 100, CompositeSequence, CompositeSelector, CompositeRandom,
    Leaf = 200,
};

public class TreeNode : GUIContent
{
    public TreeNode parent;
    public List<TreeNode> childList;
    public BehaviorType btype;
    private bool isRoot;
    public int actionIndex;
    public string actionParams;
    public int nodexIndex;

    //gui
    public Vector2 m_pos;
    protected bool m_isPreview;
    public bool m_isClick = false;
    protected Vector2 m_size = new Vector2(100, 50);
    private GenericMenu m_Menu;
    public Texture m_SelImage;


    public TreeNode(bool isroot = false)
    {
        isRoot = isroot;
        Init();
    }

    public void Show()
    {
        GUI.DrawTexture(new Rect(this.m_pos, this.m_size), this.image);

        GUIStyle style = new GUIStyle();
        style.fontSize = 25;
        style.normal.textColor = new Color(0, 0, 0);
        style.alignment = TextAnchor.MiddleCenter;
        style.clipping = TextClipping.Overflow;

        if (btype == BehaviorType.Leaf)
        {
            GUI.Label(new Rect(this.m_pos, this.m_size), MainWindow.mActions[actionIndex], style);
        }

        foreach (var child in childList)
        {
            Vector2 from = m_pos;
            from.x += m_size.x / 2;
            from.y += m_size.y;
            Vector2 to = child.m_pos;
            to.x += child.m_size.x / 2;
            DrawArrow(from, to, Color.red);
        }
        if (this.m_isClick && btype == BehaviorType.Leaf)
        {
            GUI.DrawTexture(new Rect(this.m_pos, this.m_size), this.m_SelImage);
        }
        this.HandleEvents();
    }

    private void DrawArrow(Vector2 from, Vector2 to, Color color)
    {
        Handles.BeginGUI();
        Handles.color = color;
        Handles.DrawAAPolyLine(3, from, to);
        Vector2 v0 = from - to;
        v0 *= 10 / v0.magnitude;
        Vector2 v1 = new Vector2(v0.x * 0.866f - v0.y * 0.5f, v0.x * 0.5f + v0.y * 0.866f);
        Vector2 v2 = new Vector2(v0.x * 0.866f + v0.y * 0.5f, v0.x * -0.5f + v0.y * 0.866f); ;
        Handles.DrawAAPolyLine(3, to + v1, to, to + v2);
        Handles.EndGUI();
    }

    protected virtual void Init()
    {
        btype = BehaviorType.DecoratorSucceeder;
        image = Resources.Load("BehaviorTree/EditorRes/decorator-success") as Texture;
        m_SelImage = Resources.Load("BehaviorTree/EditorRes/select") as Texture;
        m_pos = new Vector2(-1, -1);
        childList = new List<TreeNode>();
        actionIndex = 0;
        actionParams = "";

        m_Menu = new GenericMenu();
        m_Menu.AddItem(new GUIContent("删除本节点"), false, Callback, "removechild");
        m_Menu.AddItem(new GUIContent("添加子节点"), false, Callback, "addchild");
        m_Menu.AddItem(new GUIContent("清空子节点"), false, Callback, "clearchild");
        if (isRoot)
        {
            return;
        }


        m_Menu.AddSeparator("");

        m_Menu.AddItem(new GUIContent("标记节点类型/修饰节点/逆变"), false, Callback, "decorator-Inverter");
        m_Menu.AddItem(new GUIContent("标记节点类型/修饰节点/成功"), false, Callback, "decorator-Succeeder");
        m_Menu.AddItem(new GUIContent("标记节点类型/修饰节点/重复"), false, Callback, "decorator-Repeater");
        m_Menu.AddItem(new GUIContent("标记节点类型/修饰节点/重复直至失败"), false, Callback, "decorator-Repeater-Fail");

        m_Menu.AddItem(new GUIContent("标记节点类型/组合节点/并行"), false, Callback, "composite-Parallel");
        m_Menu.AddItem(new GUIContent("标记节点类型/组合节点/序列"), false, Callback, "composite-Sequence");
        m_Menu.AddItem(new GUIContent("标记节点类型/组合节点/选择"), false, Callback, "composite-Selector");
        m_Menu.AddItem(new GUIContent("标记节点类型/组合节点/随机"), false, Callback, "composite-Random");
        m_Menu.AddItem(new GUIContent("标记节点类型/叶节点"), false, Callback, "leaf");
    }

    void Callback(object userData)
    {
        string param = userData.ToString();
        if (param == "removechild")
        {
            this.parent.childList.Remove(this);
            if (this.parent.childList.Count == 0)
            {
                Callback("leaf");
            }
        }
        if (param == "addchild")
        {
            if (btype == BehaviorType.DecoratorInverter || btype == BehaviorType.DecoratorSucceeder || btype == BehaviorType.DecoratorRepeater || btype == BehaviorType.DecoratorFail)
            {
                if (childList.Count != 0)
                {
                    EditorUtility.DisplayDialog("错误", "请确保逆变节点必须有且只有一个子节点", "确定");
                    return;
                }
            }
            var node = new TreeNode();
            node.parent = this;
            node.Callback("leaf");
            childList.Add(node);
            if (btype == BehaviorType.Leaf)
            {
                Callback("composite-Sequence");
            }
        }
        if (param == "clearchild")
        {
            childList.Clear();
            Callback("leaf");
        }
        if (param == "decorator-Inverter")
        {
            if (childList.Count != 1)
            {
                EditorUtility.DisplayDialog("错误", "请确保修饰节点必须有且只有一个子节点", "确定");
                return;
            }
            image = Resources.Load("BehaviorTree/EditorRes/decorator-inverter") as Texture;
            btype = BehaviorType.DecoratorInverter;
        }
        if (param == "decorator-Succeeder")
        {
            if (childList.Count != 1)
            {
                EditorUtility.DisplayDialog("错误", "请确保修饰节点必须有且只有一个子节点", "确定");
                return;
            }
            image = Resources.Load("BehaviorTree/EditorRes/decorator-success") as Texture;
            btype = BehaviorType.DecoratorSucceeder;
        }
        if (param == "decorator-Repeater")
        {
            if (childList.Count != 1)
            {
                EditorUtility.DisplayDialog("错误", "请确保修饰节点必须有且只有一个子节点", "确定");
                return;
            }
            image = Resources.Load("BehaviorTree/EditorRes/decorator-repeat") as Texture;
            btype = BehaviorType.DecoratorRepeater;
        }
        if (param == "decorator-Repeater-Fail")
        {
            if (childList.Count != 1)
            {
                EditorUtility.DisplayDialog("错误", "请确保修饰节点必须有且只有一个子节点", "确定");
                return;
            }
            image = Resources.Load("BehaviorTree/EditorRes/decorator-repeat-fail") as Texture;
            btype = BehaviorType.DecoratorFail;
        }
        if (param == "composite-Parallel")
        {
            if (childList.Count == 0)
            {
                EditorUtility.DisplayDialog("错误", "请确保组合节点至少有一个子节点", "确定");
                return;
            }
            image = Resources.Load("BehaviorTree/EditorRes/composite-parallel") as Texture;
            btype = BehaviorType.CompositeParallel;
        }
        if (param == "composite-Sequence")
        {
            if (childList.Count == 0)
            {
                EditorUtility.DisplayDialog("错误", "请确保组合节点至少有一个子节点", "确定");
                return;
            }
            image = Resources.Load("BehaviorTree/EditorRes/composite-sequence") as Texture;
            btype = BehaviorType.CompositeSequence;
        }
        if (param == "composite-Selector")
        {
            if (childList.Count == 0)
            {
                EditorUtility.DisplayDialog("错误", "请确保组合节点至少有一个子节点", "确定");
                return;
            }
            image = Resources.Load("BehaviorTree/EditorRes/composite-selector") as Texture;
            btype = BehaviorType.CompositeSelector;
        }
        if (param == "composite-Random")
        {
            if (childList.Count == 0)
            {
                EditorUtility.DisplayDialog("错误", "请确保组合节点至少有一个子节点", "确定");
                return;
            }
            image = Resources.Load("BehaviorTree/EditorRes/composite-random") as Texture;
            btype = BehaviorType.CompositeRandom;
        }
        if (param == "leaf")
        {
            if (childList.Count > 0)
            {
                EditorUtility.DisplayDialog("错误", "请确保叶子节点没有子节点", "确定");
                return;
            }
            image = Resources.Load("BehaviorTree/EditorRes/leaf") as Texture;
            btype = BehaviorType.Leaf;
        }
    }


    private void HandleEvents()
    {
        if (Event.current.type == EventType.MouseDown)
        {
            if (new Rect(this.m_pos, this.m_size).Contains(Event.current.mousePosition))
            {
                if(this.parent.btype == BehaviorType.CompositeSequence)
                {
                    TreeNodeMgr.Instance.DragingNode = this;
                    return;
                }
            } 
        }
        if (Event.current.type == EventType.MouseDrag)
        {
            if (TreeNodeMgr.Instance.DragingNode == this)
            {
                this.m_pos = Event.current.mousePosition - this.m_size / 2;
                return;
            }
        }
            
        if (Event.current.type == EventType.MouseUp)
        {
            if (TreeNodeMgr.Instance.DragingNode != null)
            {
                var parent = TreeNodeMgr.Instance.DragingNode.parent;
                int childindex = 0;
                for (int i = 0; i < parent.childList.Count; i++)
                {
                    if (TreeNodeMgr.Instance.DragingNode == parent.childList[i])
                    {
                        childindex = i;
                        break;
                    }
                }
                parent.childList.Sort((a, b) => {
                    if( a.m_pos.x < b.m_pos.x)
                    {
                        return -1;
                    }
                    else if (a.m_pos.x == b.m_pos.x)
                    {
                        return 0;
                    }
                    {
                        return 1;
                    }
                });
                TreeNodeMgr.Instance.DragingNode = null;
                return;
            }
            
            if (Event.current.button == 0)
            {
                if (new Rect(this.m_pos, this.m_size).Contains(Event.current.mousePosition))
                {
                    this.m_isClick = true;
                    if (TreeNodeMgr.Instance.SelNode != null && TreeNodeMgr.Instance.SelNode != this)
                    {
                        TreeNodeMgr.Instance.SelNode.m_isClick = false;
                    }
                    TreeNodeMgr.Instance.SelNode = this;

                }
            }
            if (Event.current.button == 1)
            {
                if (new Rect(this.m_pos, this.m_size).Contains(Event.current.mousePosition))
                {
                    m_Menu.ShowAsContext();

                }
            }
        }
    }
}

public class TreeNodeMgr
{
    private static TreeNodeMgr mInstance = null;
    public static TreeNodeMgr Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = new TreeNodeMgr();
                mInstance.Init();
            }
            return mInstance;
        }
    }

    const int AREA_WIDTH = 100;
    const int AREA_HEIGHT = 80;
    public TreeNode RootNode;
    public TreeNode SelNode;
    public TreeNode DragingNode;

    public void Init()
    {
        RootNode = new TreeNode(true);
        RootNode.parent = RootNode;
        RootNode.m_pos = new Vector2(100, 100);
    }

    private void DfsTrees(TreeNode root)
    {
        root.Show();
        foreach (var child in root.childList)
        {
            DfsTrees(child as TreeNode);
        }
    }

    private float RePositionTrees(TreeNode root, float startx, float starty)
    {

        if (root.childList.Count > 0)
        {
            float nowx = startx;
            for (int i = 0; i < root.childList.Count; i++)
            {
                nowx = RePositionTrees(root.childList[i], nowx, starty + AREA_HEIGHT);
            }
            root.m_pos.x = (root.childList[0].m_pos.x + root.childList[root.childList.Count - 1].m_pos.x) / 2;
            root.m_pos.y = root.childList[0].m_pos.y - AREA_HEIGHT;
            return nowx;
        }
        else
        {
            root.m_pos.x = startx;
            root.m_pos.y = starty;
            return startx + AREA_WIDTH;
        }
    }
    private void AdjustTrees(TreeNode root, float offsetx, float offsety)
    {
        root.m_pos.x += offsetx;
        root.m_pos.y += offsety;
        foreach (var child in root.childList)
        {
            AdjustTrees(child, offsetx, offsety);
        }
    }


    public void Show(Vector2 pt)
    {
        if (RootNode == null)
        {
            Init();
        }
        var child = RootNode;
        while (child.childList.Count > 0)
        {
            child = child.childList[0];
        }
        if(TreeNodeMgr.Instance.DragingNode == null)
        {
            RePositionTrees(RootNode, 150, 100);
            AdjustTrees(RootNode, pt.x, pt.y);
        }

        DfsTrees(RootNode);

        //  RootNode = null;
    }
}


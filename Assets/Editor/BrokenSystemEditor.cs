using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BrokenSystemNode
{
    public Rect rect;
    public string name;
    public GameObject originObj;
    public List<BrokenSystemNode> childNodes;
}
public class BrokenSystemEditor : EditorWindow
{
    private List<BrokenSystemNode> nodes = new List<BrokenSystemNode>();
    private Vector2 defaultOffset = -Vector2.up;
    private Vector2 defaultPos = new Vector2(10f, 10f);
    private Vector2 defaultSize = new Vector2(10f, 10f);
    [MenuItem("Window/BrokenSystemEditor")]
    public static void CreateEditor()
    {
        BrokenSystemEditor editor = EditorWindow.CreateWindow<BrokenSystemEditor>();
        editor.Show();
    }
    private void OnGUI()
    {
        GetAllBreakGroup();
        Stack<BrokenSystemNode> stack = new Stack<BrokenSystemNode>();
        for(int i=nodes.Count-1;i>=0;--i)
        {
            stack.Push(nodes[i]);
        }

    }
    private void GetAllBreakGroup()
    {
        nodes.Clear();
        foreach (var item in GameObject.FindObjectsOfType<BrokenSys.BreakableGroup>())
        {
            var node = new BrokenSystemNode() { originObj = item.gameObject, name = item.gameObject.name, childNodes = null };
            Vector2 pos = defaultPos + defaultOffset;
            node.rect.x = pos.x;
            node.rect.y = pos.y;
            node.rect.width = defaultSize.x;
            node.rect.height = defaultSize.y;
            nodes.Add(node);
        }
    }
}

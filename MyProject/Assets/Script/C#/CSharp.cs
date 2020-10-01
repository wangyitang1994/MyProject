using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSharp : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Space)){
            List<int> list = new List<int>{323,5667,13,6674,123,6,12,98,68,85};
            BinaryTree tree = new BinaryTree();
            for(int i = 0;i<list.Count;i++){
                tree.Add(list[i]);
            }
            //tree.Find(6674);
            tree.Delete(5667);
            List<BinaryNode> bnList = tree.MiddleTraversal();
            SetText(bnList);
        }

    }
    void SetText(List<BinaryNode> bnList){
        Text text = GameObject.Find("Canvas/Text").GetComponent<Text>();
        string str = "";
        foreach (var item in bnList)
        {
            str += item.data+" ";
        }
        text.text = str;
    }
}

public class BinaryNode{
    public int data;
    public BinaryNode leftChild;
    public BinaryNode rightChild;
    public BinaryNode parent;
    public BinaryNode(int item){
        data = item;
    }
    public static bool operator > (BinaryNode lnode,BinaryNode rnode){
        return lnode.data > rnode.data;
    }

    public static bool operator < (BinaryNode lnode,BinaryNode rnode){
        return lnode.data < rnode.data;
    }
}

public class BinaryTree{
    BinaryNode root;
    public void Add(int item){
        BinaryNode node = new BinaryNode(item);
        Add(ref root,null,node);
    }

    private void Add(ref BinaryNode root,BinaryNode parent,BinaryNode node){
        if(root == null){
            root = node;
            root.parent = parent;
        }
        else{
            if(node<root){
                Add(ref root.leftChild,root,node);
            }
            else{
                Add(ref root.rightChild,root,node);
            }
        }
    }

    public List<BinaryNode> MiddleTraversal(){
        List<BinaryNode> temp = new List<BinaryNode>();
        MiddleTraversal(ref temp,root);
        return temp;
    }

    private void MiddleTraversal(ref List<BinaryNode> list,BinaryNode root){
        if(root==null)return;
        MiddleTraversal(ref list,root.leftChild);
        list.Add(root);
        MiddleTraversal(ref list,root.rightChild);
    }

    public BinaryNode Find(int value){
        return Find(root,value);
    }

    private BinaryNode Find(BinaryNode root,int value){
        if(root == null)return null;
        if(root.data == value){
            return root;
        }
        else{
            if(value<root.data){
                return Find(root.leftChild,value);
            }
            else{
                return Find(root.rightChild,value);
            }
        }
    }

    public bool Delete(int value){
        BinaryNode target = Find(value);
        return Delete(target);
    }

    public bool Delete(BinaryNode target){
        if(target.parent==null){
            root = null;
            return false;
        }
        if(target != null){
            //如果是叶节点就直接删除引用关系
            if(target.leftChild == null && target.rightChild == null){
                BinaryNode parent = target.parent;
                if(parent.leftChild == target){
                    parent.leftChild = null;
                }else if(parent.rightChild == target){
                    parent.rightChild = null;
                }
            }else{
                BinaryNode new_target = target;
                if(new_target.leftChild != null){
                    new_target = target.leftChild;
                    while(true){
                        if(new_target.rightChild != null){
                            new_target = new_target.rightChild;
                        }else{
                            break;
                        }
                    }
                }else{
                    new_target = target.rightChild;
                    while(true){
                        if(new_target.leftChild != null){
                            new_target = new_target.leftChild;
                        }else{
                            break;
                        }
                    }
                }
                target.data = new_target.data;
                Delete(new_target);
            }
            return true;
        }else{
            Debug.LogWarning("BinaryTree.Delete():target not exist...");
            return false;
        }
    }
}

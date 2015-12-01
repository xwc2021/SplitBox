using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplitManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

    int now_count = 1;
    public int max_count = 20;

    int receive_count;
    int max_receive_count;

    Stack<List<SplitBox>> batch_stack= new Stack<List<SplitBox>>();//記錄下每一批的分裂節點
    List<SplitBox> leaf_list = new List<SplitBox>();//葉節點才可以分裂
    SplitBox root_node;

    void start_separate(SplitBox node)
    {
        root_node = node;
        batch_stack.Clear();
        leaf_list.Clear();

        leaf_list.Add(node);
        now_count = 1;

        print("start_separate");
        do_separate(true);
    }

    void do_separate(bool first_split)
    {
        if (now_count < max_count)//開始分裂
        {
            List<SplitBox> split_list = new List<SplitBox>();
            List<SplitBox> temp_leaf_list = new List<SplitBox>();
            if (first_split)
            { 
                foreach (SplitBox node in leaf_list)
                {       
                    //分裂
                    node.do_split(temp_leaf_list);
                    split_list.Add(node);
                }
            }
            else
            {
                int diff = max_count - now_count;
                foreach (SplitBox node in leaf_list)
                {
                    if (diff == 0)
                        break;

                    if (Random.value > 0.5f)//分裂
                    {
                        node.do_split(temp_leaf_list);
                        split_list.Add(node);
                        --diff;
                    }
                    else
                        temp_leaf_list.Add(node);
                }
            }

            if (split_list.Count > 0)
            {
                leaf_list = temp_leaf_list;//更新leaf_list
                batch_stack.Push(split_list);//記錄下這批的的分裂node

                now_count = now_count + split_list.Count;
                max_receive_count = split_list.Count;
                receive_count = 0;

                print("do_separate split_count=" + split_list.Count + "now_count=" + now_count);

                //演出node分開
                foreach (SplitBox node in split_list)
                    node.call_child_split_move();
            }
            else 
                do_separate(false);
        }
        else
        {
            do_merger();
        }
    }

    
    void separate_ok()
    {
        ++receive_count;
        print("separate_ok receive_count=" + receive_count+ "now_count="+ now_count);
        if (receive_count== max_receive_count)
            do_separate(false);
    }

    void do_merger()
    {
        print("batch_stack count=" + batch_stack.Count);
        if (batch_stack.Count > 0)
        {
            print("do_merger");
            List<SplitBox> split_list = batch_stack.Pop();

            max_receive_count = split_list.Count;
            receive_count = 0;

            //演出node合併
            foreach (SplitBox node in split_list)
                node.call_child_merge_move();
        }
        else
        {
            root_node.reset_is_click();
        }
    }

    void merger_ok()
    {
        ++receive_count;
        print("merger_ok receive_count=" + receive_count + "max_receive_count=" + max_receive_count);
        if (receive_count == max_receive_count)
            do_merger();
    }

    // Update is called once per frame
    void Update () {
	
	}
}

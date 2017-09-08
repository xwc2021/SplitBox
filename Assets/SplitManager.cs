using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplitManager : MonoBehaviour {

    enum ManagerState { init, split, merge, wait }
    ManagerState state = ManagerState.init;

    int now_count = 1;
    public int max_count = 20;

    int receive_count;
    int max_receive_count;

    Stack<List<SplitBox>> batch_stack= new Stack<List<SplitBox>>();//記錄下每一批的分裂節點
    List<SplitBox> leaf_list = new List<SplitBox>();//葉節點才可以分裂
    SplitBox root_node;

    bool firstSplit;
    void start_split(SplitBox node)
    {
        root_node = node;
        batch_stack.Clear();
        leaf_list.Clear();

        leaf_list.Add(node);
        now_count = 1;

        firstSplit = true;
        print("start_separate");

        state = ManagerState.split;
    }

    void doSplitFirst()
    {
        List<SplitBox> split_list = new List<SplitBox>();
        List<SplitBox> temp_leaf_list = new List<SplitBox>();
        foreach (SplitBox node in leaf_list)
        {
            //分裂
            node.do_split(temp_leaf_list);
            split_list.Add(node);
        }

        doSplitFunc(split_list, temp_leaf_list);
    }

    void doSplit()
    {
        List<SplitBox> split_list = new List<SplitBox>();
        List<SplitBox> temp_leaf_list = new List<SplitBox>();
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

        if (split_list.Count > 0)//沒東西
            doSplitFunc(split_list, temp_leaf_list);
        else
            doSplit();
    }

    void doSplitFunc(List<SplitBox> split_list, List<SplitBox> temp_leaf_list)
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

    void separate_ok()
    {
        ++receive_count;
        print("separate_ok receive_count=" + receive_count+ "now_count="+ now_count);
        if (receive_count== max_receive_count)
            select();
    }

    void select()
    {
        if (now_count < max_count)//開始分裂
        {
            state = ManagerState.split;
            return;
        }

        if (batch_stack.Count > 0)
        {
            state = ManagerState.merge;
            return;
        }
        else
        {
            state = ManagerState.init;
            root_node.reset_is_click();
        } 
    }

    void doMerger()
    {
        print("batch_stack count=" + batch_stack.Count);

        print("do_merger");
        List<SplitBox> split_list = batch_stack.Pop();

        max_receive_count = split_list.Count;
        receive_count = 0;

        //演出node合併
        foreach (SplitBox node in split_list)
            node.call_child_merge_move();
    }

    void merge_ok()
    {
        ++receive_count;
        print("merge_ok receive_count=" + receive_count + "max_receive_count=" + max_receive_count);
        if (receive_count == max_receive_count)
            select();
    }

    private void Update()
    {
        switch (state)
        {
            case ManagerState.init:
                break;

            case ManagerState.split:
                state_split();
                break;

            case ManagerState.merge:
                state_merge();
                break;

            case ManagerState.wait:
                break;
        }
    }

    void state_merge()
    {
        doMerger();

        state = ManagerState.wait;
    }

    void state_split()
    {
        if (firstSplit)
        {
            doSplitFirst();
            firstSplit = false;
        }
        else
            doSplit();

        state = ManagerState.wait;
    }
}

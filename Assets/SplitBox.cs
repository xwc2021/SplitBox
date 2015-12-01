using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplitBox : MonoBehaviour {

    float split_factor;
    enum SplitBoxState { non,split_move, split_move_end, merge_move, merge_move_end}
    SplitBoxState state = SplitBoxState.non;
    enum Dir {X,Y,Z };
    Dir direction;

    public SplitManager manager;
    public bool can_click = false;
    bool is_click = false;
    MeshRenderer render;
    BoxCollider collider;

    public void reset_is_click()
    {
        is_click = false;
    }

    // Use this for initialization
    void Start () {
        render = GetComponent<MeshRenderer>();
        collider = GetComponent<BoxCollider>();

        collider.enabled = can_click;
    }

    SplitBox left  = null;
    SplitBox right = null;
    SplitBox parent = null;

    Vector3 target;
    Vector3 origin;

    int receive_count = 0;
    int max_receive_count = 2;
    public float speed =10.0f;

    public void do_split(List<SplitBox> leaf_list)
    { 
        //direction = Dir.Z;
        direction = (Dir)Mathf.Floor(Random.value * 3.0f);
        //split_factor = 0.2f;
        split_factor = 0.1f*(Mathf.Floor(Random.value * 9.0f)+1.0f);
        float remaining = 1.0f - split_factor;
        //print("split_factor=" + split_factor);

        Vector3 position_left =Vector3.zero;
        Vector3 position_right = Vector3.zero;
        if (direction == Dir.X)
        {
            float len = transform.localScale.x;
            float h_len = 0.5f * len;
            position_left = transform.position + (-h_len + h_len * split_factor)* transform.right;
            position_right = transform.position + (h_len - h_len * remaining) *transform.right;
        }
        else if (direction == Dir.Y)
        {
            float len = transform.localScale.y;
            float h_len = 0.5f * len;
            position_left = transform.position + (-h_len + h_len * split_factor) * transform.up;
            position_right = transform.position + (h_len - h_len * remaining) * transform.up;
        }
        else if (direction == Dir.Z)
        {
            float len = transform.localScale.z;
            float h_len = 0.5f * len;
            position_left = transform.position + (-h_len + h_len * split_factor) * transform.forward;
            position_right = transform.position + (h_len - h_len * remaining) * transform.forward;
        }

        left = (SplitBox)Instantiate(this, position_left, transform.rotation);
        right = (SplitBox)Instantiate(this, position_right, transform.rotation);

        left.parent = this;
        right.parent = this;

        left.origin = position_left;
        right.origin = position_right;

        left.can_click = false;
        right.can_click = false;

        float split_distance = Random.value * 5.0f;

        if (direction == Dir.X)
        {
            left.target = position_left- split_distance* transform.right;
            right.target = position_right + split_distance * transform.right;
        }
        else if (direction == Dir.Y)
        {
            left.target = position_left - split_distance * transform.up;
            right.target = position_right + split_distance * transform.up;
        }
        else if (direction == Dir.Z)
        {
            left.target = position_left - split_distance * transform.forward;
            right.target = position_right + split_distance * transform.forward;
        }

        leaf_list.Add(left);
        leaf_list.Add(right);

        if (direction == Dir.X)
        {
            float len = transform.localScale.x;
            left.transform.localScale = new Vector3(len * split_factor, transform.localScale.y, transform.localScale.z);
            right.transform.localScale = new Vector3(len * remaining, transform.localScale.y, transform.localScale.z);
        }
        else if (direction == Dir.Y)
        {
            float len = transform.localScale.y;
            left.transform.localScale = new Vector3(transform.localScale.x, len * split_factor , transform.localScale.z);
            right.transform.localScale = new Vector3(transform.localScale.x, len * remaining, transform.localScale.z);
        }
        else if(direction == Dir.Z)
        {
            float len = transform.localScale.z;
            left.transform.localScale = new Vector3( transform.localScale.x, transform.localScale.y, len * split_factor);
            right.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, len * remaining);
        }

        this.render.enabled = false;
    }

    public void call_child_split_move()
    {
        receive_count = 0;
        left.goTarget();
        right.goTarget();
    }

    public void call_child_merge_move()
    {
        receive_count = 0;
        left.backToOrigin();
        right.backToOrigin();
    }

    void goTarget()
    {
        state = SplitBoxState.split_move;
    }

    void backToOrigin()
    {
        state = SplitBoxState.merge_move;
    }


    void OnMouseDown()
    {
        if (!is_click)
        {
            manager.SendMessage("start_separate", this);
            is_click = true;
        }

    }

    void call_parent_separate_ok()
    {
        ++receive_count;
        if (receive_count == max_receive_count)
            manager.SendMessage("separate_ok");
    }

    void call_parent_merger_ok()
    {
        ++receive_count;
        if (receive_count == max_receive_count)
        {
            render.enabled = true;
            manager.SendMessage("merger_ok");
        }
    }

    // Update is called once per frame
    void Update () {


        float Epsilon = 0.01f;
        if (state == SplitBoxState.split_move)
        {
            transform.position = Vector3.Lerp(transform.position, target, speed*Time.deltaTime);

            if ((transform.position - target).magnitude < Epsilon)
            {
                transform.position = target;
                state = SplitBoxState.split_move_end;
                parent.SendMessage("call_parent_separate_ok");
            }
        }
        else if (state == SplitBoxState.split_move_end)
        {


        }
        else if (state == SplitBoxState.merge_move)
        {
            transform.position = Vector3.Lerp(transform.position, origin, speed*Time.deltaTime);

            if ((transform.position - origin).magnitude < Epsilon)
            {
                transform.position = origin;
                parent.SendMessage("call_parent_merger_ok");
                state = SplitBoxState.merge_move_end;
            }
        }
        else if (state == SplitBoxState.merge_move_end)
        {
            Destroy(this.gameObject);
        }
    }
}

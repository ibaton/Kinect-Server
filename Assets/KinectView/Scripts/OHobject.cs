using UnityEngine;
using System.Collections;
using System.Net;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class OHobject : MonoBehaviour
{

    const float HOVER_TIMER = 0.7f;
    const float MOVE_SPEED = 4f;
    const float SCALE_MULTIPLIER = 0.005f;

    private String itemName = "item";
    private bool selected = false;
    private GameObject inputFieldGo;

    float hoverTimer;
    bool hover = false;

    bool state = false;

    public bool Selected
    {
        get
        {
            return this.selected;
        }
        set
        {
            selected = value;
            if (!selected)
            {
                removeUI();
            }
            updateSelected();
        }
    }

    public string ItemName
    {
        get
        {
            return this.itemName;
        }
        set
        {
            itemName = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        rend.material.color = Color.blue;
    }

    /*void OnMouseDrag()
    {
        float distance_to_screen = Camera.current.ScreenPointToRay(gameObject.transform.position).z;
        transform.position = Camera.current.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
    }*/

    private float dist;
    private Vector3 v3Offset;
    private Plane plane;

    // Update is called once per frame
    void Update()
    {
        if (hover)
        {
            Debug.Log("hover time " + (Time.fixedTime - hoverTimer));
        }
        if (hover && (Time.fixedTime - hoverTimer) > HOVER_TIMER)
        {
            NetworkHandler network = GameObject.Find("MainBase").GetComponentInChildren<NetworkHandler>();
            network.SendCommand(ServerHostText.host, itemName, "ON");
            hoverTimer = Time.fixedTime;
        }

        if (selected)
        {
            float moveX = (Input.GetKey(KeyCode.Keypad4) ? -MOVE_SPEED : 0) + (Input.GetKey(KeyCode.Keypad6) ? MOVE_SPEED : 0);
            float moveY = (Input.GetKey(KeyCode.Keypad3) ? -MOVE_SPEED : 0) + (Input.GetKey(KeyCode.Keypad9) ? MOVE_SPEED : 0);
            float moveZ = (Input.GetKey(KeyCode.Keypad2) ? -MOVE_SPEED : 0) + (Input.GetKey(KeyCode.Keypad8) ? MOVE_SPEED : 0);
            Vector3 move = new Vector3(moveX, moveY, moveZ);
            transform.position += move * MOVE_SPEED * Time.deltaTime;

            float scale = 1 + ((Input.GetKey(KeyCode.Keypad1) ? -SCALE_MULTIPLIER : 0) + (Input.GetKey(KeyCode.Keypad7) ? SCALE_MULTIPLIER : 0));
            transform.localScale = transform.localScale * scale;

            if (Input.GetKeyDown("delete"))
            {
                DataStore.Instance.RemoveOhObject(this.gameObject);
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        hover = true;
        hoverTimer = Time.fixedTime;

        Renderer rend = GetComponent<Renderer>();
        rend.material.color = Color.red;

        Debug.Log("Object OnTriggerEnter");
    }

    public void OnTriggerExit(Collider other)
    {
        Renderer rend = GetComponent<Renderer>();
        rend.material.color = Color.blue;

        hover = false;
    }



    /*void OnCollisionEnter(Collision collision)
    {
        hover = true;
        hoverTimer = Time.fixedTime;

        Renderer rend = GetComponent<Renderer>();
        rend.material.color = Color.red;

        Debug.Log("Object OnCollisionEnter");
    }

    void OnCollisionExit(Collision collision)
    {
        Renderer rend = GetComponent<Renderer>();
        rend.material.color = Color.blue;

        hover = false;
    }*/

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            bool isSelected = true;
            deselectAll();
            Selected = isSelected;

            Debug.Log("Object clicked " + itemName);

            if (isSelected)
            {
                inputFieldGo = GameObject.Find("ItemNameText");
                if (inputFieldGo == null)
                {
                    inputFieldGo = Instantiate(Resources.Load("ItemNameText")) as GameObject;
                    inputFieldGo.transform.position = new Vector3(118, 130, 0);
                    GameObject canvas = GameObject.Find("Canvas");
                    inputFieldGo.transform.SetParent(canvas.transform);
                }
                InputField inputFieldCo = inputFieldGo.GetComponent<InputField>();

                inputFieldCo.onValueChange.RemoveAllListeners();
                inputFieldCo.text = itemName;
                inputFieldCo.onValueChange.AddListener(delegate
                {
                    Debug.Log("OH text " + inputFieldCo.text);
                    itemName = inputFieldCo.text;
                    DataStore.Instance.AddOhObject(this.gameObject);
                });
            }

            DataStore.Instance.AddOhObject(this.gameObject);
        }
    }

    void updateSelected()
    {
        Renderer rend = GetComponent<Renderer>();
        rend.material.color = selected ? Color.green : Color.blue;
    }

    void removeUI()
    {
        if (inputFieldGo != null)
        {
            Destroy(inputFieldGo);
        }
    }

    public void OnDestroy()
    {
        removeUI();
    }

    static void deselectAll()
    {
        foreach (var gameObject in GameObject.FindGameObjectsWithTag("OHItem"))
        {
            OHobject ohObject = gameObject.GetComponent<OHobject>();
            if (ohObject != null)
            {
                ohObject.Selected = false;
            }
        }
    }
}
